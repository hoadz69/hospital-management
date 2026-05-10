using ClinicSaaS.BuildingBlocks.Tenancy;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
    /// Bật OpenAPI JSON và Swagger UI theo route thống nhất, chỉ trong Development.
    /// </summary>
    /// <param name="app">WebApplication cần bật middleware Swagger.</param>
    /// <param name="serviceDisplayName">Tên hiển thị của service trên Swagger UI.</param>
    /// <returns>WebApplication đã cấu hình OpenAPI để tiếp tục chain startup.</returns>
    /// <remarks>
    /// Chỉ expose `/swagger` và `/openapi/v1.json` khi ASPNETCORE_ENVIRONMENT là Development.
    /// Trên Staging/Production, contract endpoint không được phát tán mặc định.
    /// Để mở Swagger trên server smoke, set ASPNETCORE_ENVIRONMENT=Development trên runtime.
    /// </remarks>
    public static WebApplication UseClinicSaaSOpenApi(
        this WebApplication app,
        string serviceDisplayName)
    {
        if (!app.Environment.IsDevelopment())
        {
            return app;
        }

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
