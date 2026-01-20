using Dynamics365.BusinessCentral.Client;
using Dynamics365.BusinessCentral.Diagnostics;
using Dynamics365.BusinessCentral.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Dynamics365.BusinessCentral;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBusinessCentral(
        this IServiceCollection services,
        Action<BusinessCentralOptions> configure)
    {
        // Register options instance
        var options = new BusinessCentralOptions();
        configure(options);
        services.AddSingleton(options);

        // Register the client using a factory so we can resolve the observer
        services.AddHttpClient<IBusinessCentralClient, BusinessCentralClient>()
            .AddTypedClient((http, sp) =>
            {
                var observer = sp.GetService<IBusinessCentralObserver>();

                return new BusinessCentralClient(
                    http,
                    sp.GetRequiredService<BusinessCentralOptions>(),
                    observer);
            });

        return services;
    }

    /// <summary>
    /// Registers a custom observer implementation to receive diagnostics events.
    /// </summary>
    public static IServiceCollection AddBusinessCentralObserver<TObserver>(
        this IServiceCollection services)
        where TObserver : class, IBusinessCentralObserver
    {
        services.AddSingleton<IBusinessCentralObserver, TObserver>();
        return services;
    }
}
