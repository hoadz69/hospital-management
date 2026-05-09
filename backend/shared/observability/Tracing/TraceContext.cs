namespace ClinicSaaS.Observability.Tracing;

/// <summary>
/// Context tracing tối thiểu để truyền trace/span/correlation giữa các layer.
/// </summary>
/// <param name="TraceId">Trace id của request hoặc workflow hiện tại.</param>
/// <param name="SpanId">Span id hiện tại nếu tracing provider có cung cấp.</param>
/// <param name="CorrelationId">Correlation id dùng để nối log giữa gateway và services.</param>
public sealed record TraceContext(string? TraceId, string? SpanId, string? CorrelationId)
{
    /// <summary>
    /// Trace context rỗng khi chưa có tracing provider hoặc correlation id.
    /// </summary>
    public static TraceContext Empty { get; } = new(null, null, null);
}
