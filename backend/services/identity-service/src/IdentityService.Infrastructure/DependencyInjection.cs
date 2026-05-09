using ClinicSaaS.BuildingBlocks.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityService.Infrastructure;

/// <summary>
/// Đăng ký dependency tầng Infrastructure của Identity Service.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Đăng ký PostgreSQL placeholder options cho Identity Service.
    /// </summary>
    /// <param name="services">Service collection của Identity Service.</param>
    /// <param name="configuration">Configuration dùng để bind PostgreSQL options.</param>
    /// <returns>Service collection đã đăng ký Infrastructure services.</returns>
    public static IServiceCollection AddIdentityServiceInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<PostgreSqlOptions>(configuration.GetSection(PostgreSqlOptions.SectionName));

        return services;
    }
}
