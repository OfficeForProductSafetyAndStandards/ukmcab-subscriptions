using UKMCAB.Subscriptions.Core.Common;
using UKMCAB.Subscriptions.Core.Common.Security;
using UKMCAB.Subscriptions.Core.Common.Security.Tokens;
using UKMCAB.Subscriptions.Core.Data;
using UKMCAB.Subscriptions.Core.Data.Models;
using UKMCAB.Subscriptions.Core.Domain;
using UKMCAB.Subscriptions.Core.Domain.Emails;
using UKMCAB.Subscriptions.Core.Domain.Exceptions;
using UKMCAB.Subscriptions.Core.Integration.OutboundEmail;
using static UKMCAB.Subscriptions.Core.Services.SubscriptionService;

namespace UKMCAB.Subscriptions.Core.Services;

public interface ISubscriptionService
{

    /// <summary>
    /// Confirms a requested search subscription.
    /// </summary>
    /// <param name="payload"></param>
    /// <returns></returns>
    Task<ConfirmSearchSubscriptionResult> ConfirmSearchSubscriptionAsync(string payload);

    /// <summary>
    /// Confirms a requested cab subscription.
    /// </summary>
    /// <param name="payload"></param>
    /// <returns></returns>
    Task<ConfirmCabSubscriptionResult> ConfirmCabSubscriptionAsync(string payload);

    /// <summary>
    /// Requests a subscription to a search.  The user will be emailed with a confirmation link which they need to click on to activate/confirm the subscription.
    /// </summary>
    /// <param name="request">The search subscription request</param>
    Task<RequestSubscriptionResult> RequestSubscriptionAsync(SearchSubscriptionRequest request);

    /// <summary>
    /// Requests a subscription to a CAB.  The user will be emailed with a confirmation link which they need to click on to activate/confirm the subscription.
    /// </summary>
    /// <param name="request">The subscription request</param>
    Task<RequestSubscriptionResult> RequestSubscriptionAsync(CabSubscriptionRequest request);
    
    /// <summary>
    /// Block email address (stops all future email being sent to this email address)
    /// </summary>
    /// <param name="emailAddress"></param>
    /// <returns></returns>
    Task<bool> BlockEmailAsync(EmailAddress emailAddress);
    
    /// <summary>
    /// Unblocks an email address by removing it from the block list
    /// </summary>
    /// <param name="emailAddress"></param>
    /// <returns></returns>
    Task<bool> UnblockEmailAsync(EmailAddress emailAddress);

    /// <summary>
    /// Unsubscribes/deletes a subscription
    /// </summary>
    /// <param name="subscriptionId"></param>
    /// <returns></returns>
    Task<bool> UnsubscribeAsync(string subscriptionId);

    /// <summary>
    /// Unsubscribes an email address from all subscriptions
    /// </summary>
    /// <param name="emailAddress"></param>
    /// <returns></returns>
    Task<int> UnsubscribeAllAsync(EmailAddress emailAddress);

    /// <summary>
    /// Requests to update an email address on a subscription
    /// </summary>
    /// <param name="options"></param>
    /// <exception cref="SubscriptionsCoreDomainException">Raised if the email address is on a blocked list, or the email is the same as the one on the current subscription or if there's another subscription for the same topic on that email address</exception>
    /// <returns></returns>
    Task<string> RequestUpdateEmailAddressAsync(UpdateEmailAddressOptions options);

    /// <summary>
    /// Confirms an updated email address
    /// </summary>
    /// <param name="payload"></param>
    /// <returns>The new subscription id</returns>
    Task<string> ConfirmUpdateEmailAddressAsync(string payload);

    /// <summary>
    /// Returns whether the user is already subscribed to a given search
    /// </summary>
    /// <param name="emailAddress"></param>
    /// <param name="searchQueryString"></param>
    /// <returns></returns>
    Task<bool> IsSubscribedToSearchAsync(EmailAddress emailAddress, string? searchQueryString);

