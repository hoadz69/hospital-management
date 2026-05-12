using ClinicSaaS.BuildingBlocks.Results;
using ClinicSaaS.BuildingBlocks.Tenancy;
using ClinicSaaS.Contracts.Domains;

namespace DomainService.Application.Domains;

/// <summary>
/// Handler Application trả contract/stub cho Domain Service Wave A, chưa ghi PostgreSQL/Redis.
/// </summary>
public sealed class DomainContractStubHandler(ITenantContextAccessor tenantContextAccessor)
{
    private static readonly Guid SeedDomainId = Guid.Parse("11111111-1111-4111-8111-111111111111");

    /// <summary>
    /// Trả danh sách domain dummy của tenant sau khi kiểm tra tenant context.
    /// </summary>
    /// <param name="tenantId">Tenant trên route cần khớp với context.</param>
    /// <returns>Kết quả chứa danh sách domain stub hoặc lỗi tenant mismatch.</returns>
    public Result<DomainListResponse> ListDomains(Guid tenantId)
    {
        var scopeError = ValidateTenantScope(tenantId);
        return scopeError is not null
            ? Result<DomainListResponse>.Failure(scopeError)
            : Result<DomainListResponse>.Success(new DomainListResponse([BuildDomain(tenantId, SeedDomainId)]));
    }

    /// <summary>
    /// Trả chi tiết domain dummy để FE ghép màn hình domain detail.
    /// </summary>
    /// <param name="tenantId">Tenant trên route cần khớp với context.</param>
    /// <param name="domainId">Domain cần lấy chi tiết.</param>
    /// <returns>Kết quả chứa domain stub hoặc lỗi validation/not found.</returns>
    public Result<DomainResponse> GetDomain(Guid tenantId, Guid domainId)
    {
        var scopeError = ValidateTenantScope(tenantId);
        if (scopeError is not null)
        {
            return Result<DomainResponse>.Failure(scopeError);
        }

        if (domainId == Guid.Empty)
        {
            return Result<DomainResponse>.Failure(DomainContractErrors.NotFound);
        }

        return Result<DomainResponse>.Success(BuildDomain(tenantId, domainId));
    }

    /// <summary>
    /// Tạo response đăng ký domain dummy, bao gồm conflict path cho domain đã bị chiếm.
    /// </summary>
    /// <param name="tenantId">Tenant trên route cần khớp với context.</param>
    /// <param name="request">Payload đăng ký domain từ API.</param>
    /// <returns>Kết quả chứa domain vừa đăng ký dummy hoặc lỗi validation/conflict.</returns>
    public Result<DomainResponse> RegisterDomain(Guid tenantId, RegisterDomainRequest request)
    {
        var scopeError = ValidateTenantScope(tenantId);
        if (scopeError is not null)
        {
            return Result<DomainResponse>.Failure(scopeError);
        }

        if (string.IsNullOrWhiteSpace(request.DomainName))
        {
            return Result<DomainResponse>.Failure(DomainContractErrors.Validation("domainName", "Domain name is required."));
        }

        var normalized = NormalizeDomain(request.DomainName);
        if (normalized.Contains("taken", StringComparison.OrdinalIgnoreCase))
        {
            return Result<DomainResponse>.Failure(DomainContractErrors.Conflict("domainName", "Domain name is already registered."));
        }

        var response = new DomainResponse(
            Guid.NewGuid(),
            tenantId,
            request.DomainName.Trim(),
            normalized,
            string.IsNullOrWhiteSpace(request.DomainType) ? "CustomDomain" : request.DomainType.Trim(),
            "Pending",
            request.IsPrimary,
            "Pending",
            "cname.clinicos.local",
            null,
            DateTimeOffset.UtcNow,
            null);

        return Result<DomainResponse>.Success(response);
    }

    /// <summary>
    /// Trả trạng thái verify DNS dummy, chưa chạy background worker hoặc ACME.
    /// </summary>
    /// <param name="tenantId">Tenant trên route cần khớp với context.</param>
    /// <param name="domainId">Domain cần verify.</param>
    /// <returns>Kết quả chứa trạng thái verify dummy.</returns>
    public Result<DomainVerificationResponse> VerifyDomain(Guid tenantId, Guid domainId)
    {
        var scopeError = ValidateTenantScope(tenantId);
        if (scopeError is not null)
        {
            return Result<DomainVerificationResponse>.Failure(scopeError);
        }

        if (domainId == Guid.Empty)
        {
            return Result<DomainVerificationResponse>.Failure(DomainContractErrors.NotFound);
        }

        return Result<DomainVerificationResponse>.Success(new DomainVerificationResponse(
            domainId,
            "Pending",
            "Pending",
            DateTimeOffset.UtcNow.AddMinutes(2),
            "DNS verify dummy accepted. Poll verify-status after the suggested time."));
    }

    /// <summary>
    /// Trả trạng thái publish dummy cho website tenant.
    /// </summary>
    /// <param name="tenantId">Tenant trên route cần khớp với context.</param>
    /// <returns>Kết quả chứa publish status dummy.</returns>
    public Result<DomainPublishResponse> Publish(Guid tenantId)
    {
        var scopeError = ValidateTenantScope(tenantId);
        return scopeError is not null
            ? Result<DomainPublishResponse>.Failure(scopeError)
            : Result<DomainPublishResponse>.Success(new DomainPublishResponse(
                tenantId,
                "queued",
                BuildDomainName(tenantId),
                DateTimeOffset.UtcNow));
    }

    private Error? ValidateTenantScope(Guid tenantId)
    {
        return string.Equals(
            tenantContextAccessor.Current.TenantId,
            tenantId.ToString(),
            StringComparison.OrdinalIgnoreCase)
            ? null
            : DomainContractErrors.TenantMismatch;
    }

    private static DomainResponse BuildDomain(Guid tenantId, Guid domainId)
    {
        var domainName = BuildDomainName(tenantId);

        return new DomainResponse(
            domainId,
            tenantId,
            domainName,
            NormalizeDomain(domainName),
            "DefaultSubdomain",
            "Pending",
            true,
            "Pending",
            "cname.clinicos.local",
            null,
            DateTimeOffset.UtcNow,
            "Wave A contract stub: DNS verify and SSL provisioning are not automated yet.");
    }

    private static string BuildDomainName(Guid tenantId) => $"{tenantId.ToString("N")[..8]}.clinicos.local";

    private static string NormalizeDomain(string domainName) => domainName.Trim().TrimEnd('.').ToLowerInvariant();
}
