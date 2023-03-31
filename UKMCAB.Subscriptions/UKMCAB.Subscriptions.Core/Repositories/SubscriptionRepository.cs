namespace UKMCAB.Subscriptions.Core.Repositories;

/// <summary>
/// Responsible for persisting subscription data
/// </summary>
public interface ISubscriptionRepository
{
    
}

/// <summary>
/// Store subscription data in Azure Table storage
/// </summary>
public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly SubscriptionServicesCoreOptions _options;

    public SubscriptionRepository(SubscriptionServicesCoreOptions options)
    {
        _options = options;
    }

}

