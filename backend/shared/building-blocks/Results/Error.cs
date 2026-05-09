namespace ClinicSaaS.BuildingBlocks.Results;

/// <summary>
/// Lỗi nghiệp vụ hoặc validation được truyền qua các layer mà không phụ thuộc HTTP.
/// </summary>
/// <param name="Code">Mã lỗi ổn định để API layer map sang status/ProblemDetails phù hợp.</param>
/// <param name="Message">Thông điệp lỗi an toàn để trả về hoặc log ở mức nghiệp vụ.</param>
/// <param name="Details">Chi tiết validation theo field khi lỗi có nhiều nguyên nhân cụ thể.</param>
public sealed record Error(string Code, string Message, IReadOnlyDictionary<string, string[]>? Details = null)
{
    /// <summary>
    /// Lỗi rỗng dùng cho kết quả thành công.
    /// </summary>
    public static Error None { get; } = new("none", string.Empty);
}
