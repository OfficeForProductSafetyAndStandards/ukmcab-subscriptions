namespace UKMCAB.Subscriptions.Core.Common
{
    /// <summary>
    /// The core options that need to be configured for this package to function
    /// </summary>
    public abstract class ConnectionString
    {
        protected readonly string _connectionString;
        public ConnectionString(string dataConnectionString) => _connectionString = dataConnectionString.Clean() ?? throw new ArgumentNullException(nameof(dataConnectionString));
        public override string ToString() => _connectionString;
    }
}