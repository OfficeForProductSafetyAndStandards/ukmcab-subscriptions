using Notify.Client;
using UKMCAB.Subscriptions.Core.Domain;

namespace UKMCAB.Subscriptions.Core.Integration.OutboundEmail;

public interface IOutboundEmailSender
{
    Task SendAsync(string templateId, EmailAddress emailAddress, Dictionary<string, dynamic> personalisation);
}

public class OutboundEmailSender : IOutboundEmailSender
{
    private readonly NotificationClient _client;

    public OutboundEmailSender(string apiKey)
    {
        _client = new NotificationClient(apiKey);
    }

    public async Task SendAsync(string templateId, EmailAddress emailAddress, Dictionary<string, dynamic> personalisation)
    {
        await _client.SendEmailAsync(emailAddress, templateId, personalisation);
    }
}
