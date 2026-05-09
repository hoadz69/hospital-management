using ClinicSaaS.Contracts.Tenancy;

namespace ClinicSaaS.Contracts.Events;

/// <summary>
/// Event dự kiến phát khi Owner Super Admin tạo tenant mới.
/// </summary>
/// <param name="Tenant">Tham chiếu tenant vừa được tạo.</param>
/// <param name="PlanCode">Mã gói dịch vụ được gán tại thời điểm tạo tenant.</param>
public sealed record TenantCreatedEvent(TenantReference Tenant, string PlanCode) : DomainEventBase, IDomainEvent
{
    /// <summary>
    /// Tên event chuẩn khi tenant được tạo.
    /// </summary>
    public override string EventName => "tenant.created";
}
