namespace UKMCAB.Subscriptions.Core;

public interface IDateTimeProvider
{
    public DateTime UtcNow { get; }
}

public class RealDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}