    /// <summary>
    /// Returns whether the user is already subscribed to a given cab
    /// </summary>
    /// <param name="emailAddress"></param>
    /// <param name="cabId"></param>
    /// <returns></returns>
    Task<bool> IsSubscribedToCabAsync(EmailAddress emailAddress, Guid cabId);

    /// <summary>
    /// Updates the frequency of a subscription
    /// </summary>
    /// <param name="subscriptionId"></param>
    /// <param name="frequency"></param>
    /// <returns></returns>
    Task UpdateFrequencyAsync(string subscriptionId, Frequency frequency);

    /// <summary>
    /// Retrieves a particular subscription
    /// </summary>
    /// <param name="subscriptionId"></param>
    /// <returns></returns>
    Task<SubscriptionModel?> GetSubscriptionAsync(string subscriptionId);

    /// <summary>
    /// Lists all subscriptions that belong to an email address
    /// </summary>
    /// <param name="emailAddress"></param>
    /// <param name="continuationToken"></param>
    /// <param name="take"></param>
    /// <returns></returns>
    Task<ListSubscriptionsResult> ListSubscriptionsAsync(EmailAddress emailAddress, string? continuationToken = null, int? take = null);
    Task<SearchResultsChangesModel?> GetSearchResultsChangesAsync(string id);
}

public class SubscriptionService : ISubscriptionService, IClearable
{
    private readonly SubscriptionsCoreServicesOptions _options;
    private readonly ILogger<SubscriptionService> _logger;
    private readonly IRepositories _repositories;
    private readonly IOutboundEmailSender _outboundEmailSender;
    private readonly ISecureTokenProcessor _secureTokenProcessor;
    private readonly IEmailTemplatesService _emailTemplatesService;

    public SubscriptionService(SubscriptionsCoreServicesOptions options, 
        ILogger<SubscriptionService> logger, 
        IRepositories repositories, 
        IOutboundEmailSender outboundEmailSender, 
        ISecureTokenProcessor secureTokenProcessor, 
        IEmailTemplatesService emailTemplatesService)
    {
        _options = options;
        _logger = logger;
        _repositories = repositories;
        _outboundEmailSender = outboundEmailSender;
        _secureTokenProcessor = secureTokenProcessor;
        _emailTemplatesService = emailTemplatesService;
        _options.EmailTemplates.Validate();
    }

    public async Task<bool> IsSubscribedToSearchAsync(EmailAddress emailAddress, string? searchQueryString) 
        => await _repositories.Subscriptions.ExistsAsync(new SubscriptionKey(emailAddress, SearchQueryString.Process(searchQueryString, _options))).ConfigureAwait(false);

    public async Task<bool> IsSubscribedToCabAsync(EmailAddress emailAddress, Guid cabId)
        => await _repositories.Subscriptions.ExistsAsync(new SubscriptionKey(emailAddress, cabId)).ConfigureAwait(false);

    public enum ValidationResult { Success, AlreadySubscribed, EmailBlocked }

    public record RequestSubscriptionResult(ValidationResult ValidationResult, string? Token);

    /// <inheritdoc />
    public async Task<RequestSubscriptionResult> RequestSubscriptionAsync(SearchSubscriptionRequest request)
    {
        request = request with { SearchQueryString = SearchQueryString.Process(request.SearchQueryString, _options) };

        var validation = await ValidateRequestAsync(request);

        if (validation == ValidationResult.Success)
        {
            var email = _emailTemplatesService.GetConfirmSearchSubscriptionEmailDefinition(request.EmailAddress, CreateConfirmationToken(request), SearchTopicName.Create(request.Keywords));
            await _outboundEmailSender.SendAsync(email);
            await _repositories.Telemetry.TrackByEmailAddressAsync(request.EmailAddress, $"Requested search subscription");
            return new(validation, email.Token);
        }

        return new(validation, null);
    }

