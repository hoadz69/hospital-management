using ClinicSaaS.Contracts.Domains;

namespace TenantService.Application.Domains;

/// <summary>
/// Port persistence cho Domain DNS/SSL state trong Tenant Service.
/// </summary>
public interface ITenantDomainOperationsRepository
{
    /// <summary>
    /// Kiem tra tenant co ton tai truoc khi tra domain list rong hoac loi 404.
    /// </summary>
    /// <param name="tenantId">Tenant can kiem tra.</param>
    /// <param name="cancellationToken">Token huy thao tac I/O.</param>
    /// <returns>True neu tenant ton tai trong PostgreSQL.</returns>
    Task<bool> TenantExistsAsync(Guid tenantId, CancellationToken cancellationToken);

    /// <summary>
    /// Liet ke domain DNS/SSL state cua tenant.
    /// </summary>
    /// <param name="tenantId">Tenant so huu domain.</param>
    /// <param name="cancellationToken">Token huy thao tac I/O.</param>
    /// <returns>Danh sach domain state theo tenant.</returns>
    Task<IReadOnlyList<DomainDnsSslStateResponse>> ListDomainsAsync(
        Guid tenantId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Doc mot domain DNS/SSL state theo tenant va domain id.
    /// </summary>
    /// <param name="tenantId">Tenant so huu domain.</param>
    /// <param name="domainId">Domain can doc.</param>
    /// <param name="cancellationToken">Token huy thao tac I/O.</param>
    /// <returns>Domain state neu tim thay; null neu tenant/domain khong khop.</returns>
    Task<DomainDnsSslStateResponse?> GetDomainAsync(
        Guid tenantId,
        Guid domainId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Tang retry count va cap nhat moc DNS retry cho domain.
    /// </summary>
    /// <param name="tenantId">Tenant so huu domain.</param>
    /// <param name="domainId">Domain can retry DNS.</param>
    /// <param name="now">Thoi diem retry theo UTC.</param>
    /// <param name="nextRetryAt">Thoi diem he thong du kien check lai.</param>
    /// <param name="cancellationToken">Token huy thao tac I/O.</param>
    /// <returns>Domain state moi neu cap nhat thanh cong; null neu tenant/domain khong khop.</returns>
    Task<DomainDnsSslStateResponse?> RetryDnsAsync(
        Guid tenantId,
        Guid domainId,
        DateTimeOffset now,
        DateTimeOffset nextRetryAt,
        CancellationToken cancellationToken);
}
