# Kế Hoạch Frontend - Living Active Plan

Ngày cập nhật: 2026-05-13

## Vai Trò File

File này là active plan để agent mới resume frontend mà không phải đọc archive. Không dùng file này làm lịch sử append-only; sau mỗi lượt chỉ cập nhật trạng thái hiện tại, điểm dừng, bước tiếp theo, verify và blocker.

Archive chi tiết: `temp/archive/plan.frontend.history.md`. Chỉ đọc archive khi owner yêu cầu rõ, active plan trỏ tới section cụ thể, hoặc cần bằng chứng debug cũ.

## Current Active Slice

**No active implementation slice approved.**

Frontend hiện không có task code mới đã được duyệt để làm ngay. Các phần đã hoàn tất gần nhất là A5 shared UI foundation và A9 `/plans` API wiring. Nếu owner nói "tiếp tục frontend/Wave A" mà không chỉ rõ slice, Lead/Frontend Agent phải dừng ở plan/report và chốt một slice cụ thể trước khi code.

Ứng viên slice tiếp theo:

1. A6 Owner Admin V3 restyle/state surfaces theo Figma V3 frame liên quan.
2. A7 empty/loading/error/conflict/detail states nếu ưu tiên state quality.
3. `/plans` polish nhỏ nếu backend owner-plan persistence được duyệt và contract thay đổi.

## Last Stopping Point

- Phase 3 Owner Admin Tenant Slice: Done/Verified.
- A5 shared UI foundation: Done/Verified.
- A5 adoption vào Owner Admin: Done/Verified.
- A9 `/plans` API wiring: Done/Verified.
- FE real API smoke: PASS có caveat owner-plan backend vẫn là contract stub.
- Không có frontend implementation đang dở.

## Progress Snapshot

| Hạng mục | Trạng thái | Ghi chú |
|---|---|---|
| Phase 3 tenant routes | Done/Verified | Đã smoke với Tenant API persistence thật qua PostgreSQL. |
| A5 shared UI | Done/Verified | `KPITile`, `ModuleChips`, `PlanBadge`, `EmptyState`, `DomainStateRow`, `CommandPalette`, `TenantSwitcher`; boundary check PASS. |
| A5 adoption | Done/Verified | Dashboard/Clinics/Clinic detail/Create wizard đã dùng shared display components phù hợp. |
| A9 `/plans` | Done/Verified | Page dùng `planCatalogClient`, mock fallback còn giữ, bulk-change dùng BE A.2 payload. |
| Wave A tiếp theo | Chưa chốt | Cần owner/Lead chọn slice trước khi code. |

## Known Touched / Resume Files

Các file này là ngữ cảnh thật từ A5/A9, chỉ đọc khi task mới liên quan trực tiếp:

```txt
frontend/apps/owner-admin/src/pages/PlanModuleCatalogPage.vue
frontend/apps/owner-admin/src/services/planCatalogClient.ts
frontend/apps/owner-admin/src/services/planCatalogMock.ts
frontend/packages/api-client/src/ownerPlanCatalogClient.ts
frontend/packages/shared-types/src/ownerPlanCatalog.ts
frontend/packages/ui/src/components/KPITile.vue
frontend/packages/ui/src/components/ModuleChips.vue
frontend/packages/ui/src/components/PlanBadge.vue
frontend/packages/ui/src/components/EmptyState.vue
frontend/packages/ui/src/components/DomainStateRow.vue
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
- Wave A step tiếp theo chưa chốt; không code frontend mới nếu chưa có active slice cụ thể.

## Archive Index

- Full frontend plan history trước cleanup: `temp/archive/plan.frontend.history.md`.
- Full frontend current-task history trước cleanup: `docs/archive/frontend-history-2026-05.md`.
- A5 completion evidence: archive section `23. A5 Completion Bundle`.
- A9 `/plans` evidence: archive section `27. Wave A Step A9`.
