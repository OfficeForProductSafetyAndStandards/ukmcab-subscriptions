using Microsoft.Extensions.DependencyInjection;
using UKMCAB.Subscriptions.Core;
using UKMCAB.Subscriptions.Core.Abstract;
using UKMCAB.Subscriptions.Test.Fakes;

namespace UKMCAB.Subscriptions.Test;

public class SubscriptionTests
{
    private FakeDateTimeProvider _datetime;
    private FakeCabSearchService _searchService;
    private FakeCabUpdatesReceiver _cabUpdatesReceiver;
    private SubscriptionServicesCoreOptions _options;
    private ServiceProvider _services;

    [OneTimeSetUp]
    public void SetupOnce()
    {
        _datetime = new FakeDateTimeProvider();
        _searchService = new FakeCabSearchService();
        _cabUpdatesReceiver = new FakeCabUpdatesReceiver();
        _options = new SubscriptionServicesCoreOptions("", "http://ukmcab-dev.beis.gov.uk");
        
        var services = new ServiceCollection().AddLogging().AddSubscriptionServices(_options, _datetime, _searchService, _cabUpdatesReceiver);

        _services = services.BuildServiceProvider();
    }

    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task Test_Subscribe_To_Cab()
    {
        const string emailAddress = "test@test.com";
        var cabId = Guid.NewGuid();

        var subscriptionService = _services.GetRequiredService<ISubscriptionService>();
        var id = await subscriptionService.SubscribeAsync(emailAddress, cabId, Frequency.Daily);
    }
}