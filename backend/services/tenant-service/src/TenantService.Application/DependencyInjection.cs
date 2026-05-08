using ClinicSaaS.BuildingBlocks.Tenancy;
using Microsoft.Extensions.DependencyInjection;

namespace TenantService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddTenantServiceApplication(this IServiceCollection services)
    {
        services.AddScoped<ITenantContextAccessor, TenantContextAccessor>();

        return services;
    }
}
