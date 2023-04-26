namespace UKMCAB.Subscriptions.Core.Domain.Emails.Uris;

public class SubscriptionUriTemplateOptions
{
    public SubscriptionUriTemplateOptions(string subscriptionIdPlaceholder, string relativeUrl)
    {
        SubscriptionIdPlaceholder = subscriptionIdPlaceholder;
        RelativeUrl = relativeUrl;
    }

    public string SubscriptionIdPlaceholder { get; }
    public string RelativeUrl { get; set; }
}
