using UKMCAB.Subscriptions.Core.Abstract;

namespace UKMCAB.Subscriptions.Core;

public class RealDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}