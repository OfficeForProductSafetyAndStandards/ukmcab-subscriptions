using Microsoft.Extensions.Logging;
using UKMCAB.Subscriptions.Core.Abstract;
using UKMCAB.Subscriptions.Core.Integration.CabUpdates;
using UKMCAB.Subscriptions.Core.Integration.OutboundEmail;
using UKMCAB.Subscriptions.Core.Integration.Search;
using UKMCAB.Subscriptions.Core.Repositories;

namespace UKMCAB.Subscriptions.Core.Services;
public class SubscriptionEngine : ISubscriptionEngine, ISubscriptionEngineTestable
{
    private readonly SubscriptionServicesCoreOptions _options;
    private readonly ILogger<SubscriptionEngine> _logger;
    private readonly ICabUpdatesReceiver _cabUpdatesReceiver;
    private readonly ICabSearchService _searchService;
    private readonly ISubscriptionRepository _repository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IOutboundEmailSender _outboundEmailSender;
    private readonly ISentNotificationRepository _sentNotificationRepository;

    public SubscriptionEngine(SubscriptionServicesCoreOptions options, ILogger<SubscriptionEngine> logger, ICabUpdatesReceiver cabUpdatesReceiver, ICabSearchService searchService, ISubscriptionRepository repository, 
        IOutboxRepository outboxRepository, IOutboundEmailSender outboundEmailSender, ISentNotificationRepository sentNotificationRepository)
    {
        _options = options;
        _logger = logger;
        _cabUpdatesReceiver = cabUpdatesReceiver;
        _searchService = searchService;
        _repository = repository;
        _outboxRepository = outboxRepository;
        _outboundEmailSender = outboundEmailSender;
        _sentNotificationRepository = sentNotificationRepository;
    }

    /// <inheritdoc />
    public async Task ProcessCabSubscribersAsync(CancellationToken cancellationToken)
    {
        while(!cancellationToken.IsCancellationRequested)
        {
            if(!await ProcessSingleCabUpdateAsync())
            {
                await Task.Delay(10_000, cancellationToken);
            }
        }
    }

    public async Task<bool> ProcessSingleCabUpdateAsync()
    {
        var update = await _cabUpdatesReceiver.GetCabUpdateMessageAsync();
        if (update != null)
        {
            // todo: create notification and persist them
            // ...

            await _cabUpdatesReceiver.MarkAsProcessedAsync(update);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <inheritdoc />
    public Task ProcessSearchSubscribersAsync(CancellationToken cancellationToken)
    {
        // for each subscription
        //  perform a search via _searchService
        //  record the list of _changes_ (added/removed cabs) since EITHER the baseline search (when they first subscribed) OR the search results from the LAST_TIME they were notified
        //  persist a model of changes (in azure table storage) to notify the subscriber ()  Overwrite if necessary.  Defer any email-send operation until ProcessSendEmailNotificationsAsync.


        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task ProcessSendEmailNotificationsAsync(CancellationToken cancellationToken)
    {
        // for each pending notification
        //  check whether it's due based on the frequency of the subscription
        

        throw new NotImplementedException();
    }
}
