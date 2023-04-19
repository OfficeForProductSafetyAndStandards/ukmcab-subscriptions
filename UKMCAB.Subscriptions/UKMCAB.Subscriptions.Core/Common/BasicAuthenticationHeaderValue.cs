using System.Net.Http.Headers;
using System.Text;

namespace UKMCAB.Subscriptions.Core.Common;

public static class BasicAuthenticationHeaderValue
{
    private const string _basic = "Basic";

    public static AuthenticationHeaderValue? Create(string? credentials)
    {
        if(credentials != null)
        {
            var encodedCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));
            return new AuthenticationHeaderValue(_basic, encodedCredentials);
        }
        else
        {
            return null;
        }
    }

    public static AuthenticationHeaderValue? Create(string username, string password) => Create($"{username}:{password}");

    public record Credentials(string? UserName, string? Password);

    public static Credentials? Parse(AuthenticationHeaderValue? header)
    {
        if(header != null && header.Scheme.DoesEqual(_basic) && header.Parameter.IsNotNullOrEmpty())
        {
            var bytes = Convert.FromBase64String(header.Parameter ?? throw new Exception($"{nameof(AuthenticationHeaderValue)}.{nameof(AuthenticationHeaderValue.Parameter)} value is null"));
            var text = Encoding.ASCII.GetString(bytes);
            var parts = text.Split(':');
            return new Credentials(parts.ElementAtOrDefault(0), parts.ElementAtOrDefault(1));
        }
        else
        {
            return null;
        }
    }
}