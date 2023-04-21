namespace UKMCAB.Subscriptions.Core.Domain.Emails.Uris;

/// <summary>
/// The frequency of email notifications on a given subscription
/// </summary>
public class UriConverter
{
    private readonly Uri _base;

    public Uri BaseUri => _base;

    public UriConverter(string absoluteUriString)
    {
        var absoluteUri = new Uri(absoluteUriString, UriKind.Absolute);
        var ub = new UriBuilder(absoluteUri)
        {
            Scheme = absoluteUri.Scheme,
            Host = absoluteUri.Host,
            Port = absoluteUri.Port
        };
        _base = ub.Uri;
    }

    public UriConverter(Uri absoluteUri) : this(absoluteUri.ToString()) { }

    public string Make(Uri relativeUri) => Make(relativeUri.ToString());

    public string Make(string relativeUri)
    {
        var uri = new Uri(relativeUri, UriKind.Relative);
        return new Uri(_base, uri).ToString();
    }
}
