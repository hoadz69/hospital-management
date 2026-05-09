using Microsoft.AspNetCore.Http;

namespace ClinicSaaS.Observability.Correlation;

/// <summary>
/// Middleware đảm bảo mỗi request có correlation id để trace qua gateway và services.
/// </summary>
/// <param name="next">Middleware kế tiếp trong ASP.NET Core pipeline.</param>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    /// <summary>
    /// Header correlation id chuẩn của platform.
    /// </summary>
    public const string HeaderName = "X-Correlation-Id";

    /// <summary>
    /// Đọc correlation id từ request hoặc tạo mới rồi ghi lại vào response.
    /// </summary>
    /// <param name="context">HttpContext của request hiện tại.</param>
    /// <returns>Task hoàn tất khi middleware kế tiếp xử lý xong.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[HeaderName].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N");
        }

        context.Items[HeaderName] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        await next(context);
    }
}
