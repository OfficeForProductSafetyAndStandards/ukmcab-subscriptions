using Azure.Data.Tables;
using UKMCAB.Subscriptions.Core.Common;

namespace UKMCAB.Subscriptions.Core.Data;

public interface ITelemetryRepository : IRepository
{
    Task TrackAsync(string emailAddress, string text);
}

public class TelemetryRepository : Repository, ITelemetryRepository
{
    public TelemetryRepository(AzureDataConnectionString dataConnectionString) : base(dataConnectionString, "telemetry") { }

    public async Task TrackAsync(string emailAddress, string text)
    {
        await UpsertAsync(new TableEntity(emailAddress, Timestamp.Get()).Pipe(x => x.Add("Text", text)));
        await UpsertAsync(new TableEntity("global", Timestamp.Reverse()).Pipe(x => x.Add("Text", text), x => x.Add("EmailAddress", emailAddress)));
    }
}

