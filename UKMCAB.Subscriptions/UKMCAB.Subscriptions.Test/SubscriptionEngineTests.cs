using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UKMCAB.Subscriptions.Core;
using UKMCAB.Subscriptions.Core.Domain;
using UKMCAB.Subscriptions.Core.Domain.Emails;
using UKMCAB.Subscriptions.Core.Integration.CabService;
using UKMCAB.Subscriptions.Core.Integration.OutboundEmail;
using UKMCAB.Subscriptions.Core.Services;
using UKMCAB.Subscriptions.Test.Fakes;
using static UKMCAB.Subscriptions.Core.Services.SubscriptionService;

namespace UKMCAB.Subscriptions.Test;

[TestFixture]
public class SubscriptionEngineTests
{
    private FakeDateTimeProvider _datetime = new();
    private FakeCabService _cabService = new();
    private ServiceProvider _services;
    private ISubscriptionService _subs;
    private ISubscriptionEngine _eng;
    private IOutboundEmailSender _outboundEmailSender;
    
    [OneTimeSetUp]
    public void SetupOnce()
    {
        Bootstrap.Configuration.Bind(SubscriptionServiceTests.CoreOptions);

        var services = new ServiceCollection().AddLogging();
        services.AddSingleton<IDateTimeProvider>(_datetime);
        services.AddSingleton<ICabService>(_cabService);

        services.AddSubscriptionsCoreServices(SubscriptionServiceTests.CoreOptions);

        _services = services.BuildServiceProvider();
        _subs = _services.GetRequiredService<ISubscriptionService>();
        _eng = _services.GetRequiredService<ISubscriptionEngine>();

        _outboundEmailSender = _services.GetRequiredService<IOutboundEmailSender>();
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
        _cabService.Inject(new CabApiService.SearchResults(1, new List<SubscriptionsCoreCabSearchResultModel> { new SubscriptionsCoreCabSearchResultModel { CabId = Guid.NewGuid(), Name = "Bob" } }));
        var result = await _eng.ProcessAsync(CancellationToken.None).ConfigureAwait(false);
        Assert.Multiple(() =>
        {
            Assert.That(_outboundEmailSender.Requests.Count, Is.EqualTo(1));
            Assert.That(result.Notified, Is.EqualTo(0));
            Assert.That(result.Initialised, Is.EqualTo(1));
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

        // change results
        _cabService.Inject(new CabApiService.SearchResults(1, new List<SubscriptionsCoreCabSearchResultModel> { new SubscriptionsCoreCabSearchResultModel { CabId = Guid.NewGuid(), Name = "Bob2" } }));
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
    public async Task ProcessSearchSubscribers_RealtimeFrequency_SpacesInUrl()
    {
        _datetime.UtcNow = new DateTime(1980, 7, 1);
        await SubscribeToSearchAsync("test@test.com", "?Keywords=&RegisteredOfficeLocations=United+Kingdom", Frequency.Realtime);

        // seed results
        _cabService.Inject(new CabApiService.SearchResults(1, new List<SubscriptionsCoreCabSearchResultModel> { new SubscriptionsCoreCabSearchResultModel { CabId = Guid.NewGuid(), Name = "Bob" } }));
        var result = await _eng.ProcessAsync(CancellationToken.None).ConfigureAwait(false);
        Assert.Multiple(() =>
        {
            Assert.That(_outboundEmailSender.Requests.Count, Is.EqualTo(1));
            Assert.That(result.Notified, Is.EqualTo(0));
            Assert.That(result.Initialised, Is.EqualTo(1));
            Assert.That(result.Errors, Is.EqualTo(0));
            Assert.That(result.NoChange, Is.EqualTo(0));
            Assert.That(result.NotDue, Is.EqualTo(0));
        });
        _outboundEmailSender.Requests.Clear();

        // change results
        _cabService.Inject(new CabApiService.SearchResults(1, new List<SubscriptionsCoreCabSearchResultModel> { new SubscriptionsCoreCabSearchResultModel { CabId = Guid.NewGuid(), Name = "Bob2" } }));
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

        var last = _outboundEmailSender.Requests.LastOrDefault();

        var viewSearchResultsLink = last.Replacements[EmailPlaceholders.ViewSearchLink];

        Assert.That(viewSearchResultsLink, Does.Not.Contain(" "));
    }


    [Test]
    public async Task ProcessSearchSubscribers_DailyFrequency()
    {
        _datetime.UtcNow = new DateTime(1980, 7, 1);
        await SubscribeToSearchAsync("test@test.com", "?name=bob", Frequency.Daily);

        _cabService.Inject(new CabApiService.SearchResults(1, new List<SubscriptionsCoreCabSearchResultModel> { new SubscriptionsCoreCabSearchResultModel { CabId = Guid.NewGuid(), Name = "Bob" } }));
        var result = await _eng.ProcessAsync(CancellationToken.None).ConfigureAwait(false);
        Assertions(result, init: 1, emailSentCount: 1);
        
        result = await _eng.ProcessAsync(CancellationToken.None).ConfigureAwait(false);
        Assertions(result, notdue: 1);

        // change results...
        _cabService.Inject(new CabApiService.SearchResults(1, new List<SubscriptionsCoreCabSearchResultModel> { new SubscriptionsCoreCabSearchResultModel { CabId = Guid.NewGuid(), Name = "Bob2" } }));
        
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

        _cabService.Inject(new CabApiService.SearchResults(1, new List<SubscriptionsCoreCabSearchResultModel> { new SubscriptionsCoreCabSearchResultModel { CabId = Guid.NewGuid(), Name = "Bob" } }));
        var result = await _eng.ProcessAsync(CancellationToken.None).ConfigureAwait(false);
        Assertions(result, init: 1, emailSentCount: 1);
        _outboundEmailSender.Requests.Clear();

        result = await _eng.ProcessAsync(CancellationToken.None).ConfigureAwait(false);
        Assertions(result, notdue: 1);

        // change results...
        _cabService.Inject(new CabApiService.SearchResults(1, new List<SubscriptionsCoreCabSearchResultModel> { new SubscriptionsCoreCabSearchResultModel { CabId = Guid.NewGuid(), Name = "Bob2" } }));

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




    [Test]
    public async Task ProcessCabSubscribers_RealtimeFrequency()
    {
        _datetime.UtcNow = new DateTime(1980, 7, 1);

        const string e = "john@cab.com";
        var cabId = Guid.NewGuid();

        var requestSubscriptionResult = await _subs.RequestSubscriptionAsync(new CabSubscriptionRequest(e, cabId, Frequency.Realtime));
        Assert.That(requestSubscriptionResult.ValidationResult, Is.EqualTo(ValidationResult.Success));

        var confirmSubscriptionResult = await _subs.ConfirmCabSubscriptionAsync(requestSubscriptionResult.Token ?? throw new Exception("Token should not be null"));
        Assert.That(confirmSubscriptionResult.ValidationResult, Is.EqualTo(ValidationResult.Success));
        Assert.That(confirmSubscriptionResult.SubscriptionId, Is.Not.Null);
        _outboundEmailSender.Requests.Clear();


        // seed results
        _cabService.Inject(new SubscriptionsCoreCabModel { CABId = cabId.ToString(), Name = "Bob" });
        var result = await _eng.ProcessAsync(CancellationToken.None).ConfigureAwait(false);
        Assert.Multiple(() =>
        {
            Assert.That(_outboundEmailSender.Requests.Count, Is.EqualTo(1)); // subscribed email notification sent.
            Assert.That(result.Notified, Is.EqualTo(0));
            Assert.That(result.Initialised, Is.EqualTo(1));
            Assert.That(result.Errors, Is.EqualTo(0));
            Assert.That(result.NoChange, Is.EqualTo(0));
            Assert.That(result.NotDue, Is.EqualTo(0));
        });
        Assert.That(_outboundEmailSender.Requests.Last().Replacements.GetValueOrDefault(EmailPlaceholders.CabName), Is.EqualTo("Bob"));
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

        // change results
        _cabService.Inject(new SubscriptionsCoreCabModel { CABId = cabId.ToString(), Name = "Bob2" });
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
        Assert.That(_outboundEmailSender.Requests.Last().Replacements.GetValueOrDefault(EmailPlaceholders.CabName), Is.EqualTo("Bob2"));
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
    public async Task ProcessSearchSubscribers_Realtime_ChangesSummary()
    {
        _datetime.UtcNow = new DateTime(1980, 7, 1);
        await SubscribeToSearchAsync("test@test.com", "?name=bob", Frequency.Realtime);

        var results = new
        {
            bob = new SubscriptionsCoreCabSearchResultModel { CabId = Guid.NewGuid(), Name = "Bob" },
            rob = new SubscriptionsCoreCabSearchResultModel { CabId = Guid.NewGuid(), Name = "Rob" },
            sid = new SubscriptionsCoreCabSearchResultModel { CabId = Guid.NewGuid(), Name = "Sid" },
        };

        var resultList = new List<SubscriptionsCoreCabSearchResultModel>(new[] { results.bob, results.rob });

        // seed results
        _cabService.Inject(new CabApiService.SearchResults(1, resultList));
        var result = await _eng.ProcessAsync(CancellationToken.None).ConfigureAwait(false);
        Assert.Multiple(() =>
        {
            Assert.That(_outboundEmailSender.Requests.Count, Is.EqualTo(1));
            Assert.That(result.Initialised, Is.EqualTo(1));
        });
        _outboundEmailSender.Requests.Clear();


        // change results
        resultList.Remove(results.bob);
        resultList.Add(results.sid);
        results.rob.Name = "Robert";

        
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

        var cdid = _outboundEmailSender.GetLastMetaItem("changeDescriptorId");
        Assert.That(cdid, Is.Not.Null); 

        var changes = await _subs.GetSearchResultsChangesAsync(cdid).ConfigureAwait(false);
        Assert.That(changes.Added.Count, Is.EqualTo(1), "One item should have been added (sid)");  
        Assert.That(changes.Removed.Count, Is.EqualTo(1), "One item should have been removed (bob)");  
        Assert.That(changes.Modified.Count, Is.EqualTo(1), "One item should have been changed (rob->Robert)");  

        _outboundEmailSender.Requests.Clear();
    }

    [Test]
    [TestCase("123456789", "UKMCAB search results for '123456789'")]
    [TestCase(null, "UKMCAB search results")]
    public async Task ProcessSearchSubscribers_Realtime_SearchTopicNameIsCorrect(string keywords, string expectedSearchTopicName)
    {
        _datetime.UtcNow = new DateTime(1980, 7, 1);
        await SubscribeToSearchAsync("test@test.com", "?name=bob", Frequency.Realtime, keywords);

        var results = new
        {
            bob = new SubscriptionsCoreCabSearchResultModel { CabId = Guid.NewGuid(), Name = "Bob" },
            rob = new SubscriptionsCoreCabSearchResultModel { CabId = Guid.NewGuid(), Name = "Rob" },
            sid = new SubscriptionsCoreCabSearchResultModel { CabId = Guid.NewGuid(), Name = "Sid" },
        };

        var resultList = new List<SubscriptionsCoreCabSearchResultModel>(new[] { results.bob, results.rob });

        // seed results
        _cabService.Inject(new CabApiService.SearchResults(1, resultList));
        var result = await _eng.ProcessAsync(CancellationToken.None).ConfigureAwait(false);
        Assert.Multiple(() =>
        {
            Assert.That(_outboundEmailSender.Requests.Count, Is.EqualTo(1));
            Assert.That(result.Initialised, Is.EqualTo(1));
            Assert.That(_outboundEmailSender.Requests.First().Replacements.GetValueOrDefault(EmailPlaceholders.SearchTopicName), Is.EqualTo(expectedSearchTopicName));
        });
        _outboundEmailSender.Requests.Clear();


        // change results
        resultList.Remove(results.bob);
        resultList.Add(results.sid);
        results.rob.Name = "Robert";

        // process subscriptions
        result = await _eng.ProcessAsync(CancellationToken.None).ConfigureAwait(false);
        Assert.Multiple(() =>
        {
            Assert.That(_outboundEmailSender.Requests.Count, Is.EqualTo(1), "An email notification should have been sent");
            Assert.That(result.Notified, Is.EqualTo(1));
            Assert.That(_outboundEmailSender.Requests.First().Replacements.GetValueOrDefault(EmailPlaceholders.SearchTopicName), Is.EqualTo(expectedSearchTopicName));
        });
        _outboundEmailSender.Requests.Clear();
    }


    private async Task SubscribeToSearchAsync(string email, string query, Frequency frequency, string keywords = "test")
    {
        var req = new SearchSubscriptionRequest(email, query, frequency, keywords);

        var requestSubscriptionResult = await _subs.RequestSubscriptionAsync(req);
        Assert.That(requestSubscriptionResult.ValidationResult, Is.EqualTo(ValidationResult.Success));

        var confirmSubscriptionResult = await _subs.ConfirmSearchSubscriptionAsync(requestSubscriptionResult.Token);
        Assert.Multiple(() =>
        {
            Assert.That(confirmSubscriptionResult.ValidationResult, Is.EqualTo(ValidationResult.Success));
            Assert.That(confirmSubscriptionResult.SubscriptionId, Is.Not.Null);
        });

        _outboundEmailSender.Requests.Clear();
    }
}
