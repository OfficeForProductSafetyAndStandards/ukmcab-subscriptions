namespace UKMCAB.Subscriptions.Core.Domain.Exceptions;

[Serializable]
public class EmailAddressOnBlockedListException : SubscriptionsCoreDomainException
{
    public EmailAddressOnBlockedListException() { }
    public EmailAddressOnBlockedListException(string message) : base(message) { }
    public EmailAddressOnBlockedListException(string message, Exception inner) : base(message, inner) { }
    protected EmailAddressOnBlockedListException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
