# Current Task - Verify Phase 2 Tenant MVP Backend Staged Changes

Ngày cập nhật: 2026-05-09

## Trạng Thái

🟡 Verify Partial Pass - DB Runtime Smoke Blocked

## Yêu Cầu Của Owner

Verify Phase 2 Tenant MVP Backend staged changes theo `temp/plan.md` và handoff context.

Không implement thêm feature mới, không revert staged changes, không commit, không sửa frontend, không tạo Figma file mới.

## Phase Hiện Tại

Phase 2 - Tenant MVP Backend.

Phase 1.3 API Boundary Standardization đã Done và đã verify.

Phase 2 staged implementation hiện có trong git index và đã được verify ở mức restore/build/docker config/startup smoke. Chưa thể xác nhận Done vì full CRUD smoke qua PostgreSQL bị chặn bởi Docker daemon không chạy.

## Staged Changes Đã Verify

Các nhóm file staged chính:

```txt
backend/services/tenant-service/src/TenantService.Domain/Tenants/*
backend/services/tenant-service/src/TenantService.Application/Tenants/*
backend/services/tenant-service/src/TenantService.Infrastructure/Persistence/*
backend/services/tenant-service/src/TenantService.Infrastructure/Migrations/0001_create_tenant_mvp.sql
backend/services/tenant-service/src/TenantService.Api/Endpoints/TenantEndpoints.cs
backend/services/api-gateway/src/ApiGateway.Application/Tenants/*
backend/services/api-gateway/src/ApiGateway.Infrastructure/Tenants/*
backend/services/api-gateway/src/ApiGateway.Api/Endpoints/TenantEndpoints.cs
backend/shared/contracts/Tenancy/*
infrastructure/postgres/init.sql
docs/commands/implement-phase.md
ai gen/clinic_saas_full_handoff_context.md
```

Ghi chú worktree:

```txt
ai gen/clinic_saas_full_handoff_context.md đang ở trạng thái AM:
- staged base từ trước
- có unstaged bổ sung handoff ở lượt tài liệu gần nhất
```

Không revert các staged/unstaged changes nếu owner chưa yêu cầu.

## Verify Đã Chạy

Git:

```powershell
git status --short
git diff --cached --stat
git diff --cached --name-only
```

Kết quả:

```txt
Staged changes: 40 files, 3101 insertions, 9 deletions.
Có Phase 2 Tenant MVP Backend staged changes.
```

.NET:

```powershell
dotnet restore backend/ClinicSaaS.Backend.sln
```

Kết quả:

```txt
Fail do PATH đang trỏ C:\Program Files (x86)\dotnet\dotnet.exe và không thấy SDK.
```

Chạy lại bằng SDK x64:

```powershell
& 'C:\Program Files\dotnet\dotnet.exe' restore backend/ClinicSaaS.Backend.sln
& 'C:\Program Files\dotnet\dotnet.exe' build backend/ClinicSaaS.Backend.sln --no-restore
& 'C:\Program Files\dotnet\dotnet.exe' test backend/ClinicSaaS.Backend.sln --no-build
```

Kết quả:

```txt
restore: pass.
build: pass, 0 warnings, 0 errors.
test: exit code 0, nhưng test projects hiện là placeholder chưa có test framework/test cases thực tế.
```

Docker compose:

```powershell
docker compose -f infrastructure/docker/docker-compose.dev.yml config
```

Kết quả:

```txt
pass.
```

Runtime startup smoke:

```txt
tenant-service:
- /health: 200
- /openapi/v1.json: 200

api-gateway:
- /health: 200
- /openapi/v1.json: 200
```

Full Tenant CRUD smoke:

```txt
Blocked.
Docker daemon không chạy:
open //./pipe/dockerDesktopLinuxEngine: The system cannot find the file specified.

Vì không có PostgreSQL local đang chạy, chưa thể verify:
- POST /api/tenants
- GET /api/tenants
- GET /api/tenants/{id}
- PATCH /api/tenants/{id}/status
- duplicate slug/domain -> 409
```

## Scope Đã Giữ

Không implement thêm feature mới.
Không sửa frontend.
Không tạo Figma file mới.
Không commit.
Không revert staged changes.

## Còn Thiếu / Bị Chặn

Phase 2 chưa nên đánh dấu Done cho tới khi chạy được DB runtime smoke thật:

```txt
1. Bật Docker Desktop hoặc chuẩn bị PostgreSQL local.
2. Apply schema từ infrastructure/postgres/init.sql hoặc migration SQL.
3. Run tenant-service và api-gateway.
4. Smoke:
   - POST /api/tenants
   - GET /api/tenants
   - GET /api/tenants/{id}
   - PATCH /api/tenants/{id}/status
   - duplicate slug/domain trả 409
5. Sau khi pass, cập nhật docs/current-task.md và roadmap sang Done cho Phase 2.
```

## Bước Tiếp Theo

Đề xuất bước tiếp theo:

```txt
Chạy DB runtime smoke cho Phase 2 sau khi Docker/PostgreSQL local sẵn sàng.
```

Không chuyển sang feature mới trước khi owner quyết định xử lý phần DB smoke hoặc chấp nhận trạng thái verify partial.
