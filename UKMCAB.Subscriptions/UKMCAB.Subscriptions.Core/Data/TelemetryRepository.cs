using Azure.Data.Tables;
using UKMCAB.Subscriptions.Core.Common;

namespace UKMCAB.Subscriptions.Core.Data;

public interface ITelemetryRepository : IRepository
{
    Task TrackAsync(string key, string text);
    Task TrackByEmailAddressAsync(string emailAddress, string text);
}

public class TelemetryRepository : Repository, ITelemetryRepository
{
    public TelemetryRepository(AzureDataConnectionString dataConnectionString) : base(dataConnectionString, $"{SubscriptionsCoreServicesOptions.TableNamePrefix}telemetry") { }

    public async Task TrackByEmailAddressAsync(string emailAddress, string text)
    {
        await Task.WhenAll(
            UpsertAsync(new TableEntity(emailAddress, Timestamp.Get()).Pipe(x => x.Add("Text", text))),
            UpsertAsync(new TableEntity("global", Timestamp.Reverse()).Pipe(x => x.Add("Text", text), x => x.Add("EmailAddress", emailAddress))));
    }

    public async Task TrackAsync(string key, string text) => await UpsertAsync(new TableEntity(key, Timestamp.Get()).Pipe(x => x.Add("Text", text)));
}

