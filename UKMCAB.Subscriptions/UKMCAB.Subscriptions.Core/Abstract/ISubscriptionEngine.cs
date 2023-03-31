namespace UKMCAB.Subscriptions.Core.Abstract;

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
    Task ProcessCabSubsribersAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Sends buffered/persisted email notifications once they reach their due-date based on the frequency of the email subscription.
    /// This method will be called continuously until cancellationToken.IsCancellationRequested==true.
    /// Each each email send, the fact the email has been sent should be recorded straight away, to prevent re-send.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>

    Task ProcessSendEmailNotificationsAsync(CancellationToken cancellationToken);
}
