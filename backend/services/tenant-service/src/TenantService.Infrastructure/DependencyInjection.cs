using ClinicSaaS.BuildingBlocks.Options;
using Dapper;
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
    private static int _dapperTypeHandlersRegistered;

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

        RegisterDapperTypeHandlersOnce();

        return services;
    }

    /// <summary>
    /// Đăng ký các Dapper type handler dùng chung cho Tenant Service đúng một lần per process.
    /// </summary>
    /// <remarks>
    /// Npgsql 6+ trả `timestamptz` về `DateTime` (Kind=Utc) thay vì `DateTimeOffset`, gây
    /// `InvalidCastException` khi Dapper map vào row record dùng `DateTimeOffset`. Handler bridge
    /// được đăng ký ở tầng tĩnh của Dapper nên cần guard để tránh đăng ký trùng khi DI chạy lại
    /// trong test host hoặc reload configuration.
    /// </remarks>
    private static void RegisterDapperTypeHandlersOnce()
    {
        if (Interlocked.Exchange(ref _dapperTypeHandlersRegistered, 1) == 1)
        {
            return;
        }

        SqlMapper.AddTypeHandler(new DateTimeOffsetTypeHandler());
    }
}
