using System.Security.Cryptography;

namespace UKMCAB.Subscriptions.Core.Common.Security.Tokens;

public struct KeyIV
{
    public byte[] Key { get; set; }

    public byte[] IV { get; set; }

    public override string ToString() => string.Concat(Convert.ToBase64String(Key), "!", Convert.ToBase64String(IV));

    public static KeyIV Create(string payload) => Create(payload.Split("!").Select(x => Convert.FromBase64String(x)).ToArray());

    public static KeyIV Create(byte[][] payload) => new() { Key = payload[0], IV = payload[1] };

    /// <summary>
    /// Creates a new KeyIV
    /// </summary>
    /// <returns></returns>
    public static KeyIV Create()
    {
        using var aes = Aes.Create();
        aes.GenerateKey();
        aes.GenerateIV();
        return new KeyIV { IV = aes.IV, Key = aes.Key };
    }

    /// <summary>
    /// Generates a new KeyIV and converts to string
    /// </summary>
    /// <returns></returns>
    public static string GenerateKey() => Create().ToString();
}