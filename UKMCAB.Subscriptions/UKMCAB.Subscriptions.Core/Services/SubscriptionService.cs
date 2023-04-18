using Azure.Core;
using Microsoft.Extensions.Logging;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using UKMCAB.Subscriptions.Core.Common;
using UKMCAB.Subscriptions.Core.Common.Security;
using UKMCAB.Subscriptions.Core.Common.Security.Tokens;
using UKMCAB.Subscriptions.Core.Data;
using UKMCAB.Subscriptions.Core.Data.Models;
using UKMCAB.Subscriptions.Core.Domain;
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
    Task<ConfirmSubscriptionResult> ConfirmSearchSubscriptionAsync(string payload);

    /// <summary>
    /// Confirms a requested cab subscription.
    /// </summary>
    /// <param name="payload"></param>
    /// <returns></returns>
    Task<ConfirmSubscriptionResult> ConfirmCabSubscriptionAsync(string payload);

    /// <summary>
    /// Requests a subscription to a search.  The user will be emailed with a confirmation link which they need to click on to activate/confirm the subscription.
    /// </summary>
    /// <param name="request">The search subscription request</param>
    /// <param name="absoluteConfirmationUrlFormat">The absolute url (format) to the confirm-subscription page, including the `@payload` replacement token for the payload. e.g., https://ukmcab.service.gov.uk/confirm-subscription?p=@payload</param>
    Task<RequestSubscriptionResult> RequestSubscriptionAsync(SearchSubscriptionRequest request, string absoluteConfirmationUrlFormat);

    /// <summary>
    /// Requests a subscription to a CAB.  The user will be emailed with a confirmation link which they need to click on to activate/confirm the subscription.
    /// </summary>
    /// <param name="request">The subscription request</param>
    /// <param name="absoluteConfirmationUrlFormat">The absolute url (format) to the confirm-subscription page, including the `@payload` replacement token for the payload. e.g., https://ukmcab.service.gov.uk/confirm-subscription?p=@payload</param>
    Task<RequestSubscriptionResult> RequestSubscriptionAsync(CabSubscriptionRequest request, string absoluteConfirmationUrlFormat);
    
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
    /// <param name="absoluteConfirmationUrlFormat"></param>
    /// <exception cref="SubscriptionsCoreDomainException">Raised if the email address is on a blocked list, or the email is the same as the one on the current subscription or if there's another subscription for the same topic on that email address</exception>
    /// <returns></returns>
    Task<string> RequestUpdateEmailAddressAsync(UpdateEmailAddressOptions options, string absoluteConfirmationUrlFormat);

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
    Task<Subscription?> GetSubscriptionAsync(string subscriptionId);

    /// <summary>
    /// Lists all subscriptions that belong to an email address
    /// </summary>
    /// <param name="emailAddress"></param>
    /// <param name="continuationToken"></param>
    /// <param name="take"></param>
    /// <returns></returns>
    Task<ListSubscriptionsResult> ListSubscriptionsAsync(EmailAddress emailAddress, string? continuationToken = null, int? take = null);
}

public interface ISubscriptionDataService
{
    /// <summary>
    /// Deletes all data
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    Task DeleteAllAsync(string code);
}

public class SubscriptionService : ISubscriptionService, ISubscriptionDataService
{
    private readonly SubscriptionServicesCoreOptions _options;
    private readonly ILogger<SubscriptionService> _logger;
    private readonly IRepositories _repositories;
    private readonly IOutboundEmailSender _outboundEmailSender;
    private readonly ISecureTokenProcessor _secureTokenProcessor;

    private const string _placeholderPayload = "@payload";

    public SubscriptionService(SubscriptionServicesCoreOptions options, ILogger<SubscriptionService> logger, IRepositories repositories, IOutboundEmailSender outboundEmailSender, ISecureTokenProcessor secureTokenProcessor)
    {
        _options = options;
        _logger = logger;
        _repositories = repositories;
        _outboundEmailSender = outboundEmailSender;
        _secureTokenProcessor = secureTokenProcessor;
    }

    public async Task<bool> IsSubscribedToSearchAsync(EmailAddress emailAddress, string? searchQueryString) 
        => await _repositories.Subscriptions.ExistsAsync(new SubscriptionKey(emailAddress, SearchQueryString.Process(searchQueryString, _options))).ConfigureAwait(false);

