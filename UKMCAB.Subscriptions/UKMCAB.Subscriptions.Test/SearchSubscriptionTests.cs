using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UKMCAB.Subscriptions.Core;
using UKMCAB.Subscriptions.Core.Domain;
using UKMCAB.Subscriptions.Core.Services;
using UKMCAB.Subscriptions.Test.Fakes;
using static UKMCAB.Subscriptions.Core.Services.SubscriptionService;

namespace UKMCAB.Subscriptions.Test;

public class SearchSubscriptionTests
{
    private FakeDateTimeProvider _datetime = new();
    private FakeCabSearchService _searchService;
    private FakeCabUpdatesReceiver _cabUpdatesReceiver;
    private FakeOutboundEmailSender _outboundEmailSender = new();
    private SubscriptionServicesCoreOptions _options = new (){  SearchQueryStringPagingKeys = new[] { "page", "pagesize" } };
    private ServiceProvider _services;
    private const string _fakeConfirmationUrl = "http://test.com/?payload=@payload";

    [OneTimeSetUp]
    public void SetupOnce()
    {
        Bootstrap.Configuration.Bind(_options);

        _searchService = new FakeCabSearchService();
        _cabUpdatesReceiver = new FakeCabUpdatesReceiver();

        var services = new ServiceCollection().AddLogging().AddSubscriptionServices(_options, x =>
        {
            x.DateTimeProvider = _datetime;
            x.OutboundEmailSender = _outboundEmailSender;
        });

        services.AddSingleton(x => (ISubscriptionEngineTestable) x.GetRequiredService<ISubscriptionEngine>());

        _services = services.BuildServiceProvider();
    }

    [SetUp]
    public async Task SetupAsync()
    {
        var ds = _services.GetRequiredService<ISubscriptionService>() as ISubscriptionDataService ?? throw new Exception("hold on a minute, cannot cast to ISubscriptionDataService!");
        await ds.DeleteAllAsync("yes_i_really_want_to_delete_everything");
    }

    [TestCase("?&&&&name=test&&&")]
    [TestCase("?&&&&&&&")]
    [TestCase("?")]
    [TestCase("")]
    [TestCase(null)]
    public async Task Subscribe_Search(string searchQueryString)
    {
        var req = new SearchSubscriptionRequest("test@test.com", searchQueryString, Frequency.Realtime);

        var subs = _services.GetRequiredService<ISubscriptionService>();
        var r1 = await subs.RequestSubscriptionAsync(req, _fakeConfirmationUrl);
        Assert.That(r1.ValidationResult,Is.EqualTo(ValidationResult.Success));

        var confirmationPayload = _outboundEmailSender.GetLastPayload();
        var r2 = await subs.ConfirmSearchSubscriptionAsync(confirmationPayload);
        Assert.That(r2.ValidationResult, Is.EqualTo(ValidationResult.Success));
        Assert.That(r2.Id, Is.Not.Null);

        var r3 = await subs.ConfirmSearchSubscriptionAsync(confirmationPayload);
        Assert.That(r3.ValidationResult, Is.EqualTo(ValidationResult.AlreadySubscribed));
    }

    [Test]
    public async Task Subscribe_Searches()
    {
        const string email = "test@test.co.uk";
        var subs = _services.GetRequiredService<ISubscriptionService>();
        
        await SubscribeSearchAsync(subs, email, "a=1").ConfigureAwait(false);
        await SubscribeSearchAsync(subs, email, "a=2").ConfigureAwait(false);
        await SubscribeSearchAsync(subs, email, "a=3").ConfigureAwait(false);

    }

    [Test]
    [TestCase("?&&&&name=test&&&", "name=test", "?&name=test&")]
    [TestCase(null, "", "   ", "?&&&&&&&&&&")]
    [TestCase("?&&&&&&&&&&", "", "   ", null)]
    [TestCase("a=b&a=c&d=e&f=g&g=h", "?a=b&a=c&d=e&f=g&g=h&", "&d=e&f=g&a=b&a=c&g=h", "&g=h&a=c&d=e&f=g&a=b")]
    [TestCase("a=b&a=c&d=e&f=g&g=h", "?a=b&a=c&d=e&f=g&g=h&", "&d=e&f=g&a=b&a=c&g=h", "&g=h&a=c&d=e&f=g&a=b&page=1&pagesize=10")]
    [TestCase("a=b&a=c&d=e&f=g&g=h", "?a=b&a=c&d=e&f=g&g=h&", "&d=e&f=g&a=b&a=c&g=h", "&g=h&a=c&d=e&f=g&a=b&page=4&pagesize=44", "&g=h&a=c&d=e&f=g&a=b&page=1&pagesize=45")]
    [TestCase("a=b&a=c&d=e&f=g&g=h", "?a=b&a=c&d=e&f=g&g=h&", "&d=e&f=g&a=b&a=c&g=h", "&g=h&a=c&d=e&f=g&a=b&page=18", "&g=h&a=c&d=e&f=g&a=b&page=1")]
    public async Task Subscribe_Search_Already_Subscrbd(string searchQueryString, params string[] equivs)
    {
        var req = new SearchSubscriptionRequest("test@test.com", searchQueryString, Frequency.Realtime);
        var subs = _services.GetRequiredService<ISubscriptionService>();
        var r1 = await subs.RequestSubscriptionAsync(req, _fakeConfirmationUrl);
        Assert.That(r1.ValidationResult, Is.EqualTo(ValidationResult.Success));

        var confirmationPayload = _outboundEmailSender.GetLastPayload();
        var r2 = await subs.ConfirmSearchSubscriptionAsync(confirmationPayload);
        Assert.That(r2.ValidationResult, Is.EqualTo(ValidationResult.Success));
        Assert.That(r2.Id, Is.Not.Null);

        foreach ( var equiv in equivs )
        {
            var reqn = new SearchSubscriptionRequest(req.EmailAddress, equiv, Frequency.Realtime);
            var rn = await subs.RequestSubscriptionAsync(reqn, _fakeConfirmationUrl);
            Assert.That(rn.ValidationResult, Is.EqualTo(ValidationResult.AlreadySubscribed));
        }
    }

