using ClinicSaaS.BuildingBlocks.Security;
using ClinicSaaS.BuildingBlocks.Tenancy;
using DomainService.Application.Domains;
using Microsoft.Extensions.DependencyInjection;

namespace DomainService.Application;

/// <summary>
/// Đăng ký dependency tầng Application của Domain Service.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Đăng ký tenant/user accessor và handler stub cho contract Wave A.
    /// </summary>
    /// <param name="services">Service collection của Domain Service.</param>
    /// <returns>Service collection đã đăng ký Application services.</returns>
    public static IServiceCollection AddDomainServiceApplication(this IServiceCollection services)
    {
        services.AddScoped<ITenantContextAccessor, TenantContextAccessor>();
        services.AddScoped<IUserContextAccessor, UserContextAccessor>();
        services.AddScoped<DomainContractStubHandler>();

        return services;
    }
}
