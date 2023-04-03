namespace UKMCAB.Subscriptions.Core.Integration.CabUpdates;

/// <summary>
/// Responsible for receiving messages about CABs being updated
/// </summary>
public interface ICabUpdatesReceiver
{
    Task<CabUpdateMessage?> GetCabUpdateMessageAsync();
    Task MarkAsProcessedAsync(CabUpdateMessage message);
}
