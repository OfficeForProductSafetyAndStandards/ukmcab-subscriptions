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

        services.AddSingleton(x => (ISubscriptionEngineTestable) x.GetRequiredService<ISubscriptionEngine>());

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

        _datetime.UtcNow = new DateTime(2023, 1, 1);

        var subscriptionService = _services.GetRequiredService<ISubscriptionService>();
        var id = await subscriptionService.SubscribeAsync(emailAddress, cabId, Frequency.Daily);
        var id2 = await subscriptionService.SubscribeAsync(emailAddress, cabId, Frequency.Daily);
        Assert.That(id, Is.EqualTo(id2), "There should only ever be one subscription per email address per cab");

        var isSubscribed = await subscriptionService.IsSubscribedAsync(emailAddress, cabId);
        Assert.IsTrue(isSubscribed, "Given I am definitely subscribed here, this should be TRUE");

        // Push a fake CAB update message
        _cabUpdatesReceiver.Push(new Core.Integration.CabUpdates.CabUpdateMessage { CabId = cabId, Name = "KHD KAB!" });

        var result = await _services.GetRequiredService<ISubscriptionEngineTestable>().ProcessSingleCabUpdateAsync();
        Assert.IsTrue(result, "The cab update message should have been processed.");
    }
}