using UKMCAB.Subscriptions.Core.Integration.CabService;

namespace UKMCAB.Subscriptions.Test.Fakes;

public class FakeCabService : ICabService
{
    private CabService.SearchResults _result = new CabService.SearchResults(0, new List<CabSearchApiModel>());
    private CabApiModel? _resultApiModel = null;
    public Task<CabService.SearchResults> SearchAsync(string? query) => Task.FromResult(_result);
    public Task<CabApiModel?> GetAsync(Guid id) => Task.FromResult(_resultApiModel);

    public void Inject(CabService.SearchResults result) => _result = result;
    public void Inject(CabApiModel model) => _resultApiModel = model;
    public void Dispose() => GC.SuppressFinalize(this);
}
