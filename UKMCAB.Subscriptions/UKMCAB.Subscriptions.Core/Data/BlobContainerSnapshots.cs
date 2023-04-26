using Azure.Storage.Blobs;

namespace UKMCAB.Subscriptions.Core.Data;

public static class BlobContainerSnapshots
{
    public static BlobContainerClient Create(string? dataConnectionString)
    {
        ArgumentNullException.ThrowIfNull(dataConnectionString);
        return new BlobContainerClient(dataConnectionString, $"{SubscriptionsCoreServicesOptions.BlobContainerPrefix}snapshots");
    }
}
