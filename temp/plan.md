# Plan - Scaffold Skeleton Clinic SaaS

Ngay: 2026-05-09
Trang thai: Scaffold skeleton/placeholder da thuc hien
Pham vi: Tao skeleton theo structure da duyet, khong implement business logic sau

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

## 9. Buoc Tiep Theo De Xuat

1. Dong bo cac file source-of-truth/rules/prompt con nhac structure cu sang structure moi.
2. Tao .NET project files thuc te cho phase 1: `api-gateway`, `identity-service`, `tenant-service`.
3. Mo rong frontend routing/layout placeholder theo Figma cho 3 apps.
4. Them test/checklist tenant isolation va routing smoke test.
