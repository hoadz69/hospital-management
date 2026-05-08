using ClinicSaaS.BuildingBlocks.Tenancy;
using ClinicSaaS.BuildingBlocks.Security;
using Microsoft.Extensions.DependencyInjection;

namespace ApiGateway.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApiGatewayApplication(this IServiceCollection services)
    {
        services.AddScoped<ITenantContextAccessor, TenantContextAccessor>();
        services.AddScoped<IUserContextAccessor, UserContextAccessor>();

        return services;
    }
}
