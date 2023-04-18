using System.Text.Json;
using System.Text.Json.Serialization;

namespace UKMCAB.Subscriptions.Core.Common;

public static class JsonUtil
{
    public static JsonSerializerOptions JsonSerializerOptions { get; } = new JsonSerializerOptions
    {
        WriteIndented = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public static T? TryDeserialize<T>(string? json) where T : class
    {
        if (json?.Clean() != null)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(json, JsonSerializerOptions);
            }
            catch { }
        }
        return null;
    }

    public static T? Deserialize<T>(string? json) where T : class
    {
        ArgumentNullException.ThrowIfNull(json);
        return JsonSerializer.Deserialize<T>(json, JsonSerializerOptions);
    }
}
