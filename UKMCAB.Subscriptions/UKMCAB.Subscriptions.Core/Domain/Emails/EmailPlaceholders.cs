namespace UKMCAB.Subscriptions.Core.Domain.Emails;

public static class EmailPlaceholders
{
    public static string ConfirmLink { get; set; } = "confirm_link";
    public static string ManageMySubscriptionLink { get; set; } = "manage_subscription_link";
    public static string UnsubscribeLink { get; set; } = "unsubscribe_link";
    public static string UnsubscribeAllLink { get; set; } = "unsubscribe_all_link";
    public static string ViewCabLink { get; set; } = "view_cab_link";
    public static string ViewSearchLink { get; set; } = "view_search_link";
    public static string ViewSearchChangesSummaryLink { get; set; } = "view_search_changes_summary";
    public static string CabName { get; set; } = "cab_name";
    public static string SearchTopicName { get; set; } = "search_topic_name";
}
