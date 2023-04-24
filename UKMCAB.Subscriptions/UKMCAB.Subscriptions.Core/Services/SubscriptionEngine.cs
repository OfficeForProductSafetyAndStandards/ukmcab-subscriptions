﻿using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using UKMCAB.Subscriptions.Core.Common;
using UKMCAB.Subscriptions.Core.Data;
using UKMCAB.Subscriptions.Core.Data.Models;
using UKMCAB.Subscriptions.Core.Domain.Emails;
using UKMCAB.Subscriptions.Core.Integration.CabService;
using UKMCAB.Subscriptions.Core.Integration.OutboundEmail;

namespace UKMCAB.Subscriptions.Core.Services;

public interface ISubscriptionEngine
{
    bool CanProcess();
    Task<SubscriptionEngine.ResultAccumulator> ProcessAsync(CancellationToken cancellationToken);
}


public class SubscriptionEngine : ISubscriptionEngine, IClearable
{
    private readonly SubscriptionsCoreServicesOptions _options;
    private readonly ILogger<SubscriptionEngine> _logger;
    private readonly IOutboundEmailSender _outboundEmailSender;
    private readonly IRepositories _repositories;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICabService _cabService;
    private readonly IEmailTemplatesService _emailTemplatesService;
    private readonly BlobContainerClient _blobs;

    public SubscriptionEngine(SubscriptionsCoreServicesOptions options, ILogger<SubscriptionEngine> logger, 
        IOutboundEmailSender outboundEmailSender, IRepositories repositories, IDateTimeProvider dateTimeProvider, ICabService cabService, IEmailTemplatesService emailTemplatesService)
    {
        _options = options;
        _logger = logger;
        _outboundEmailSender = outboundEmailSender;
        _repositories = repositories;
        _dateTimeProvider = dateTimeProvider;
        _cabService = cabService;
        _emailTemplatesService = emailTemplatesService;
        _blobs = new BlobContainerClient(_options.DataConnectionString, $"{SubscriptionsCoreServicesOptions.BlobContainerPrefix}snapshots");
        _options.EmailTemplates.Validate();
    }

    public enum Result { Notified, Initialised, NoChange, NotDue, Error }

    public class ResultAccumulator
    {
        public int Notified { get; set; }
        public int Initialised { get; set; }
        public int NoChange { get; set; }
        public int NotDue { get; set; }
        public int Errors { get; set; }

