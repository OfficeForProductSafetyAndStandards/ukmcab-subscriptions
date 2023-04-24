using UKMCAB.Subscriptions.Core.Data.Models;

namespace UKMCAB.Subscriptions.Core.Domain;

public class SubscriptionModel
{
    internal SubscriptionModel(SubscriptionEntity entity)
    {
        Id = entity.GetKeys().ToString();
        SubscriptionType = entity.SubscriptionType;
        EmailAddress = entity.EmailAddress;
        Frequency = entity.Frequency;
        
        SearchQueryString = entity.SearchQueryString;
        CabId = entity.CabId;
        CabName = entity.CabName;
        Timestamp = entity.Timestamp;
    }

    public string Id { get; }
    public SubscriptionType SubscriptionType { get; }
    public string? SearchQueryString { get; }
    public Frequency Frequency { get; }
    public string EmailAddress { get; }
    public Guid? CabId { get; }
    public string? CabName { get; }
    public DateTimeOffset? Timestamp { get; }
}