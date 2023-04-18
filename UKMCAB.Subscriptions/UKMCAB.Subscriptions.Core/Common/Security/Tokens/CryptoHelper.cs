using System.Security.Cryptography;

namespace UKMCAB.Subscriptions.Core.Common.Security.Tokens;

public class CryptoHelper
{
    public static KeyIV GenerateKeyIV() => KeyIV.Create();

    public static byte[] Encrypt(string text, KeyIV keyiv) => Encrypt(text, keyiv.Key, keyiv.IV);

    public static string Decrypt(byte[] data, KeyIV keyiv) => Decrypt(data, keyiv.Key, keyiv.IV);

    public static byte[] Encrypt(string text, byte[] key, byte[] iv)
    {
        Validate(text, key, iv);

        using var aes = CreateAes(key, iv);

        Validate(aes, key, iv);

        var encryptor = aes.CreateEncryptor();

        using var msEncrypt = new MemoryStream();
        using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
        using var swEncrypt = new StreamWriter(csEncrypt) { AutoFlush = true };

        swEncrypt.Write(text);
        swEncrypt.Close();

        var encrypted = msEncrypt.ToArray();

        return encrypted;
    }

    public static string Decrypt(byte[] data, byte[] key, byte[] iv)
    {
        Validate(data, key, iv);

        using var aes = CreateAes(key, iv);

        Validate(aes, key, iv);

        var decryptor = aes.CreateDecryptor();

        using var msDecrypt = new MemoryStream(data);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);

        var decrypted = srDecrypt.ReadToEnd();

        return decrypted;
    }

    private static void Validate(byte[] data, byte[] key, byte[] iv)
    {
        if (data == null || data.Length <= 0)
        {
            throw new ArgumentNullException(nameof(data));
        }
        Validate(key, iv);
    }

    private static void Validate(string text, byte[] key, byte[] iv)
    {
        if (text == null || text.Length <= 0)
        {
            throw new ArgumentNullException(nameof(text));
        }
        Validate(key, iv);
    }

    private static void Validate(byte[] key, byte[] iv)
    {
        if (key == null || key.Length <= 0)
        {
            throw new ArgumentNullException(nameof(key));
        }
        else if (iv == null || iv.Length <= 0)
        {
            throw new ArgumentNullException(nameof(iv));
        }
    }

    private static void Validate(Aes aes, byte[] key, byte[] iv)
    {
        if (!key.SequenceEqual(aes.Key))
        {
            throw new Exception("The keys do not match");
        }

        if (!iv.SequenceEqual(aes.IV))
        {
            throw new Exception("The IVs do not match");
        }
    }

    private static Aes CreateAes(byte[] key, byte[] iv)
    {
        var a = Aes.Create();
        a.Key = key;
        a.IV = iv;
        return a;
    }
}