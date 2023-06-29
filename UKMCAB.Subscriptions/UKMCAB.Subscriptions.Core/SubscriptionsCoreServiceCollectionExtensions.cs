using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text.Json;
using UKMCAB.Subscriptions.Core;
using UKMCAB.Subscriptions.Core.Common;
using UKMCAB.Subscriptions.Core.Common.Security.Tokens;
using UKMCAB.Subscriptions.Core.Data;
using UKMCAB.Subscriptions.Core.Domain;
using UKMCAB.Subscriptions.Core.Domain.Emails;
using UKMCAB.Subscriptions.Core.Integration.CabService;
using UKMCAB.Subscriptions.Core.Integration.OutboundEmail;
using UKMCAB.Subscriptions.Core.Services;

public static class SubscriptionsCoreServiceCollectionExtensions
{
    /// <summary>
    /// Adds Subscriptions Core services
    /// </summary>
    /// <param name="services">The services collection</param>
    /// <param name="options">The core options</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static IServiceCollection AddSubscriptionsCoreServices(this IServiceCollection services, SubscriptionsCoreServicesOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton(options);

        services.AddSingleton(new AzureDataConnectionString(options.DataConnectionString ?? throw new Exception($"{nameof(options)}.{nameof(options.DataConnectionString)} is null")));

        // repositories
        services.AddSingleton<ISubscriptionRepository, SubscriptionRepository>();
        services.AddSingleton<IBlockedEmailsRepository, BlockedEmailsRepository>();
        services.AddSingleton<ITelemetryRepository, TelemetryRepository>();
        services.AddSingleton<IRepositories, Repositories>();

        services.AddSingleton<IEmailTemplatesService>(x => new EmailTemplatesService(options.EmailTemplates, options.UriTemplateOptions));

        // configurable dependencies
        services.TryAddSingleton<ICabService>(x => new CabApiService(options.CabApiOptions ?? throw new Exception($"{nameof(options)}.{nameof(options.CabApiOptions)} is null")));
        services.TryAddSingleton<IDateTimeProvider>(x => new RealDateTimeProvider());
        services.TryAddSingleton<IOutboundEmailSender>(x => new OutboundEmailSender(options.GovUkNotifyApiKey ?? throw new Exception($"{nameof(options)}.{nameof(options.GovUkNotifyApiKey)} is null"), options.OutboundEmailSenderMode));

        var jsonSerializerOptions = new JsonSerializerOptions();
        jsonSerializerOptions.Converters.Add(new EmailAddressConverter());
        services.AddSingleton<ISecureTokenProcessor>(new SecureTokenProcessor(options.EncryptionKey ?? throw new Exception($"{nameof(options)}.{nameof(options.EncryptionKey)} is null"), jsonSerializerOptions));

        // the main consumable services
        services.AddSingleton<ISubscriptionEngine, SubscriptionEngine>();
        services.AddSingleton<ISubscriptionService, SubscriptionService>();

        return services;
    }
}