    public async Task<bool> IsSubscribedToCabAsync(EmailAddress emailAddress, Guid cabId)
        => await _repositories.Subscriptions.ExistsAsync(new SubscriptionKey(emailAddress, cabId)).ConfigureAwait(false);

    public enum ValidationResult { Success, AlreadySubscribed, EmailBlocked }

    public record RequestSubscriptionResult(ValidationResult ValidationResult, string? Token);

    /// <inheritdoc />
    public async Task<RequestSubscriptionResult> RequestSubscriptionAsync(SearchSubscriptionRequest request, string absoluteConfirmationUrlFormat)
    {
        request = request with { SearchQueryString = SearchQueryString.Process(request.SearchQueryString, _options) };

        var validation = await ValidateRequestAsync(request);

        if (validation == ValidationResult.Success)
        {
            var createConfirmUrlResult = CreateConfirmUrl(absoluteConfirmationUrlFormat, nameof(ConfirmSearchSubscriptionAsync), request);
            await _outboundEmailSender.SendAsync(_options.EmailTemplateConfirmSearchSubscription, request.EmailAddress, new Dictionary<string, dynamic> { ["link"] = createConfirmUrlResult.Url });
            await _repositories.Telemetry.TrackAsync(request.EmailAddress, $"Requested search subscription");
            return new(validation, createConfirmUrlResult.Token);
        }

        return new(validation, null);
    }

    /// <inheritdoc />
    public async Task<RequestSubscriptionResult> RequestSubscriptionAsync(CabSubscriptionRequest request, string absoluteConfirmationUrlFormat)
    {
        var validation = await ValidateRequestAsync(request);

        if (validation == ValidationResult.Success)
        {
            var createConfirmUrlResult = CreateConfirmUrl(absoluteConfirmationUrlFormat, nameof(ConfirmSearchSubscriptionAsync), request);
            await _outboundEmailSender.SendAsync(_options.EmailTemplateConfirmCabSubscription, request.EmailAddress, new Dictionary<string, dynamic> { ["link"] = createConfirmUrlResult.Url });
            await _repositories.Telemetry.TrackAsync(request.EmailAddress, $"Requested cab subscription (cabid={request.CabId})");
            return new(validation, createConfirmUrlResult.Token);
        }

        return new(validation, null);
    }

    public record ConfirmSubscriptionResult(string? Id, ValidationResult ValidationResult);

    /// <inheritdoc />
    public async Task<ConfirmSubscriptionResult> ConfirmSearchSubscriptionAsync(string payload)
    {
        var parsed = _secureTokenProcessor.Disclose<ExpiringToken<SearchSubscriptionRequest>>(payload);
        var options = parsed?.GetAndValidate() ?? throw new Exception("The incoming payload was unparseable");

        var validation = await ValidateRequestAsync(options);

        if (validation == ValidationResult.Success)
        {
            var key = new SubscriptionKey(options.EmailAddress, SearchQueryString.Process(options.SearchQueryString, _options));

            var e = new SubscriptionEntity(key)
            {
                EmailAddress = options.EmailAddress,
                SearchQueryString = options.SearchQueryString,
                Frequency = options.Frequency,
            };

            await _repositories.Subscriptions.UpsertAsync(e).ConfigureAwait(false);
            await _repositories.Telemetry.TrackAsync(e.EmailAddress, $"Confirmed search subscription ({key})");

            return new ConfirmSubscriptionResult(key, validation);
        }

        return new ConfirmSubscriptionResult(null, validation);
    }


    public async Task<ConfirmSubscriptionResult> ConfirmCabSubscriptionAsync(string payload)
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
            await _repositories.Telemetry.TrackAsync(e.EmailAddress, $"Confirmed cab subscription ({key})");

