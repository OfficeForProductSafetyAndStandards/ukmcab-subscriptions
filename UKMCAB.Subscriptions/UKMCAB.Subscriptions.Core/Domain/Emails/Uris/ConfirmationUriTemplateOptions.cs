namespace UKMCAB.Subscriptions.Core.Domain.Emails.Uris;

public class ConfirmationUriTemplateOptions
{
    public ConfirmationUriTemplateOptions(string tokenPlaceholder, string relativeUrl)
    {
        TokenPlaceholder = tokenPlaceholder;
        RelativeUrl = relativeUrl;
    }

    public string TokenPlaceholder { get; set; }
    public string RelativeUrl { get; set; }
}
