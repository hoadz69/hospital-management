# Ke Hoach Backend/DevOps - Living Active Plan

Ngay cap nhat: 2026-05-14

## Vai Tro File

File nay la active plan de agent moi resume Backend/DevOps ma khong phai doc archive. Khong dung file nay lam lich su append-only; sau moi luot chi cap nhat trang thai hien tai, diem dung, buoc tiep theo, verify va blocker.

Archive chi tiet: `temp/archive/plan.backend.history.md`. Chi doc archive khi owner yeu cau ro, active plan tro toi section cu the, hoac can bang chung debug cu.

## Current Active Slice

**Domain DNS/SSL API toi thieu implemented backend ngay 2026-05-13; runtime smoke pending.**

Owner da go blocker Domain cho scope toi thieu de FE thay mock. Domain Service hien van scaffold/stub, nen implementation dat trong Tenant Service vi da co `platform.tenant_domains`; API Gateway chi forward sang Tenant Service, khong truy DB.

## API Contract FE Dung De Bo Mock

| Method | Path | Guard | Status |
|---|---|---|---|
| GET | `/api/tenants/{tenantId}/domains` | `X-Tenant-Id` khop route + OwnerSuperAdmin | Implemented gateway forward -> Tenant Service. |
| POST | `/api/tenants/{tenantId}/domains/{domainId}/dns-retry` | `X-Tenant-Id` khop route + OwnerSuperAdmin | Implemented: validate tenant/domain, tang `retryCount`, cap nhat `lastCheckedAt`, `nextRetryAt`, `message`. |
| GET | `/api/tenants/{tenantId}/domains/{domainId}/ssl-status` | `X-Tenant-Id` khop route + OwnerSuperAdmin | Implemented: tra SSL state hien tai. |

Response item/list item:

```txt
domainId
domainName
dnsStatus
dnsRecords
lastCheckedAt
retryCount
nextRetryAt
sslStatus
sslIssuer
expiresAt
message
```

## Persistence / Stub Status

- **Persistence implemented, runtime DB smoke pending.**
- Tenant Service migration: `0003_add_tenant_domain_dns_ssl_state.sql`.
- Table extended: `platform.tenant_domains` them `dns_status`, `dns_records`, `last_checked_at_utc`, `retry_count`, `next_retry_at_utc`, `ssl_status`, `ssl_issuer`, `expires_at_utc`, `status_message`.
- Repository: Dapper/Npgsql `DapperTenantDomainOperationsRepository`.
- Local bootstrap: `infrastructure/postgres/init.sql` co 0003 addendum cho domain DNS/SSL.
- Khong co DNS resolver/ACME/background worker that trong slice nay; API ghi state retry de FE bo mock.
- Cac route Domain legacy con lai trong `Phase4ContractEndpoints` (`GET /domains/{domainId}`, register, verify, verify-status, publish) van la contract stub va khong duoc tinh DB Done.

## API Inventory Theo FE Hien Tai

| Nhom | Endpoint FE dang goi/mock | Trang thai backend |
|---|---|---|
| Tenant | `GET/POST /api/tenants`, `GET/PATCH /api/tenants/{id}` | DB that Done/Verified tu Phase 2. |
| Owner Plan | `GET /api/owner/plans`, `GET /api/owner/modules`, `GET /api/owner/tenant-plan-assignments`, `POST /api/owner/tenant-plan-assignments/bulk-change` | DB that PASS ngay 2026-05-13. |
| Domain DNS/SSL | 3 endpoint tren `/api/tenants/{tenantId}/domains...` | Implemented PostgreSQL/Dapper + gateway forward; runtime smoke pending. |
| Template | Khong thay FE API client that | Gateway contract stub, thieu FE need/spec that. |
| Website CMS/settings/slider | Khong thay FE API client that; public-web dang dung `mockTenant` | Gateway contract stub, thieu FE need/spec that. |
| Public/Clinic Admin | App placeholder dung `mockTenant`, chi co HTTP client context | Chua co endpoint FE can/spec that. |

## Last Stopping Point

- Phase 2 Tenant API runtime smoke: Done/Verified.
- Owner-plan persistence: Done/Verified ngay 2026-05-13.
- Domain DNS/SSL API: code + migration + tests PASS; runtime smoke pending vi Docker daemon local offline va khong co server env trong session.
- A.10: Blocked vi thieu spec.

## Known Touched / Resume Files

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
docs/current-task.backend.md
temp/plan.backend.md
docs/current-task.md
```

Owner-plan files from previous dirty slice are still part of worktree and must not be reverted.

## Acceptance / Verify Snapshot

```txt
gitnexus analyze: PASS
git diff --check: PASS (LF/CRLF warnings only)
C:\Program Files\dotnet\dotnet.exe restore backend/ClinicSaaS.Backend.sln: PASS
C:\Program Files\dotnet\dotnet.exe build backend/ClinicSaaS.Backend.sln --no-restore: PASS, 0 warning/error
C:\Program Files\dotnet\dotnet.exe test backend/ClinicSaaS.Backend.sln --no-build: PASS, 35/35 tests
docker compose -f infrastructure/docker/docker-compose.dev.yml config: PASS
docker ps: FAIL, Docker daemon offline
server smoke env: missing DEPLOY_HOST/API_GATEWAY_URL
QA/checkpoint 2026-05-14: rerun restore/build/test PASS 35/35; diff secret scan khong thay private key/token/connection string that. QA doc lap danh dau FAIL push-gate vi runtime smoke DB/gateway va migration 0003 tren DB runtime van pending.
```

Runtime smoke pending:

```txt
GET /api/tenants/{tenantId}/domains via gateway: pending
POST /api/tenants/{tenantId}/domains/{domainId}/dns-retry via gateway: pending
GET /api/tenants/{tenantId}/domains/{domainId}/ssl-status via gateway: pending
Negative 403 wrong role: pending
Negative 400/404 invalid tenant/domain: pending
```

## Blockers / Caveats

- Chua apply migration 0003 vao DB runtime trong session nay.
- Chua chay gateway smoke vi Docker daemon local offline va khong co server smoke env.
- Domain DNS/SSL API chua lam DNS resolver/SSL provisioning thật; chi persist state/toi thieu de FE bo mock.
- Template/Website CMS/Public/Clinic Admin API van ngoai scope.
- Khong dung stub/mock de danh dau Done neu chua smoke duoc API that.

## Next Step

1. Co runtime: apply `0003_add_tenant_domain_dns_ssl_state.sql`, restart Tenant Service + API Gateway.
2. Smoke qua gateway: happy path 200 cho 3 endpoint, wrong role 403, empty/missing tenant/domain 400/404.
3. Neu owner yeu cau commit, split backend/database/gateway/docs hop ly, khong stage generated artifacts.

## Archive Index

- Full backend plan history truoc cleanup: `temp/archive/plan.backend.history.md`.
- Full backend current-task history truoc cleanup: `docs/archive/backend-history-2026-05.md`.
- Owner-plan persistence design cu: archive section `16. Backend Phase 4 Wave B`.
