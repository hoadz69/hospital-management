using ClinicSaaS.BuildingBlocks.Tenancy;
using ClinicSaaS.BuildingBlocks.Security;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityService.Application;

/// <summary>
/// Đăng ký dependency tầng Application của Identity Service.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Đăng ký accessor dùng chung cho tenant/user context trong Identity Service.
    /// </summary>
    /// <param name="services">Service collection của Identity Service.</param>
    /// <returns>Service collection đã đăng ký Application services.</returns>
    public static IServiceCollection AddIdentityServiceApplication(this IServiceCollection services)
    {
        services.AddScoped<ITenantContextAccessor, TenantContextAccessor>();
        services.AddScoped<IUserContextAccessor, UserContextAccessor>();

        return services;
    }
}
