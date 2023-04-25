using Azure;
using Azure.Data.Tables;
using System.Runtime.Serialization;
using UKMCAB.Subscriptions.Core.Domain;
using static System.Formats.Asn1.AsnWriter;

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
    public string? CabName { get; set; }
    public string? SearchQueryString { get; set; }
    public DateTime? DueBaseDate { get; set; }
    public string? LastThumbprint { get; set; }
    public string? BlobName { get; set; }

    public DateTime? GetNextDueDate(IDateTimeProvider dateTimeProvider) => Frequency switch
    {
        Frequency.Realtime => dateTimeProvider.UtcNow.AddMinutes(-1),
        Frequency.Daily => DueBaseDate?.AddDays(1),
        Frequency.Weekly => DueBaseDate?.AddDays(7),
        _ => throw new NotImplementedException(),
    };

    public bool IsDue(IDateTimeProvider dateTimeProvider) => GetNextDueDate(dateTimeProvider) < dateTimeProvider.UtcNow;

    public bool IsInitialised() => LastThumbprint is not null;

    [IgnoreDataMember]
    public SubscriptionType SubscriptionType => CabId.HasValue ? SubscriptionType.Cab : SubscriptionType.Search;

    public SubscriptionEntity() { }

    public SubscriptionEntity(Keys keys)
    {
        PartitionKey = keys.PartitionKey;
        RowKey = keys.RowKey;
        BlobName = $"{Guid.NewGuid()}.json";
    }

    public void SetKeys(Keys keys)
    {
        PartitionKey = keys.PartitionKey;
        RowKey = keys.RowKey;
    }

    public SubscriptionKey GetKeys() => new SubscriptionKey(new Keys(PartitionKey, RowKey));
}
