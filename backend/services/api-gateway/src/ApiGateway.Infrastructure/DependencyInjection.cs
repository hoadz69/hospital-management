using ClinicSaaS.BuildingBlocks.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApiGateway.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddApiGatewayInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<PostgreSqlOptions>(configuration.GetSection(PostgreSqlOptions.SectionName));

        return services;
    }
}
