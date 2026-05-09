namespace ClinicSaaS.BuildingBlocks.Results;

/// <summary>
/// Kết quả xử lý không kèm payload, dùng để trả success/failure có cấu trúc.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Cho biết operation thành công.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Cho biết operation thất bại.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Lỗi nghiệp vụ/validation khi operation thất bại.
    /// </summary>
    public Error Error { get; }

    /// <summary>
    /// Tạo kết quả thành công không kèm payload.
    /// </summary>
    /// <returns>Kết quả thành công với lỗi rỗng.</returns>
    public static Result Success() => new(true, Error.None);

    /// <summary>
    /// Tạo kết quả thất bại không kèm payload.
    /// </summary>
    /// <param name="error">Lỗi nghiệp vụ hoặc validation cần trả về cho caller.</param>
    /// <returns>Kết quả thất bại chứa lỗi đã truyền vào.</returns>
    public static Result Failure(Error error) => new(false, error);
}
