using UKMCAB.Subscriptions.Core.Domain;

namespace UKMCAB.Subscriptions.Core.Integration.OutboundEmail;

public interface IOutboundEmailSender
{
    Task SendAsync(Notification notification);
}

public class OutboundEmailSender : IOutboundEmailSender
{
    public async Task SendAsync(Notification notification)
    {
        //todo
        throw new NotImplementedException();
    }
}
