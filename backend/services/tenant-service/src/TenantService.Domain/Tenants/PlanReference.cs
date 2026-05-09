namespace TenantService.Domain.Tenants;

/// <summary>
/// Tham chiếu gói dịch vụ tại thời điểm tạo tenant, chưa phải billing logic đầy đủ.
/// </summary>
/// <param name="PlanCode">Mã gói dịch vụ được lưu cùng tenant.</param>
/// <param name="DisplayName">Tên hiển thị của gói dịch vụ nếu có snapshot từ caller.</param>
public sealed record PlanReference(string PlanCode, string? DisplayName);
