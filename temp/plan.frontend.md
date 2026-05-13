# Kế Hoạch Frontend - Living Active Plan

Ngày cập nhật: 2026-05-13

## Vai Trò File

File này là active plan để agent mới resume frontend mà không phải đọc archive. Không dùng file này làm lịch sử append-only; sau mỗi lượt chỉ cập nhật trạng thái hiện tại, điểm dừng, bước tiếp theo, verify và blocker.

Archive chi tiết: `temp/archive/plan.frontend.history.md`. Chỉ đọc archive khi owner yêu cầu rõ, active plan trỏ tới section cụ thể, hoặc cần bằng chứng debug cũ.

## Current Active Slice

**Không có implementation đang dở. Last completed slice: Owner Admin Domain DNS Retry + SSL Pending UI mock-first.**

Prompt 2026-05-13 yêu cầu làm luôn frontend-only, không backend/server/SSH/Docker, không đổi API contract thật. Slice đã hoàn tất trên `/clinics/:tenantId`, dùng tenant/domain data hiện có để hiển thị mock-first domain operations panel.

Acceptance riêng cho slice này:

- `/clinics/:tenantId` có Domain DNS Retry card/state, SSL Pending card/state, DNS record table mock và retry verify action mock. Done.
- Loading/empty/error/success state được hỗ trợ trong UI state local, không gọi backend mới. Done.
- Responsive desktop/mobile, dùng shared UI/design token hiện có. Done.
- Không sửa backend, server, API contract thật, store hoặc route ngoài scope. Done.
- Verify: `git diff --check`, `cd frontend && npm run typecheck`, `cd frontend && npm run build`; smoke route liên quan nếu chạy được. PASS 2026-05-13.

Ứng viên slice tiếp theo:

1. A7 follow-up nhỏ: mở rộng `StatePanel` vào component-level states nếu phát sinh màn mới.
2. `/plans` polish nhỏ nếu backend owner-plan persistence được duyệt và contract thay đổi.
3. Domain operations nối backend thật khi có domain-service/DNS/SSL API contract được duyệt riêng.
4. Slice frontend mới theo Owner/Lead chỉ định, phải ghi acceptance criteria trước khi code.

## Last Stopping Point

- Phase 3 Owner Admin Tenant Slice: Done/Verified.
- A5 shared UI foundation: Done/Verified.
- A5 adoption vào Owner Admin: Done/Verified.
- A9 `/plans` API wiring: Done/Verified.
- A7 state surfaces: Done/Verified ngay 2026-05-13.
- A6 Owner Admin V3 visual polish: Done/Verified ngay 2026-05-13.
- Owner Admin Domain DNS Retry + SSL Pending UI mock-first: Done/Verified ngay 2026-05-13.
- FE real API smoke: PASS có caveat owner-plan backend vẫn là contract stub.
- Không có frontend implementation đang dở.

## Progress Snapshot

| Hạng mục | Trạng thái | Ghi chú |
|---|---|---|
| Phase 3 tenant routes | Done/Verified | Đã smoke với Tenant API persistence thật qua PostgreSQL. |
| A5 shared UI | Done/Verified | `KPITile`, `ModuleChips`, `PlanBadge`, `EmptyState`, `DomainStateRow`, `CommandPalette`, `TenantSwitcher`; boundary check PASS. |
| A5 adoption | Done/Verified | Dashboard/Clinics/Clinic detail/Create wizard đã dùng shared display components phù hợp. |
| A9 `/plans` | Done/Verified | Page dùng `planCatalogClient`, mock fallback còn giữ, bulk-change dùng BE A.2 payload. |
| A7 state surfaces | Done/Verified | Thêm shared `StatePanel`; adopt loading/error surface cho Dashboard, Clinics, Clinic detail, Create wizard, Plans catalog. |
| A6 visual polish | Done/Verified | Thống nhất V3 page heading surface, KPI/card/table density, hover/focus polish, create wizard preview, Plan Catalog cards/matrix và StatePanel accent rail cho 5 route chính. |
| Domain DNS Retry + SSL Pending UI | Done/Verified | `/clinics/:tenantId` có mock-first domain operations panel, DNS record table, retry verify local state, SSL pending pipeline, loading/empty/error/success states. |
| Wave A tiếp theo | Chưa chốt | Cần owner/Lead chọn slice mới trước khi code. |

## Known Touched / Resume Files

Các file này là ngữ cảnh thật từ A5/A9, chỉ đọc khi task mới liên quan trực tiếp:

```txt
frontend/apps/owner-admin/src/pages/PlanModuleCatalogPage.vue
frontend/apps/owner-admin/src/pages/DashboardPage.vue
frontend/apps/owner-admin/src/pages/ClinicsPage.vue
frontend/apps/owner-admin/src/pages/ClinicDetailPage.vue
frontend/apps/owner-admin/src/pages/CreateClinicPage.vue
frontend/apps/owner-admin/src/components/CreateTenantWizard.vue
frontend/apps/owner-admin/src/components/DomainDnsRetryState.vue
frontend/apps/owner-admin/src/components/SslPendingState.vue
frontend/apps/owner-admin/src/components/TenantTable.vue
frontend/apps/owner-admin/src/services/planCatalogClient.ts
frontend/apps/owner-admin/src/services/planCatalogMock.ts
frontend/packages/api-client/src/ownerPlanCatalogClient.ts
frontend/packages/shared-types/src/ownerPlanCatalog.ts
frontend/packages/ui/src/components/KPITile.vue
frontend/packages/ui/src/components/ModuleChips.vue
frontend/packages/ui/src/components/PlanBadge.vue
frontend/packages/ui/src/components/EmptyState.vue
frontend/packages/ui/src/components/DomainStateRow.vue
frontend/packages/ui/src/components/StatePanel.vue
frontend/packages/ui/src/components/CommandPalette.vue
frontend/packages/ui/src/components/TenantSwitcher.vue
frontend/packages/ui/src/index.ts
```

## Allowed Areas Khi Có Slice Mới

Chỉ mở rộng các vùng này khi active slice mới đã ghi rõ scope:

```txt
frontend/apps/owner-admin/src/pages/**
frontend/apps/owner-admin/src/components/**
frontend/apps/owner-admin/src/layouts/**
frontend/packages/ui/src/components/**
frontend/packages/api-client/src/**
frontend/packages/shared-types/src/**
frontend/packages/design-tokens/src/v3/**
docs/current-task.frontend.md
temp/plan.frontend.md
```

Không sửa backend, database, infrastructure hoặc Figma nếu owner không giao rõ.

## Acceptance Criteria

Trước khi implement slice mới, phải bổ sung acceptance criteria riêng cho slice đó vào file này. Default tối thiểu:

- UI/route trong scope render không crash.
- `typecheck` và `build` PASS nếu sửa frontend code.
- Không đổi route/API/store/business behavior ngoài scope.
- Mock fallback chỉ là dev/offline fallback, không dùng để đánh dấu E2E Done.
- Shared UI component không import router, Pinia, API client, app store, env/storage.
- Task visual/restyle lớn phải có visual QA/screenshot khi owner yêu cầu hoặc trước commit UI lớn.

FE A7 acceptance đã hoàn tất:

- Shared `StatePanel` nằm trong `frontend/packages/ui`, không import router/Pinia/API/env/storage.
- Owner Admin loading/error surfaces dùng tone thống nhất cho `/dashboard`, `/clinics`, `/clinics/create`, `/clinics/:tenantId`, `/plans`.
- Không đổi API contract, route, store hoặc backend behavior.
- `git diff --check`, `cd frontend && npm run typecheck`, `cd frontend && npm run build` PASS.

FE A6 acceptance đã hoàn tất:

- `/dashboard`, `/clinics`, `/clinics/create`, `/clinics/:tenantId`, `/plans` có V3 visual polish nhất quán cho page heading, KPI/card/table/state surface.
- `StatePanel` giữ shared UI boundary và có accent rail/box shadow nhẹ; không import router/Pinia/API/env/storage.
- Không đổi API contract, route, store hoặc backend behavior.
- `git diff --check`, `cd frontend && npm run typecheck`, `cd frontend && npm run build` PASS.

Owner Admin Domain DNS Retry + SSL Pending UI acceptance đã hoàn tất:

- `/clinics/:tenantId` có domain operations panel mock-first với DNS Retry card, SSL Pending card, DNS record table, retry verify action.
- DNS/SSL loading, empty, error và success states dùng local UI state; không gọi backend mới.
- Không đổi route/API contract thật/store/backend behavior.
- `git diff --check`, `cd frontend && npm run typecheck`, `cd frontend && npm run build` PASS.
- Smoke route `http://127.0.0.1:5185/clinics/tenant-river-eye` mock mode HTTP 200; Playwright render DNS retry queue, retry success state và SSL pending pipeline PASS.

## Verify Plan

Docs-only/no-code:

```txt
git diff --check
```

Frontend code:

```txt
git diff --check
cd frontend && npm run typecheck
cd frontend && npm run build
```

Route/API smoke chỉ chạy khi task đổi route, UI behavior hoặc API flow:

```txt
/dashboard
/clinics
/clinics/create
/clinics/{tenantId thật hoặc fixture}
/plans
```

## Blockers / Caveats

- Owner-plan endpoints chạy qua backend/gateway thật nhưng implementation vẫn là contract stub BE A.2/A.3.
- Owner-plan persistence/schema cần Backend/DB approval riêng; frontend không tự chuyển stub thành persistence.
- Domain DNS Retry + SSL Pending hiện là UI mock-first local state; backend DNS/SSL retry API cần contract riêng trước khi nối thật.
- Wave A step tiếp theo sau Domain DNS Retry + SSL Pending UI chưa chốt; không code frontend mới nếu chưa có prompt/action trigger cụ thể.

## Archive Index

- Full frontend plan history trước cleanup: `temp/archive/plan.frontend.history.md`.
- Full frontend current-task history trước cleanup: `docs/archive/frontend-history-2026-05.md`.
- A5 completion evidence: archive section `23. A5 Completion Bundle`.
- A9 `/plans` evidence: archive section `27. Wave A Step A9`.