        public int Accept(Result result) => result switch
        {
            Result.Initialised => ++Initialised,
            Result.Notified => ++Notified,
            Result.NoChange => ++NoChange,
            Result.NotDue => ++NotDue,
            Result.Error => ++Errors,
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// Returns whether the engine *can* process subscriptions (whether the uri templates are configured)
    /// </summary>
    /// <returns></returns>
    public bool CanProcess() => _emailTemplatesService.IsConfigured();

    /// <inheritdoc />
    public async Task<ResultAccumulator> ProcessAsync(CancellationToken cancellationToken)
    {
        _emailTemplatesService.AssertIsUriTemplateOptionsConfigured();

        var rv = new ResultAccumulator();

        await EnsureBlobContainerAsync();

        var pages = await _repositories.Subscriptions.GetAllAsync(take:10);
        await foreach(var page in pages)
        {
            foreach(var subscription in page.Values)
            {
                await ProcessSubscriptionAsync(rv, subscription);

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }

            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
        }

        return rv;
    }

    private async Task ProcessSubscriptionAsync(ResultAccumulator rv, SubscriptionEntity subscription)
    {
        if (subscription.IsInitialised())
        {
            if (subscription.IsDue(_dateTimeProvider))
            {
                rv.Accept(await HandleDueSubscription(subscription));
            }
            else
            {
                rv.Accept(Result.NotDue);
            }
        }
        else
        {
            await InitialiseSubscriptionAsync(subscription);
            rv.Accept(Result.Initialised);
        }
    }

    private Task InitialiseSubscriptionAsync(SubscriptionEntity subscription)=> subscription.SubscriptionType == SubscriptionType.Search 
        ? InitialiseSearchSubscriptionAsync(subscription) 
        : InitialiseCabSubscriptionAsync(subscription);

    private Task<Result> HandleDueSubscription(SubscriptionEntity subscription) => subscription.SubscriptionType == SubscriptionType.Search
        ? HandleDueSearchSubscription(subscription)
        : HandleDueCabSubscription(subscription);

    private async Task InitialiseSearchSubscriptionAsync(SubscriptionEntity subscription)
    {
        Guard.IsTrue(subscription.SubscriptionType == SubscriptionType.Search, $"The subscription type should be '{SubscriptionType.Search}'");
        Guard.IsTrue(subscription.LastThumbprint is null, "The subscription does not need to be initialised.");

        var data = await GetSearchResultDataAsync(subscription.SearchQueryString);

        subscription.LastThumbprint = data.Thumbprint;
        subscription.BlobName = subscription.CreateBlobName();
        subscription.DueBaseDate = _dateTimeProvider.UtcNow;
        await _blobs.UploadBlobAsync(subscription.BlobName, new BinaryData(data.Json)).ConfigureAwait(false);
        await _repositories.Subscriptions.UpsertAsync(subscription).ConfigureAwait(false);
        await _repositories.Telemetry.TrackAsync(subscription.GetKeys(), $"Initialised subscription with thumbprint '{subscription.LastThumbprint}' and blob '{subscription.BlobName}'").ConfigureAwait(false);   
    }

    private async Task InitialiseCabSubscriptionAsync(SubscriptionEntity subscription)
    {
        Guard.IsTrue(subscription.SubscriptionType == SubscriptionType.Cab, $"The subscription type should be '{SubscriptionType.Cab}'");
        Guard.IsTrue(subscription.LastThumbprint is null, "The subscription does not need to be initialised.");
        Guard.IsNotNull(subscription.CabId, $"{nameof(subscription.CabId)} should not be null");
        
        var data = await GetCabDataAsync(subscription.CabId!.Value) ?? throw new Exception("Cab data not found when initialising CAB subscription");

        subscription.LastThumbprint = data.Thumbprint;
        subscription.BlobName = subscription.CreateBlobName();
        subscription.DueBaseDate = _dateTimeProvider.UtcNow;
        await _blobs.UploadBlobAsync(subscription.BlobName, new BinaryData(data.Json)).ConfigureAwait(false);
        await _repositories.Subscriptions.UpsertAsync(subscription).ConfigureAwait(false);
        await _repositories.Telemetry.TrackAsync(subscription.GetKeys(), $"Initialised subscription with thumbprint '{subscription.LastThumbprint}'").ConfigureAwait(false);
    }

    private async Task<Result> HandleDueSearchSubscription(SubscriptionEntity subscription)
    {
        var rv = Result.NoChange;
        Guard.IsTrue(subscription.SubscriptionType == SubscriptionType.Search, $"The subscription type should be '{SubscriptionType.Search}'");
        Guard.IsTrue(subscription.LastThumbprint is not null, "The subscription needs to be initialised.");

        var data = await GetSearchResultDataAsync(subscription.SearchQueryString);
        
        if (subscription.LastThumbprint.DoesNotEqual(data.Thumbprint, StringComparison.Ordinal)) // search results have changed.
        {
            var old = new
            {
                subscription.LastThumbprint,
                subscription.BlobName,
            };

            if (subscription.BlobName != null)
            {
                await _blobs.DeleteBlobIfExistsAsync(subscription.BlobName).ConfigureAwait(false);
            }
            subscription.LastThumbprint = data.Thumbprint;
            subscription.DueBaseDate = _dateTimeProvider.UtcNow;
            await _blobs.UploadBlobAsync(subscription.BlobName, new BinaryData(data.Json)).ConfigureAwait(false);

            await _repositories.Telemetry.TrackAsync(subscription.GetKeys(),
                $"Notified subscription: Search updated. (old:{old.LastThumbprint}; {old.BlobName}, new:{subscription.LastThumbprint}, {subscription.BlobName}) ").ConfigureAwait(false);

            try
            {
                var email = _emailTemplatesService.GetSearchUpdatedEmailDefinition(subscription.EmailAddress, subscription.GetKeys(), subscription.SearchQueryString);
                await _outboundEmailSender.SendAsync(email).ConfigureAwait(false);
                await _repositories.Subscriptions.UpsertAsync(subscription).ConfigureAwait(false);
                await _repositories.Telemetry.TrackAsync(subscription.GetKeys(),
                    $"Notified subscription: Search updated. (old:{old.LastThumbprint}; {old.BlobName}, new:{subscription.LastThumbprint}, {subscription.BlobName}) ").ConfigureAwait(false);
                rv = Result.Notified;
            }
            catch (Exception ex)
            {
                await _repositories.Telemetry.TrackAsync(subscription.GetKeys(), $"Failed to notify change on subscription; error: {ex}").ConfigureAwait(false);
                rv = Result.Error;
            }

        }
        return rv;
    }


    private async Task<Result> HandleDueCabSubscription(SubscriptionEntity subscription)
    {
        var rv = Result.NoChange;
        Guard.IsTrue(subscription.SubscriptionType == SubscriptionType.Cab, $"The subscription type should be '{SubscriptionType.Cab}'");
        Guard.IsTrue(subscription.LastThumbprint is not null, "The subscription needs to be initialised.");

        var cabId = subscription.CabId ?? throw new Exception("Cab ID is null");

        var data = await GetCabDataAsync(cabId); // if the CAB is no longer found, just leave the subscription dormant.

        if (data != null && subscription.LastThumbprint.DoesNotEqual(data.Thumbprint, StringComparison.Ordinal)) // search results have changed.
        {
            var old = new
            {
                subscription.LastThumbprint,
                subscription.BlobName,
            };

            if (subscription.BlobName != null)
            {
                await _blobs.DeleteBlobIfExistsAsync(subscription.BlobName).ConfigureAwait(false);
            }

            subscription.LastThumbprint = data.Thumbprint;
            subscription.DueBaseDate = _dateTimeProvider.UtcNow;

            await _blobs.UploadBlobAsync(subscription.BlobName, new BinaryData(data.Json)).ConfigureAwait(false);

            await _repositories.Telemetry.TrackAsync(subscription.GetKeys(),
                $"Notified subscription: CAB updated. (old:{old.LastThumbprint}; {old.BlobName}, new:{subscription.LastThumbprint}, {subscription.BlobName}) ").ConfigureAwait(false);

            try
            {
                var email = _emailTemplatesService.GetCabUpdatedEmailDefinition(subscription.EmailAddress, subscription.GetKeys(), cabId);
                await _outboundEmailSender.SendAsync(email).ConfigureAwait(false);
                await _repositories.Subscriptions.UpsertAsync(subscription).ConfigureAwait(false);
                await _repositories.Telemetry.TrackAsync(subscription.GetKeys(),
                    $"Notified subscription: Search updated. (old:{old.LastThumbprint}; {old.BlobName}, new:{subscription.LastThumbprint}, {subscription.BlobName}) ").ConfigureAwait(false);
                rv = Result.Notified;
            }
            catch (Exception ex)
            {
                await _repositories.Telemetry.TrackAsync(subscription.GetKeys(), $"Failed to notify change on subscription; error: {ex}").ConfigureAwait(false);
                rv = Result.Error;
            }

        }
        return rv;
    }

    record Data(string Thumbprint, string Json);

    private async Task<Data> GetSearchResultDataAsync(string? searchQueryString)
    {
        var results = (await _cabService.SearchAsync(searchQueryString)) ?? throw new Exception("Search returned null");
        results = results with { Results = results.Results.OrderBy(x => x.CabId).ToList() };
        var json = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = false }) ?? throw new Exception("Serializing search results returned null");
        var thumbprint = json.Md5() ?? throw new Exception("MD5 hashing returned null");
        return new(thumbprint, json);
    }

    private async Task<Data?> GetCabDataAsync(Guid id)
    {
        var cab = await _cabService.GetAsync(id);
        if(cab != null)
        {
            var json = JsonSerializer.Serialize(cab, new JsonSerializerOptions { WriteIndented = false }) ?? throw new Exception("Serializing cab returned null");
            var thumbprint = json.Md5() ?? throw new Exception("MD5 hashing returned null");
            return new(thumbprint, json);
        }
        else
        {
            return null;
        }
    }

    private async Task EnsureBlobContainerAsync()
    {
        if (Memory.Set(GetType(), nameof(EnsureBlobContainerAsync)))
        {
            await _blobs.CreateIfNotExistsAsync();
        }
    }

    async Task IClearable.ClearDataAsync()
    {
        var pages = _blobs.GetBlobsAsync().AsPages(pageSizeHint: 10);
        await foreach (var page in pages)
        {
            foreach (var blob in page.Values)
            {
                await _blobs.DeleteBlobIfExistsAsync(blob.Name).ConfigureAwait(false);
            }
        }
    }
}
