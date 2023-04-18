using Azure;
using Azure.Data.Tables;
using UKMCAB.Subscriptions.Core.Domain;

namespace UKMCAB.Subscriptions.Core.Data.Models;

public class SubscriptionEntity : ITableEntity
{
    public string? PartitionKey { get; set; }
    public string? RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public string EmailAddress { get; set; } = null!;
    public Frequency Frequency { get; set; }
    public Guid? CabId { get; set; }
    public string? SearchQueryString { get; set; }

    public SubscriptionEntity()
    {
        
    }

    public SubscriptionEntity(Keys keys)
    {
        PartitionKey = keys.PartitionKey;
        RowKey = keys.RowKey;
    }

    public void SetKeys(Keys keys)
    {
        PartitionKey = keys.PartitionKey;
        RowKey = keys.RowKey;
    }

    public SubscriptionKey GetKeys() => new SubscriptionKey(new Keys(PartitionKey, RowKey));
}