            return new ConfirmSubscriptionResult(key, validation);
        }

        return new ConfirmSubscriptionResult(null, validation);
    }


    public record UpdateEmailAddressOptions(string SubscriptionId, EmailAddress EmailAddress);

    /// <inheritdoc />
    public async Task<string> RequestUpdateEmailAddressAsync(UpdateEmailAddressOptions options, string absoluteConfirmationUrlFormat)
    {
        var sub = await _repositories.Subscriptions.GetAsync(new SubscriptionKey(options.SubscriptionId))
            ?? throw new SubscriptionsCoreDomainException("Subscription does not exist");
        
        await ValidateRequestAsync(options, sub);

        var createConfirmUrlResult = CreateConfirmUrl(absoluteConfirmationUrlFormat, nameof(ConfirmUpdateEmailAddressAsync), options);
        await _outboundEmailSender.SendAsync(_options.EmailTemplateUpdateEmailAddress, options.EmailAddress, new Dictionary<string, dynamic> { ["link"] = createConfirmUrlResult.Url });
        await _repositories.Telemetry.TrackAsync(options.EmailAddress, $"Requested update email address for subscription ({options.SubscriptionId})");
        await _repositories.Telemetry.TrackAsync(sub.EmailAddress, $"Requested update email address for subscription ({options.SubscriptionId})");

        return createConfirmUrlResult.Token;
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

        await _repositories.Telemetry.TrackAsync(options.EmailAddress, $"Confirmed updated email address to '{options.EmailAddress}' on subscription (old: {old.Key}, new: {key})");
        await _repositories.Telemetry.TrackAsync(old.EmailAddress, $"Confirmed updated email address from '{old.EmailAddress}' to '{options.EmailAddress}' on subscription (old: {old.Key}, new: {key})");

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
            await _repositories.Telemetry.TrackAsync(sub.EmailAddress ?? throw new InvalidOperationException("EmailAddress cannot be null"), $"Unsubscribed (deleted) search subscription ({key})");
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
        var page = await (_repositories.Subscriptions.GetAllAsync(SubscriptionKey.CreatePartitionKey(emailAddress))).FirstAsync();
        var count = 0;

        while (page.Values.Count > 0)
        {
            foreach (var subscription in page.Values)
            {
                await _repositories.Subscriptions.DeleteAsync(subscription.GetKeys()).ConfigureAwait(false);
                count++;
            }
            page = await (_repositories.Subscriptions.GetAllAsync(SubscriptionKey.CreatePartitionKey(emailAddress))).FirstAsync();
        }

        await _repositories.Telemetry.TrackAsync(emailAddress, "Unsubscribed all").ConfigureAwait(false);
        return count;
    }

    public record ListSubscriptionsResult(IEnumerable<Subscription> Subscriptions, string? ContinuationToken = null);

    public async Task<ListSubscriptionsResult> ListSubscriptionsAsync(EmailAddress emailAddress, string? continuationToken = null, int? take = null)
    {
        var page = await (_repositories.Subscriptions.GetAllAsync(SubscriptionKey.CreatePartitionKey(emailAddress), continuationToken, take)).FirstAsync();
        return new(page.Values.Select(x => new Subscription(x.GetKeys(), x.SubscriptionType, x.Frequency)).ToList(), page.ContinuationToken);
    }

    public record Subscription(string Id, SubscriptionType SubscriptionType, Frequency Frequency );

    public async Task<Subscription?> GetSubscriptionAsync(string subscriptionId)
    {
        var subscription = await _repositories.Subscriptions.GetAsync(new SubscriptionKey(subscriptionId));
        if (subscription != null)
        {
            return new Subscription(subscriptionId, subscription.CabId.HasValue ? SubscriptionType.Cab : SubscriptionType.Search, subscription.Frequency);
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
            await _repositories.Telemetry.TrackAsync(emailAddress, "Blocked");
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
            await _repositories.Telemetry.TrackAsync(emailAddress, "Unblocked");
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <inheritdoc />
    async Task ISubscriptionDataService.DeleteAllAsync(string code)
    {
        if (code == "yes_i_really_want_to_delete_everything")
        {
            await _repositories.Blocked.DeleteAllAsync().ConfigureAwait(false);
            await _repositories.Subscriptions.DeleteAllAsync().ConfigureAwait(false);
            await _repositories.Telemetry.DeleteAllAsync().ConfigureAwait(false);
        }
        else
        {
            throw new Exception("code was incorrect");
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

    public record CreateConfirmUrlResult(string Url, string Token);

    private CreateConfirmUrlResult CreateConfirmUrl<T>(string absoluteConfirmationUrlFormat, string method, T options)
    {
        Guard.IsTrue(absoluteConfirmationUrlFormat.DoesContain(_placeholderPayload),
                    $"Parameter {nameof(absoluteConfirmationUrlFormat)} should contain the token '{_placeholderPayload}' which will be replaced with an encrypted payload that can be passed into `{method}`");

        var tok = _secureTokenProcessor.Enclose(new ExpiringToken<T>(options, 7 * 24)) ?? throw new Exception("Token cannot be null");
        var url = absoluteConfirmationUrlFormat.Replace(_placeholderPayload, tok);
        return new(url, tok);
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
}