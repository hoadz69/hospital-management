namespace TenantService.Domain.Tenants;

/// <summary>
/// Hồ sơ nghiệp vụ của phòng khám thuộc một tenant.
/// </summary>
/// <param name="TenantId">Định danh tenant sở hữu hồ sơ phòng khám.</param>
/// <param name="ClinicName">Tên phòng khám hiển thị trong hệ thống.</param>
/// <param name="ContactEmail">Email liên hệ của phòng khám nếu có.</param>
/// <param name="PhoneNumber">Số điện thoại liên hệ của phòng khám nếu có.</param>
/// <param name="AddressLine">Địa chỉ phòng khám nếu có.</param>
/// <param name="Specialty">Chuyên khoa chính của phòng khám nếu có.</param>
public sealed record ClinicProfile(
    Guid TenantId,
    string ClinicName,
    string? ContactEmail,
    string? PhoneNumber,
    string? AddressLine,
    string? Specialty);
