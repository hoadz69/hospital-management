using ClinicSaaS.BuildingBlocks.Tenancy;
using ClinicSaaS.BuildingBlocks.Security;
using Microsoft.Extensions.DependencyInjection;

namespace ApiGateway.Application;

/// <summary>
/// Đăng ký dependency tầng Application của API Gateway.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Đăng ký accessor dùng chung cho tenant/user context trong API Gateway.
    /// </summary>
    /// <param name="services">Service collection của API Gateway.</param>
    /// <returns>Service collection đã đăng ký Application services.</returns>
    public static IServiceCollection AddApiGatewayApplication(this IServiceCollection services)
    {
        services.AddScoped<ITenantContextAccessor, TenantContextAccessor>();
        services.AddScoped<IUserContextAccessor, UserContextAccessor>();

        return services;
    }
}
