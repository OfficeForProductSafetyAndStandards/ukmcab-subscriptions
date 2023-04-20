using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text.Json;
using UKMCAB.Subscriptions.Core;
using UKMCAB.Subscriptions.Core.Common;
using UKMCAB.Subscriptions.Core.Common.Security.Tokens;
using UKMCAB.Subscriptions.Core.Data;
using UKMCAB.Subscriptions.Core.Domain;
using UKMCAB.Subscriptions.Core.Integration.CabService;
using UKMCAB.Subscriptions.Core.Integration.OutboundEmail;
using UKMCAB.Subscriptions.Core.Services;

public static class SubscriptionsCoreServiceCollectionExtensions
{
    public class Configuration
    {
        public Func<IServiceProvider, ICabService>? CabServiceFactory { get; set; }
        public Func<IServiceProvider, IOutboundEmailSender>? OutboundEmailSenderFactory { get; set; }
        public Func<IServiceProvider, IDateTimeProvider>? DateTimeProviderFactory { get; set; }
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
        AddCabService(services, options, config);
        AddDateTimeProvider(services, config);
        AddOutboundEmailSender(services, options, config);

        var jsonSerializerOptions = new JsonSerializerOptions();
        jsonSerializerOptions.Converters.Add(new EmailAddressConverter());
        services.AddSingleton<ISecureTokenProcessor>(new SecureTokenProcessor(options.EncryptionKey ?? throw new Exception($"{nameof(options)}.{nameof(options.EncryptionKey)} is null"), jsonSerializerOptions));

        // the main consumable services
        services.AddSingleton<ISubscriptionEngine, SubscriptionEngine>();
        services.AddSingleton<ISubscriptionService, SubscriptionService>();

        return services;
    }

    private static void AddCabService(IServiceCollection services, SubscriptionServicesCoreOptions options, Configuration config)
    {
        if (config.CabServiceFactory != null)
        {
            services.AddSingleton(config.CabServiceFactory);
        }
        else
        {
            services.AddSingleton(new CabApiService(options.CabApiOptions ?? throw new Exception($"{nameof(options)}.{nameof(options.CabApiOptions)} is null")));
        }
    }

    private static void AddDateTimeProvider(IServiceCollection services, Configuration config)
    {
        if (config.DateTimeProviderFactory != null)
        {
            services.AddSingleton(config.DateTimeProviderFactory);
        }
        else
        {
            services.AddSingleton(new RealDateTimeProvider());
        }
    }

    private static void AddOutboundEmailSender(IServiceCollection services, SubscriptionServicesCoreOptions options, Configuration config)
    {
        if (config.OutboundEmailSenderFactory != null)
        {
            services.AddSingleton(config.OutboundEmailSenderFactory);
        }
        else
        {
            services.AddSingleton(new OutboundEmailSender(options.GovUkNotifyApiKey ?? throw new Exception($"{nameof(options)}.{nameof(options.GovUkNotifyApiKey)} is null")));
        }
    }
}