    [Test]
    public async Task Unsubscribe()
    {
        var subs = _services.GetRequiredService<ISubscriptionService>();
        
        var confirmSearchSubscriptionResult = await SubscribeSearchAsync(subs, "test@test.com", "");

        var unsubscribeResult = await subs.UnsubscribeAsync(confirmSearchSubscriptionResult.Id);
        Assert.That(unsubscribeResult, Is.True);

        var unsubscribeResult2 = await subs.UnsubscribeAsync(confirmSearchSubscriptionResult.Id);
        Assert.That(unsubscribeResult2, Is.False);
    }

    

    [Test]
    public async Task UnsubscribeAll()
    {
        const string email = "test@test.com";
        var subs = _services.GetRequiredService<ISubscriptionService>();

        var queries = Enumerable.Range(1, 100).Select(x => $"?a={x}").ToArray();

        var tasks = queries.Select(x => SubscribeSearchAsync(subs, email, x)).ToArray();
        await Task.WhenAll(tasks).ConfigureAwait(false);

        Assert.That(tasks.All(x => x.Result.ValidationResult == ValidationResult.Success), Is.True);

        var tasks2 = queries.Select(x => subs.IsSubscribedToSearchAsync(email, x));
        Assert.That(tasks2.All(x => x.Result == true), Is.True);

        await subs.UnsubscribeAsync(tasks.First().Result.Id).ConfigureAwait(false);

        var unsubscribedCount = await subs.UnsubscribeAllAsync(email).ConfigureAwait(false);
        Assert.That(unsubscribedCount, Is.EqualTo(tasks.Length - 1));
    }

    [Test]
    public async Task UpdateEmailAddress()
    {
        var subs = _services.GetRequiredService<ISubscriptionService>();

        // Request subscription
        var requestSubscriptionResult = await subs.RequestSubscriptionAsync(new SearchSubscriptionRequest("test@test.com", "", Frequency.Daily), _fakeConfirmationUrl);
        Assert.That(requestSubscriptionResult.ValidationResult, Is.EqualTo(ValidationResult.Success));
        
        // Confirm subscription
        var confirmSearchSubscriptionResult = await subs.ConfirmSearchSubscriptionAsync(_outboundEmailSender.GetLastPayload());
        Assert.Multiple(() =>
        {
            Assert.That(confirmSearchSubscriptionResult.ValidationResult, Is.EqualTo(ValidationResult.Success));
            Assert.That(confirmSearchSubscriptionResult.Id, Is.Not.Null);
        });

        // Request update email
        await subs.RequestUpdateEmailAddressAsync(new UpdateEmailAddressOptions(confirmSearchSubscriptionResult.Id, "test2@test.com"), _fakeConfirmationUrl);

        // Confirm update email
        var id = await subs.ConfirmUpdateEmailAddressAsync(_outboundEmailSender.GetLastPayload());
        Assert.That(id, Is.Not.Null);

        var isSubscribedOld = await subs.IsSubscribedToSearchAsync("test@test.com", "");
        var isSubscribedNew = await subs.IsSubscribedToSearchAsync("test2@test.com", "");

        Assert.Multiple(() =>
        {
            Assert.That(isSubscribedOld, Is.False);
            Assert.That(isSubscribedNew);
        });
    }


    private async Task<ConfirmSearchSubscriptionResult> SubscribeSearchAsync(ISubscriptionService subs, string email, string query)
    {
        var requestSubscriptionResult = await subs.RequestSubscriptionAsync(new SearchSubscriptionRequest(email, query, Frequency.Daily), _fakeConfirmationUrl);
        Assert.That(requestSubscriptionResult.ValidationResult, Is.EqualTo(ValidationResult.Success));
        var confirmSearchSubscriptionResult = await subs.ConfirmSearchSubscriptionAsync(requestSubscriptionResult.Token);
        Assert.That(confirmSearchSubscriptionResult.ValidationResult, Is.EqualTo(ValidationResult.Success));
        Assert.That(confirmSearchSubscriptionResult.Id, Is.Not.Null);
        return confirmSearchSubscriptionResult;
    }
}
