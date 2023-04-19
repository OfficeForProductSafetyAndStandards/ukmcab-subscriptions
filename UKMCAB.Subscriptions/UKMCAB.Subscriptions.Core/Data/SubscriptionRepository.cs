using Azure;
using UKMCAB.Subscriptions.Core.Data.Models;
using UKMCAB.Subscriptions.Core.Domain;

namespace UKMCAB.Subscriptions.Core.Data;

public interface ISubscriptionRepository : IRepository
{
    Task UpsertAsync(SubscriptionEntity entity);
    Task<SubscriptionEntity?> GetAsync(SubscriptionKey key);
    IAsyncEnumerable<Page<SubscriptionEntity>> GetAllAsync(string? partitionKey = null, string? skip = null, int? take = null);
}

/// <summary>
/// Store subscription data in Azure Table storage
/// </summary>
public class SubscriptionRepository : Repository, ISubscriptionRepository
{
    public SubscriptionRepository(AzureDataConnectionString dataConnectionString) : base(dataConnectionString, "subscriptions") { }

    public async Task UpsertAsync(SubscriptionEntity entity)
    {
        await base.UpsertAsync(entity);
    }

    public async Task<SubscriptionEntity?> GetAsync(SubscriptionKey key)
    {
        return await GetAsync<SubscriptionEntity>(key);
    }

    public IAsyncEnumerable<Page<SubscriptionEntity>> GetAllAsync(string? partitionKey = null, string? skip = null, int? take = null)
    {
        return GetAllAsync<SubscriptionEntity>(partitionKey, skip, take);
    }
}

