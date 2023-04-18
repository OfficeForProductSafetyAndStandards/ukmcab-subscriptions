namespace UKMCAB.Subscriptions.Core.Data;

public interface IRepositories
{
    IBlockedEmailsRepository Blocked { get; }
    ISubscriptionRepository Subscriptions { get; }
    ITelemetryRepository Telemetry { get; }
}

public class Repositories : IRepositories
{
    public IBlockedEmailsRepository Blocked { get; }
    public ISubscriptionRepository Subscriptions { get; }
    public ITelemetryRepository Telemetry { get; }

    public Repositories(IBlockedEmailsRepository blocked, ISubscriptionRepository subscriptions, ITelemetryRepository telemetry)
    {
        Blocked = blocked;
        Subscriptions = subscriptions;
        Telemetry = telemetry;
    }
}
