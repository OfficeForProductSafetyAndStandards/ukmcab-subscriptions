namespace UKMCAB.Subscriptions.Core.Domain.Emails.Uris;

public class SearchUriTemplateOptions
{
    public SearchUriTemplateOptions(string relativeUrl) => RelativeUrl = relativeUrl;
    public string RelativeUrl { get; set; }
}
