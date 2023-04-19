using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using UKMCAB.Subscriptions.Core.Common.Security.Tokens;
using UKMCAB.Subscriptions.Core.Data;
using UKMCAB.Subscriptions.Core.Domain;
using UKMCAB.Subscriptions.Core.Integration.CabService;
using UKMCAB.Subscriptions.Core.Integration.OutboundEmail;
using UKMCAB.Subscriptions.Core.Services;

namespace UKMCAB.Subscriptions.Core;

public static class SubscriptionsCoreServiceCollectionExtensions
{
    public class Configuration
    {
        public IDateTimeProvider? DateTimeProvider { get; set; }
        public IOutboundEmailSender? OutboundEmailSender { get; set; }
        public ICabService? CabService { get; set; }
    }

    public static IServiceCollection AddSubscriptionServices(this IServiceCollection services, SubscriptionServicesCoreOptions options, Action<Configuration>? configurator = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var config = new Configuration();
        configurator?.Invoke(config);

        services.AddSingleton(options);

        services.AddSingleton(new AzureDataConnectionString(options.DataConnectionString ?? throw new Exception($"{nameof(options)}.{nameof(options.DataConnectionString)} is null")));

        // repositories
        services.AddSingleton<IOutboxRepository, OutboxRepository>();
        services.AddSingleton<ISubscriptionRepository, SubscriptionRepository>();
        services.AddSingleton<ISentNotificationRepository, SentNotificationRepository>();
        services.AddSingleton<IBlockedEmailsRepository, BlockedEmailsRepository>();
        services.AddSingleton<ITelemetryRepository, TelemetryRepository>();
        services.AddSingleton<IRepositories, Repositories>();
        
        // configurable dependencies
        services.AddSingleton(config.DateTimeProvider ?? new RealDateTimeProvider());
        services.AddSingleton(config.CabService ?? new CabService(options.CabApiOptions ?? throw new Exception($"{nameof(options)}.{nameof(options.CabApiOptions)} is null")));
        services.AddSingleton(config.OutboundEmailSender ?? new OutboundEmailSender(options.GovUkNotifyApiKey ?? throw new Exception($"{nameof(options)}.{nameof(options.GovUkNotifyApiKey)} is null")));

        var jsonSerializerOptions = new JsonSerializerOptions();
        jsonSerializerOptions.Converters.Add(new EmailAddressConverter());
        services.AddSingleton<ISecureTokenProcessor>(new SecureTokenProcessor(options.EncryptionKey ?? throw new Exception($"{nameof(options)}.{nameof(options.EncryptionKey)} is null"), jsonSerializerOptions));

        // the main consumable services
        services.AddSingleton<ISubscriptionEngine, SubscriptionEngine>();
        services.AddSingleton<ISubscriptionService, SubscriptionService>();

        return services;
    }
}
