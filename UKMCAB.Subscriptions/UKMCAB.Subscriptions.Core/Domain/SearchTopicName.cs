using UKMCAB.Subscriptions.Core.Common;

namespace UKMCAB.Subscriptions.Core.Domain;

public static class SearchTopicName
{
    public static string Create(string? keywords) => $"UKMCAB search results{keywords.PrependIf(" for '").AppendIf("'")}";
}