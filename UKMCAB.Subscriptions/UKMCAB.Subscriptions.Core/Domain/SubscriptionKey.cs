using UKMCAB.Subscriptions.Core.Common;
using UKMCAB.Subscriptions.Core.Data;

namespace UKMCAB.Subscriptions.Core.Domain;

public class SubscriptionKey : Keys
{
    public SubscriptionKey(string id) : base(id) { }

    public SubscriptionKey(EmailAddress emailAddress, string searchQueryString) : base(emailAddress.ToString().Md5(), searchQueryString.Md5()) { }
    
    public SubscriptionKey(EmailAddress emailAddress, Guid cabId) : base(emailAddress.ToString().Md5(), cabId.ToString()) { }

    public SubscriptionKey(Keys keys) : base(keys.PartitionKey, keys.RowKey) { }

    public EmailAddress EmailAddress { set => PartitionKey = CreatePartitionKey(value); }

    public static string CreatePartitionKey(EmailAddress emailAddress) => emailAddress.ToString().Md5()!;

    public SubscriptionKey WithNewEmail(EmailAddress newEmailAddress)
    {
        EmailAddress = newEmailAddress;
        return this;
    }
}