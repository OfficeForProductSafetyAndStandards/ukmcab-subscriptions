namespace UKMCAB.Subscriptions.Core.Integration.CabUpdates;

/// <summary>
/// This implementation will retrieve messages by polling an Azure Storage Queue.
/// </summary>
public class CabUpdatesReceiver : ICabUpdatesReceiver
{
    private readonly SubscriptionServicesCoreOptions _options;

    public CabUpdatesReceiver(SubscriptionServicesCoreOptions options)
    {
        _options = options;
    }


    /// <summary>
    /// Gets a batch of messages from the Queue
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<IEnumerable<CabUpdateMessage>> GetCabUpdateMessagesAsync()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Marks an update as 'processed' by deleting the message from the Azure Queue
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task MarkAsProcessedAsync(CabUpdateMessage message)
    {
        throw new NotImplementedException();
    }
}