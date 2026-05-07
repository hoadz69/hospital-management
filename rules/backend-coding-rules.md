# Backend Coding Rules - Clinic SaaS

Use these rules when implementing .NET backend services.

## Service Structure

Each service should follow:

```txt
ServiceName/
  src/
    ServiceName.Api/
    ServiceName.Application/
    ServiceName.Domain/
    ServiceName.Infrastructure/
  tests/
    ServiceName.UnitTests/
    ServiceName.IntegrationTests/
```

## Layer Boundaries

- `Api`: HTTP endpoints, middleware, auth integration, OpenAPI, request/response mapping.
- `Application`: use cases, commands, queries, DTOs, validators, transaction boundaries.
- `Domain`: entities, value objects, domain events, business rules.
- `Infrastructure`: PostgreSQL, MongoDB, Redis, Kafka, email/SMS/Zalo providers, external services.

Rules:

- Controllers must not contain business logic.
- Domain must not reference ASP.NET, database libraries, Redis, Kafka, or HTTP clients.
- Infrastructure must implement interfaces defined by Application/Domain where needed.
- Cross-service access must go through contracts/API/events, not direct infrastructure injection.

## Tenant Isolation

- Every tenant-owned command/query must require tenant context.
- Never fallback to a hardcoded tenant id.
- Reject missing or invalid tenant context.
- Add `tenant_id` to tenant-owned tables/collections.
- Add indexes for common tenant-scoped queries.
- Owner Super Admin cross-tenant use cases must be explicit.

## Data Access

- PostgreSQL for transactional data.
- MongoDB for CMS/layout/template JSON.
- Redis for cache and locks.
- Kafka/Event Bus for async workflows.
- Migration scripts must be explicit and reviewable.

## Naming

- Use clear service names based on Clinic SaaS bounded contexts.
- Avoid copied old namespaces or service names.
- SQL naming should be lowercase `snake_case` unless the owner approves another convention.

## Error Handling

- Validate inputs at application boundary.
- Return consistent error responses from API layer.
- Log with correlation/request id where possible.
- Do not swallow exceptions that should fail a command.

## Background Work

- Use hosted/background services or queue consumers for long-running work.
- Do not start fire-and-forget tasks from request handlers unless the architecture explicitly allows it.
- Event-driven workflows should publish clear domain/integration events.
