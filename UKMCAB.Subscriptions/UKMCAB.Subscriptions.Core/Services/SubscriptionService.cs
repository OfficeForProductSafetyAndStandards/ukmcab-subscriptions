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
    /// Requests a subscription to a search.  The user will be emailed with a confirmation link which they need to click on to activate/confirm the subscription.
    /// </summary>
    /// <param name="options">The main search subscription options (options.SearchQueryString should NOT contain any paging info, e.g., page index or page size.)</param>
    /// <param name="absoluteConfirmationUrlFormat">The absolute url (format) to the confirm-subscription page, including the `@payload` replacement token for the payload. e.g., https://ukmcab.service.gov.uk/confirm-subscription?p=@payload</param>
    /// <returns>true; if the request was successfully sent. false; if the email address is on the _blocked_ email list (the 'never send' list).</returns>
    /// <remarks>Remember to exclude any paging info from the search query string</remarks>
    Task<RequestSubscriptionResult> RequestSubscriptionAsync(SearchSubscriptionRequest request, string absoluteConfirmationUrlFormat);
    
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

    Task<bool> IsSubscribedToSearchAsync(EmailAddress emailAddress, string? searchQueryString);
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

    public enum ValidationResult { Success, AlreadySubscribed, EmailBlocked }

    public record RequestSubscriptionResult(ValidationResult ValidationResult, string? Token);

    /// <inheritdoc />
    public async Task<RequestSubscriptionResult> RequestSubscriptionAsync(SearchSubscriptionRequest request, string absoluteConfirmationUrlFormat)
    {
        request = request with { SearchQueryString = SearchQueryString.Process(request.SearchQueryString, _options) };

        var validation = await ValidateSubscribeSearchAsync(request.EmailAddress, request.SearchQueryString);

        if (validation == ValidationResult.Success)
        {
            var createConfirmUrlResult = CreateConfirmUrl(absoluteConfirmationUrlFormat, nameof(ConfirmSearchSubscriptionAsync), request);
            await _outboundEmailSender.SendAsync(_options.EmailTemplateConfirmSubscription, request.EmailAddress, new Dictionary<string, dynamic> { ["link"] = createConfirmUrlResult.Url });
            await _repositories.Telemetry.TrackAsync(request.EmailAddress, $"Requested search subscription");
            return new(validation, createConfirmUrlResult.Token);
        }

        return new(validation, null);
    }

    public record ConfirmSearchSubscriptionResult(string? Id, ValidationResult ValidationResult);

    /// <inheritdoc />
    public async Task<ConfirmSearchSubscriptionResult> ConfirmSearchSubscriptionAsync(string payload)
    {
        var parsed = _secureTokenProcessor.Disclose<ExpiringToken<SearchSubscriptionRequest>>(payload);
        var options = parsed?.GetAndValidate() ?? throw new Exception("The incoming payload was unparseable");

        var validation = await ValidateSubscribeSearchAsync(options.EmailAddress, options.SearchQueryString);

        if (validation == ValidationResult.Success)
        {
            var key = new SubscriptionKey(options.EmailAddress, SearchQueryString.Process(options.SearchQueryString, _options));

            var e = new SubscriptionEntity(key)
            {
                EmailAddress = options.EmailAddress,
                SearchQueryString = options.SearchQueryString,
                Frequency = options.Frequency,
            };

            await _repositories.Subscriptions.AddAsync(e).ConfigureAwait(false);
            await _repositories.Telemetry.TrackAsync(e.EmailAddress, $"Confirmed search subscription ({key})");

            return new ConfirmSearchSubscriptionResult(key, validation);
        }

        return new ConfirmSearchSubscriptionResult(null, validation);
    }

    public record UpdateEmailAddressOptions(string SubscriptionId, EmailAddress EmailAddress);

    /// <inheritdoc />
    public async Task<string> RequestUpdateEmailAddressAsync(UpdateEmailAddressOptions options, string absoluteConfirmationUrlFormat)
    {
        var sub = await _repositories.Subscriptions.GetAsync(new SubscriptionKey(options.SubscriptionId))
            ?? throw new SubscriptionsCoreDomainException("Subscription does not exist");
        
        await ValidateUpdateEmailRequestAsync(options, sub);

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

        await ValidateUpdateEmailRequestAsync(options, sub);

        var old = new
        {
            Key = new SubscriptionKey(options.SubscriptionId),
            sub.EmailAddress,
        };

        var key = new SubscriptionKey(options.SubscriptionId).WithNewEmail(options.EmailAddress);
        sub.Pipe(x => x.SetKeys(key), x => x.EmailAddress = options.EmailAddress); // update the keys and the email address

        await _repositories.Subscriptions.AddAsync(sub);
        await _repositories.Subscriptions.DeleteAsync(old.Key);

        await _repositories.Telemetry.TrackAsync(options.EmailAddress, $"Confirmed updated email address to '{options.EmailAddress}' on subscription (old: {old.Key}, new: {key})");
        await _repositories.Telemetry.TrackAsync(old.EmailAddress, $"Confirmed updated email address from '{old.EmailAddress}' to '{options.EmailAddress}' on subscription (old: {old.Key}, new: {key})");

        return key;
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
        var subscriptions = _repositories.Subscriptions.GetAllAsync(SubscriptionKey.CreatePartitionKey(emailAddress));
        var count = 0;
        await foreach(var subscription in subscriptions)
        {
            await _repositories.Subscriptions.DeleteAsync(subscription.GetKeys()).ConfigureAwait(false);
            count++;
        }
        await _repositories.Telemetry.TrackAsync(emailAddress, "Unsubscribed all").ConfigureAwait(false);
        return count;
    }

    /// <inheritdoc />
    public async Task<bool> BlockEmailAsync(EmailAddress emailAddress)
    {
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

    private async Task<ValidationResult> ValidateSubscribeSearchAsync(EmailAddress emailAddress, string? searchQueryString)
    {
        if (await _repositories.Blocked.IsBlockedAsync(emailAddress))
        {
            return ValidationResult.EmailBlocked;
        }

        if (await IsSubscribedToSearchAsync(emailAddress, searchQueryString))
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

    private async Task ValidateUpdateEmailRequestAsync(UpdateEmailAddressOptions options, SubscriptionEntity sub)
    {
        if (sub.EmailAddress == options.EmailAddress)
        {
            throw new SubscriptionsCoreDomainException("The email address supplied is the same as the email address on the subscription");
        }

        if (await _repositories.Blocked.IsBlockedAsync(options.EmailAddress))
        {
            throw new SubscriptionsCoreDomainException("The requested email address is on a block list");
        }

        if (await _repositories.Subscriptions.ExistsAsync(new SubscriptionKey(options.SubscriptionId).WithNewEmail(options.EmailAddress)))
        {
            throw new SubscriptionsCoreDomainException("Already subscribed to this topic under the updated email address");
        }
    }
}