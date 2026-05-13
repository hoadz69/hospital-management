# Current Task Frontend - Active Dashboard

Ngay cap nhat: 2026-05-13

## Vai tro file

File nay chi giu trang thai active cua frontend lane. Lich su chi tiet da chuyen sang `docs/archive/frontend-history-2026-05.md`.

Archive la cold storage: khong doc trong task thuong ngay, khong dua vao default reading list cua Lead/Frontend/Backend Agent. Chi doc archive khi owner yeu cau ro, file active tro toi section cu the, hoac can bang chung debug cu.

## Active State

- Phase 3 Owner Admin Tenant Slice: Done/Verified.
- FE real API smoke qua server test: PASS co caveat owner-plan API hien van la contract-stub backend implementation.
- Tenant slices da smoke qua Tenant API persistence that tren PostgreSQL.
- Phase 4 Wave A / Owner Admin V3 foundation: FE A5-A9 Done/Verified; FE A7 state surfaces Done/Verified; FE A6 Owner Admin V3 visual polish Done/Verified ngay 2026-05-13.
- Owner Admin Domain DNS Retry + SSL Pending UI mock-first: Done/Verified ngay 2026-05-13 tren `/clinics/:tenantId`.
- Owner Admin Tenant Lifecycle Confirm Modal mock-first: Done/Verified ngay 2026-05-13 tren `/clinics/:tenantId`.
- Owner Admin Audit Log Drawer mock-first: Done/Verified ngay 2026-05-13 tren `/clinics/:tenantId`.
- Khong commit/stage/push neu owner chua yeu cau ro.

## Latest Verify Snapshot

- `git diff --check`: PASS ngay 2026-05-13 cho Audit Log Drawer, chi co canh bao LF/CRLF cua git tren Windows va dirty ngoai scope.
- `cd frontend && npm run typecheck`: PASS ngay 2026-05-13 cho Audit Log Drawer.
- `cd frontend && npm run build`: PASS ngay 2026-05-13 cho Audit Log Drawer.
- Route smoke moi nhat: Vite owner-admin mock mode `http://127.0.0.1:5187/clinics/tenant-aurora-dental` HTTP 200; Playwright mo Audit log drawer, render 4 mock events lifecycle/domain/plan/mock, filter `Domain` con 1 event. Console chi co favicon 404 khong lien quan.
- Route smoke truoc do: Vite owner-admin mock mode `http://127.0.0.1:5186/clinics/tenant-aurora-dental` HTTP 200; Playwright mo modal Suspend, confirm disabled khi thieu reason, nhap reason, confirm thanh cong va status doi local sang Suspended. Console chi co favicon 404 khong lien quan.
- Route smoke truoc do: Vite owner-admin mock mode `http://127.0.0.1:5185/clinics/tenant-river-eye` HTTP 200; Playwright render DNS retry queue, click Retry chuyen success state, click SSL Pending render SSL pipeline. Console chi co favicon 404 khong lien quan.
- Route smoke gan nhat: `/plans`, `/dashboard`, `/clinics`, `/clinics/create`, `/clinics/{tenantId that}` 200 qua FE real API smoke.
- Browser/API smoke xac nhan `/plans` goi `/api/owner/*`; clinic routes goi `/api/tenants` va `/api/tenants/{tenantId}` qua gateway that.

## Active Blockers / Caveats

- Owner-plan endpoints chay qua gateway/backend that nhung implementation hien tai van la contract stub theo BE A.2/A.3, chua persistence DB that.
- Neu can ket luan E2E that cho owner-plan persistence, phai doi backend persistence/schema plan-module duoc duyet rieng.
- Khong dung mock/stub de danh dau E2E Done khi server test co the chay API that.
- Domain DNS Retry + SSL Pending hien la UI mock-first local state, khong co backend/server/API contract that.
- Tenant Lifecycle Confirm Modal hien update status bang local UI state cho lifecycle action, khong goi backend lifecycle API moi.
- Audit Log Drawer hien dung mock events/local UI state, khong goi backend audit API moi.

## Next Step

1. Voi frontend task nho: doc file nay + `temp/plan.frontend.md` + rule frontend lien quan, khong doc archive.
2. Neu tiep tuc Wave A: chot slice moi trong `temp/plan.frontend.md` truoc khi code; A6/A7, Domain DNS Retry + SSL Pending UI, Tenant Lifecycle Confirm Modal va Audit Log Drawer hien da Done/Verified.
3. Neu can doi chieu lich su A5-A9 hoac visual QA cu: doc section tuong ung trong `docs/archive/frontend-history-2026-05.md`.

## Archive Index

- Full frontend current-task history truoc cleanup: `docs/archive/frontend-history-2026-05.md`.
- Full frontend plan history truoc cleanup: `temp/archive/plan.frontend.history.md`.
