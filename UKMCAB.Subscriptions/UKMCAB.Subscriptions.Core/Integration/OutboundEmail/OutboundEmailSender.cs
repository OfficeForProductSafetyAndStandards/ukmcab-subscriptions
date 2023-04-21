using Notify.Client;
using System.Collections.Concurrent;
using UKMCAB.Subscriptions.Core.Common;
using UKMCAB.Subscriptions.Core.Domain.Emails;

namespace UKMCAB.Subscriptions.Core.Integration.OutboundEmail;

public interface IOutboundEmailSender
{
    ConcurrentBag<EmailDefinition> Requests { get; set; }
    OutboundEmailSenderMode Mode { get; set; }
    Task SendAsync(EmailDefinition emailDefinition);
}

public enum OutboundEmailSenderMode
{
    /// <summary>
    /// Actually sends out email
    /// </summary>
    Send,

    /// <summary>
    /// Pretends to send email and logs the request
    /// </summary>
    Pretend
}

public class OutboundEmailSender : IOutboundEmailSender
{
    private readonly NotificationClient _client;

    public const string PlaceholderLink = "link";
    public const string MetaNameToken = "token";
    
    
    public OutboundEmailSenderMode Mode { get; set; } = OutboundEmailSenderMode.Send;
    
    public ConcurrentBag<EmailDefinition> Requests { get; set; } = new();

    public OutboundEmailSender(string apiKey, OutboundEmailSenderMode mode)
    {
        _client = new NotificationClient(apiKey);
        Mode = mode;
    }

    public async Task SendAsync(EmailDefinition emailDefinition)
    {
        if (Mode == OutboundEmailSenderMode.Send)
        {
            var replacements = emailDefinition.Replacements.ToDictionary(x => x.Key, x => x.Value as dynamic);
            await _client.SendEmailAsync(emailDefinition.Recipient, emailDefinition.TemplateId, replacements);
        }

        if (Requests.Count > 20)
        {
            Requests.Clear();
        }

        Requests.Add(emailDefinition);
    }
}

public static class OutboundEmailSenderExtensions
{
    public static string GetLastToken(this IOutboundEmailSender sender, string metaName = OutboundEmailSender.MetaNameToken)
    {
        return sender.GetLastMetaItem(metaName) ?? throw new Exception("Token was not found");
    }

    public static string GetLastMetaItem(this IOutboundEmailSender sender, string metaName) 
        => ((sender??throw new Exception("sender is null")).Requests.OrderBy(x=>x.Timestamp).LastOrDefault()?.Metadata.Get(metaName)) ?? throw new Exception("Meta item was not found");
}