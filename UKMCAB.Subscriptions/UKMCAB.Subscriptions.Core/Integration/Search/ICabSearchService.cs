namespace UKMCAB.Subscriptions.Core.Integration.Search;

/// <summary>
/// Responsible for searching CABs based on the query supplied
/// </summary>
public interface ICabSearchService
{
    Task<IEnumerable<CabSearchResult>> SearchAsync(string query);
}
