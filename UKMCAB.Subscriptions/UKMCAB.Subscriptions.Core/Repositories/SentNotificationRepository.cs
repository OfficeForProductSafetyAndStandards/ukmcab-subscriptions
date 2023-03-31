using UKMCAB.Subscriptions.Core.Domain;

namespace UKMCAB.Subscriptions.Core.Repositories;

/// <summary>
/// Responsible for persisting sent notification
/// </summary>
public interface ISentNotificationRepository
{
    Task SaveAsync(Notification notification);
}


/// <summary>
/// Store subscription data in Azure Table storage
/// </summary>
public class SentNotificationRepository : ISentNotificationRepository
{
    private readonly SubscriptionServicesCoreOptions _options;

    public SentNotificationRepository(SubscriptionServicesCoreOptions options)
    {
        _options = options;
    }

    public Task SaveAsync(Notification notification)
    {
        throw new NotImplementedException();
    }
}