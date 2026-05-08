namespace ClinicSaaS.Contracts.Events;

public interface IDomainEvent
{
    Guid EventId { get; }

    DateTimeOffset OccurredAtUtc { get; }

    string EventName { get; }
}
