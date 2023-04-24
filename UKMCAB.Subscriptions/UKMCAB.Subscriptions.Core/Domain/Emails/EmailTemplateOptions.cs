namespace UKMCAB.Subscriptions.Core.Domain.Emails;

public class EmailTemplateOptions
{
    public string ConfirmSearchSubscriptionTemplateId { get; set; } = null!;
    public string ConfirmCabSubscriptionTemplateId { get; set; } = null!;
    public string ConfirmUpdateEmailAddressTemplateId { get; set; } = null!;
    public string SearchUpdatedTemplateId { get; set; } = null!;
    public string CabUpdatedTemplateId { get; set; } = null!;
    public string SubscribedSearchNotificationTemplateId { get; set; } = null!;
    public string SubscribedCabNotificationTemplateId { get; set; } = null!;

    public void Validate()
    {
        _ = ConfirmSearchSubscriptionTemplateId ?? throw new Exception($"{nameof(ConfirmSearchSubscriptionTemplateId)} should not be null");
        _ = ConfirmCabSubscriptionTemplateId ?? throw new Exception($"{nameof(ConfirmCabSubscriptionTemplateId)} should not be null");
        _ = ConfirmUpdateEmailAddressTemplateId ?? throw new Exception($"{nameof(ConfirmUpdateEmailAddressTemplateId)} should not be null");
        _ = SearchUpdatedTemplateId ?? throw new Exception($"{nameof(SearchUpdatedTemplateId)} should not be null");
        _ = CabUpdatedTemplateId ?? throw new Exception($"{nameof(CabUpdatedTemplateId)} should not be null");
        _ = SubscribedSearchNotificationTemplateId ?? throw new Exception($"{nameof(SubscribedSearchNotificationTemplateId)} should not be null");
        _ = SubscribedCabNotificationTemplateId ?? throw new Exception($"{nameof(SubscribedCabNotificationTemplateId)} should not be null");
    }
}
