using Microsoft.Extensions.Logging;
using UKMCAB.Subscriptions.Core.Abstract;
using UKMCAB.Subscriptions.Core.Repositories;

namespace UKMCAB.Subscriptions.Core.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly SubscriptionServicesCoreOptions _options;
    private readonly ILogger<SubscriptionService> _logger;
    private readonly ISubscriptionRepository _subscriptionRepository;

    public SubscriptionService(SubscriptionServicesCoreOptions options, ILogger<SubscriptionService> logger, ISubscriptionRepository subscriptionRepository)
    {
        _options = options;
        _logger = logger;
        _subscriptionRepository = subscriptionRepository;
    }

    public Task<bool> IsSubscribedAsync(string emailAddress, string searchQuery)
    {
        // use _subscriptionRepository to get a record for this subscription

        throw new NotImplementedException();
    }

    public Task<bool> IsSubscribedAsync(string emailAddress, Guid cabId)
    {
        throw new NotImplementedException();
    }

    public Task<string> SubscribeAsync(string emailAddress, string searchQuery, Frequency frequency)
    {
        throw new NotImplementedException();
    }

    public Task<string> SubscribeAsync(string emailAddress, Guid cabId, Frequency frequency)
    {
        throw new NotImplementedException();
    }

    public Task UnsubscribeAllAsync(string emailAddress)
    {
        throw new NotImplementedException();
    }

    public Task UnsubscribeAsync(string emailAddress)
    {
        throw new NotImplementedException();
    }

    public Task UpdateFrequencyAsync(string subscriptionId, Frequency frequency)
    {
        throw new NotImplementedException();
    }
}