    /// <inheritdoc />
    public async Task<RequestSubscriptionResult> RequestSubscriptionAsync(CabSubscriptionRequest request)
    {
        var validation = await ValidateRequestAsync(request);

        if (validation == ValidationResult.Success)
        {
            var email = _emailTemplatesService.GetConfirmCabSubscriptionEmailDefinition(request.EmailAddress, CreateConfirmationToken(request), request.CabName);
            await _outboundEmailSender.SendAsync(email);
            await _repositories.Telemetry.TrackByEmailAddressAsync(request.EmailAddress, $"Requested cab subscription (cabid={request.CabId})");
            return new(validation, email.Token);
        }

        return new(validation, null);
    }

    public record ConfirmSearchSubscriptionResult(string? SubscriptionId, SearchSubscriptionRequest SearchSubscriptionRequest, ValidationResult ValidationResult);

    /// <inheritdoc />
    public async Task<ConfirmSearchSubscriptionResult> ConfirmSearchSubscriptionAsync(string payload)
    {
        var parsed = _secureTokenProcessor.Disclose<ExpiringToken<SearchSubscriptionRequest>>(payload);
        var options = parsed?.GetAndValidate() ?? throw new Exception("The incoming payload was unparseable");

        var validation = await ValidateRequestAsync(options);

        if (validation == ValidationResult.Success)
        {
            var cleansedSearchQueryString = SearchQueryString.Process(options.SearchQueryString, _options);
            var key = new SubscriptionKey(options.EmailAddress, cleansedSearchQueryString);

            var e = new SubscriptionEntity(key)
            {
                EmailAddress = options.EmailAddress,
                SearchQueryString = options.SearchQueryString,
                SearchKeywords = options.Keywords,
                Frequency = options.Frequency,
            };

            await _repositories.Subscriptions.UpsertAsync(e).ConfigureAwait(false);
            await _repositories.Telemetry.TrackByEmailAddressAsync(e.EmailAddress, $"Confirmed search subscription ({key})");

            return new ConfirmSearchSubscriptionResult(key, options, validation);
        }

        return new ConfirmSearchSubscriptionResult(null, options, validation);
    }


    public record ConfirmCabSubscriptionResult(string? SubscriptionId, CabSubscriptionRequest CabSubscriptionRequest, ValidationResult ValidationResult);

    public async Task<ConfirmCabSubscriptionResult> ConfirmCabSubscriptionAsync(string payload)
    {
        var parsed = _secureTokenProcessor.Disclose<ExpiringToken<CabSubscriptionRequest>>(payload);
        var options = parsed?.GetAndValidate() ?? throw new Exception("The incoming payload was unparseable");

        var validation = await ValidateRequestAsync(options);

        if (validation == ValidationResult.Success)
        {
            var key = new SubscriptionKey(options.EmailAddress, options.CabId);

            var e = new SubscriptionEntity(key)
            {
                EmailAddress = options.EmailAddress,
                CabId = options.CabId,
                Frequency = options.Frequency,
            };

            await _repositories.Subscriptions.UpsertAsync(e).ConfigureAwait(false);
            await _repositories.Telemetry.TrackByEmailAddressAsync(e.EmailAddress, $"Confirmed cab subscription ({key})");

            return new ConfirmCabSubscriptionResult(key, options, validation);
        }

        return new ConfirmCabSubscriptionResult(null, options, validation);
    }


    public record UpdateEmailAddressOptions(string SubscriptionId, EmailAddress EmailAddress);

    /// <inheritdoc />
    public async Task<string> RequestUpdateEmailAddressAsync(UpdateEmailAddressOptions options)
    {
        var sub = await _repositories.Subscriptions.GetAsync(new SubscriptionKey(options.SubscriptionId))
            ?? throw new SubscriptionsCoreDomainException("Subscription does not exist");
        
        await ValidateRequestAsync(options, sub);
        
        var email = _emailTemplatesService.GetConfirmUpdateEmailAddressEmailDefinition(options.EmailAddress, CreateConfirmationToken(options), sub.GetKeys());
        await _outboundEmailSender.SendAsync(email);
        await _repositories.Telemetry.TrackByEmailAddressAsync(options.EmailAddress, $"Requested update email address for subscription ({options.SubscriptionId})");
        await _repositories.Telemetry.TrackByEmailAddressAsync(sub.EmailAddress, $"Requested update email address for subscription ({options.SubscriptionId})");

        return email.Token ?? throw new InvalidOperationException("Token should be present");
    }

