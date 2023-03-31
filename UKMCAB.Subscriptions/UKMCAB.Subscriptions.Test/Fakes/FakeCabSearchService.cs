using UKMCAB.Subscriptions.Core.Integration.Search;

namespace UKMCAB.Subscriptions.Test.Fakes;

public class FakeCabSearchService : ICabSearchService
{
    public Task<IEnumerable<CabSearchResult>> SearchAsync(string query)
    {
        throw new NotImplementedException();
    }
}
