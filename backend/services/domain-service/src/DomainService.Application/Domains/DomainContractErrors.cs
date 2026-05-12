using ClinicSaaS.BuildingBlocks.Results;

namespace DomainService.Application.Domains;

/// <summary>
/// Các lỗi contract ổn định để API layer map sang ProblemDetails rõ ràng.
/// </summary>
public static class DomainContractErrors
{
    /// <summary>
    /// Lỗi tenant context không khớp tenant trên route.
    /// </summary>
    public static Error TenantMismatch { get; } = new(
        "domains.tenant_mismatch",
        "Tenant context does not match the tenant route parameter.");

    /// <summary>
    /// Lỗi domain không tồn tại trong stub.
    /// </summary>
    public static Error NotFound { get; } = new(
        "domains.not_found",
        "Domain was not found.");

    /// <summary>
    /// Tạo lỗi validation theo field.
    /// </summary>
    /// <param name="field">Field request bị lỗi.</param>
    /// <param name="message">Thông điệp lỗi an toàn.</param>
    /// <returns>Lỗi validation chứa field tương ứng.</returns>
    public static Error Validation(string field, string message) => new(
        "domains.validation",
        message,
        new Dictionary<string, string[]> { [field] = [message] });

    /// <summary>
    /// Tạo lỗi conflict theo field.
    /// </summary>
    /// <param name="field">Field request bị conflict.</param>
    /// <param name="message">Thông điệp lỗi an toàn.</param>
    /// <returns>Lỗi conflict chứa field tương ứng.</returns>
    public static Error Conflict(string field, string message) => new(
        "domains.conflict",
        message,
        new Dictionary<string, string[]> { [field] = [message] });
}
