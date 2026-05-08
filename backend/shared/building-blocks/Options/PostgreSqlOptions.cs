namespace ClinicSaaS.BuildingBlocks.Options;

public sealed class PostgreSqlOptions
{
    public const string SectionName = OptionsSectionNames.PostgreSql;

    public bool Enabled { get; init; }

    public string Provider { get; init; } = "PostgreSQL";

    public string? ConnectionString { get; init; }

    public string? ConnectionStringEnvironmentVariable { get; init; }
}
