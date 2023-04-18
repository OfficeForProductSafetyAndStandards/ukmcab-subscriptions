namespace UKMCAB.Subscriptions.Core.Domain.Exceptions;

[Serializable]
public class EmailAddressAlreadySubscribedToTopicException : SubscriptionsCoreDomainException
{
    public EmailAddressAlreadySubscribedToTopicException() { }
    public EmailAddressAlreadySubscribedToTopicException(string message) : base(message) { }
    public EmailAddressAlreadySubscribedToTopicException(string message, Exception inner) : base(message, inner) { }
    protected EmailAddressAlreadySubscribedToTopicException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}