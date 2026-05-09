namespace ClinicSaaS.BuildingBlocks.Tenancy;

/// <summary>
/// Kết quả resolve tenant context từ header, JWT claim hoặc trạng thái chưa xác định.
/// </summary>
/// <param name="IsResolved">Cho biết request đã resolve được tenant context hợp lệ hay chưa.</param>
/// <param name="TenantContext">Tenant context được resolve hoặc context rỗng nếu chưa có tenant.</param>
/// <param name="FailureReason">Lý do fail rõ ràng khi input tenant context mâu thuẫn hoặc không hợp lệ.</param>
public sealed record TenantResolutionResult(bool IsResolved, TenantContext TenantContext, string? FailureReason)
{
    /// <summary>
    /// Tạo kết quả resolve tenant thành công.
    /// </summary>
    /// <param name="tenantId">Tenant id đã đọc từ header hoặc JWT claim.</param>
    /// <param name="source">Nguồn chứa tenant id để phục vụ debug/audit.</param>
    /// <returns>Kết quả đã resolve kèm tenant context hợp lệ.</returns>
    public static TenantResolutionResult Resolved(string tenantId, string source)
    {
        return new(true, new TenantContext(tenantId, source), null);
    }

    /// <summary>
    /// Tạo kết quả chưa resolve tenant nhưng không phải lỗi.
    /// </summary>
    /// <returns>Kết quả rỗng để platform-scoped endpoint hoặc endpoint chưa khai báo scope tự xử lý tiếp.</returns>
    public static TenantResolutionResult Unresolved()
    {
        return new(false, TenantContext.Empty, null);
    }

    /// <summary>
    /// Tạo kết quả resolve tenant thất bại do input mâu thuẫn hoặc không hợp lệ.
    /// </summary>
    /// <param name="reason">Lý do fail cần trả về cho client ở mức tenant context.</param>
    /// <returns>Kết quả thất bại kèm lý do cụ thể.</returns>
    public static TenantResolutionResult NotResolved(string reason)
    {
        return new(false, TenantContext.Empty, reason);
    }
}
