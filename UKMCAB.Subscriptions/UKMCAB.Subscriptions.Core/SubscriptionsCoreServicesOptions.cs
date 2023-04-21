using UKMCAB.Subscriptions.Core.Domain.Emails;
using UKMCAB.Subscriptions.Core.Domain.Emails.Uris;
using UKMCAB.Subscriptions.Core.Integration.CabService;
using UKMCAB.Subscriptions.Core.Integration.OutboundEmail;

namespace UKMCAB.Subscriptions.Core;

/// <summary>
/// The core options that need to be configured for this package to function
/// </summary>
public class SubscriptionsCoreServicesOptions
{
    internal const string BlobContainerPrefix = "subscriptionscore";
    internal const string TableNamePrefix = "subscriptionscore";

    public string? DataConnectionString { get; set; }
    public EmailTemplateOptions EmailTemplates { get; set; } = new EmailTemplateOptions();
    public string? GovUkNotifyApiKey { get; set; }
    public CabApiOptions? CabApiOptions { get; set; }
    public OutboundEmailSenderMode OutboundEmailSenderMode { get; set; } = OutboundEmailSenderMode.Send;

    /// <summary>
    /// Uri templates can be configured here or use services.GetRequiredService<IEmailTemplatesService>().Configure(UriTemplateOptions uriTemplateOptions)
    /// <see cref="IEmailTemplatesService.Configure(UriTemplateOptions)"/>
    /// </summary>
    public UriTemplateOptions? UriTemplateOptions { get; set; }

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
