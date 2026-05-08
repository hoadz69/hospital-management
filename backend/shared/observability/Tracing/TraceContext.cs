namespace ClinicSaaS.Observability.Tracing;

public sealed record TraceContext(string? TraceId, string? SpanId, string? CorrelationId)
{
    public static TraceContext Empty { get; } = new(null, null, null);
}
