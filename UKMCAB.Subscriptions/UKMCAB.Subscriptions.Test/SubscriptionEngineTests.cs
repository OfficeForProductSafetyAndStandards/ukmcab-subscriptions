using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UKMCAB.Subscriptions.Core;
using UKMCAB.Subscriptions.Core.Domain;
using UKMCAB.Subscriptions.Core.Integration.CabService;
using UKMCAB.Subscriptions.Core.Services;
using UKMCAB.Subscriptions.Test.Fakes;
using static UKMCAB.Subscriptions.Core.Services.SubscriptionService;

namespace UKMCAB.Subscriptions.Test;

[TestFixture]
public class SubscriptionEngineTests
{
    private FakeDateTimeProvider _datetime = new();
    private FakeCabService _cabService;
    private FakeOutboundEmailSender _outboundEmailSender = new();
    private ServiceProvider _services;
    private ISubscriptionService _subs;
    private ISubscriptionEngine _eng;
    private const string _fakeConfirmationUrl = "http://test.com/?payload=@payload";

    [OneTimeSetUp]
    public void SetupOnce()
    {
        Bootstrap.Configuration.Bind(SubscriptionServiceTests.CoreOptions);

        _cabService = new FakeCabService();

        var services = new ServiceCollection().AddLogging().AddSubscriptionServices(SubscriptionServiceTests.CoreOptions, x =>
        {
            x.DateTimeProvider = _datetime;
            x.OutboundEmailSender = _outboundEmailSender;
            x.CabService = _cabService;
        });

        _services = services.BuildServiceProvider();
        _subs = _services.GetRequiredService<ISubscriptionService>();
        _eng = _services.GetRequiredService<ISubscriptionEngine>();
    }

    [SetUp]
    public async Task SetupAsync()
    {
        await (_subs as IClearable ?? throw new Exception("hold on a mo, cannot cast _subs to IClearable")).ClearDataAsync();
        await (_eng as IClearable ?? throw new Exception("hold on a mo, cannot cast _eng to IClearable")).ClearDataAsync();

        _outboundEmailSender.Requests.Clear();
    }


    [Test]
    public async Task ProcessSearchSubscribers_RealtimeFrequency()
    {
        _datetime.UtcNow = new DateTime(1980, 7, 1);
        await SubscribeToSearchAsync("test@test.com", "?name=bob", Frequency.Realtime);

        // seed results
        _cabService.Inject(new CabService.SearchResults(1, new List<CabSearchApiModel> { new CabSearchApiModel { CabId = Guid.NewGuid(), Name = "Bob" } }));
        var result = await _eng.ProcessAsync(CancellationToken.None).ConfigureAwait(false);
        Assert.Multiple(() =>
        {
            Assert.That(_outboundEmailSender.Requests.Count, Is.EqualTo(0));
            Assert.That(result.Notified, Is.EqualTo(0));
            Assert.That(result.Initialised, Is.EqualTo(1));
            Assert.That(result.Errors, Is.EqualTo(0));
            Assert.That(result.NoChange, Is.EqualTo(0));
            Assert.That(result.NotDue, Is.EqualTo(0));
        });

        // process again (nothing has changed)
        result = await _eng.ProcessAsync(CancellationToken.None).ConfigureAwait(false);
        Assert.Multiple(() =>
        {
            Assert.That(_outboundEmailSender.Requests.Count, Is.EqualTo(0));
            Assert.That(result.Notified, Is.EqualTo(0));
            Assert.That(result.Initialised, Is.EqualTo(0));
            Assert.That(result.Errors, Is.EqualTo(0));
            Assert.That(result.NoChange, Is.EqualTo(1));
            Assert.That(result.NotDue, Is.EqualTo(0));
        });

        // change results
        _cabService.Inject(new CabService.SearchResults(1, new List<CabSearchApiModel> { new CabSearchApiModel { CabId = Guid.NewGuid(), Name = "Bob2" } }));
        result = await _eng.ProcessAsync(CancellationToken.None).ConfigureAwait(false);
        Assert.Multiple(() =>
        {
            Assert.That(_outboundEmailSender.Requests.Count, Is.EqualTo(1), "An email notification should have been sent");
            Assert.That(result.Notified, Is.EqualTo(1));
            Assert.That(result.Initialised, Is.EqualTo(0));
            Assert.That(result.Errors, Is.EqualTo(0));
            Assert.That(result.NoChange, Is.EqualTo(0));
            Assert.That(result.NotDue, Is.EqualTo(0));
        });
        _outboundEmailSender.Requests.Clear();


        // process again (nothing has changed)
        result = await _eng.ProcessAsync(CancellationToken.None).ConfigureAwait(false);
        Assert.Multiple(() =>
        {
            Assert.That(_outboundEmailSender.Requests.Count, Is.EqualTo(0));
            Assert.That(result.Notified, Is.EqualTo(0));
            Assert.That(result.Initialised, Is.EqualTo(0));
            Assert.That(result.Errors, Is.EqualTo(0));
            Assert.That(result.NoChange, Is.EqualTo(1));
            Assert.That(result.NotDue, Is.EqualTo(0));
        });
    }


