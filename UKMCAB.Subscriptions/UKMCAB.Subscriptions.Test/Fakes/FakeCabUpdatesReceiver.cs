using UKMCAB.Subscriptions.Core.Integration.CabUpdates;

namespace UKMCAB.Subscriptions.Test.Fakes;

public class FakeCabUpdatesReceiver : ICabUpdatesReceiver
{
    public Task<IEnumerable<CabUpdateMessage>> GetCabUpdateMessagesAsync()
    {
        throw new NotImplementedException();
    }

    public Task MarkAsProcessedAsync(CabUpdateMessage message)
    {
        throw new NotImplementedException();
    }
}
