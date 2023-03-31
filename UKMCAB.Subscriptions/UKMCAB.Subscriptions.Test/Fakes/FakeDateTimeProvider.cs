using UKMCAB.Subscriptions.Core.Abstract;

namespace UKMCAB.Subscriptions.Test.Fakes;
public class FakeDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow { get; set; }
}
