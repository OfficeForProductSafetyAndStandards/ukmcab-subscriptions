
/// <summary>
/// The core options that need to be configured for this package to function
/// </summary>
public class SubscriptionServicesCoreOptions
{
    public string DataConnectionString { get; set; }
    public string BaseUrl { get; set; }
    public string EmailTemplateConfirmSubscription { get; set; }
    public string EmailTemplateUpdateEmailAddress { get; set; }
    public string SearchApiUrl { get; set; }
    public string GovUkNotifyApiKey { get; set; }

    /// <summary>
    /// The AES encryption key for secure token processing.
    /// </summary>
    /// <remarks>
    /// If you don't have one, use `UKMCAB.Subscriptions.Core.Common.Security.Tokens.KeyIV.GenerateKey()` and put somewhere safe
    /// </remarks>
    public string EncryptionKey { get; set; }

    /// <summary>
    ///     The query string keys that contain paging data, such as page size and page index, or skip and take keys.
    /// </summary>
    /// <remarks>
    ///     These keys will be removed from search query string 
    /// </remarks>
    public string[] SearchQueryStringPagingKeys { get; set; } = Array.Empty<string>();
}
