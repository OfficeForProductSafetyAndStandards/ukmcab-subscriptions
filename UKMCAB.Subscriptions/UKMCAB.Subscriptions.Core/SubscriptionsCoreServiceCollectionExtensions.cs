using Azure.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using UKMCAB.Subscriptions.Core.Common.Security.Tokens;
using UKMCAB.Subscriptions.Core.Data;
using UKMCAB.Subscriptions.Core.Domain;
using UKMCAB.Subscriptions.Core.Integration.CabUpdates;
using UKMCAB.Subscriptions.Core.Integration.OutboundEmail;
using UKMCAB.Subscriptions.Core.Integration.Search;
using UKMCAB.Subscriptions.Core.Services;

namespace UKMCAB.Subscriptions.Core;

public static class SubscriptionsCoreServiceCollectionExtensions
{
    public class Configuration
    {
        public IDateTimeProvider? DateTimeProvider { get; set; }
        public IOutboundEmailSender? OutboundEmailSender { get; set; }
    }

    public static IServiceCollection AddSubscriptionServices(this IServiceCollection services, SubscriptionServicesCoreOptions options, Action<Configuration>? configurator = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var config = new Configuration();
        configurator?.Invoke(config);

        services.AddSingleton(options);

        services.AddSingleton(new AzureDataConnectionString(options.DataConnectionString));

        // repositories
        services.AddSingleton<IOutboxRepository, OutboxRepository>();
        services.AddSingleton<ISubscriptionRepository, SubscriptionRepository>();
        services.AddSingleton<ISentNotificationRepository, SentNotificationRepository>();
        services.AddSingleton<IBlockedEmailsRepository, BlockedEmailsRepository>();
        services.AddSingleton<ITelemetryRepository, TelemetryRepository>();
        services.AddSingleton<IRepositories, Repositories>();
        
        // integration dependency services
        services.AddSingleton<ICabSearchService, CabSearchService>();
        services.AddSingleton<ICabUpdatesReceiver, CabUpdatesReceiver>();
        
        // date time provider
        services.AddSingleton(config.DateTimeProvider ?? new RealDateTimeProvider());

        services.AddSingleton(config.OutboundEmailSender ?? new OutboundEmailSender(options.GovUkNotifyApiKey));

        var jsonSerializerOptions = new JsonSerializerOptions();
        jsonSerializerOptions.Converters.Add(new EmailAddressConverter());
        services.AddSingleton<ISecureTokenProcessor>(new SecureTokenProcessor(options.EncryptionKey, jsonSerializerOptions));

        // the main consumable services
        services.AddSingleton<ISubscriptionEngine, SubscriptionEngine>();
        services.AddSingleton<ISubscriptionService, SubscriptionService>();

        return services;
    }
}
