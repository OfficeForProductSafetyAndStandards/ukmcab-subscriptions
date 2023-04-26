namespace UKMCAB.Subscriptions.Core.Domain.Emails.Uris;

public class UriTemplateOptions
{
    public Uri? BaseUri { get; set; }
    public ConfirmationUriTemplateOptions? ConfirmSearchSubscription { get; set; }
    public ConfirmationUriTemplateOptions? ConfirmCabSubscription { get; set; }
    public ConfirmationUriTemplateOptions? ConfirmUpdateEmailAddress { get; set; }
    public SubscriptionUriTemplateOptions? ManageSubscription { get; set; }
    public SearchUriTemplateOptions? Search { get; set; }
    public SearchUpdatedChangesSummaryUriTemplateOptions? SearchChangesSummary { get; set; }
    public ViewCabUriTemplateOptions? CabDetails { get; set; }
    public SubscriptionUriTemplateOptions? Unsubscribe { get; set; }
    public UnsubscribeAllUriTemplateOptions? UnsubscribeAll { get; set; }
}
