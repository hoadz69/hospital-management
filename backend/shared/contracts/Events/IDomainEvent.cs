namespace ClinicSaaS.Contracts.Events;

/// <summary>
/// Contract tối thiểu cho domain/integration event chia sẻ giữa services.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Định danh duy nhất của event.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Thời điểm event phát sinh theo UTC.
    /// </summary>
    DateTimeOffset OccurredAtUtc { get; }

    /// <summary>
    /// Tên event dùng cho routing/logging.
    /// </summary>
    string EventName { get; }
}
