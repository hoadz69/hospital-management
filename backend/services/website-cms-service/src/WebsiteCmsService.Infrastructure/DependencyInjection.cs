using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WebsiteCmsService.Infrastructure;

/// <summary>
/// Đăng ký dependency tầng Infrastructure của Website CMS Service.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Giữ extension DI cho boundary Infrastructure; Wave A chưa có MongoDB/Redis persistence.
    /// </summary>
    /// <param name="services">Service collection của Website CMS Service.</param>
    /// <param name="configuration">Configuration của service, sẽ dùng khi bật persistence thật.</param>
    /// <returns>Service collection đã đăng ký Infrastructure services.</returns>
    public static IServiceCollection AddWebsiteCmsServiceInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        _ = configuration;
        return services;
    }
}
