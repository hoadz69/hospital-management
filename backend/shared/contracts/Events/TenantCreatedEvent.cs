using ClinicSaaS.Contracts.Tenancy;

namespace ClinicSaaS.Contracts.Events;

public sealed record TenantCreatedEvent(TenantReference Tenant, string PlanCode) : DomainEventBase, IDomainEvent
{
    public override string EventName => "tenant.created";
}
