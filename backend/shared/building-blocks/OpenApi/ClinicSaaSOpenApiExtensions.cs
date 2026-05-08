using ClinicSaaS.BuildingBlocks.Tenancy;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicSaaS.BuildingBlocks.OpenApi;

public static class ClinicSaaSOpenApiExtensions
{
    public static IServiceCollection AddClinicSaaSOpenApi(
        this IServiceCollection services,
        string serviceName,
        string serviceDisplayName)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new()
            {
                Title = $"{serviceDisplayName} API",
                Version = "v1",
                Description = $"Clinic SaaS {serviceName} API boundary contract."
            });
        });

        return services;
    }

    public static WebApplication UseClinicSaaSOpenApi(
        this WebApplication app,
        string serviceDisplayName)
    {
        app.UseSwagger(options =>
        {
            options.RouteTemplate = "openapi/{documentName}.json";
        });

        app.UseSwaggerUI(options =>
        {
            options.DocumentTitle = $"{serviceDisplayName} API";
            options.RoutePrefix = "swagger";
            options.SwaggerEndpoint("/openapi/v1.json", $"{serviceDisplayName} API v1");
        });

        return app;
    }
}
