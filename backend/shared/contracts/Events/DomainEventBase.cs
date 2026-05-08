namespace ClinicSaaS.Contracts.Events;

public abstract record DomainEventBase
{
    public Guid EventId { get; init; } = Guid.NewGuid();

    public DateTimeOffset OccurredAtUtc { get; init; } = DateTimeOffset.UtcNow;

    public abstract string EventName { get; }
}
