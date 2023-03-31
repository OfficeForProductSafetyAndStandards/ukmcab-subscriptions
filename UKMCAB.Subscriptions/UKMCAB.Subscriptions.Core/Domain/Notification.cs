namespace UKMCAB.Subscriptions.Core.Domain;

public class Notification
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public string EmailAddress { get; set; }
    public string Body { get; set; }
}
