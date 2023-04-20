using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
    public static IServiceCollection AddSubscriptionServices(this IServiceCollection services, SubscriptionsCoreServicesOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton(options);

        services.AddSingleton(new AzureDataConnectionString(options.DataConnectionString ?? throw new Exception($"{nameof(options)}.{nameof(options.DataConnectionString)} is null")));

        // repositories
        services.AddSingleton<ISubscriptionRepository, SubscriptionRepository>();
        services.AddSingleton<IBlockedEmailsRepository, BlockedEmailsRepository>();
        services.AddSingleton<ITelemetryRepository, TelemetryRepository>();
        services.AddSingleton<IRepositories, Repositories>();

        // configurable dependencies
        services.TryAddSingleton<ICabService>(x => new CabApiService(options.CabApiOptions ?? throw new Exception($"{nameof(options)}.{nameof(options.CabApiOptions)} is null")));
        services.TryAddSingleton<IDateTimeProvider>(x => new RealDateTimeProvider());
        services.TryAddSingleton<IOutboundEmailSender>(x => new OutboundEmailSender(options.GovUkNotifyApiKey ?? throw new Exception($"{nameof(options)}.{nameof(options.GovUkNotifyApiKey)} is null")));

        var jsonSerializerOptions = new JsonSerializerOptions();
        jsonSerializerOptions.Converters.Add(new EmailAddressConverter());
        services.AddSingleton<ISecureTokenProcessor>(new SecureTokenProcessor(options.EncryptionKey ?? throw new Exception($"{nameof(options)}.{nameof(options.EncryptionKey)} is null"), jsonSerializerOptions));

        // the main consumable services
        services.AddSingleton<ISubscriptionEngine, SubscriptionEngine>();
        services.AddSingleton<ISubscriptionService, SubscriptionService>();

        return services;
    }
}
