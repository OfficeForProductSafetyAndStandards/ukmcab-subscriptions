using UKMCAB.Subscriptions.Core.Common;

namespace UKMCAB.Subscriptions.Core.Domain.Emails.Uris;

public class UriTemplates
{
    private readonly UriConverter _uriConverter;

    private readonly ConfirmationUriTemplateOptions _confirmSearchSubscription;
    private readonly ConfirmationUriTemplateOptions _confirmCabSubscription;
    private readonly ConfirmationUriTemplateOptions _confirmUpdateEmailAddress;
    private readonly SubscriptionUriTemplateOptions _manageSubscription;
    private readonly SearchUpdatedChangesSummaryUriTemplateOptions _searchChangesSummary;
    private readonly SearchUriTemplateOptions _search;
    private readonly ViewCabUriTemplateOptions _cabDetails;
    private readonly SubscriptionUriTemplateOptions _unsubscribe;
    private readonly UnsubscribeAllUriTemplateOptions _unsubscribeAll;

    public UriTemplates(UriTemplateOptions options)
    {
        _uriConverter = new UriConverter(options.BaseUri ?? throw new UriTemplatesNotConfiguredException($"{nameof(options.BaseUri)} not set"));
        _confirmSearchSubscription = options.ConfirmSearchSubscription ?? throw new UriTemplatesNotConfiguredException($"{nameof(options.ConfirmSearchSubscription)} not set");
        _confirmCabSubscription = options.ConfirmCabSubscription ?? throw new UriTemplatesNotConfiguredException($"{nameof(options.ConfirmCabSubscription)} not set");
        _confirmUpdateEmailAddress = options.ConfirmUpdateEmailAddress ?? throw new UriTemplatesNotConfiguredException($"{nameof(options.ConfirmUpdateEmailAddress)} not set");
        _manageSubscription = options.ManageSubscription ?? throw new UriTemplatesNotConfiguredException($"{nameof(options.ManageSubscription)} not set");
        _search = options.Search ?? throw new UriTemplatesNotConfiguredException($"{nameof(options.Search)} not set");
        _cabDetails = options.CabDetails ?? throw new UriTemplatesNotConfiguredException($"{nameof(options.CabDetails)} not set");
        _unsubscribe = options.Unsubscribe ?? throw new UriTemplatesNotConfiguredException($"{nameof(options.Unsubscribe)} not set");
        _unsubscribeAll = options.UnsubscribeAll ?? throw new UriTemplatesNotConfiguredException($"{nameof(options.UnsubscribeAll)} not set");
        _searchChangesSummary = options.SearchChangesSummary ?? throw new UriTemplatesNotConfiguredException($"{nameof(options.SearchChangesSummary)} not set");
    }

    public string GetConfirmSearchSubscriptionUrl(string token)
    {
        var url = _uriConverter.Make(_confirmSearchSubscription.RelativeUrl).Replace(_confirmSearchSubscription.TokenPlaceholder, token);
        return url;
    }

    public string GetConfirmCabSubscriptionUrl(string token)
    {
        var url = _uriConverter.Make(_confirmCabSubscription.RelativeUrl).Replace(_confirmCabSubscription.TokenPlaceholder, token);
        return url;
    }


    public string GetConfirmUpdateEmailAddressUrl(string token)
    {
        var url = _uriConverter.Make(_confirmUpdateEmailAddress.RelativeUrl).Replace(_confirmUpdateEmailAddress.TokenPlaceholder, token);
        return url;
    }

    public string GetManageMySubscriptionUrl(string subscriptionId)
    {
        var url = _uriConverter.Make(_manageSubscription.RelativeUrl).Replace(_manageSubscription.SubscriptionIdPlaceholder, subscriptionId);
        return url;
    }

    public string GetSearchChangesSummaryUrl(string subscriptionId, string changeDescriptorId)
    {
        var url = _uriConverter.Make(_searchChangesSummary.RelativeUrl)
            .Replace(_searchChangesSummary.SubscriptionIdPlaceholder, subscriptionId)
            .Replace(_searchChangesSummary.ChangeDescriptorIdPlaceholder, subscriptionId);
        return url;
    }

    public string GetSearchUrl(string? query)
    {
        var url = _uriConverter.Make(_search.RelativeUrl);
        var retVal = string.Concat(url.TrimEnd('?'), query?.EnsureStartsWith("?") ?? "");
        return retVal;
    }

    public string GetCabDetailsUrl(Guid cabId)
    {
        var url = _uriConverter.Make(_cabDetails.RelativeUrl).Replace(_cabDetails.CabIdPlaceholder, cabId.ToString());
        return url;
    }

    public string GetUnsubscribeUrl(string subscriptionId)
    {
        var url = _uriConverter.Make(_unsubscribe.RelativeUrl).Replace(_unsubscribe.SubscriptionIdPlaceholder, subscriptionId);
        return url;
    }

    public string GetUnsubscribeAllUrl(string emailAddress)
    {
        var encodedEmailAddress = System.Net.WebUtility.UrlEncode(emailAddress);
        var url = _uriConverter.Make(_unsubscribeAll.RelativeUrl).Replace(_unsubscribeAll.EmailAddressPlaceholder, encodedEmailAddress);
        return url;
    }
}
