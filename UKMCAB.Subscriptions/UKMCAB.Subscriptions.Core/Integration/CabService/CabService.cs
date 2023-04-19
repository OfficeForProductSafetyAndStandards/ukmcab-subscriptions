using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace UKMCAB.Subscriptions.Core.Integration.CabService;

public interface ICabService : IDisposable
{
    Task<CabApiModel?> GetAsync(Guid id);

    /// <summary>
    /// Searches for CABs
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    Task<CabService.SearchResults> SearchAsync(string? query);
}

/// <summary>
/// The API options
/// </summary>
/// <param name="BaseUri">The API base url</param>
/// <param name="AuthorizationHeaderValue">Any auth header required.  If using basic auth, this can be created with `BasicAuthenticationHeaderValue.Create(username, password)`</param>
/// <see cref="Common.BasicAuthenticationHeaderValue"/>
public record CabApiOptions(Uri BaseUri, AuthenticationHeaderValue? AuthorizationHeaderValue = null);

public class CabService : ICabService
{
    private readonly HttpClient _client;

    public CabService(CabApiOptions options)
    {
        _client = new HttpClient
        {
            BaseAddress = options.BaseUri ?? throw new Exception($"{nameof(CabApiOptions)}.{nameof(CabApiOptions.BaseUri)} is null"),
        };

        if (options.AuthorizationHeaderValue != null)
        {
            _client.DefaultRequestHeaders.Authorization = options.AuthorizationHeaderValue;
        }
    }

    public record SearchResults(int Total, List<CabSearchApiModel> Results);

    public async Task<SearchResults> SearchAsync(string? query)
    {
        var uri = $"/__api/search{query?.EnsureStartsWith("?")}";
        var response = await _client.GetAsync(uri);
        response.EnsureSuccessStatusCode();
        var count = response.Headers.GetValues("x-count").FirstOrDefault().ToInteger().GetValueOrDefault();
        var results = await response.Content.ReadFromJsonAsync<List<CabSearchApiModel>>() ?? throw new Exception("Search results deserialised to null");
        return new SearchResults(count, results);
    }

    public async Task<CabApiModel?> GetAsync(Guid id)
    {
        var uri = $"/__api/cab/{id}";
        var response = await _client.GetAsync(uri);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        else
        {
            response.EnsureSuccessStatusCode();
            var model = await response.Content.ReadFromJsonAsync<CabApiModel>() ?? throw new Exception("Search results deserialised to null");
            return model;
        }
    }

    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }
}
