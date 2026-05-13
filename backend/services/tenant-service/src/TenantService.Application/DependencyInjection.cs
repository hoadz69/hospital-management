using ClinicSaaS.BuildingBlocks.Tenancy;
using ClinicSaaS.BuildingBlocks.Security;
using Microsoft.Extensions.DependencyInjection;
using TenantService.Application.Domains;
using TenantService.Application.Plans;
using TenantService.Application.Tenants;

namespace TenantService.Application;

/// <summary>
/// Đăng ký dependency tầng Application của Tenant Service.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Đăng ký accessor và các tenant use case handlers.
    /// </summary>
    /// <param name="services">Service collection của Tenant Service.</param>
    /// <returns>Service collection đã đăng ký Application services.</returns>
    public static IServiceCollection AddTenantServiceApplication(this IServiceCollection services)
    {
        services.AddScoped<ITenantContextAccessor, TenantContextAccessor>();
        services.AddScoped<IUserContextAccessor, UserContextAccessor>();
        services.AddScoped<CreateTenantHandler>();
        services.AddScoped<GetTenantByIdHandler>();
        services.AddScoped<ListTenantsHandler>();
        services.AddScoped<UpdateTenantStatusHandler>();
        services.AddScoped<TenantDomainOperationsHandler>();
        services.AddScoped<OwnerPlanCatalogHandler>();

        return services;
    }
}
