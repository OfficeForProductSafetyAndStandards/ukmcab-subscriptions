namespace UKMCAB.Subscriptions.Core.Domain.Exceptions;

[Serializable]
public class SubscriptionNotFoundException : SubscriptionsCoreDomainException
{
    public SubscriptionNotFoundException() { }
    public SubscriptionNotFoundException(string message) : base(message) { }
    public SubscriptionNotFoundException(string message, Exception inner) : base(message, inner) { }
    protected SubscriptionNotFoundException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}