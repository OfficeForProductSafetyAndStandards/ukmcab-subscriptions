using UKMCAB.Subscriptions.Core;
using UKMCAB.Subscriptions.Core.Common;

namespace UKMCAB.Subscriptions.Test.Fakes;
public class FakeDateTimeProvider : IDateTimeProvider
{
    private DateTime _utcNow;

    public DateTime UtcNow { get => _utcNow; set => _utcNow = value.AsUtc(); }
}
