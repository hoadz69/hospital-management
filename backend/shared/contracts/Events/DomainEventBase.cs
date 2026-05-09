namespace ClinicSaaS.Contracts.Events;

/// <summary>
/// Base event cung cấp id và thời điểm phát sinh theo UTC.
/// </summary>
public abstract record DomainEventBase
{
    /// <summary>
    /// Định danh duy nhất của event.
    /// </summary>
    public Guid EventId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Thời điểm event phát sinh theo UTC.
    /// </summary>
    public DateTimeOffset OccurredAtUtc { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Tên event dùng cho routing/logging.
    /// </summary>
    public abstract string EventName { get; }
}
