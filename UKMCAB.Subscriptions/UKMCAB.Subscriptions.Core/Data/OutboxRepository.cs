using UKMCAB.Subscriptions.Core.Domain;

namespace UKMCAB.Subscriptions.Core.Data;

/// <summary>
/// Responsible for persisting outbound notifications until they are sent
/// </summary>
public interface IOutboxRepository
{
    /// <summary>
    /// Saves a notification to the outbox
    /// </summary>
    /// <param name="notification"></param>
    /// <returns></returns>
    Task SaveAsync(Notification notification);

    /// <summary>
    /// Gets a list of notifications
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<Notification>> ListAsync();

    /// <summary>
    /// Deletes a notification
    /// </summary>
    /// <param name="notification"></param>
    /// <returns></returns>
    Task DeleteAsync(Notification notification);
}

/// <summary>
/// Store outbound notification data in Azure Table storage
/// </summary>
public class OutboxRepository : IOutboxRepository
{
    private readonly SubscriptionServicesCoreOptions _options;

    public OutboxRepository(SubscriptionServicesCoreOptions options)
    {
        _options = options;
    }

    public Task DeleteAsync(Notification notification)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Notification>> ListAsync()
    {
        throw new NotImplementedException();
    }

    public Task SaveAsync(Notification notification)
    {
        throw new NotImplementedException();
    }
}


