# Backend Coding Rules - Clinic SaaS

Áp dụng cho mọi thay đổi backend: API, Application, Domain, Infrastructure, config, database, logging, migration.

## ⚠️ BẮT BUỘC ĐỌC TRƯỚC KHI VIẾT BACKEND

- Đọc `AGENTS.md`, `CLAUDE.md`, `clinic_saas_report.md`, `architech.txt` và `docs/current-task.md`.
- Chỉ implementation khi có `temp/plan.md` được owner duyệt, trừ khi owner yêu cầu làm ngay.
- Không dùng connection string/server/token thật nếu owner chưa cung cấp trong task hiện tại.
- Mỗi thay đổi backend phải giữ tenant isolation.
- Nếu thay đổi liên quan database/schema/SQL/migration/persistence/index/seed, phải đọc thêm `rules/database-rules.md`.
- Sau khi làm xong phải report lại: đã sửa gì, file nào, kiểm tra gì, còn thiếu/bị chặn gì.

## 1. Service Structure

Mỗi service theo cấu trúc Clean Architecture:

```txt
backend/services/service-name/
  src/
    ServiceName.Api/
    ServiceName.Application/
    ServiceName.Domain/
    ServiceName.Infrastructure/
  tests/
    ServiceName.UnitTests/
    ServiceName.IntegrationTests/
```

Service dự kiến:

- `identity-service`
- `tenant-service`
- `website-cms-service`
- `template-service`
- `domain-service`
- `booking-service`
- `catalog-service`
- `customer-service`
- `billing-service`
- `report-service`
- `notification-service`
- `realtime-gateway`

## 2. Layer Boundaries

- `Api`: HTTP endpoints, middleware, auth integration, OpenAPI, request/response mapping.
- `Application`: use cases, commands, queries, DTOs, validators, transaction boundaries.
- `Domain`: entities, value objects, domain events, business rules.
- `Infrastructure`: PostgreSQL, MongoDB, Redis, Kafka, email/SMS/Zalo providers, external services.

Rules:

- Controller không chứa business logic.
- Domain không reference ASP.NET, database libraries, Redis, Kafka hoặc HTTP clients.
- Infrastructure implement interface do Application/Domain định nghĩa khi cần.
- Cross-service access đi qua contracts/API/events, không inject trực tiếp infrastructure của service khác.
- Outer layers reference inner layers; inner layers không reference outer layers.

## 3. Shared Projects Convention

Backend shared code đặt dưới `backend/shared/`.

Structure hiện tại đã chốt:

```txt
backend/shared/
  building-blocks/    -> ClinicSaaS.BuildingBlocks
  contracts/          -> ClinicSaaS.Contracts
  observability/      -> ClinicSaaS.Observability
```

Rules:

- `ClinicSaaS.BuildingBlocks` chứa primitive dùng chung: tenant context, endpoint metadata, result/error, guard, options, middleware kỹ thuật thật sự dùng chung.
- `ClinicSaaS.Contracts` chứa DTO/contract/event/role/permission constant chia sẻ giữa services.
- `ClinicSaaS.Observability` chứa correlation id, trace/logging constants, health tags.
- Không tự tạo shared project mới như `SharedKernel` hoặc `Infrastructure.Shared` nếu chưa có plan được owner duyệt.
- Không đưa business logic tenant/clinic cụ thể vào shared projects.

## 3.1 Comment Rule

- Comment trong C# code, XML doc comment, SQL comment và database object comment phải viết bằng tiếng Việt.
- Được giữ nguyên tên type/member/parameter, endpoint, role/permission, keyword hoặc thuật ngữ kỹ thuật chuẩn khi cần.
- Public contract, DTO, middleware, endpoint metadata, domain rule, security/tenant rule và transaction boundary nếu cần comment thì phải giải thích mục đích bằng tiếng Việt.
- Không thêm comment máy móc cho code tự rõ nghĩa. Nếu comment không giúp hiểu nghiệp vụ/rủi ro/kỹ thuật thì không thêm.
- Khi sửa đoạn có XML doc tiếng Anh trong phạm vi task, chuyển đoạn comment liên quan sang tiếng Việt.
- XML doc cho public type/member backend phải đủ `summary`, `param` và `returns` khi có liên quan:
  - `summary` nói rõ type/hàm làm gì, thuộc boundary/use case/domain rule nào.
  - `param` mô tả từng đầu vào dùng để làm gì; không để `param` rỗng.
  - `returns` bắt buộc với public method có return value, mô tả response/result/connection/query output trả về.
  - Với DTO/record/contract/event public, mô tả từng primary-constructor parameter quan trọng bằng `param`.
  - Không dùng comment tiếng Anh chung chung hoặc comment chỉ lặp lại tên hàm.

Ví dụ:

```csharp
/// <summary>
/// Mô tả yêu cầu role/permission tối thiểu mà endpoint cần để kiểm tra RBAC.
/// </summary>
/// <param name="Role">Role bắt buộc của user khi truy cập endpoint.</param>
/// <param name="Permission">Permission bắt buộc gắn với hành động nghiệp vụ.</param>
/// <param name="RequiresTenant">Cho biết endpoint có cần tenant context hợp lệ hay không.</param>
public sealed record AuthRbacRequirement(string Role, string Permission, bool RequiresTenant);
```

## 4. Configuration Rules

- Không để service chạy với connection string/config placeholder kiểu `${...}`.
- Startup phải resolve config rõ ràng hoặc fail-fast với lỗi dễ hiểu trước khi khởi tạo DB/cache/message bus.
- Shared config nếu dùng thì đặt thống nhất dưới `backend/shared/config/` hoặc một package shared đã được owner duyệt:
  - `backend/shared/config/appsettings.shared.json`
  - `backend/shared/config/appsettings.{Environment}.shared.json`
- Không hardcode absolute path trong `Program.cs` hoặc startup.
- Ưu tiên resolve path từ `builder.Environment.ContentRootPath` + `Path.GetFullPath(...)`.
- Config extension phải dùng tên Clinic SaaS, ví dụ `AddClinicSaaSConfig(...)`, không dùng tên project cũ.
- Validate required configs tại startup.

## 5. Naming Conventions

- Command/Query naming phải consistent với namespace và feature name.
- Controller parameters phải match với Command/Query properties.
- Async methods phải có suffix `Async`.
- SQL naming mặc định lowercase `snake_case` nếu owner chưa duyệt convention khác.
- Không đặt tên gây lệch domain phòng khám/bệnh viện.

Ví dụ:

```csharp
// Đúng
namespace ClinicSaaS.Tenant.Application.Commands.CreateTenant;
public record CreateTenantCommand(...) : IRequest<Result<Guid>>;
public class CreateTenantHandler : IRequestHandler<CreateTenantCommand, Result<Guid>>;

// Sai
namespace ClinicSaaS.Tenant.Application.Commands.CreateTenant;
public record RegisterClinicCommand(...);
public class CreateTenantHandler(...);
```

## 6. Command / Query Structure

- Handler chỉ orchestrate use case, không nhét toàn bộ validation/data access/business rule vào handler.
- Business rule thuộc Domain hoặc Application use case phù hợp.
- Business errors nên dùng typed result/error object nhất quán.
- Không expose domain entities trực tiếp ra API response; dùng response DTOs.
- Command ghi nhiều DB operations phải có transaction boundary rõ ràng.
- Query-only handler không cần transaction trừ khi có lý do cụ thể.

## 7. Validation Rules

- Validate input ở application boundary.
- Dùng FluentValidation hoặc validation mechanism thống nhất cho commands/queries.
- Nếu dùng CQRS pipeline, implement `ValidationBehavior`.
- Validation errors phải có format chuẩn, localize được nếu cần.
- Validation error trả 4xx rõ ràng, không rơi thành 500.

Ví dụ:

```csharp
public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.ClinicName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.OwnerEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.PlanCode).NotEmpty();
    }
}
```

## 8. Error Handling Rules

- Implement global exception middleware cho tất cả services.
- API layer trả error response nhất quán.
- Không trả stack trace ra API response ngoài môi trường Development.
- Chỉ log chi tiết lỗi ở server log.
- Không catch-all exception trong handler nếu global middleware đã xử lý.
- Chỉ catch khi cần cleanup, map lỗi domain cụ thể hoặc persist trạng thái cụ thể.
- Không nuốt exception đáng lẽ phải fail command.
- Không để endpoint public throw `NotImplementedException`; route chưa làm xong phải không expose hoặc trả status rõ ràng.

Error response gợi ý:

```csharp
public class ErrorResponse
{
    public string Code { get; set; } = default!;
    public string Message { get; set; } = default!;
    public Dictionary<string, string[]>? Details { get; set; }
}
```

## 9. Tenant Isolation

- Mọi tenant-owned command/query phải require tenant context.
- Không fallback sang hardcoded tenant id.
- Reject request thiếu hoặc sai tenant context.
- Tenant-owned table/collection phải có `tenant_id`.
- Tạo index cho query tenant-scoped phổ biến.
- Use case cross-tenant của Owner Super Admin phải explicit.
- Public Website tenant resolve bằng domain/subdomain.
- Clinic Admin chỉ truy cập tenant của họ.

## 10. Repository / Data Access Rules

