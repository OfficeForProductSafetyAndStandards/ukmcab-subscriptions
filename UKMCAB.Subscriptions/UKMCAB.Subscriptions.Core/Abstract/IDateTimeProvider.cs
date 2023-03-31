namespace UKMCAB.Subscriptions.Core.Abstract;
public interface IDateTimeProvider
{
    public DateTime UtcNow { get; }
}
