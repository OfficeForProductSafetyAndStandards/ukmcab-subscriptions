namespace UKMCAB.Subscriptions.Core.Common.Security;

public class ExpiringToken<T>
{
    public T Data { get; set; } = default!;

    public DateTime ExpiresUtc { get; set; } = DateTime.UtcNow.AddHours(5);

    public bool IsValid() => ExpiresUtc > DateTime.UtcNow;

    public ExpiringToken(T data, int? expiresInHours = null)
    {
        Data = data;
        ExpiresUtc = DateTime.UtcNow.AddHours(expiresInHours ?? 5);
    }

    public ExpiringToken()
    { }

    public T GetAndValidate()
    {
        if (IsValid())
        {
            return Data;
        }
        else
        {
            throw new Exception("Token has expired.");
        }
    }
}