    [Test]
    public async Task ProcessSearchSubscribers_DailyFrequency()
    {
        _datetime.UtcNow = new DateTime(1980, 7, 1);
        await SubscribeToSearchAsync("test@test.com", "?name=bob", Frequency.Daily);

        _cabService.Inject(new CabService.SearchResults(1, new List<CabSearchApiModel> { new CabSearchApiModel { CabId = Guid.NewGuid(), Name = "Bob" } }));
        var result = await _eng.ProcessAsync(CancellationToken.None).ConfigureAwait(false);
        Assertions(result, init: 1);
        
        result = await _eng.ProcessAsync(CancellationToken.None).ConfigureAwait(false);
        Assertions(result, notdue: 1);

        // change results...
        _cabService.Inject(new CabService.SearchResults(1, new List<CabSearchApiModel> { new CabSearchApiModel { CabId = Guid.NewGuid(), Name = "Bob2" } }));
        
        result = await _eng.ProcessAsync(CancellationToken.None).ConfigureAwait(false);
        Assertions(result, notdue: 1);

        // move time fwd...
        _datetime.UtcNow = _datetime.UtcNow.AddDays(1).AddMinutes(10);

        result = await _eng.ProcessAsync(CancellationToken.None).ConfigureAwait(false);
        Assertions(result, notified: 1, emailSentCount: 1);

        result = await _eng.ProcessAsync(CancellationToken.None).ConfigureAwait(false);
        Assertions(result, notdue: 1);

        // move time fwd...
        _datetime.UtcNow = _datetime.UtcNow.AddDays(1).AddMinutes(10);

        result = await _eng.ProcessAsync(CancellationToken.None).ConfigureAwait(false);
        Assertions(result, nochange: 1);


        void Assertions(SubscriptionEngine.ResultAccumulator result, int emailSentCount = 0, int notified = 0, int init = 0, int err = 0, int nochange = 0, int notdue = 0)
        {
            Assert.Multiple(() =>
            {
                Assert.That(_outboundEmailSender.Requests.Count, Is.EqualTo(emailSentCount), "emailSentCount incorrect");
                Assert.That(result.Notified, Is.EqualTo(notified), "notified count incorrect");
                Assert.That(result.Initialised, Is.EqualTo(init), "initialised count incorrect");
                Assert.That(result.Errors, Is.EqualTo(err), "err count incorrect");
                Assert.That(result.NoChange, Is.EqualTo(nochange), "nochange count incorrect");
                Assert.That(result.NotDue, Is.EqualTo(notdue), "notdue  count incorrect");
                _outboundEmailSender.Requests.Clear();
            });
        }
    }

    [Test]
    public async Task ProcessSearchSubscribers_WeeklyFrequency()
    {
        _datetime.UtcNow = new DateTime(1980, 7, 1);
        await SubscribeToSearchAsync("test@test.com", "?name=bob", Frequency.Weekly);

        _cabService.Inject(new CabService.SearchResults(1, new List<CabSearchApiModel> { new CabSearchApiModel { CabId = Guid.NewGuid(), Name = "Bob" } }));
        var result = await _eng.ProcessAsync(CancellationToken.None).ConfigureAwait(false);
        Assertions(result, init: 1);

        result = await _eng.ProcessAsync(CancellationToken.None).ConfigureAwait(false);
        Assertions(result, notdue: 1);

        // change results...
        _cabService.Inject(new CabService.SearchResults(1, new List<CabSearchApiModel> { new CabSearchApiModel { CabId = Guid.NewGuid(), Name = "Bob2" } }));

        result = await _eng.ProcessAsync(CancellationToken.None).ConfigureAwait(false);
        Assertions(result, notdue: 1);

        // move time fwd... JUST BY A DAY
        _datetime.UtcNow = _datetime.UtcNow.AddDays(1).AddMinutes(10);


        result = await _eng.ProcessAsync(CancellationToken.None).ConfigureAwait(false);
        Assertions(result, notdue: 1);


        // move time fwd by a WEEK... 
        _datetime.UtcNow = _datetime.UtcNow.AddDays(7).AddMinutes(10);

        result = await _eng.ProcessAsync(CancellationToken.None).ConfigureAwait(false);
        Assertions(result, notified: 1, emailSentCount: 1);

        result = await _eng.ProcessAsync(CancellationToken.None).ConfigureAwait(false);
        Assertions(result, notdue: 1);

        // move time fwd...
        _datetime.UtcNow = _datetime.UtcNow.AddDays(8).AddMinutes(10);

        result = await _eng.ProcessAsync(CancellationToken.None).ConfigureAwait(false);
        Assertions(result, nochange: 1);


        void Assertions(SubscriptionEngine.ResultAccumulator result, int emailSentCount = 0, int notified = 0, int init = 0, int err = 0, int nochange = 0, int notdue = 0)
        {
            Assert.Multiple(() =>
            {
                Assert.That(_outboundEmailSender.Requests.Count, Is.EqualTo(emailSentCount), "emailSentCount incorrect");
                Assert.That(result.Notified, Is.EqualTo(notified), "notified count incorrect");
                Assert.That(result.Initialised, Is.EqualTo(init), "initialised count incorrect");
                Assert.That(result.Errors, Is.EqualTo(err), "err count incorrect");
                Assert.That(result.NoChange, Is.EqualTo(nochange), "nochange count incorrect");
                Assert.That(result.NotDue, Is.EqualTo(notdue), "notdue  count incorrect");
                _outboundEmailSender.Requests.Clear();
            });
        }
    }

    private async Task SubscribeToSearchAsync(string email, string query, Frequency frequency)
    {
        var req = new SearchSubscriptionRequest(email, query, frequency);

        var requestSubscriptionResult = await _subs.RequestSubscriptionAsync(req, _fakeConfirmationUrl);
        Assert.That(requestSubscriptionResult.ValidationResult, Is.EqualTo(ValidationResult.Success));

        var confirmSubscriptionResult = await _subs.ConfirmSearchSubscriptionAsync(requestSubscriptionResult.Token);
        Assert.Multiple(() =>
        {
            Assert.That(confirmSubscriptionResult.ValidationResult, Is.EqualTo(ValidationResult.Success));
            Assert.That(confirmSubscriptionResult.Id, Is.Not.Null);
        });

        _outboundEmailSender.Requests.Clear();
    }
}
