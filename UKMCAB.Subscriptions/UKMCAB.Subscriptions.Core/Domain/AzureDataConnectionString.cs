
/// <summary>
/// The core options that need to be configured for this package to function
/// </summary>
public class AzureDataConnectionString : ConnectionString
{
    public AzureDataConnectionString(string dataConnectionString) : base(dataConnectionString) { }
    public static implicit operator string(AzureDataConnectionString d) => d._connectionString;
    public static implicit operator AzureDataConnectionString(string d) => new(d);
}
