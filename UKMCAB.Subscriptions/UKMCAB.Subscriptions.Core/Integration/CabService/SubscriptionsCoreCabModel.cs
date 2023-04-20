namespace UKMCAB.Subscriptions.Core.Integration.CabService;

public class SubscriptionsCoreCabModel
{
    public string? CABId { get; set; }
    public DateTime? PublishedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string? Name { get; set; }
    public string? UKASReferenceNumber { get; set; }
    public string? Address { get; set; }
    public string? Website { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? RegisteredOfficeLocation { get; set; }
    public List<string>? RegisteredTestLocations { get; set; }
    public string? BodyNumber { get; set; }
    public List<string>? BodyTypes { get; set; }
    public List<string>? LegislativeAreas { get; set; }
    public List<SubscriptionsCoreCabFileModel>? ProductSchedules { get; set; }
}
