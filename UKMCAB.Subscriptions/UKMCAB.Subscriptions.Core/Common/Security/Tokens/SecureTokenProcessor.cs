using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

namespace UKMCAB.Subscriptions.Core.Common.Security.Tokens;

public interface ISecureTokenProcessor
{
    T? Disclose<T>(string token);

    string? Enclose<T>(T obj);
}

public class SecureTokenProcessor : ISecureTokenProcessor
{
    private readonly KeyIV _keyiv;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public SecureTokenProcessor(string key, JsonSerializerOptions jsonSerializerOptions)
    {
        Guard.IsNotNull(key.Clean(), $"{nameof(SecureTokenProcessor)}.ctor(key) parameter");
        _keyiv = KeyIV.Create(key);
        _jsonSerializerOptions = jsonSerializerOptions;
    }

    public string? Enclose<T>(T obj)
    {
        var retVal = null as string;
        if (obj != null)
        {
            // convert to json
            var json = JsonSerializer.Serialize(obj, _jsonSerializerOptions);

            // encrypt
            var bytes = CryptoHelper.Encrypt(json, _keyiv);

            // url token encoding
            retVal = Base64UrlEncoder.Encode(bytes);
        }
        return retVal;
    }

    public T? Disclose<T>(string token)
    {
        var retVal = default(T);
        if (!string.IsNullOrWhiteSpace(token))
        {
            var bytes = Base64UrlEncoder.DecodeBytes(token);
            var json = CryptoHelper.Decrypt(bytes, _keyiv); 
            var obj = JsonSerializer.Deserialize<T>(json, _jsonSerializerOptions);
            return obj;   
        }
        return retVal;
    }
}