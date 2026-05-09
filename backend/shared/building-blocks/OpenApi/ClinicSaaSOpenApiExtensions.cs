using ClinicSaaS.BuildingBlocks.Tenancy;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicSaaS.BuildingBlocks.OpenApi;

/// <summary>
/// Extension chuẩn hóa OpenAPI/Swagger cho các service Clinic SaaS.
/// </summary>
public static class ClinicSaaSOpenApiExtensions
{
    /// <summary>
    /// Đăng ký OpenAPI document cho service.
    /// </summary>
    /// <param name="services">Service collection của ứng dụng ASP.NET Core.</param>
    /// <param name="serviceName">Tên kỹ thuật của service dùng trong mô tả OpenAPI.</param>
    /// <param name="serviceDisplayName">Tên hiển thị của service trên Swagger UI.</param>
    /// <returns>Service collection đã đăng ký OpenAPI để tiếp tục cấu hình DI.</returns>
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

    /// <summary>
    /// Bật OpenAPI JSON và Swagger UI theo route thống nhất.
    /// </summary>
    /// <param name="app">WebApplication cần bật middleware Swagger.</param>
    /// <param name="serviceDisplayName">Tên hiển thị của service trên Swagger UI.</param>
    /// <returns>WebApplication đã cấu hình OpenAPI để tiếp tục chain startup.</returns>
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
