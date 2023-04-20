using Microsoft.AspNetCore.WebUtilities;
using UKMCAB.Subscriptions.Core;
using UKMCAB.Subscriptions.Core.Domain;
using UKMCAB.Subscriptions.Core.Common;
using UKMCAB.Subscriptions.Core.Integration.OutboundEmail;

namespace UKMCAB.Subscriptions.Test.Fakes;
public class FakeDateTimeProvider : IDateTimeProvider
{
    private DateTime _utcNow;

    public DateTime UtcNow { get => _utcNow; set => _utcNow = value.AsUtc(); }
}

public class FakeOutboundEmailSender : IOutboundEmailSender
{
    public record Request(string TemplateId, EmailAddress EmailAddress, Dictionary<string, dynamic> Personalisation);

    public List<Request> Requests { get; set; } = new();

    public string GetLastPayload(string key = "link")
    {
        var uri = new Uri(Requests.Last().Personalisation[key], UriKind.Absolute);
        var qs = QueryHelpers.ParseQuery(uri.Query);
        return qs["payload"].ToString();
    }

    public Task SendAsync(string templateId, EmailAddress emailAddress, Dictionary<string, dynamic> personalisation)
    {
        Requests.Add(new Request(templateId, emailAddress, personalisation));
        return Task.CompletedTask;
    }
}