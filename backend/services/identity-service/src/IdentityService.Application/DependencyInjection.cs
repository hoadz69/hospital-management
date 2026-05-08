using ClinicSaaS.BuildingBlocks.Tenancy;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityServiceApplication(this IServiceCollection services)
    {
        services.AddScoped<ITenantContextAccessor, TenantContextAccessor>();

        return services;
    }
}
