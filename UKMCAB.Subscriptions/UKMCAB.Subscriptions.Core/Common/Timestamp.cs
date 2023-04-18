namespace UKMCAB.Subscriptions.Core.Common;

public static class Timestamp
{
    /// <summary>
    /// Returns a lexicographically sortable timestamp
    /// </summary>
    public static string Get() => Get(DateTimeOffset.UtcNow);

    /// <summary>
    /// Returns a lexicographically reverse-sortable timestamp
    /// </summary>
    /// <returns></returns>
    public static string Reverse() => Reverse(DateTimeOffset.UtcNow);

    /// <summary>
    /// Returns a lexicographically sortable timestamp
    /// </summary>
    public static string Get(DateTimeOffset dt) => string.Concat(GetTimestamp(dt), GetRandomPart());

    /// <summary>
    /// Returns a lexicographically reverse-sortable timestamp
    /// </summary>
    /// <returns></returns>
    public static string Reverse(DateTimeOffset dt) => string.Concat(GetReverseTimestamp(dt), GetRandomPart());

    private static string GetReverseTimestamp(DateTimeOffset dt) => Pad(DateTimeOffset.MaxValue.ToUnixTimeMilliseconds() - dt.ToUnixTimeMilliseconds());

    private static string GetTimestamp(DateTimeOffset dt) => Pad(dt.ToUnixTimeMilliseconds());

    private static string GetRandomPart() => RandomNumber.Next(1, 999).ToString().PadLeft(3, '0');

    private static string Pad(long number) => number.ToString().PadLeft(16, '0');
}
