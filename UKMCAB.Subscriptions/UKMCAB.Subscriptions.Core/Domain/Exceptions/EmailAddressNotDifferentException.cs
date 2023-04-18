namespace UKMCAB.Subscriptions.Core.Domain.Exceptions;

[Serializable]
public class EmailAddressNotDifferentException : SubscriptionsCoreDomainException
{
    public EmailAddressNotDifferentException() { }
    public EmailAddressNotDifferentException(string message) : base(message) { }
    public EmailAddressNotDifferentException(string message, Exception inner) : base(message, inner) { }
    protected EmailAddressNotDifferentException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
