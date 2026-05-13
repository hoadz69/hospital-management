# Kế Hoạch Backend/DevOps - Living Active Plan

Ngày cập nhật: 2026-05-13

## Vai Trò File

File này là active plan để agent mới resume Backend/DevOps mà không phải đọc archive. Không dùng file này làm lịch sử append-only; sau mỗi lượt chỉ cập nhật trạng thái hiện tại, điểm dừng, bước tiếp theo, verify và blocker.

Archive chi tiết: `temp/archive/plan.backend.history.md`. Chỉ đọc archive khi owner yêu cầu rõ, active plan trỏ tới section cụ thể, hoặc cần bằng chứng debug cũ.

## Current Active Slice

**No active implementation slice approved.**

Backend hiện không có task code mới được duyệt để làm ngay.

- A.10: **Blocked**, thiếu spec thật.
- Owner-plan persistence: **Plan ready / approval gate**, chưa được duyệt implement.
- `/api/owner/*`: đã sẵn sàng cho FE A9 ở mức contract/stub BE A.2/A.3.

Nếu owner nói "tiếp tục backend" mà không chỉ rõ task, agent không được tự tạo endpoint mới. Phải chốt một trong các hướng sau:

1. Owner duyệt owner-plan persistence -> implement theo approval gate bên dưới.
2. Owner giao A.10 -> trước tiên cần spec thật; nếu chưa có thì report blocker.
3. Owner yêu cầu verify/smoke BE thật -> chạy verify/smoke, không sửa code nếu chỉ verify.
4. Task backend khác -> thêm active slice cụ thể vào file này trước khi implement nếu chưa có plan duyệt.

## Last Stopping Point

- Phase 2 Tenant API runtime smoke: Done/Verified.
- BE A.2 Owner Plan Module contract/stub: Done/Verified.
- BE A.3 contract hardening + FE A9 support: Done/Verified.
- A.10 preparation: baseline verify PASS, nhưng không implement vì thiếu spec.
- Owner-plan persistence: chỉ mới plan approval, chưa migration/schema/repository.
- Không có backend implementation đang dở.

## Progress Snapshot

| Hạng mục | Trạng thái | Ghi chú |
|---|---|---|
| Phase 2 Tenant API | Done/Verified | create/list/detail/status/duplicate conflict đã PASS qua PostgreSQL/server test theo handoff gần nhất. |
| BE A.2 `/api/owner/*` | Done/Verified | Tenant Service + API Gateway expose 4 route; không migration/persistence. |
| BE A.3 hardening | Done/Verified | Tests/metadata/validation/403 guard PASS; giữ response shape cho FE A9. |
| Owner-plan persistence | Approval gate | Chưa tạo migration/schema/repository. |
| A.10 | Blocked | Chưa có service/path/request-response/acceptance/spec thật. |

## A.10 Blocker

A.10 chưa được định nghĩa trong source of truth. Trước khi implement cần chốt:

1. Service owner.
2. Endpoint path/method.
3. Request/response contract.
4. Guard/tenant scope.
5. Persistence/transaction/409 hay chỉ stub.
6. Acceptance criteria và smoke case.

Các endpoint dưới đây là A.2/A.3, **không phải A.10**:

```txt
GET /api/owner/plans
GET /api/owner/modules
GET /api/owner/tenant-plan-assignments
POST /api/owner/tenant-plan-assignments/bulk-change
```

Không được tự tạo endpoint mới hoặc bịa contract A.10 chỉ dựa trên tên task.

## Owner-Plan Persistence Approval Gate

Chỉ implement khi owner nói rõ đã duyệt. Mục tiêu khi được duyệt:

- Implement persistence thật phía sau 4 endpoint `/api/owner/*` hiện có.
- Tenant Service sở hữu plan catalog, module entitlement và tenant-plan assignment.
- API Gateway không truy DB, chỉ forwarding/typed client tới Tenant Service.
- Giữ response contract hiện tại để không phá FE A8/A9.
- Bulk-change phải có transaction boundary rõ.

Không làm trước khi được duyệt:

