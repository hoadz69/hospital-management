namespace ClinicSaaS.Observability.Health;

public static class HealthCheckTags
{
    public const string Live = "live";
    public const string Ready = "ready";
    public const string Database = "database";
    public const string Cache = "cache";
    public const string Messaging = "messaging";
}
