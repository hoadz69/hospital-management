using ClinicSaaS.BuildingBlocks.Results;
using ClinicSaaS.Contracts.Domains;

namespace TenantService.Application.Domains;

/// <summary>
/// Use case doc va cap nhat DNS/SSL state cua tenant domain tu PostgreSQL.
/// </summary>
public sealed class TenantDomainOperationsHandler
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromMinutes(2);

    private readonly ITenantDomainOperationsRepository _repository;

    /// <summary>
    /// Khoi tao handler voi repository domain operations.
    /// </summary>
    /// <param name="repository">Repository Dapper/Npgsql cho bang tenant_domains.</param>
    public TenantDomainOperationsHandler(ITenantDomainOperationsRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Liet ke domain state cua tenant sau khi validate tenant id.
    /// </summary>
    /// <param name="tenantId">Tenant can doc domain state.</param>
    /// <param name="cancellationToken">Token huy request.</param>
    /// <returns>Danh sach DNS/SSL state hoac loi validation/not found.</returns>
    public async Task<Result<DomainDnsSslListResponse>> ListDomainsAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var validation = ValidateTenantId(tenantId);
        if (validation is not null)
        {
            return Result<DomainDnsSslListResponse>.Failure(validation);
        }

        if (!await _repository.TenantExistsAsync(tenantId, cancellationToken))
        {
            return Result<DomainDnsSslListResponse>.Failure(
                TenantDomainOperationErrors.TenantNotFound(tenantId));
        }

        var items = await _repository.ListDomainsAsync(tenantId, cancellationToken);
        return Result<DomainDnsSslListResponse>.Success(new DomainDnsSslListResponse(items));
    }

    /// <summary>
    /// Tang retry count va tra lai DNS/SSL state moi cua domain.
    /// </summary>
    /// <param name="tenantId">Tenant so huu domain.</param>
    /// <param name="domainId">Domain can retry DNS.</param>
    /// <param name="cancellationToken">Token huy request.</param>
    /// <returns>Domain state moi hoac loi validation/not found.</returns>
    public async Task<Result<DomainDnsSslStateResponse>> RetryDnsAsync(
        Guid tenantId,
        Guid domainId,
        CancellationToken cancellationToken)
    {
        var validation = ValidateTenantAndDomainId(tenantId, domainId);
        if (validation is not null)
        {
            return Result<DomainDnsSslStateResponse>.Failure(validation);
        }

        var now = DateTimeOffset.UtcNow;
        var response = await _repository.RetryDnsAsync(
            tenantId,
            domainId,
            now,
            now.Add(RetryDelay),
            cancellationToken);

        return response is not null
            ? Result<DomainDnsSslStateResponse>.Success(response)
            : await NotFoundAsync<DomainDnsSslStateResponse>(tenantId, domainId, cancellationToken);
    }

    /// <summary>
    /// Doc SSL state cua domain theo tenant.
    /// </summary>
    /// <param name="tenantId">Tenant so huu domain.</param>
    /// <param name="domainId">Domain can doc SSL state.</param>
    /// <param name="cancellationToken">Token huy request.</param>
    /// <returns>Domain DNS/SSL state hoac loi validation/not found.</returns>
    public async Task<Result<DomainDnsSslStateResponse>> GetSslStatusAsync(
        Guid tenantId,
        Guid domainId,
        CancellationToken cancellationToken)
    {
        var validation = ValidateTenantAndDomainId(tenantId, domainId);
        if (validation is not null)
        {
            return Result<DomainDnsSslStateResponse>.Failure(validation);
        }

        var response = await _repository.GetDomainAsync(tenantId, domainId, cancellationToken);
        return response is not null
            ? Result<DomainDnsSslStateResponse>.Success(response)
            : await NotFoundAsync<DomainDnsSslStateResponse>(tenantId, domainId, cancellationToken);
    }

    private async Task<Result<T>> NotFoundAsync<T>(
        Guid tenantId,
        Guid domainId,
        CancellationToken cancellationToken)
    {
        var error = await _repository.TenantExistsAsync(tenantId, cancellationToken)
            ? TenantDomainOperationErrors.DomainNotFound(domainId)
            : TenantDomainOperationErrors.TenantNotFound(tenantId);

        return Result<T>.Failure(error);
    }

    private static Error? ValidateTenantId(Guid tenantId)
    {
        return tenantId == Guid.Empty
            ? TenantDomainOperationErrors.Validation(new Dictionary<string, string[]>
            {
                ["tenantId"] = ["Tenant id must be a non-empty UUID."]
            })
            : null;
    }

    private static Error? ValidateTenantAndDomainId(Guid tenantId, Guid domainId)
    {
        var details = new Dictionary<string, string[]>();
        if (tenantId == Guid.Empty)
        {
            details["tenantId"] = ["Tenant id must be a non-empty UUID."];
        }

        if (domainId == Guid.Empty)
        {
            details["domainId"] = ["Domain id must be a non-empty UUID."];
        }

        return details.Count == 0 ? null : TenantDomainOperationErrors.Validation(details);
    }
}
