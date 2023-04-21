namespace UKMCAB.Subscriptions.Core.Domain.Emails;

public class EmailDefinition
{
    public const string MetadataKeyToken = "token";

    public string TemplateId { get; set; }
    public EmailAddress Recipient { get; set; }
    public Dictionary<string, string> Replacements { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
    public string? Token => Metadata.GetValueOrDefault(MetadataKeyToken);
    public void AddMetadataToken(string token) => Metadata.Add(MetadataKeyToken, token);
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    public EmailDefinition(string templateId, EmailAddress recipient)
    {
        TemplateId = templateId;
        Recipient = recipient;
    }

    public EmailDefinition(string templateId,
        EmailAddress recipient,
        Dictionary<string, string> replacements,
        Dictionary<string, string> metadata)
        : this(templateId, recipient)
    {
        Replacements = replacements;
        Metadata = metadata;
    }
}