    /// <inheritdoc />
    public async Task<string> ConfirmUpdateEmailAddressAsync(string payload)
    {
        var parsed = _secureTokenProcessor.Disclose<ExpiringToken<UpdateEmailAddressOptions>>(payload);
        var options = parsed?.GetAndValidate() ?? throw new Exception("The incoming payload was unparseable");

        var sub = await _repositories.Subscriptions.GetAsync(new SubscriptionKey(options.SubscriptionId))
            ?? throw new SubscriptionsCoreDomainException("Subscription does not exist");

        await ValidateRequestAsync(options, sub);

        var old = new
        {
            Key = new SubscriptionKey(options.SubscriptionId),
            sub.EmailAddress,
        };

        var key = new SubscriptionKey(options.SubscriptionId).WithNewEmail(options.EmailAddress);
        sub.Pipe(x => x.SetKeys(key), x => x.EmailAddress = options.EmailAddress); // update the keys and the email address

        await _repositories.Subscriptions.UpsertAsync(sub);
        await _repositories.Subscriptions.DeleteAsync(old.Key);

        await _repositories.Telemetry.TrackByEmailAddressAsync(options.EmailAddress, $"Confirmed updated email address to '{options.EmailAddress}' on subscription (old: {old.Key}, new: {key})");
        await _repositories.Telemetry.TrackByEmailAddressAsync(old.EmailAddress, $"Confirmed updated email address from '{old.EmailAddress}' to '{options.EmailAddress}' on subscription (old: {old.Key}, new: {key})");

        return key;
    }

