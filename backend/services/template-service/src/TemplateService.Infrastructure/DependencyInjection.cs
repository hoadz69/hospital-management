using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TemplateService.Infrastructure;

/// <summary>
/// Đăng ký dependency tầng Infrastructure của Template Service.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Giữ extension DI cho boundary Infrastructure; Wave A chưa có MongoDB/PostgreSQL persistence.
    /// </summary>
    /// <param name="services">Service collection của Template Service.</param>
    /// <param name="configuration">Configuration của service, sẽ dùng khi bật persistence thật.</param>
    /// <returns>Service collection đã đăng ký Infrastructure services.</returns>
    public static IServiceCollection AddTemplateServiceInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        _ = configuration;
        return services;
    }
}
