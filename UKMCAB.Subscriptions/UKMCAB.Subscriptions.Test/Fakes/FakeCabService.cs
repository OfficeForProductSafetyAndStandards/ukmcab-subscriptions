using UKMCAB.Subscriptions.Core.Integration.CabService;

namespace UKMCAB.Subscriptions.Test.Fakes;

public class FakeCabService : ICabService
{
    private CabApiService.SearchResults _result = new CabApiService.SearchResults(0, new List<SubscriptionsCoreCabSearchResultModel>());
    private SubscriptionsCoreCabModel? _resultApiModel = null;
    public Task<CabApiService.SearchResults> SearchAsync(string? query) => Task.FromResult(_result);
    public Task<SubscriptionsCoreCabModel?> GetAsync(Guid id) => Task.FromResult(_resultApiModel);

    public void Inject(CabApiService.SearchResults result) => _result = result;
    public void Inject(SubscriptionsCoreCabModel model) => _resultApiModel = model;
    public void Dispose() => GC.SuppressFinalize(this);
}
