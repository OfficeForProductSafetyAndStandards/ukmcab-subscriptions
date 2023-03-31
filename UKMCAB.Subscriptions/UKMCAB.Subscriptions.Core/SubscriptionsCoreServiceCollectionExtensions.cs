using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UKMCAB.Subscriptions.Core.Abstract;
using UKMCAB.Subscriptions.Core.Integration.CabUpdates;
using UKMCAB.Subscriptions.Core.Integration.Search;
using UKMCAB.Subscriptions.Core.Repositories;
using UKMCAB.Subscriptions.Core.Services;

namespace UKMCAB.Subscriptions.Core;

public static class SubscriptionsCoreServiceCollectionExtensions
{
    public static IServiceCollection AddSubscriptionServices(this IServiceCollection services, SubscriptionServicesCoreOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton(options);

        // repositories
        services.AddSingleton<IOutboxRepository, OutboxRepository>();
        services.AddSingleton<ISubscriptionRepository, SubscriptionRepository>();
        services.AddSingleton<ISentNotificationRepository, SentNotificationRepository>();
        
        // integration dependency services
        services.AddSingleton<ICabSearchService, CabSearchService>();
        services.AddSingleton<ICabUpdatesReceiver, CabUpdatesReceiver>();
        
        // date time provider
        services.AddSingleton<IDateTimeProvider, RealDateTimeProvider>();
        
        // the main consumable services
        services.AddSingleton<ISubscriptionEngine, SubscriptionEngine>();
        services.AddSingleton<ISubscriptionService, SubscriptionService>();

        return services;
    }

    public static IServiceCollection AddSubscriptionServices(this IServiceCollection services, SubscriptionServicesCoreOptions options, IDateTimeProvider dateTimeProvider, ICabSearchService cabSearchService, ICabUpdatesReceiver cabUpdatesReceiver)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton(options);

        // repositories
        services.AddSingleton<IOutboxRepository, OutboxRepository>();
        services.AddSingleton<ISubscriptionRepository, SubscriptionRepository>();
        services.AddSingleton<ISentNotificationRepository, SentNotificationRepository>();

        // integration dependency services
        services.AddSingleton(cabSearchService);
        services.AddSingleton(cabUpdatesReceiver);

        // date time provider
        services.AddSingleton(dateTimeProvider);

        // the main consumable services
        services.AddSingleton<ISubscriptionEngine, SubscriptionEngine>();
        services.AddSingleton<ISubscriptionService, SubscriptionService>();

        return services;
    }
}
