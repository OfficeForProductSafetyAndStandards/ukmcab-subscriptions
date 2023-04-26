namespace UKMCAB.Subscriptions.Core.Domain.Emails.Uris;

public class SearchUpdatedChangesSummaryUriTemplateOptions
{
    public SearchUpdatedChangesSummaryUriTemplateOptions(string subscriptionIdPlaceholder, string changeDescriptorIdPlaceholder, string relativeUrl)
    {
        SubscriptionIdPlaceholder = subscriptionIdPlaceholder;
        ChangeDescriptorIdPlaceholder = changeDescriptorIdPlaceholder;
        RelativeUrl = relativeUrl;
    }

    public string SubscriptionIdPlaceholder { get; }
    public string RelativeUrl { get; set; }
    public string ChangeDescriptorIdPlaceholder { get; set; }
}
