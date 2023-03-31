using Microsoft.Extensions.Logging;
using UKMCAB.Subscriptions.Core.Abstract;
using UKMCAB.Subscriptions.Core.Integration.CabUpdates;
using UKMCAB.Subscriptions.Core.Integration.Search;
using UKMCAB.Subscriptions.Core.Repositories;

namespace UKMCAB.Subscriptions.Core.Services;
public class SubscriptionEngine : ISubscriptionEngine
{
    private readonly SubscriptionServicesCoreOptions _options;
    private readonly ILogger<SubscriptionEngine> _logger;
    private readonly ICabUpdatesReceiver _cabUpdatesReceiver;
    private readonly ICabSearchService _searchService;
    private readonly ISubscriptionRepository _repository;
    private readonly IOutboxRepository _outboxRepository;

    public SubscriptionEngine(SubscriptionServicesCoreOptions options, ILogger<SubscriptionEngine> logger, ICabUpdatesReceiver cabUpdatesReceiver, ICabSearchService searchService, ISubscriptionRepository repository, IOutboxRepository outboxRepository)
    {
        _options = options;
        _logger = logger;
        _cabUpdatesReceiver = cabUpdatesReceiver;
        _searchService = searchService;
        _repository = repository;
        _outboxRepository = outboxRepository;
    }

    /// <inheritdoc />
    public async Task ProcessCabSubsribersAsync(CancellationToken cancellationToken)
    {
        while(!cancellationToken.IsCancellationRequested)
        {
            var updates = await _cabUpdatesReceiver.GetCabUpdateMessagesAsync();
            if(updates.Any())
            {
                foreach(var update in updates)
                {
                    // todo: create notification and persist them


                    await _cabUpdatesReceiver.MarkAsProcessedAsync(update);
                }
            }
            await Task.Delay(1000, cancellationToken);
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
        //  create the email and put it into an Outbox (azure storage table)
        // NOTE: WE DO NOT WANT TO SEND ANY REAL EMAILS YET, JUST SAVE THEM IN TABLE STORAGE IN AN OUTBOX FOR NOW.

        /*
            e.g.
            await _outboxRepository.SaveAsync(new Domain.Notification()
            {
                EmailAddress = 
            })
        */

        throw new NotImplementedException();
    }
}
