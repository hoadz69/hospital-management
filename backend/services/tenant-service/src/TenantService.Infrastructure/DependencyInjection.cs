using ClinicSaaS.BuildingBlocks.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TenantService.Application.Tenants;
using TenantService.Infrastructure.Persistence;

namespace TenantService.Infrastructure;

/// <summary>
/// Đăng ký dependency tầng Infrastructure của Tenant Service.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Đăng ký PostgreSQL connection factory và Dapper repository cho tenant persistence.
    /// </summary>
    /// <param name="services">Service collection của Tenant Service.</param>
    /// <param name="configuration">Configuration dùng để bind PostgreSQL options.</param>
    /// <returns>Service collection đã đăng ký Infrastructure services.</returns>
    public static IServiceCollection AddTenantServiceInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<PostgreSqlOptions>(configuration.GetSection(PostgreSqlOptions.SectionName));
        services.AddSingleton<IPostgreSqlConnectionFactory, NpgsqlConnectionFactory>();
        services.AddScoped<ITenantRepository, DapperTenantRepository>();

        return services;
    }
}
