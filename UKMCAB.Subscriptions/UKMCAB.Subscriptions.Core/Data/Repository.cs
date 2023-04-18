using Azure.Data.Tables;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UKMCAB.Subscriptions.Core.Domain;

namespace UKMCAB.Subscriptions.Core.Data;

public interface IRepository
{
    Task DeleteAllAsync();
    Task DeleteAsync(Keys keys);
    Task<bool> ExistsAsync(Keys keys);
}

public class Repository : IRepository
{
    private readonly TableClient _tableClient;
    private readonly ConcurrentDictionary<string, bool> _initMap = new();

    protected Repository(AzureDataConnectionString dataConnectionString, string tableName)
    {
        _tableClient = new TableClient(dataConnectionString, tableName);
    }

    protected async Task AddAsync(ITableEntity entity)
    {
        await EnsureAsync().ConfigureAwait(false);
        await _tableClient.AddEntityAsync(entity);
    }

    protected async Task UpsertAsync(ITableEntity entity)
    {
        await EnsureAsync().ConfigureAwait(false);
        await _tableClient.UpsertEntityAsync(entity);
    }

    protected async Task<T?> GetAsync<T>(Keys keys) where T : class, ITableEntity
    {
        await EnsureAsync().ConfigureAwait(false);
        var response = await _tableClient.GetEntityIfExistsAsync<T>(keys.PartitionKey, keys.RowKey);
        return response.HasValue ? response.Value : default;
    }

    public async Task<bool> ExistsAsync(Keys keys)
    {
        await EnsureAsync().ConfigureAwait(false);
        var response = await _tableClient.GetEntityIfExistsAsync<TableEntity>(keys.PartitionKey, keys.RowKey, new[] { nameof(ITableEntity.PartitionKey) });
        return response.HasValue;
    }

    public IAsyncEnumerable<T> GetAllAsync<T>(string partitionKey) where T : class, ITableEntity
    {
        return _tableClient.QueryAsync<T>(x => x.PartitionKey == partitionKey, 20);
    }

    protected async Task UpdateAsync(ITableEntity entity)
    {
        await EnsureAsync().ConfigureAwait(false);
        await _tableClient.UpdateEntityAsync(entity, Azure.ETag.All);
    }

    public async Task DeleteAsync(Keys keys)
    {
        await EnsureAsync().ConfigureAwait(false);
        await _tableClient.DeleteEntityAsync(keys.PartitionKey, keys.RowKey);
    }

    public async Task DeleteAllAsync()
    {
        await EnsureAsync().ConfigureAwait(false);

        var items = await _tableClient.QueryAsync<TableEntity>(maxPerPage: 10).ToListAsync();
        foreach (var item in items)
        {
            await _tableClient.DeleteEntityAsync(item.PartitionKey, item.RowKey);
        }
    }
    private async Task EnsureAsync()
    {
        if (!_initMap.GetValueOrDefault(_tableClient.Name))
        {
            _initMap.TryAdd(_tableClient.Name, true);
            await _tableClient.CreateIfNotExistsAsync();
        }
    }
}
