# Ke Hoach Frontend - Active Plan

Ngay cap nhat: 2026-05-13

## Vai tro file

File nay chi giu active plan cho frontend lane. Lich su plan day du da chuyen sang `temp/archive/plan.frontend.history.md`.

Archive la cold storage: khong doc trong task thuong ngay. Chi doc khi owner yeu cau ro, file active tro toi section cu the, hoac can bang chung debug cu.

## Active Scope

- Frontend lane hien tai tap trung Owner Admin Phase 4 Wave A va `/plans` integration baseline.
- Phase 3 Owner Admin Tenant Slice da Done/Verified.
- `/plans` da wire qua `planCatalogClient` toi BE A.2 contract va giu mock fallback cho dev/offline.
- FE real API smoke gan nhat PASS qua server test voi caveat owner-plan backend van la contract-stub implementation.

## Active Plan / Resume Rules

1. Neu task la frontend UI/component nho: chi doc `docs/current-task.frontend.md`, file nay, `rules/coding-rules.md`, va source lien quan.
2. Neu task cham API real mode: giu `real` mode qua API Gateway that khi server test san sang; mock fallback chi la dev/offline fallback.
3. Neu task tiep tuc Wave A: giu scope surgical, khong sua backend/Figma neu owner khong giao ro.
4. Neu can visual QA: chi chup screenshot khi owner yeu cau visual QA, restyle lon, hoac truoc commit UI lon.

## Verify Commands Theo Scope

```txt
git diff --check
cd frontend && npm run typecheck
cd frontend && npm run build
```

Voi route/UI smoke, chon route dai dien theo task thay vi chup/toan bo route.

## Active Blockers / Dependencies

- Owner-plan persistence that phu thuoc Backend/DB approval rieng; FE khong tu chuyen stub thanh persistence.
- Neu server test/API Gateway khong truy cap duoc, report blocker ro; khong danh dau E2E Done bang mock/stub.

## Out Of Scope Mac Dinh

- Khong commit/stage/push.
- Khong sua backend, database, infrastructure, Figma neu owner khong giao ro.
- Khong doc `temp/archive/**` trong task thuong ngay.

## Archive Index

- Full frontend plan history truoc cleanup: `temp/archive/plan.frontend.history.md`.
- Full frontend current-task history truoc cleanup: `docs/archive/frontend-history-2026-05.md`.