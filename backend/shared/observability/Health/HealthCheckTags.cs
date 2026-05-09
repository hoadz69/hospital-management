namespace ClinicSaaS.Observability.Health;

/// <summary>
/// Tag health check chuẩn cho live/ready và dependency checks.
/// </summary>
public static class HealthCheckTags
{
    /// <summary>
    /// Tag cho health check sống/chết cơ bản.
    /// </summary>
    public const string Live = "live";

    /// <summary>
    /// Tag cho readiness check trước khi nhận traffic.
    /// </summary>
    public const string Ready = "ready";

    /// <summary>
    /// Tag cho dependency database.
    /// </summary>
    public const string Database = "database";

    /// <summary>
    /// Tag cho dependency cache.
    /// </summary>
    public const string Cache = "cache";

    /// <summary>
    /// Tag cho dependency messaging/event bus.
    /// </summary>
    public const string Messaging = "messaging";
}
