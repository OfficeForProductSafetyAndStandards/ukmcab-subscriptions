namespace UKMCAB.Subscriptions.Core.Abstract;

public interface ISubscriptionService
{
    /// <summary>
    /// Subscribes the email address to search results
    /// </summary>
    /// <param name="emailAddress"></param>
    /// <param name="searchQuery"></param>
    /// <returns>An ID that uniquely identifies the search (e.g., an MD5 hash of the emailAddress and searchQuery)</returns>
    Task<string> SubscribeAsync(string emailAddress, string searchQuery, Frequency frequency);

    /// <summary>
    /// Subscribes the email address to the CAB record
    /// </summary>
    /// <param name="emailAddress"></param>
    /// <param name="cabId"></param>
    /// <returns>An ID that uniquely identifies the search (e.g., an MD5 hash of the emailAddress and cabId)</returns>
    Task<string> SubscribeAsync(string emailAddress, Guid cabId, Frequency frequency);

    Task UpdateFrequencyAsync(string subscriptionId, Frequency frequency);

    Task IsSubscribedAsync(string emailAddress, string searchQuery);

    Task IsSubscribedAsync(string emailAddress, Guid cabId);

    Task UnsubscribeAsync(string emailAddress);

    Task UnsubscribeAllAsync(string emailAddress);
}
