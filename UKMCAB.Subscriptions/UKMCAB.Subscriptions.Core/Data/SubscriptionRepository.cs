using UKMCAB.Subscriptions.Core.Data.Models;
using UKMCAB.Subscriptions.Core.Domain;

namespace UKMCAB.Subscriptions.Core.Data;

public interface ISubscriptionRepository : IRepository
{
    Task AddAsync(SubscriptionEntity entity);
    IAsyncEnumerable<SubscriptionEntity> GetAllAsync(string partitionKey);
    Task<SubscriptionEntity?> GetAsync(SubscriptionKey key);
}

/// <summary>
/// Store subscription data in Azure Table storage
/// </summary>
public class SubscriptionRepository : Repository, ISubscriptionRepository
{
    public SubscriptionRepository(AzureDataConnectionString dataConnectionString) : base(dataConnectionString, "subscriptions") { }

    public async Task AddAsync(SubscriptionEntity entity)
    {
        await UpsertAsync(entity);
    }

    public async Task<SubscriptionEntity?> GetAsync(SubscriptionKey key)
    {
        return await GetAsync<SubscriptionEntity>(key);
    }

    public IAsyncEnumerable<SubscriptionEntity> GetAllAsync(string partitionKey)
    {
        return GetAllAsync<SubscriptionEntity>(partitionKey);
    }
}

