using ClinicSaaS.BuildingBlocks.Security;
using ClinicSaaS.BuildingBlocks.Tenancy;
using Microsoft.Extensions.DependencyInjection;
using TemplateService.Application.Templates;

namespace TemplateService.Application;

/// <summary>
/// Đăng ký dependency tầng Application của Template Service.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Đăng ký accessor và handler stub cho template registry/apply contract.
    /// </summary>
    /// <param name="services">Service collection của Template Service.</param>
    /// <returns>Service collection đã đăng ký Application services.</returns>
    public static IServiceCollection AddTemplateServiceApplication(this IServiceCollection services)
    {
        services.AddScoped<ITenantContextAccessor, TenantContextAccessor>();
        services.AddScoped<IUserContextAccessor, UserContextAccessor>();
        services.AddScoped<TemplateContractStubHandler>();

        return services;
    }
}
