# Ke Hoach Backend/DevOps - Active Plan

Ngay cap nhat: 2026-05-13

## Vai tro file

File nay chi giu active plan cho Backend/DevOps lane. Lich su plan day du da chuyen sang `temp/archive/plan.backend.history.md`.

Archive la cold storage: khong doc trong task thuong ngay. Chi doc khi owner yeu cau ro, file active tro toi section cu the, hoac can bang chung debug cu.

## Active Scope

- Phase 2 API Runtime Smoke Gate da PASS theo handoff gan nhat.
- Backend/DevOps Phase 4 A.10 hien la preparation lane, chua implement vi thieu spec that.
- Owner-plan `/api/owner/*` contract/stub la baseline cho FE; persistence/schema plan-module that can owner approval rieng.

## Active Plan / Resume Rules

1. Voi backend task nho: doc `docs/current-task.backend.md`, file nay, `rules/coding-rules.md`, `rules/backend-coding-rules.md`; them `rules/database-rules.md` hoac `rules/backend-testing-rules.md` neu scope can.
2. Voi runtime/API smoke: uu tien server test qua env/session owner cung cap; khong ghi secret/key/token/connection string that vao repo/docs/log.
3. Voi DB/persistence: phai co tenant context, transaction boundary ro, migration/index/seed theo `rules/database-rules.md`.
4. Voi A.10: neu chua co spec service/path/contract/guard/persistence/acceptance thi dung o plan/report blocker.

## Verify Commands Theo Scope

```txt
git diff --check
# Neu chay backend:
dotnet restore backend/ClinicSaaS.Backend.sln
dotnet build backend/ClinicSaaS.Backend.sln --no-restore
dotnet test backend/ClinicSaaS.Backend.sln --no-build
```

Server smoke chi chay khi co runtime/env phu hop va task yeu cau.

## Active Blockers / Dependencies

- A.10 thieu spec that.
- Owner-plan persistence/schema chua duoc duyet.
- Stub chi la fallback contract path, khong dung de danh dau E2E Done neu server test co API that.

## Out Of Scope Mac Dinh

- Khong commit/stage/push.
- Khong sua frontend/Figma neu owner khong giao ro.
- Khong doc `temp/archive/**` trong task thuong ngay.

## Archive Index

- Full backend plan history truoc cleanup: `temp/archive/plan.backend.history.md`.
- Full backend current-task history truoc cleanup: `docs/archive/backend-history-2026-05.md`.