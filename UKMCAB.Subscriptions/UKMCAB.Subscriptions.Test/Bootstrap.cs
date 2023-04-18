using Microsoft.Extensions.Configuration;

[SetUpFixture]
public class Bootstrap
{
    public static IConfigurationRoot Configuration { get; private set; }

    [OneTimeSetUp]
    public void Do()
    {
        Configuration = new ConfigurationBuilder().AddUserSecrets<Bootstrap>().Build();
    }
}