- PostgreSQL cho transactional data.
- MongoDB cho CMS/layout/template JSON.
- Redis cho cache và locks.
- Kafka/Event Bus cho async workflows.
- Tenant Service hiện chốt dùng Dapper + Npgsql, không dùng EF Core và không dùng EF migrations.
- Repository methods phải async và nhận `CancellationToken` khi có I/O.
- Dùng connection factory hoặc DbContext factory thống nhất, không tạo connection tùy tiện trong handler.
- Log data access errors đủ context nhưng không log secret/token/password.
- Mọi raw SQL phải dùng parameterized query; không nối chuỗi trực tiếp từ input.
- Migration scripts phải rõ ràng và review được.
- Nếu dùng SQL migration thủ công, đặt file dạng `[version]_[description_snake_case].sql` hoặc `[timestamp]_[description_snake_case].sql` trong service tương ứng.
- Với Phase 2 Tenant MVP Backend, migration nằm trong `backend/services/tenant-service/src/TenantService.Infrastructure/Migrations/` và `infrastructure/postgres/init.sql` chỉ dùng mirror/bootstrap local.
- Nếu dùng EF Core, map table/column rõ theo convention `snake_case` để thống nhất DB.
- Quy tắc chi tiết về schema, comment DB, index và migration nằm trong `rules/database-rules.md`.

Ví dụ repository:

```csharp
public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid tenantId, CancellationToken ct = default);
    Task AddAsync(Tenant tenant, CancellationToken ct = default);
}
```

## 11. Dependency Injection Rules

- Database contexts, repositories, unit of work, tenant context nên scoped.
- Configuration/options object có thể singleton nếu immutable.
- External clients có thể singleton/typed client tùy SDK, nhưng phải thread-safe.
- Utility stateless có thể transient.
- Không inject Infrastructure của service khác vào service hiện tại.

## 12. Logging Rules

- Dùng structured logging.
- Log kèm correlation/request id khi có thể.
- Log levels:
  - `Debug`: chi tiết implementation, chỉ dùng khi cần.
  - `Information`: normal operations, user actions, successful completions.
  - `Warning`: business warning, retry, degraded path.
  - `Error`: application errors, exceptions, failed operations.
- Không log secret/token/password/connection string.
- Nếu dùng session-based logging, session id phải được enrich vào log entries và hiển thị khi service start.

Structured properties nên có:

```json
{
  "Service": "tenant-service",
  "CorrelationId": "...",
  "TenantId": "...",
  "RequestPath": "/api/tenants",
  "DurationMs": 97
}
```

## 13. Background Work Rules

- Dùng hosted/background services, queue consumers hoặc event consumers cho long-running work.
- Không tạo fire-and-forget task từ request handler nếu kiến trúc chưa cho phép.
- Không dùng `Task.Run` để giấu async work khỏi request lifecycle.
- Background worker phải log error và có retry/dead-letter strategy khi cần.
- Event-driven workflow phải publish domain/integration event rõ nghĩa.

## 14. Security Rules

- Validate tất cả input từ client.
- Authentication dùng JWT hoặc provider được owner duyệt.
- Authorization dùng role/module permission rõ ràng.
- Owner Super Admin cross-tenant phải explicit.
- Clinic Admin tenant-scoped phải enforce ở use case/data access.
- Refresh token nếu có phải rotation/revoke rõ ràng.
- Không đưa secrets vào repo.

## 15. Testing Expectations

- Khi test structure đã tồn tại, ưu tiên unit/integration/contract tests cho critical business logic và API endpoints.
- Khi chưa có test structure, phải có manual/API verification rõ ràng.
- Test naming gợi ý: `MethodName_Scenario_ExpectedResult`.
- Tenant isolation phải test cả allowed và forbidden path.
- API validation phải test invalid input trả 4xx.

## 16. Anti-Patterns Từ Review Cũ Cần Tránh

- Cross-service Infrastructure/repository injection.
- Fire-and-forget bằng `Task.Run` từ request handler.
- Rethrow sau khi set trạng thái lỗi khiến transaction rollback mất trạng thái đó.
- Command nhiều DB operations nhưng không có transaction boundary.
- Tenant/user/security context thiếu hoặc invalid nhưng fallback hardcoded.
- External payload/API response không null-check trước khi đọc property.
- Catch-all exception làm mất stack trace hoặc che lỗi retry.
- Endpoint public chưa xong nhưng throw `NotImplementedException`.
- Config thiếu nhưng service vẫn chạy với placeholder.

## 17. Checklist Trước Khi Submit Backend Code

- [ ] Đúng approved plan và scope.
- [ ] Không có cross-service infrastructure injection.
- [ ] Tenant-owned data có tenant context và `tenant_id`.
- [ ] Clinic Admin không thể truy cập tenant khác.
- [ ] Owner Super Admin cross-tenant là explicit use case.
- [ ] Handler không chứa business/data access quá mức.
- [ ] Command ghi nhiều DB operations có transaction boundary.
- [ ] External input/payload/API response đã null-check.
- [ ] Validation error trả 4xx.
- [ ] API không expose stack trace ngoài Development.
- [ ] Không có `NotImplementedException` ở public route.
- [ ] Raw SQL parameterized.
- [ ] Log có correlation/request context, không log secret.
- [ ] Đã report lại file sửa, kiểm tra, blocker, bước tiếp theo.
