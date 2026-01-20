using Dynamics365.BusinessCentral.Client;
using Dynamics365.BusinessCentral.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Dynamics365.BusinessCentral;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBusinessCentral(
        this IServiceCollection services,
        Action<BusinessCentralOptions> configure)
    {
        var options = new BusinessCentralOptions();
        configure(options);

        Validate(options);

        services.AddSingleton(options);
        services.AddHttpClient<IBusinessCentralClient, BusinessCentralClient>();

        return services;
    }

    private static void Validate(BusinessCentralOptions o)
    {
        if (string.IsNullOrWhiteSpace(o.TenantId)) throw new ArgumentException("TenantId is required");
        if (string.IsNullOrWhiteSpace(o.ClientId)) throw new ArgumentException("ClientId is required");
        if (string.IsNullOrWhiteSpace(o.ClientSecret)) throw new ArgumentException("ClientSecret is required");
        if (string.IsNullOrWhiteSpace(o.BaseUrl)) throw new ArgumentException("BaseUrl is required");
        if (string.IsNullOrWhiteSpace(o.Company)) throw new ArgumentException("Company is required");
    }
}