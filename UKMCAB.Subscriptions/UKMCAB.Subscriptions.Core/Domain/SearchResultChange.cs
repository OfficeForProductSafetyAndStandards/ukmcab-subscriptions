using UKMCAB.Subscriptions.Core.Integration.CabService;

namespace UKMCAB.Subscriptions.Core.Domain;

public class SearchResultChange
{
    public Guid CabId => Old.CabId;
    public SubscriptionsCoreCabSearchResultModel Old { get; set; }
    public SubscriptionsCoreCabSearchResultModel New { get; set; }

    public SearchResultChange(SubscriptionsCoreCabSearchResultModel old, SubscriptionsCoreCabSearchResultModel @new)
    {
        Old = old;
        New = @new;
    }
}