- Không tạo migration 0002.
- Không tạo repository/schema owner-plan persistence.
- Không kéo Billing/payment provider vào scope.
- Không sửa frontend contract nếu owner chưa duyệt riêng.

Thiết kế chi tiết nằm ở archive section `16. Backend Phase 4 Wave B - Owner Plan/Module Persistence Preparation`.

## Known Touched / Resume Files

Các file baseline đã được rà/verify trong A.2/A.3/A.10 preparation:

```txt
backend/services/tenant-service/src/TenantService.Api/Endpoints/OwnerPlanCatalogEndpoints.cs
backend/services/api-gateway/src/ApiGateway.Api/Endpoints/OwnerPlanCatalogContractEndpoints.cs
backend/services/tenant-service/tests/TenantService.Tests/OwnerPlanCatalogStubHandlerTests.cs
backend/services/tenant-service/tests/TenantService.Tests/OwnerPlanCatalogEndpointMetadataTests.cs
```

## Allowed Areas Khi Có Slice Mới

Chỉ mở rộng các vùng này khi active slice mới đã ghi rõ scope:

```txt
backend/shared/contracts/**
backend/services/tenant-service/src/TenantService.Api/Endpoints/**
backend/services/tenant-service/src/TenantService.Application/Plans/**
backend/services/tenant-service/src/TenantService.Application/Tenants/**
backend/services/tenant-service/src/TenantService.Infrastructure/Persistence/**
backend/services/tenant-service/src/TenantService.Infrastructure/Migrations/**
backend/services/tenant-service/tests/**
backend/services/api-gateway/src/ApiGateway.Api/Endpoints/**
backend/services/api-gateway/tests/**
docs/current-task.backend.md
temp/plan.backend.md
```

Không sửa frontend/Figma/Billing/payment provider nếu owner không giao rõ.

## Acceptance Criteria

Trước khi implement slice mới, phải bổ sung acceptance criteria riêng cho slice đó vào file này. Default tối thiểu:

- `git diff --check` PASS.
- Restore/build/test backend PASS nếu sửa backend code.
- Owner/platform endpoint phải guard đúng OwnerSuperAdmin; ClinicAdmin không bypass được bằng `X-Tenant-Id`.
- Tenant-owned data phải có tenant context hợp lệ.
- DB write nhiều bước phải có transaction boundary.
- API Gateway không truy DB trực tiếp.
- Response contract thay đổi phải ghi rõ impact frontend và cập nhật plan trước khi code.

## Verify Plan

Docs-only/no-code:

```txt
git diff --check
```

Backend code:

```txt
git diff --check
dotnet restore backend/ClinicSaaS.Backend.sln
dotnet build backend/ClinicSaaS.Backend.sln --no-restore
dotnet test backend/ClinicSaaS.Backend.sln --no-build
```

Runtime smoke chỉ chạy khi task yêu cầu và runtime/env phù hợp:

```txt
GET /health
GET /openapi/v1.json
GET /api/owner/plans
GET /api/owner/modules
GET /api/owner/tenant-plan-assignments
POST /api/owner/tenant-plan-assignments/bulk-change
Negative: ClinicAdmin 403, invalid owner role 403, invalid/missing bulk-change request 400
Tenant API smoke nếu task chạm tenant persistence
```

## Blockers / Caveats

- A.10 thiếu spec thật.
- Owner-plan persistence/schema chưa được duyệt; A.2/A.3 hiện là contract/stub.
- 409 conflict cho owner-plan assignment chưa áp dụng trong stub; chỉ cover khi persistence được duyệt.
- Không đánh dấu Done bằng stub/mock nếu server test có thể chạy API thật.

## Archive Index

- Full backend plan history trước cleanup: `temp/archive/plan.backend.history.md`.
- Full backend current-task history trước cleanup: `docs/archive/backend-history-2026-05.md`.
- Owner-plan persistence design: archive section `16. Backend Phase 4 Wave B`.
- A.3 evidence: archive section `17. BE A.3 Contract Hardening`.
- A.10 blocker evidence: archive section `18. Backend/DevOps Phase 4 Task A.10`.
