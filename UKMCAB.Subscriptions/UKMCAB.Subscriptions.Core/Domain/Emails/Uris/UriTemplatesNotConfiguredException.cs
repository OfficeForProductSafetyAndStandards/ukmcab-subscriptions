using UKMCAB.Subscriptions.Core.Domain.Exceptions;

namespace UKMCAB.Subscriptions.Core.Domain.Emails.Uris;

[Serializable]
public class UriTemplatesNotConfiguredException : SubscriptionsCoreDomainException
{
    public UriTemplatesNotConfiguredException() { }
    public UriTemplatesNotConfiguredException(string message) : base(message) { }
    public UriTemplatesNotConfiguredException(string message, Exception inner) : base(message, inner) { }
    protected UriTemplatesNotConfiguredException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}