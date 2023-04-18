using Microsoft.Extensions.Logging;
using UKMCAB.Subscriptions.Core.Data;
using UKMCAB.Subscriptions.Core.Integration.CabUpdates;
using UKMCAB.Subscriptions.Core.Integration.OutboundEmail;
using UKMCAB.Subscriptions.Core.Integration.Search;

namespace UKMCAB.Subscriptions.Core.Services;

public interface ISubscriptionEngine
{
    /// <summary>
    /// This method processes and potentially notifies subscribers on search result changes.
    /// This method will be called once per hour. 
    /// 
    /// For each search subscription, 
    ///     get the latest search results.  
    ///     Projects a list of CAB ids ordered by id, creates an MD5 hash of those ids.
    ///     Checks storage to find out whether the last MD5 hash from the previous invocation of this function is different.
    ///     If the hashes are different (or if the prior one is null) then summarise the differences in text form.
    ///     Persist the email notification for later sending.
    ///     Records the current hash to storage    
    ///     Move to the next subscription until all are processed.
    /// </summary>
    /// <returns></returns>
    Task ProcessSearchSubscribersAsync(CancellationToken cancellationToken);

    /// <summary>
    /// This method polls an Azure Storage Queue for messages about CAB updates.
    /// It will run continually until cancellationToken.IsCancellationRequested==true.
    /// For each CAB update, and each subscriber, it will persist the email to be sent at a later.  
    /// If there's an existing buffered email relating to this subscription/cab, then update
    /// the buffered email to represent the latest change state.
    /// </summary>
    /// <returns></returns>
    Task ProcessCabSubscribersAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Sends buffered/persisted email notifications once they reach their due-date based on the frequency of the email subscription.
    /// This method will be called continuously until cancellationToken.IsCancellationRequested==true.
    /// Each each email send, the fact the email has been sent should be recorded straight away, to prevent re-send.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>

    Task ProcessSendEmailNotificationsAsync(CancellationToken cancellationToken);
}

public interface ISubscriptionEngineTestable
{
    Task<bool> ProcessSingleCabUpdateAsync();
}

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

    internal SubscriptionEngine(SubscriptionServicesCoreOptions options, ILogger<SubscriptionEngine> logger, ICabUpdatesReceiver cabUpdatesReceiver, ICabSearchService searchService, ISubscriptionRepository repository, 
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
