namespace UKMCAB.Subscriptions.Core.Integration.CabUpdates;
public class CabUpdateMessage
{
    public string MessageId { get; set; }
    public Guid CabId { get; set; }
    public string Name { get; set; }
}
