namespace UKMCAB.Subscriptions.Core.Domain.Emails.Uris;

public class UnsubscribeAllUriTemplateOptions
{
    public UnsubscribeAllUriTemplateOptions(string emailAddressPlaceholder, string relativeUrl)
    {
        EmailAddressPlaceholder = emailAddressPlaceholder;
        RelativeUrl = relativeUrl;
    }

    public string EmailAddressPlaceholder { get; }
    public string RelativeUrl { get; set; }
}
