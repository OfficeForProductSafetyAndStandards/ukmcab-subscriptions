namespace UKMCAB.Subscriptions.Core.Integration.Search;

/// <summary>
/// This implementation searches CABs by using the Search JSON API hosted on the website
/// </summary>
public class CabSearchService : ICabSearchService
{
    private readonly SubscriptionServicesCoreOptions _options;
    private readonly string _searchApiUrl;

    public CabSearchService(SubscriptionServicesCoreOptions options)
    {
        _options = options;
        _searchApiUrl = $"{_options.BaseUrl}/api/search";
    }

    /// <summary>
    /// Searches for CABs with the query string provided
    /// </summary>
    /// <param name="query"></param>
    /// <returns>The CAB search results.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<IEnumerable<CabSearchResult>> SearchAsync(string query)
    {
        var searchUrl = string.Concat(_searchApiUrl, '?', query);

        // use httpclient with this searchurl to get the list of CABs for the query provided.


        throw new NotImplementedException();
    }
}
