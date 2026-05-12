# Current Task Frontend - Active Dashboard

Ngay cap nhat: 2026-05-13

## Vai tro file

File nay chi giu trang thai active cua frontend lane. Lich su chi tiet da chuyen sang `docs/archive/frontend-history-2026-05.md`.

Archive la cold storage: khong doc trong task thuong ngay, khong dua vao default reading list cua Lead/Frontend/Backend Agent. Chi doc archive khi owner yeu cau ro, file active tro toi section cu the, hoac can bang chung debug cu.

## Active State

- Phase 3 Owner Admin Tenant Slice: Done/Verified.
- FE real API smoke qua server test: PASS co caveat owner-plan API hien van la contract-stub backend implementation.
- Tenant slices da smoke qua Tenant API persistence that tren PostgreSQL.
- Phase 4 Wave A / Owner Admin V3 foundation: da co nhieu step FE A5-A9 hoan tat; task hien tai nen tiep tuc theo plan active trong `temp/plan.frontend.md`.
- Khong commit/stage/push neu owner chua yeu cau ro.

## Latest Verify Snapshot

- `cd frontend && npm run typecheck`: PASS trong lan verify gan nhat.
- `cd frontend && npm run build`: PASS trong lan verify gan nhat.
- Route smoke gan nhat: `/plans`, `/dashboard`, `/clinics`, `/clinics/create`, `/clinics/{tenantId that}` 200 qua FE real API smoke.
- Browser/API smoke xac nhan `/plans` goi `/api/owner/*`; clinic routes goi `/api/tenants` va `/api/tenants/{tenantId}` qua gateway that.

## Active Blockers / Caveats

- Owner-plan endpoints chay qua gateway/backend that nhung implementation hien tai van la contract stub theo BE A.2/A.3, chua persistence DB that.
- Neu can ket luan E2E that cho owner-plan persistence, phai doi backend persistence/schema plan-module duoc duyet rieng.
- Khong dung mock/stub de danh dau E2E Done khi server test co the chay API that.

## Next Step

1. Voi frontend task nho: doc file nay + `temp/plan.frontend.md` + rule frontend lien quan, khong doc archive.
2. Neu tiep tuc Wave A: resume tu active plan trong `temp/plan.frontend.md`, chay verify toi thieu theo scope.
3. Neu can doi chieu lich su A5-A9 hoac visual QA cu: doc section tuong ung trong `docs/archive/frontend-history-2026-05.md`.

## Archive Index

- Full frontend current-task history truoc cleanup: `docs/archive/frontend-history-2026-05.md`.
- Full frontend plan history truoc cleanup: `temp/archive/plan.frontend.history.md`.