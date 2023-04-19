using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using MoreLinq;

namespace UKMCAB.Subscriptions.Core.Common;

public class QueryString2
{
    private readonly Dictionary<string, StringValues> _dictionary;

    private QueryString2(Dictionary<string, StringValues> dictionary) => _dictionary = dictionary;

    public QueryString2 Add(string key, string value)
    {
        if( _dictionary.ContainsKey(key))
        {
            _dictionary[key] += value;
        }
        else
        {
            _dictionary.Add(key, value);
        }
        return this;
    }

    public QueryString2 Remove(string key)
    {
        _dictionary.Remove(key);
        return this;
    }

    public QueryString2 Remove(string[] keys)
    {
        keys?.ForEach(x => Remove(x));
        return this;
    }

    public override string ToString()
    {
        var ordered = _dictionary.OrderBy(x => x.Key)
            .Select(x => new KeyValuePair<string, StringValues>(x.Key, new StringValues(x.Value.OrderBy(s => s).ToArray())))
            .ToDictionary();

        var qc = new QueryCollection(ordered);
        var qs = QueryString.Create(qc);
        return qs.Value;
    }

    public static QueryString2 Parse(string? queryString)
    {
        queryString = queryString.Clean();
        return new QueryString2(QueryHelpers.ParseQuery(queryString));
    }
}
