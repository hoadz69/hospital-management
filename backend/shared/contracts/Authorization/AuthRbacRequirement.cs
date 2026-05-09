namespace ClinicSaaS.Contracts.Authorization;

/// <summary>
/// Mô tả yêu cầu role/permission tối thiểu của một endpoint hoặc use case.
/// </summary>
/// <param name="Role">Role bắt buộc của user khi truy cập endpoint hoặc thực hiện use case.</param>
/// <param name="Permission">Permission bắt buộc gắn với hành động nghiệp vụ.</param>
/// <param name="RequiresTenant">Cho biết yêu cầu này cần tenant context hợp lệ hay không.</param>
public sealed record AuthRbacRequirement(string Role, string Permission, bool RequiresTenant);