    /// <inheritdoc />
    public async Task UpdateFrequencyAsync(string subscriptionId, Frequency frequency)
    {
        var key = new SubscriptionKey(subscriptionId);
        var sub = await _repositories.Subscriptions.GetAsync(key) ?? throw new SubscriptionNotFoundException($"Subscription not found for id {subscriptionId}");
        sub.Frequency = frequency;
        await _repositories.Subscriptions.UpsertAsync(sub).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> UnsubscribeAsync(string subscriptionId)
    {
        var key = new SubscriptionKey(subscriptionId);
        var sub = await _repositories.Subscriptions.GetAsync(key);
        if (sub != null)
        {
            await _repositories.Subscriptions.DeleteAsync(key).ConfigureAwait(false);
            await _repositories.Telemetry.TrackByEmailAddressAsync(sub.EmailAddress ?? throw new InvalidOperationException("EmailAddress cannot be null"), $"Unsubscribed (deleted) search subscription ({key})");
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<int> UnsubscribeAllAsync(EmailAddress emailAddress)
    {
        var page = await (await (_repositories.Subscriptions.GetAllAsync(SubscriptionKey.CreatePartitionKey(emailAddress)))).FirstAsync();
        var count = 0;

        while (page.Values.Count > 0)
        {
            foreach (var subscription in page.Values)
            {
                await _repositories.Subscriptions.DeleteAsync(subscription.GetKeys()).ConfigureAwait(false);
                count++;
            }
            page = await (await (_repositories.Subscriptions.GetAllAsync(SubscriptionKey.CreatePartitionKey(emailAddress)))).FirstAsync();
        }

        await _repositories.Telemetry.TrackByEmailAddressAsync(emailAddress, "Unsubscribed all").ConfigureAwait(false);
        return count;
    }

    public record ListSubscriptionsResult(IEnumerable<SubscriptionModel> Subscriptions, string? ContinuationToken = null);

    public async Task<ListSubscriptionsResult> ListSubscriptionsAsync(EmailAddress emailAddress, string? continuationToken = null, int? take = null)
    {
        var page = await (await (_repositories.Subscriptions.GetAllAsync(SubscriptionKey.CreatePartitionKey(emailAddress), continuationToken, take))).FirstAsync();
        return new(page.Values.Select(x => new SubscriptionModel(x)).ToList(), page.ContinuationToken);
    }

    public async Task<SubscriptionModel?> GetSubscriptionAsync(string subscriptionId)
    {
        var subscription = await _repositories.Subscriptions.GetAsync(new SubscriptionKey(subscriptionId));
        if (subscription != null)
        {
            return new SubscriptionModel(subscription);
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<bool> BlockEmailAsync(EmailAddress emailAddress)
    {
        await UnsubscribeAllAsync(emailAddress).ConfigureAwait(false);

        if (!await _repositories.Blocked.IsBlockedAsync(emailAddress))
        {
            await _repositories.Blocked.BlockAsync(emailAddress);
            await _repositories.Telemetry.TrackByEmailAddressAsync(emailAddress, "Blocked");
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> UnblockEmailAsync(EmailAddress emailAddress)
    {
        if (await _repositories.Blocked.IsBlockedAsync(emailAddress))
        {
            await _repositories.Blocked.UnblockAsync(emailAddress);
            await _repositories.Telemetry.TrackByEmailAddressAsync(emailAddress, "Unblocked");
            return true;
        }
        else
        {
            return false;
        }
    }

    public async Task<SearchResultsChangesModel?> GetSearchResultsChangesAsync(string id)
    {
        var blobs = BlobContainerSnapshots.Create(_options.DataConnectionString);
        var bc = blobs.GetBlobClient(id);
        if (await bc.ExistsAsync())
        {
            var content = await bc.DownloadContentAsync();
            return content.Value.Content.ToObjectFromJson<SearchResultsChangesModel?>();
        }
        else
        {
            return null;
        }
    }

    private async Task<ValidationResult> ValidateRequestAsync(SearchSubscriptionRequest request)
    {
        if (await _repositories.Blocked.IsBlockedAsync(request.EmailAddress))
        {
            return ValidationResult.EmailBlocked;
        }

        if (await IsSubscribedToSearchAsync(request.EmailAddress, request.SearchQueryString))
        {
            return ValidationResult.AlreadySubscribed;
        }

        return ValidationResult.Success;
    }

    private async Task<ValidationResult> ValidateRequestAsync(CabSubscriptionRequest request)
    {
        if (await _repositories.Blocked.IsBlockedAsync(request.EmailAddress))
        {
            return ValidationResult.EmailBlocked;
        }

        if (await IsSubscribedToCabAsync(request.EmailAddress, request.CabId))
        {
            return ValidationResult.AlreadySubscribed;
        }

        return ValidationResult.Success;
    }

    private string CreateConfirmationToken<T>(T options)
    {
        var tok = _secureTokenProcessor.Enclose(new ExpiringToken<T>(options, 7 * 24)) ?? throw new Exception("Token cannot be null");
        return tok;
    }

    private async Task ValidateRequestAsync(UpdateEmailAddressOptions options, SubscriptionEntity sub)
    {
        if (sub.EmailAddress == options.EmailAddress)
        {
            throw new EmailAddressNotDifferentException("The email address supplied is the same as the email address on the subscription");
        }

        if (await _repositories.Blocked.IsBlockedAsync(options.EmailAddress))
        {
            throw new EmailAddressOnBlockedListException("The requested email address is on a block list");
        }

        if (await _repositories.Subscriptions.ExistsAsync(new SubscriptionKey(options.SubscriptionId).WithNewEmail(options.EmailAddress)))
        {
            throw new EmailAddressAlreadySubscribedToTopicException("Already subscribed to this topic under the updated email address");
        }
    }

    async Task IClearable.ClearDataAsync()
    {
        await _repositories.Blocked.DeleteAllAsync().ConfigureAwait(false);
        await _repositories.Subscriptions.DeleteAllAsync().ConfigureAwait(false);
        await _repositories.Telemetry.DeleteAllAsync().ConfigureAwait(false);
    }
}