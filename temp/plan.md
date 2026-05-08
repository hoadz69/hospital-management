# Plan - Clinic SaaS Current Task

Ngay: 2026-05-09
Trang thai: Da dong bo tai lieu/rules/prompt theo structure hien tai
Pham vi: Docs/rules/prompt/FigJam note, khong implement business logic sau

## 1. Structure Da Scaffold

```txt
frontend/
  apps/
    public-web/
    clinic-admin/
    owner-admin/
  packages/
    ui/
    design-tokens/
    api-client/
    shared-types/
    config/

backend/
  services/
    api-gateway/
    identity-service/
    tenant-service/
    website-cms-service/
    template-service/
    domain-service/
    booking-service/
    catalog-service/
    customer-service/
    billing-service/
    report-service/
    notification-service/
    realtime-gateway/
  shared/
    building-blocks/
    contracts/
    observability/

infrastructure/
  docker/
  postgres/
  mongodb/
  redis/
  kafka/
  scripts/

docs/
  architecture/
  api/
  setup/
```

## 2. Nguyen Tac Da Giu

- Khong dung root-level `apps/`, `packages/`, `services/`.
- Khong implement business logic sau.
- Khong noi database that.
- Khong code full UI chi tiet.
- Khong tao Figma file moi.
- Khong commit.
- Moi placeholder lien quan tenant-owned data deu ghi ro tenant context/`tenant_id`.

## 3. Frontend Skeleton

- `frontend/package.json`: npm workspace cho frontend.
- `frontend/apps/public-web`: tenant-aware public app shell.
- `frontend/apps/clinic-admin`: tenant-scoped admin shell.
- `frontend/apps/owner-admin`: platform-scoped owner shell.
- `frontend/packages/ui`: `AppButton`, `StatusPill` placeholder.
- `frontend/packages/design-tokens`: token constants placeholder.
- `frontend/packages/api-client`: tenant context guard placeholder.
- `frontend/packages/shared-types`: shared tenant mock/type placeholder.
- `frontend/packages/config`: shared frontend config placeholder.

## 4. Backend Skeleton

- `backend/services/*`: moi service co README, `src/*Api`, `src/*Application`, `src/*Domain`, `src/*Infrastructure`, `tests`.
- `backend/shared/building-blocks`: shared backend primitive placeholder.
- `backend/shared/contracts`: shared DTO/event contract placeholder.
- `backend/shared/observability`: logging/tracing/metrics convention placeholder.

## 5. Infrastructure Skeleton

- `infrastructure/docker/docker-compose.dev.yml`: local placeholder cho PostgreSQL, MongoDB, Redis, Kafka.
- `infrastructure/docker/docker-compose.prod.yml`: production compose placeholder.
- `infrastructure/docker/nginx/default.conf`: reverse proxy placeholder.
- `infrastructure/postgres/init.sql`: schema/table placeholder co `tenant_id` index.
- `infrastructure/mongodb/init.js`: collection/index placeholder co `tenant_id`.
- `infrastructure/redis/redis.conf`: Redis local config.
- `infrastructure/kafka/topics.dev.json`: topic placeholder.
- `infrastructure/kafka/create-topics.sh`: topic creation placeholder.
- `infrastructure/scripts/*.ps1`: local dev scripts placeholder.

## 6. Docs Skeleton

- `docs/architecture/overview.md`
- `docs/architecture/service-boundaries.md`
- `docs/architecture/tenant-isolation.md`
- `docs/api/README.md`
- `docs/setup/local-development.md`
- `docs/setup/troubleshooting.md`

## 7. Verify Da Chay

Frontend:

```powershell
cd frontend
npm install
npm run typecheck
npm run build
```

Infrastructure:

```powershell
docker compose -f infrastructure/docker/docker-compose.dev.yml config
docker compose -f infrastructure/docker/docker-compose.prod.yml config
```

File scaffold:

```powershell
rg --files frontend backend infrastructure docs
```

Ket qua:

- `rg --files frontend backend infrastructure docs`: pass.
- `docker compose -f infrastructure/docker/docker-compose.dev.yml config`: pass.
- `docker compose -f infrastructure/docker/docker-compose.prod.yml config`: pass.
- `cd frontend; npm install`: pass, 0 vulnerabilities theo npm audit summary.
- `cd frontend; npm run typecheck`: pass cho `clinic-admin`, `owner-admin`, `public-web`.
- `cd frontend; npm run build`: pass cho `clinic-admin`, `owner-admin`, `public-web`.
- Da don `frontend/node_modules` va `dist` generated sau verify; giu `frontend/package-lock.json`.

## 8. Ghi Chu Worktree

- `temp/plan.md` co the bi git ignore theo cau hinh hien tai nen can kiem tra rieng neu can track.
- Cac file modified co san tu truoc nhu `AGENTS.md`, `CLAUDE.md`, `architech.txt`, `clinic_saas_report.md`, `docs/codex-setup.md` khong duoc sua trong buoc scaffold nay.

## 9. Docs Sync Da Hoan Thanh

Muc tieu:

- Doi cac reference root-level `apps/`, `packages/`, `services/` sang `frontend/apps/`, `frontend/packages/`, `backend/services/`.
- Doi legacy report path, legacy lead-agent file reference va link FigJam cu sang source hien tai.
- Giu nguyen Clinic SaaS Multi Tenant va tech stack da chot.
- Khong sua code skeleton, khong tao Figma file moi, khong implement business logic, khong commit.

Figma/FigJam:

- Da search 2 FigJam board va UI design file bang MCP; khong thay text node con chua structure cu.
- Da them note `Current Repo Structure - Clinic SaaS` vao Technical Architecture FigJam de dong nhat structure hien tai.

Verify:

- Legacy report/playbook/link cu: khong con match.
- Structure scan: chi con match trong tree hop le, full path hien tai, route public `/services/:slug`, hoac rule cam root-level structure.
- Khong chay build/typecheck vi task chi sua docs/rules/prompt.

## 10. Buoc Tiep Theo De Xuat

1. Tao .NET project files thuc te cho phase 1: `api-gateway`, `identity-service`, `tenant-service`.
2. Mo rong frontend routing/layout placeholder theo Figma cho 3 apps.
3. Them test/checklist tenant isolation va routing smoke test.

## 11. Backend Phase 1 Plan

Ngay: 2026-05-09
Trang thai: Da hoan thanh backend phase 1 skeleton
Pham vi: chi tao .NET solution/project skeleton that cho:

- `backend/services/api-gateway`
- `backend/services/identity-service`
- `backend/services/tenant-service`

Success criteria:

- Moi service co solution file rieng va cac project:
  - `src/<Service>.Api`
  - `src/<Service>.Application`
  - `src/<Service>.Domain`
  - `src/<Service>.Infrastructure`
  - `tests/<Service>.Tests`
- Project references dung Clean Architecture:
  - Api -> Application, Infrastructure
  - Infrastructure -> Application
  - Application -> Domain
  - Tests -> Domain/Application
- Api co placeholder cho tenant context middleware, auth/RBAC, OpenAPI, health check.
- Infrastructure co PostgreSQL config placeholder, khong tao connection that.
- Khong secret, khong Figma file moi, khong commit, khong sua frontend routing/layout.

Runtime note:

- Tai lieu cu co nhac `.NET 11` la dinh huong ban dau, nhung local x64 SDK hien co la .NET SDK 9.0.304.
- De restore/build duoc tren may hien tai, skeleton phase 1 dung `net9.0`.
- Khi owner cai SDK .NET LTS moi hon, co the nang `TargetFramework` sau bang mot task rieng.

Verify du kien:

```powershell
& 'C:\Program Files\dotnet\dotnet.exe' restore backend/services/api-gateway/ApiGateway.sln
& 'C:\Program Files\dotnet\dotnet.exe' build backend/services/api-gateway/ApiGateway.sln --no-restore
& 'C:\Program Files\dotnet\dotnet.exe' restore backend/services/identity-service/IdentityService.sln
& 'C:\Program Files\dotnet\dotnet.exe' build backend/services/identity-service/IdentityService.sln --no-restore
& 'C:\Program Files\dotnet\dotnet.exe' restore backend/services/tenant-service/TenantService.sln
& 'C:\Program Files\dotnet\dotnet.exe' build backend/services/tenant-service/TenantService.sln --no-restore
```

Verify da chay them:

```powershell
& 'C:\Program Files\dotnet\dotnet.exe' restore backend/ClinicSaaS.Backend.sln
& 'C:\Program Files\dotnet\dotnet.exe' build backend/ClinicSaaS.Backend.sln --no-restore
```

Ket qua:

- Restore/build pass cho 3 service solutions.
- Restore/build pass cho `backend/ClinicSaaS.Backend.sln`.
- Build root solution: 0 warnings, 0 errors.

## 12. Shared Backend Primitives Plan

Ngay: 2026-05-09
Trang thai: Owner yeu cau tao shared backend primitives va ap dung toi thieu vao phase 1 services
Pham vi:

- `backend/shared/building-blocks`
- `backend/shared/contracts`
- `backend/shared/observability`
- `backend/services/api-gateway`
- `backend/services/identity-service`
- `backend/services/tenant-service`

Success criteria:

- Tao project `ClinicSaaS.BuildingBlocks`, `ClinicSaaS.Contracts`, `ClinicSaaS.Observability`, target `net9.0`.
- BuildingBlocks co placeholder:
  - tenant context/accessor
  - tenant resolution result
  - Result/Error model
  - guard/validation helper
  - options pattern placeholder
- Contracts co placeholder:
  - TenantReference DTO
  - UserContext DTO
  - role/permission constants
  - domain event base
  - TenantCreated event
  - auth/RBAC contract
- Observability co placeholder:
  - logging constants
  - correlation id middleware
  - trace context
  - health check tag constants
- 3 phase 1 services reference shared projects where hop ly.
- Khong implement business logic sau, khong connect PostgreSQL that, khong secret, khong frontend, khong Figma, khong commit.

Verify du kien:

```powershell
& 'C:\Program Files\dotnet\dotnet.exe' restore backend/ClinicSaaS.Backend.sln
& 'C:\Program Files\dotnet\dotnet.exe' build backend/ClinicSaaS.Backend.sln --no-restore
```

Verify da chay:

- Restore pass cho `backend/ClinicSaaS.Backend.sln`.
- Build pass cho `backend/ClinicSaaS.Backend.sln`.
- Build result: 0 warnings, 0 errors.

Ket qua implementation:

- Tao `ClinicSaaS.BuildingBlocks`, `ClinicSaaS.Contracts`, `ClinicSaaS.Observability`.
- 3 service phase 1 da reference shared projects:
  - Api -> BuildingBlocks, Contracts, Observability
  - Application -> BuildingBlocks, Contracts
  - Infrastructure -> BuildingBlocks
  - Tests -> BuildingBlocks
- Local duplicate tenant context/RBAC/PostgreSQL options placeholders trong 3 services da thay bang shared types.
