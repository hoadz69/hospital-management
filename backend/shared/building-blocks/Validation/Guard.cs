namespace ClinicSaaS.BuildingBlocks.Validation;

/// <summary>
/// Guard helper tối thiểu cho validation ở boundary nội bộ.
/// </summary>
public static class Guard
{
    /// <summary>
    /// Đảm bảo chuỗi input không null, rỗng hoặc chỉ chứa khoảng trắng.
    /// </summary>
    /// <param name="value">Giá trị chuỗi cần kiểm tra.</param>
    /// <param name="parameterName">Tên tham số dùng trong exception nếu input không hợp lệ.</param>
    /// <returns>Chuỗi hợp lệ đã truyền vào.</returns>
    public static string AgainstNullOrWhiteSpace(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", parameterName);
        }

        return value;
    }

    /// <summary>
    /// Đảm bảo reference input không null.
    /// </summary>
    /// <param name="value">Giá trị reference cần kiểm tra.</param>
    /// <param name="parameterName">Tên tham số dùng trong exception nếu input null.</param>
    /// <returns>Giá trị reference hợp lệ đã truyền vào.</returns>
    public static T AgainstNull<T>(T? value, string parameterName)
        where T : class
    {
        return value ?? throw new ArgumentNullException(parameterName);
    }
}
