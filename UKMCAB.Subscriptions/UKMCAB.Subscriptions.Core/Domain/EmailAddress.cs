using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace UKMCAB.Subscriptions.Core.Domain;

public partial class EmailAddress
{
    private readonly string _emailAddress;

    [JsonConstructor]
    public EmailAddress(string emailAddress)
    {
        emailAddress = Cleanse(emailAddress) ?? throw new ArgumentException("The email address supplied is empty/null", nameof(emailAddress));
        
        if (IsValidEmail(emailAddress))
        {
            _emailAddress = emailAddress;
        }
        else
        {
            throw new ArgumentException("The email address supplied is invalid", nameof(emailAddress));
        }
    }

    private static string? Cleanse(string? text) => text?.Clean()?.Trim()?.ToLower();

    public static bool IsNotValidEmail(string? text) => !IsValidEmail(text);

    public static bool IsValidEmail(string? text)
    {
        text = text?.Clean();
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }
        else
        {
            return Regex.IsMatch(text, @"\A\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,24}\b\Z", RegexOptions.IgnoreCase);
        }
    }

    public override string ToString() => _emailAddress;



    public static implicit operator string(EmailAddress d) => d._emailAddress;

    public static implicit operator EmailAddress(string d) => new(d);
}


public class EmailAddressConverter : JsonConverter<EmailAddress>
{
    public override EmailAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            new(reader.GetString()!);

    public override void Write(Utf8JsonWriter writer, EmailAddress emailAddress, JsonSerializerOptions options)
    {
        writer.WriteStringValue(emailAddress.ToString());
    }
}


