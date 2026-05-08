namespace ClinicSaaS.BuildingBlocks.Results;

public sealed record Error(string Code, string Message, IReadOnlyDictionary<string, string[]>? Details = null)
{
    public static Error None { get; } = new("none", string.Empty);
}
