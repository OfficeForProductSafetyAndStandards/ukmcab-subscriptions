using UKMCAB.Subscriptions.Core.Common;

namespace UKMCAB.Subscriptions.Core.Domain;

public record SearchSubscriptionRequest(EmailAddress EmailAddress, string? SearchQueryString, Frequency Frequency, string? Keywords);

public record CabSubscriptionRequest(EmailAddress EmailAddress, Guid CabId, Frequency Frequency);

public static class SearchQueryString
{
    public static string Process(string? queryString, SubscriptionsCoreServicesOptions options)
    {
        return QueryString2.Parse(queryString).Remove(options.SearchQueryStringRemoveKeys).ToString();
    }
}