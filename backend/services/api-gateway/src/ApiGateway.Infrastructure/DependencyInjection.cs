using ClinicSaaS.BuildingBlocks.Options;
using ApiGateway.Application.Tenants;
using ApiGateway.Infrastructure.Tenants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ApiGateway.Infrastructure;

/// <summary>
/// Đăng ký dependency tầng Infrastructure của API Gateway.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Đăng ký config PostgreSQL placeholder và typed HttpClient tới Tenant Service.
    /// </summary>
    /// <param name="services">Service collection của API Gateway.</param>
    /// <param name="configuration">Configuration dùng để bind service URLs và placeholder options.</param>
    /// <returns>Service collection đã đăng ký Infrastructure services.</returns>
    public static IServiceCollection AddApiGatewayInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<PostgreSqlOptions>(configuration.GetSection(PostgreSqlOptions.SectionName));
        services.Configure<TenantServiceClientOptions>(configuration.GetSection(TenantServiceClientOptions.SectionName));
        services.AddHttpClient<ITenantServiceClient, TenantServiceClient>((serviceProvider, httpClient) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<TenantServiceClientOptions>>().Value;
            var baseUrl = ResolveTenantServiceBaseUrl(options);
            httpClient.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
        });

        return services;
    }

    private static string ResolveTenantServiceBaseUrl(TenantServiceClientOptions options)
    {
        var fromEnvironment = string.IsNullOrWhiteSpace(options.BaseUrlEnvironmentVariable)
            ? null
            : Environment.GetEnvironmentVariable(options.BaseUrlEnvironmentVariable);
        var baseUrl = string.IsNullOrWhiteSpace(fromEnvironment)
            ? options.BaseUrl
            : fromEnvironment;

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException(
                $"Tenant Service base URL is missing. Set '{options.BaseUrlEnvironmentVariable}' or Services:TenantService:BaseUrl.");
        }

        return baseUrl;
    }
}
