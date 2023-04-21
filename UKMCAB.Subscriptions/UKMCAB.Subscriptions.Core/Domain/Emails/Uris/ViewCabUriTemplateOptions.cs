namespace UKMCAB.Subscriptions.Core.Domain.Emails.Uris;

public class ViewCabUriTemplateOptions
{
    public ViewCabUriTemplateOptions(string cabIdPlaceholder, string relativeUrl)
    {
        CabIdPlaceholder = cabIdPlaceholder;
        RelativeUrl = relativeUrl;
    }

    public string CabIdPlaceholder { get; }
    public string RelativeUrl { get; set; }
}
