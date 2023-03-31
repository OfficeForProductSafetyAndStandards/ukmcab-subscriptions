using UKMCAB.Subscriptions.Core.Abstract;
/// <summary>
/// The core options that need to be configured for this package to function
/// </summary>
public class SubscriptionServicesCoreOptions
{
    public string DataConnectionString { get; set; }
    public string BaseUrl { get; set; }

    public SubscriptionServicesCoreOptions(string dataConnectionString, string baseUrl)
    {
        DataConnectionString = dataConnectionString;
        BaseUrl = baseUrl;
    }
}