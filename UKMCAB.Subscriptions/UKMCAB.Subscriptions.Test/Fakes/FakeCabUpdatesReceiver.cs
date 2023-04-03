using UKMCAB.Subscriptions.Core.Integration.CabUpdates;

namespace UKMCAB.Subscriptions.Test.Fakes;

public class FakeCabUpdatesReceiver : ICabUpdatesReceiver
{
    private CabUpdateMessage? _cabUpdateMessage = null;

    public void Push(CabUpdateMessage cabUpdateMessage)
    {
        _cabUpdateMessage = cabUpdateMessage;
    }

    public Task<CabUpdateMessage?> GetCabUpdateMessageAsync()
    {
        var rv = _cabUpdateMessage;
        _cabUpdateMessage = null;
        return Task.FromResult(rv);
    }

    public Task MarkAsProcessedAsync(CabUpdateMessage message) => Task.CompletedTask;

}
