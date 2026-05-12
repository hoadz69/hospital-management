using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DomainService.Infrastructure;

/// <summary>
/// Đăng ký dependency tầng Infrastructure của Domain Service.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Giữ extension DI cho boundary Infrastructure; Wave A chưa có PostgreSQL/Redis persistence.
    /// </summary>
    /// <param name="services">Service collection của Domain Service.</param>
    /// <param name="configuration">Configuration của service, sẽ dùng khi bật persistence thật.</param>
    /// <returns>Service collection đã đăng ký Infrastructure services.</returns>
    public static IServiceCollection AddDomainServiceInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        _ = configuration;
        return services;
    }
}
