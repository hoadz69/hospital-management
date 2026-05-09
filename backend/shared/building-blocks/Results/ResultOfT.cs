namespace ClinicSaaS.BuildingBlocks.Results;

/// <summary>
/// Kết quả xử lý có payload, giúp handler không ném exception cho lỗi nghiệp vụ dự kiến.
/// </summary>
public sealed class Result<T> : Result
{
    private Result(T? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    /// <summary>
    /// Payload nghiệp vụ khi operation thành công.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Tạo kết quả thành công kèm payload.
    /// </summary>
    /// <param name="value">Payload nghiệp vụ cần trả về cho caller.</param>
    /// <returns>Kết quả thành công chứa payload đã truyền vào.</returns>
    public static Result<T> Success(T value) => new(value, true, Error.None);

    /// <summary>
    /// Tạo kết quả thất bại cho handler có payload.
    /// </summary>
    /// <param name="error">Lỗi nghiệp vụ hoặc validation cần trả về cho caller.</param>
    /// <returns>Kết quả thất bại không có payload và chứa lỗi đã truyền vào.</returns>
    public static new Result<T> Failure(Error error) => new(default, false, error);
}
