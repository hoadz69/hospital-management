# Current Task - Scaffold Skeleton Clinic SaaS

Last updated: 2026-05-09

## Trang Thai

Owner da duyet structure va yeu cau scaffold project. Skeleton/placeholder da duoc tao theo structure `frontend/`, `backend/`, `infrastructure/`, `docs/`, `temp/`.

## Yeu Cau Owner

- Chi tao skeleton/placeholder.
- Khong implement business logic sau.
- Khong noi database that.
- Khong code full UI chi tiet.
- Khong tao Figma file moi.
- Khong commit.
- Sau khi xong report:
  1. File/folder da tao.
  2. Cach chay local.
  3. Lenh verify.
  4. Ket qua verify neu da chay.
  5. Buoc tiep theo de xuat.

## Da Hoan Thanh

- Tao frontend workspace trong `frontend/`.
- Tao 3 frontend app placeholder:
  - `frontend/apps/public-web`
  - `frontend/apps/clinic-admin`
  - `frontend/apps/owner-admin`
- Tao shared frontend packages:
  - `frontend/packages/ui`
  - `frontend/packages/design-tokens`
  - `frontend/packages/api-client`
  - `frontend/packages/shared-types`
  - `frontend/packages/config`
- Tao backend service skeleton trong `backend/services/` cho tat ca services da chot.
- Tao backend shared skeleton:
  - `backend/shared/building-blocks`
  - `backend/shared/contracts`
  - `backend/shared/observability`
- Tao infrastructure skeleton:
  - Docker Compose dev/prod placeholder.
  - PostgreSQL init placeholder.
  - MongoDB init/index placeholder.
  - Redis config placeholder.
  - Kafka topic/script placeholder.
  - PowerShell scripts dev placeholder.
- Tao docs skeleton:
  - `docs/architecture/overview.md`
  - `docs/architecture/service-boundaries.md`
  - `docs/architecture/tenant-isolation.md`
  - `docs/api/README.md`
  - `docs/setup/local-development.md`
  - `docs/setup/troubleshooting.md`

## Structure Hien Tai

```txt
frontend/
backend/
infrastructure/
docs/
temp/
```

## Chua Lam / Ngoai Scope

- Chua implement business logic sau.
- Chua noi database that.
- Chua tao full UI chi tiet theo Figma.
- Chua tao Figma file moi.
- Chua commit.
- Chua xoa/sua cac file cu ngoai scope.

## Verify Da Chay

```powershell
cd frontend
npm install
npm run typecheck
npm run build
```

```powershell
docker compose -f infrastructure/docker/docker-compose.dev.yml config
docker compose -f infrastructure/docker/docker-compose.prod.yml config
```

```powershell
rg --files frontend backend infrastructure docs
```

Ket qua:

- `rg --files frontend backend infrastructure docs`: pass.
- `docker compose -f infrastructure/docker/docker-compose.dev.yml config`: pass.
- `docker compose -f infrastructure/docker/docker-compose.prod.yml config`: pass.
- `cd frontend; npm install`: pass, 0 vulnerabilities theo npm audit summary.
- `cd frontend; npm run typecheck`: pass cho 3 frontend apps.
- `cd frontend; npm run build`: pass cho 3 frontend apps.
- Da don `frontend/node_modules` va cac `dist` generated sau verify; `frontend/package-lock.json` duoc giu lai.

## Buoc Tiep Theo De Xuat

1. Dong bo cac source-of-truth/rules/prompt con nhac structure cu sang structure moi.
2. Tao .NET project files thuc te cho phase 1 backend.
3. Mo rong frontend routing/layout placeholder theo Figma.
4. Them smoke tests/checklists cho tenant isolation, routing, compose config.
