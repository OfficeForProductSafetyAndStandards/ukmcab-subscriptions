namespace UKMCAB.Subscriptions.Core.Domain;
public class Keys
{
    public string? PartitionKey { get; set; }
    public string? RowKey { get; set; }

    public Keys(string? partitionKey, string? rowKey)
    {
        PartitionKey = partitionKey;
        RowKey = rowKey;
    }

    public Keys(string composite) : this(composite.Split('$')) { }

    private Keys(string[] composite) : this(composite[0], composite[1]) { }

    public override string ToString() => string.Concat(PartitionKey, '$', RowKey);

    public static implicit operator string(Keys d) => d.ToString();

    public static implicit operator Keys(string d) => new(d);
}
