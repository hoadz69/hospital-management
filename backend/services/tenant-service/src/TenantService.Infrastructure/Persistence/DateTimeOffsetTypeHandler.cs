using System.Data;
using Dapper;

namespace TenantService.Infrastructure.Persistence;

/// <summary>
/// Type handler Dapper map cột PostgreSQL `timestamptz` đọc từ Npgsql 6+ (trả `DateTime` Kind=Utc)
/// sang `DateTimeOffset` của entity/row record trong Tenant Service.
/// </summary>
/// <remarks>
/// Từ Npgsql 6 trở lên, `timestamptz` mặc định được provider trả về dưới dạng `DateTime` với `Kind=Utc`,
/// thay vì `DateTimeOffset` như các phiên bản trước. Dapper khi gặp property kiểu `DateTimeOffset`
/// sẽ ném `InvalidCastException`. Handler này bridge an toàn giữa hai kiểu mà không thay đổi schema
/// (`timestamptz`) hoặc contract (`DateTimeOffset`).
/// </remarks>
internal sealed class DateTimeOffsetTypeHandler : SqlMapper.TypeHandler<DateTimeOffset>
{
    /// <summary>
    /// Ghi tham số `DateTimeOffset` xuống Npgsql; offset bắt buộc là 0 (UTC) theo ràng buộc của
    /// Npgsql 6+ với cột `timestamptz`.
    /// </summary>
    /// <param name="parameter">Parameter Dapper sẽ gửi xuống provider.</param>
    /// <param name="value">Giá trị `DateTimeOffset` cần gán cho parameter.</param>
    public override void SetValue(IDbDataParameter parameter, DateTimeOffset value)
    {
        parameter.Value = value.UtcDateTime;
    }

    /// <summary>
    /// Đọc giá trị `timestamptz` về `DateTimeOffset` UTC, chấp nhận cả khi provider trả `DateTime`
    /// hoặc `DateTimeOffset`.
    /// </summary>
    /// <param name="value">Giá trị raw mà Npgsql trả về cho cột `timestamptz`.</param>
    /// <returns>`DateTimeOffset` ở UTC tương ứng với giá trị PostgreSQL đã đọc.</returns>
    public override DateTimeOffset Parse(object value)
    {
        return value switch
        {
            DateTimeOffset dto => dto,
            DateTime dt => new DateTimeOffset(DateTime.SpecifyKind(dt, DateTimeKind.Utc), TimeSpan.Zero),
            _ => throw new InvalidCastException(
                $"Không thể chuyển giá trị kiểu '{value?.GetType().FullName ?? "null"}' sang DateTimeOffset.")
        };
    }
}
