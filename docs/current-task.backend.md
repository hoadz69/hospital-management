# Current Task Backend/DevOps - Active Dashboard

Ngay cap nhat: 2026-05-14

## Vai tro file

File nay chi giu trang thai active cua Backend/DevOps lane. Lich su chi tiet o `docs/archive/backend-history-2026-05.md` la cold storage, chi doc khi owner yeu cau ro hoac active file tro toi section archive cu the.

## Active State

- Phase 2 Tenant API: **DB that Done/Verified** qua PostgreSQL: create/list/detail/status/duplicate conflict.
- Owner Plan APIs `/api/owner/*`: **DB that Done/Verified** ngay 2026-05-13. Tenant Service so huu PostgreSQL persistence; API Gateway chi forward, khong truy DB.
- Domain DNS/SSL API toi thieu: **Implemented backend + local build/test PASS, runtime smoke pending** ngay 2026-05-13.
- Domain persistence status: **PostgreSQL/Dapper implementation** trong Tenant Service voi migration `0003_add_tenant_domain_dns_ssl_state.sql`; chua bao DB Done vi chua apply/smoke duoc runtime trong session nay.
- API Gateway forward-only cho 3 endpoint FE dung de bo mock:
  - `GET /api/tenants/{tenantId}/domains`
  - `POST /api/tenants/{tenantId}/domains/{domainId}/dns-retry`
  - `GET /api/tenants/{tenantId}/domains/{domainId}/ssl-status`
- Response item co field: `domainId`, `domainName`, `dnsStatus`, `dnsRecords`, `lastCheckedAt`, `retryCount`, `nextRetryAt`, `sslStatus`, `sslIssuer`, `expiresAt`, `message`.
- Guard: tenant context bat buoc, route tenant phai khop `X-Tenant-Id`, OwnerSuperAdmin role enforced khi co header/claims role; gateway khong truy DB.
- Template/Website CMS/Public/Clinic Admin API ngoai scope nay van thieu spec/source-of-truth that.
- Khong commit/stage/push neu owner chua yeu cau ro.

## Latest Verify Snapshot

- `gitnexus analyze`: PASS ngay 2026-05-13, index refreshed.
- `git diff --check`: PASS ngay 2026-05-13, chi co warning LF/CRLF tren Windows.
- Backend restore/build/test bang `C:\Program Files\dotnet\dotnet.exe`:
  - restore PASS.
  - build PASS, 0 warning/error.
  - test PASS 35/35.
- `docker compose -f infrastructure/docker/docker-compose.dev.yml config`: PASS.
- Runtime smoke qua gateway: **SKIPPED/PENDING** trong session nay vi Docker daemon local offline va khong co `DEPLOY_HOST`/`API_GATEWAY_URL` env de chay server smoke.
- QA/checkpoint 2026-05-14: rerun restore/build/test PASS 35/35; diff secret scan khong thay private key/token/connection string that. QA doc lap danh dau **FAIL push-gate** vi runtime smoke DB/gateway va migration 0003 tren DB runtime van pending.

## API Inventory

| Nhom | Endpoint FE dang goi/mock | Trang thai backend |
|---|---|---|
| Tenant | `GET/POST /api/tenants`, `GET/PATCH /api/tenants/{id}` | DB that Done/Verified. |
| Owner Plan | `GET /api/owner/plans`, `GET /api/owner/modules`, `GET /api/owner/tenant-plan-assignments`, `POST /api/owner/tenant-plan-assignments/bulk-change` | DB that Done/Verified. |
| Domain DNS/SSL | `GET /api/tenants/{tenantId}/domains`, `POST /api/tenants/{tenantId}/domains/{domainId}/dns-retry`, `GET /api/tenants/{tenantId}/domains/{domainId}/ssl-status` | Implemented Tenant Service PostgreSQL/Dapper + gateway forward; runtime DB smoke pending. |
| Domain legacy contract | `GET /domains/{domainId}`, `POST /domains`, `/verify`, `/verify-status`, `/publish` under tenant route | Con lai contract stub trong `Phase4ContractEndpoints`; khong danh dau DB Done. |
| Template | Khong thay FE API client that | Gateway contract stub, thieu FE need/spec that. |
| Website CMS/settings/slider | Khong thay FE API client that; public-web dang dung `mockTenant` | Gateway contract stub, thieu FE need/spec that. |
| Public/Clinic Admin | App placeholder dung `mockTenant`, chi co HTTP client context | Chua co endpoint FE can/spec that. |

## Files Changed/Reviewed

Backend/domain files changed in latest slice:

```txt
backend/shared/contracts/Domains/DomainContracts.cs
backend/services/tenant-service/src/TenantService.Api/Endpoints/TenantDomainOperationsEndpoints.cs
backend/services/tenant-service/src/TenantService.Api/Program.cs
backend/services/tenant-service/src/TenantService.Application/DependencyInjection.cs
backend/services/tenant-service/src/TenantService.Application/Domains/ITenantDomainOperationsRepository.cs
backend/services/tenant-service/src/TenantService.Application/Domains/TenantDomainOperationErrors.cs
backend/services/tenant-service/src/TenantService.Application/Domains/TenantDomainOperationsHandler.cs
backend/services/tenant-service/src/TenantService.Infrastructure/DependencyInjection.cs
backend/services/tenant-service/src/TenantService.Infrastructure/Migrations/0003_add_tenant_domain_dns_ssl_state.sql
backend/services/tenant-service/src/TenantService.Infrastructure/Persistence/DapperTenantDomainOperationsRepository.cs
backend/services/tenant-service/tests/TenantService.Tests/TenantDomainOperationsEndpointMetadataTests.cs
backend/services/tenant-service/tests/TenantService.Tests/TenantDomainOperationsHandlerTests.cs
backend/services/api-gateway/src/ApiGateway.Api/Endpoints/Phase4ContractEndpoints.cs
backend/services/api-gateway/src/ApiGateway.Application/Tenants/ITenantServiceClient.cs
backend/services/api-gateway/src/ApiGateway.Infrastructure/Tenants/TenantServiceClient.cs
infrastructure/postgres/init.sql
```

Owner-plan persistence files from previous slice are still dirty/uncommitted and were preserved.

## Active Blockers / Caveats

- Runtime smoke required by owner (`200`, `403`, `400/404`) is pending until Docker daemon or server smoke env is available.
- Migration 0003 must be applied before running Domain DNS/SSL API against an existing DB.
- Domain DNS/SSL implementation stores operational state only; no real DNS resolver, ACME, queue, or background worker is implemented in this slice.
- `infrastructure/postgres/init.sql` has 0003 addendum for local bootstrap; owner-plan 0002 mirror caveat from prior slice remains.

## Next Step

1. Khi co Docker/server smoke, apply migration 0003, restart Tenant Service + API Gateway, smoke 3 endpoint qua gateway.
2. FE co the thay mock bang 3 endpoint Domain DNS/SSL tren; van can gui `X-Tenant-Id` khop route tenant.
3. Neu owner yeu cau commit, split backend/database/gateway/docs hop ly, khong stage generated artifacts.

## Archive Index

- Full backend current-task history truoc cleanup: `docs/archive/backend-history-2026-05.md`.
- Full backend plan history truoc cleanup: `temp/archive/plan.backend.history.md`.
