using UKMCAB.Subscriptions.Core.Domain;
using UKMCAB.Subscriptions.Core.Integration.CabService;

namespace UKMCAB.Subscriptions.Core;

/// <summary>
/// The core options that need to be configured for this package to function
/// </summary>
public class SubscriptionsCoreServicesOptions
{
    internal const string BlobContainerPrefix = "subscriptionscore";
    internal const string TableNamePrefix = "subscriptionscore";

    public string? DataConnectionString { get; set; }
    public EmailTemplates EmailTemplates { get; set; } = new EmailTemplates();
    public string? GovUkNotifyApiKey { get; set; }
    public CabApiOptions? CabApiOptions { get; set; }

    /// <summary>
    /// The AES encryption key for secure token processing.
    /// </summary>
    /// <remarks>
    /// If you don't have one, use `UKMCAB.Subscriptions.Core.Common.Security.Tokens.KeyIV.GenerateKey()` and put somewhere safe
    /// </remarks>
    public string? EncryptionKey { get; set; }

    /// <summary>
    ///     The query string keys that contain paging/sorting data, such as page size and page index, sort, or skip and take keys.
    ///     Defaults to: "pagenumber", "pagesize", "sort" 
    /// </summary>
    /// <remarks>
    ///     These keys will be removed from search query string when a user subscribes to a search.
    /// </remarks>
    public string[] SearchQueryStringRemoveKeys { get; set; } = new[] { "pagenumber", "pagesize", "sort" };
}
