namespace UKMCAB.Subscriptions.Core.Domain;

/// <summary>
/// Represents a business rule/domain exception for UKMCAB.Subscriptions.Core
/// </summary>
[Serializable]
public class SubscriptionsCoreDomainException : Exception
{
    public SubscriptionsCoreDomainException() { }
    public SubscriptionsCoreDomainException(string message) : base(message) { }
    public SubscriptionsCoreDomainException(string message, Exception inner) : base(message, inner) { }
    protected SubscriptionsCoreDomainException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}