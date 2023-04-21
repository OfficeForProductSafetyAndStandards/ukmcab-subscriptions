using UKMCAB.Subscriptions.Core.Domain.Emails.Uris;

namespace UKMCAB.Subscriptions.Core.Domain.Emails;

public interface IEmailTemplatesService
{
    void AssertIsUriTemplateOptionsConfigured();
    void Configure(UriTemplateOptions uriTemplateOptions);
    EmailDefinition GetCabUpdatedEmailDefinition(EmailAddress recipient, string subscriptionId, Guid cabId);
    EmailDefinition GetConfirmCabSubscriptionEmailDefinition(EmailAddress recipient, string token);
    EmailDefinition GetConfirmSearchSubscriptionEmailDefinition(EmailAddress recipient, string token);
    EmailDefinition GetConfirmUpdateEmailAddressEmailDefinition(EmailAddress recipient, string token);
    EmailDefinition GetSearchUpdatedEmailDefinition(EmailAddress recipient, string subscriptionId, string? query);
    bool IsConfigured();
}

public class EmailTemplatesService : IEmailTemplatesService
{
    private readonly EmailTemplateOptions _emailTemplateOptions;
    private UriTemplates? _uriTemplates = null;

    public EmailTemplatesService(EmailTemplateOptions emailTemplates, UriTemplateOptions? uriTemplateOptions = null)
    {
        emailTemplates.Validate();
        _emailTemplateOptions = emailTemplates;
        if (uriTemplateOptions != null)
        {
            Configure(uriTemplateOptions);
        }
    }

    public void Configure(UriTemplateOptions uriTemplateOptions)
    {
        _uriTemplates = new UriTemplates(uriTemplateOptions);
    }

    public void AssertIsUriTemplateOptionsConfigured() => _ = _uriTemplates ?? throw new UriTemplatesNotConfiguredException();

    public bool IsConfigured() => _uriTemplates != null;

    public EmailDefinition GetConfirmSearchSubscriptionEmailDefinition(EmailAddress recipient, string token)
    {
        AssertIsUriTemplateOptionsConfigured();

        var confirmUrl = _uriTemplates!.GetConfirmSearchSubscriptionUrl(token);
        var def = new EmailDefinition(_emailTemplateOptions.ConfirmSearchSubscriptionTemplateId, recipient);
        def.Replacements.Add(EmailPlaceholders.ConfirmLink, confirmUrl);
        def.AddMetadataToken(token);
        return def;
    }

    public EmailDefinition GetConfirmCabSubscriptionEmailDefinition(EmailAddress recipient, string token)
    {
        AssertIsUriTemplateOptionsConfigured();

        var confirmUrl = _uriTemplates!.GetConfirmCabSubscriptionUrl(token);
        var def = new EmailDefinition(_emailTemplateOptions.ConfirmCabSubscriptionTemplateId, recipient);
        def.Replacements.Add(EmailPlaceholders.ConfirmLink, confirmUrl);
        def.AddMetadataToken(token);
        return def;
    }

    public EmailDefinition GetConfirmUpdateEmailAddressEmailDefinition(EmailAddress recipient, string token)
    {
        AssertIsUriTemplateOptionsConfigured();

        var confirmUrl = _uriTemplates!.GetConfirmUpdateEmailAddressUrl(token);
        var def = new EmailDefinition(_emailTemplateOptions.ConfirmUpdateEmailAddressTemplateId, recipient);
        def.Replacements.Add(EmailPlaceholders.ConfirmLink, confirmUrl);
        def.AddMetadataToken(token);
        return def;
    }

    public EmailDefinition GetSearchUpdatedEmailDefinition(EmailAddress recipient, string subscriptionId, string? query)
    {
        AssertIsUriTemplateOptionsConfigured();

        var def = new EmailDefinition(_emailTemplateOptions.SearchUpdatedTemplateId, recipient);

        var searchUrl = _uriTemplates!.GetSearchUrl(query);
        def.Replacements.Add(EmailPlaceholders.ViewSearchLink, searchUrl);

        var unsubscribeUrl = _uriTemplates.GetUnsubscribeUrl(subscriptionId);
        def.Replacements.Add(EmailPlaceholders.UnsubscribeLink, unsubscribeUrl);

        var unsubscribeAllUrl = _uriTemplates.GetUnsubscribeAllUrl(recipient);
        def.Replacements.Add(EmailPlaceholders.UnsubscribeAllLink, unsubscribeAllUrl);

        return def;
    }

    public EmailDefinition GetCabUpdatedEmailDefinition(EmailAddress recipient, string subscriptionId, Guid cabId)
    {
        AssertIsUriTemplateOptionsConfigured();

        var def = new EmailDefinition(_emailTemplateOptions.CabUpdatedTemplateId, recipient);

        var cabDetailsUrl = _uriTemplates!.GetCabDetailsUrl(cabId);
        def.Replacements.Add(EmailPlaceholders.ViewCabLink, cabDetailsUrl);

        var unsubscribeUrl = _uriTemplates.GetUnsubscribeUrl(subscriptionId);
        def.Replacements.Add(EmailPlaceholders.UnsubscribeLink, unsubscribeUrl);

        var unsubscribeAllUrl = _uriTemplates.GetUnsubscribeAllUrl(recipient);
        def.Replacements.Add(EmailPlaceholders.UnsubscribeAllLink, unsubscribeAllUrl);

        return def;
    }

}
