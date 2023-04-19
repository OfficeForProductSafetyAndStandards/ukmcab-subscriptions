namespace UKMCAB.Subscriptions.Core.Domain;

public class EmailTemplates
{
    public string ConfirmSearchSubscription { get; set; } = null!;
    public string ConfirmCabSubscription { get; set; } = null!;
    public string ConfirmUpdateEmailAddress { get; set; } = null!;
    public string SearchUpdated { get; set; } = null!;
    public string CabUpdated { get; set; } = null!;

    public void Validate()
    {
        _ = ConfirmSearchSubscription ?? throw new Exception($"{nameof(ConfirmSearchSubscription)} should not be null");
        _ = ConfirmCabSubscription ?? throw new Exception($"{nameof(ConfirmCabSubscription)} should not be null");
        _ = ConfirmUpdateEmailAddress ?? throw new Exception($"{nameof(ConfirmUpdateEmailAddress)} should not be null");
        _ = SearchUpdated ?? throw new Exception($"{nameof(SearchUpdated)} should not be null");
        _ = CabUpdated ?? throw new Exception($"{nameof(CabUpdated)} should not be null");
    }
}
