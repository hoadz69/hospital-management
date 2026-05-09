namespace ClinicSaaS.Observability.Logging;

/// <summary>
/// Tên thuộc tính logging chuẩn để log có thể query thống nhất giữa services.
/// </summary>
public static class LoggingPropertyNames
{
    /// <summary>
    /// Tên service sinh log.
    /// </summary>
    public const string ServiceName = "ServiceName";

    /// <summary>
    /// Correlation id nối log giữa gateway và services.
    /// </summary>
    public const string CorrelationId = "CorrelationId";

    /// <summary>
    /// Tenant id của request nếu có.
    /// </summary>
    public const string TenantId = "TenantId";

    /// <summary>
    /// User id của request nếu có auth context.
    /// </summary>
    public const string UserId = "UserId";
}
