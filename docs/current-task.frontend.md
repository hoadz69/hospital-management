# Current Task Frontend - Phase 3 Done + Phase 4 Wave A V3 Foundation Planning

Ngày cập nhật: 2026-05-12

## Full Team Final Real API Attempt (2026-05-12)

## Server Test Runtime Rule - 2026-05-12

Frontend lane khi cần API thật phải ưu tiên API Gateway thật trên server test/dev smoke do owner cung cấp qua `DEPLOY_HOST`, `DEPLOY_USER`, `SSH_KEY_PATH`. Nếu backend local Windows thiếu Docker/.NET hoặc gateway local không sẵn, đó không còn là blocker FE real smoke; Lead/DevOps phải dùng server test hoặc SSH tunnel tới gateway server.

FE smoke rule:
- Vite proxy trỏ tới API Gateway thật trên server test hoặc URL tunnel local tới gateway server.
- `real` mode phải gọi gateway thật khi backend sẵn; mock fallback vẫn giữ cho UI dev nhưng không dùng để đánh dấu E2E Done.
- Không ghi private key, token, secret hoặc connection string thật vào docs/log; chỉ ghi tên biến/session và trạng thái smoke.

Trạng thái: **PARTIAL PASS - không đánh dấu FE Done hoàn toàn với API thật** vì tenant persistence runtime bị chặn bởi cấu hình DB. Phần Owner Plan A9 đã verify bằng API Gateway thật; phần tenant slices đã verify route/build/client wiring và vẫn giữ fallback, nhưng API tenant thật chưa pass.

Kết quả Backend runtime thật:
- Bật được Tenant Service bằng `C:\Users\nvhoa2\.dotnet\dotnet.exe` tại `http://localhost:5006`.
- Bật được API Gateway bằng `C:\Users\nvhoa2\.dotnet\dotnet.exe` tại `http://localhost:5018`.
- Docker CLI vẫn không có trong PATH, nên không bật được PostgreSQL container local.
- Tenant Service `/api/tenants` trả 500 vì thiếu `CLINICSAAS_TENANT_SERVICE_POSTGRES` hoặc `PostgreSql:ConnectionString`; đây là blocker DB/runtime, không phải lỗi FE wiring.

API thật qua API Gateway:
```txt
GET /api/owner/plans -> PASS, 3 items
GET /api/owner/modules -> PASS, 12 items
GET /api/owner/tenant-plan-assignments -> PASS, 6 items
POST /api/owner/tenant-plan-assignments/bulk-change -> PASS, accepted-stub, effectiveAt=next_renewal
400 bulk-change validation -> PASS
403 wrong owner role -> PASS
500 tenant list missing postgres -> PASS như blocker runtime thật
409 tenant conflict -> chưa verify được bằng API thật vì tenant persistence DB chưa sẵn
```

FE/QA:
```txt
Route shell /plans, /dashboard, /clinics, /clinics/create, /clinics/tenant-smoke-a -> PASS qua Vite route smoke
git diff --check -> PASS (CRLF warning only)
npm run typecheck -> PASS
npm run build -> PASS
```

Kết luận:
- `/plans` A9 đã wire API thật và verify PASS với API Gateway thật.
- Tenant slices đã wire `tenantClient` real path và có mock fallback; nhưng chưa thể xác nhận tenant API thật end-to-end cho list/detail/create/status khi thiếu PostgreSQL runtime.
- Không sửa thêm FE API code; blocker còn lại thuộc backend/devops runtime DB.

## Full Fast Mode - FE Lane Completion Verify (2026-05-12)

Trạng thái: **PASS có blocker runtime thật**. Lead Agent đã chạy team giả lập Backend + Frontend + QA + Documentation để verify FE lane cho A9 và tenant slices. FE lane được xem là Done/Verified ở mức client wiring + proxy contract smoke; runtime API Gateway/Tenant Service thật vẫn là blocker môi trường trong phiên này.

Agents:
- Backend Agent: rà code endpoint và thử runtime thật.
- Frontend Agent: xác nhận `/plans`, `/dashboard`, `/clinics`, `/clinics/create`, `/clinics/:tenantId` đều dùng real client path, fallback vẫn giữ.
- QA Agent: smoke route/API/negative path, typecheck, build.
- Documentation Agent: cập nhật lane report.

Backend runtime:
- `where dotnet` và `where docker` không tìm thấy executable.
- Port `5005`, `5006`, `5018` không lắng nghe trước khi smoke.
- Vì runtime thật không sẵn, Lead dựng Node contract stub tạm trong `temp/` qua Vite proxy, chạy FE với `real` mode và fallback tắt.

Verify PASS:
```txt
Route shell:
  /plans, /dashboard, /clinics, /clinics/create, /clinics/tenant-smoke-a -> 200
API happy path:
  GET /api/owner/plans -> 3 items
  GET /api/owner/modules -> 3 items trong smoke stub
  GET /api/owner/tenant-plan-assignments -> 2 items
  POST /api/owner/tenant-plan-assignments/bulk-change -> accepted-stub, effectiveAt=next_renewal, auditReason ok
  GET /api/tenants -> 1 item
  GET /api/tenants/tenant-smoke-a -> PASS
  PATCH /api/tenants/tenant-smoke-a/status -> Suspended
Negative path:
  400 bulk-change validation -> PASS
  403 wrong owner role -> PASS
  409 duplicate tenant -> PASS
  500 forced server error -> PASS
Static verify:
  git diff --check -> PASS (CRLF warning only)
  npm run typecheck -> PASS
  npm run build -> PASS
```

Kết luận FE lane:
- `/plans` đã wire BE A.2 contract qua `planCatalogClient`/`createOwnerPlanCatalogClient`.
- Tenant slices đã wire Tenant API qua `tenantClient` real path.
- Mock fallback/error path vẫn giữ để UI không crash khi backend fail.
- Không cần sửa thêm FE API code trong scope A9 hiện tại.

Blocker còn lại:
- Cần môi trường có `dotnet` hoặc Docker/API Gateway thật để chạy smoke runtime thật thay cho contract stub tạm.

## Fast Verify Round 2 - FE A9 With Backend/Frontend Lanes (2026-05-12)

Trạng thái: PASS - **FE A9 Done/Verified** trong Frontend lane. Lead Agent đã chạy Backend lane check + Frontend lane smoke cho `FE A9 /plans` và tenant slices.

Backend lane:
- Code endpoint đã tồn tại ở Tenant Service và API Gateway cho `GET /api/owner/plans`, `GET /api/owner/modules`, `GET /api/owner/tenant-plan-assignments`, `POST /api/owner/tenant-plan-assignments/bulk-change`.
- Runtime thật local không bật được trong máy hiện tại: không có `dotnet`, không có Docker CLI, các port `5005`, `5006`, `5018` ban đầu không lắng nghe.
- Đã dùng Node contract stub local qua Vite proxy để smoke FE trong `real` mode và fallback tắt.

Frontend lane:
- `/plans` đã smoke với `/api/owner/*`; bulk-change gửi `auditReason` và `effectiveAt: "next_renewal"`.
- `/dashboard`, `/clinics`, `/clinics/create`, `/clinics/:tenantId` đã smoke route shell và API tenant proxy path.
- Mock fallback/error path vẫn còn trong `tenantClient` và `planCatalogClient`; không tạo mock production mới.

Verify PASS:
```txt
git diff --check -> PASS (CRLF warning only)
npm run typecheck -> PASS
npm run build -> PASS
Smoke Vite proxy real-mode/fallback-off:
  /plans, /dashboard, /clinics, /clinics/create, /clinics/tenant-smoke-a -> 200
  /api/owner/plans -> 3 items
  /api/owner/modules -> 3 items trong smoke stub
  /api/owner/tenant-plan-assignments -> 2 items
  POST /api/owner/tenant-plan-assignments/bulk-change -> accepted-stub, effectiveAt=next_renewal
  /api/tenants, /api/tenants/tenant-smoke-a, PATCH /api/tenants/tenant-smoke-a/status -> PASS
Negative path:
  400 bulk-change validation -> PASS
  403 wrong owner role -> PASS
  409 duplicate tenant -> PASS
  500 forced server error -> PASS
```

Blocked/Next:
- Chỉ còn blocker runtime thật do thiếu `dotnet`/Docker trên máy này. Khi gateway thật chạy, smoke lại cùng FE command với `VITE_DEV_PROXY_TARGET` trỏ gateway thật.

## Fast Verify - FE Real API Wiring Owner Plans/Tenants (2026-05-12)

Trạng thái: ✅ **PASS**. Lead Agent đã rà toàn bộ frontend slice `/plans`, `/dashboard`, `/clinics`, `/clinics/create`, `/clinics/:tenantId` và xác nhận FE đã wire real client, vẫn giữ mock fallback/error path.

Kết quả wiring:
- `/plans`: dùng `planCatalogClient` -> `createOwnerPlanCatalogClient` gọi real `/api/owner/plans`, `/api/owner/modules`, `/api/owner/tenant-plan-assignments`, `POST /api/owner/tenant-plan-assignments/bulk-change`; payload bulk-change có `effectiveAt: "next_renewal"` và `auditReason`.
- `/dashboard`: dùng `tenantClient.listTenants()` real `/api/tenants`, derive KPI/dashboard từ dữ liệu real shape qua adapter, fallback mock khi auto mode và API lỗi.
- `/clinics`: dùng `tenantClient.listTenants()`, `tenantClient.getTenant()`, `tenantClient.updateTenantStatus()`; có loading/error/empty/filter state, fallback mock còn giữ.
- `/clinics/create`: dùng `tenantClient.createTenant()`; 409 vẫn normalize về `ApiConflictError`, UI không crash và giữ form data.
- `/clinics/:tenantId`: dùng `tenantClient.getTenant()` và `tenantClient.updateTenantStatus()`; lỗi 400/403/409/500 đi qua `HttpError`/fallback hoặc error state.

Backend rà được trong code:
- Tenant Service có `MapOwnerPlanCatalogEndpoints()` cho 4 route `/api/owner/*`.
- API Gateway có `MapOwnerPlanCatalogContractEndpoints()` cho 4 route `/api/owner/*` contract/stub.
- Local runtime thật chưa sẵn vì máy hiện tại không có `dotnet`/Docker CLI và các port `5005`, `5006`, `5018` ban đầu đều chưa lắng nghe. Lead đã dựng Node HTTP contract stub local qua Vite proxy để verify FE wiring.

Verify đã chạy:
```txt
git diff --check -> PASS (chỉ warning CRLF từ file dirty có sẵn và file router vừa sửa)
npm run typecheck -> PASS
npm run build -> PASS
Smoke Vite proxy real-mode/fallback-off qua stub http://127.0.0.1:5185:
  /plans, /dashboard, /clinics, /clinics/create, /clinics/tenant-smoke-a -> 200
  /api/owner/plans -> 3 items
  /api/owner/modules -> 3 items trong smoke stub
  /api/owner/tenant-plan-assignments -> 2 items
  POST /api/owner/tenant-plan-assignments/bulk-change -> accepted-stub, effectiveAt=next_renewal
  /api/tenants và /api/tenants/tenant-smoke-a -> PASS
```

File sửa trong frontend lane:
- `frontend/apps/owner-admin/src/router/index.ts`: cập nhật subtitle `/plans` từ mock-only wording sang BE A.2 contract + mock fallback.
- `docs/current-task.frontend.md`, `temp/plan.frontend.md`: cập nhật trạng thái verify wiring.

## Trạng Thái Phase 3

Phase 3 Owner Admin Tenant Slice ✅ **Implementation Done** (commit `7f6366d`).

```txt
- Pre-Phase 4 Hardening committed: P1.6 (CreateTenantWizard isStepValid gate), P1.7 (httpClient JSON.parse guard).
- QA F-real Round 2 PASS 5/5 trên real API: HTTP smoke + contract adapter.
- Build/typecheck PASS x3 app cuối session.
- Phase 3 sẵn sàng đóng. Real API smoke đã hoàn tất qua adapter cho 4 endpoint.
```

## Phase 4 Wave A V3 Foundation Planning

Trạng thái: 🟡 **Planning — chờ owner duyệt plan**.

## In-progress Checkpoint - 2026-05-10 Crash Recovery Owner Admin V3 Restyle

Scope đang làm:
- Restyle Owner Admin theo Figma V3 trong phạm vi frontend lane.
- Giữ nguyên backend/API behavior, không thêm route mới nếu không cần.
- Ưu tiên CSS/token/layout/component state, không đụng API client/store.

Đã hoàn thành trước khi session bị compact/chết:
- Figma đã inspect các surface Owner Admin V3 liên quan: nền muted/ivory, sidebar slate, topbar mỏng, card radius 12-16, table compact, drawer sheet 560px, wizard card + stepper ngang, command palette overlay.
- Đã bắt đầu cập nhật shared UI component theo CSS variables với fallback cũ để Owner Admin nhận token mới.
- Đã bắt đầu restyle layout/sidebar/topbar/filter/table và thêm command palette Owner Admin.

File đang dirty theo `git status --short`:
- `frontend/packages/ui/src/components/AppButton.vue`
- `frontend/packages/ui/src/components/AppCard.vue`
- `frontend/packages/ui/src/components/MetricCard.vue`
- `frontend/packages/ui/src/components/StatusPill.vue`
- `frontend/apps/owner-admin/src/components/AdminSidebar.vue`
- `frontend/apps/owner-admin/src/components/AdminTopbar.vue`
- `frontend/apps/owner-admin/src/components/TenantFilterBar.vue`
- `frontend/apps/owner-admin/src/components/TenantTable.vue`
- `frontend/apps/owner-admin/src/layouts/OwnerAdminLayout.vue`
- `frontend/apps/owner-admin/src/components/OwnerCommandPalette.vue` (mới)

File ngoài frontend lane đang dirty:
- `.claude/settings.local.json` — không thuộc scope restyle Owner Admin; không được tự revert nếu owner chưa yêu cầu.

Chưa verify / còn thiếu:
- Chưa có report cuối vì session trước chết sau khi sửa file.
- Cần chạy lại verify từ repo thật, không tin vào context cũ.
- Cần review diff để bảo đảm không đụng API/store/backend/route ngoài scope.
- Nếu build/typecheck pass, cập nhật lại checkpoint này thành report hoàn tất.

Lệnh cần chạy khi resume:

```powershell
git status --short
git diff --stat
git diff --check
git diff -- frontend/packages/ui/src/components frontend/apps/owner-admin/src/components frontend/apps/owner-admin/src/layouts
cd frontend
npm run typecheck
npm run build
```

Bước resume tiếp theo:
1. Đọc `docs/session-continuity.md` Crash Recovery & Checkpoint Protocol.
2. Đọc diff 10 file frontend ở trên.
3. Chỉ sửa tiếp trong frontend lane nếu diff còn lỗi rõ.
4. Chạy typecheck/build.
5. Cập nhật checkpoint/report trước khi trả owner.

Guardrail:
- Không revert thay đổi đang dở nếu chưa rõ chủ sở hữu.
- Không sửa backend/API behavior.
- Không commit/push nếu owner chưa yêu cầu.
- Không đánh dấu Done nếu verify chưa pass.

Mục tiêu Wave A: token V3 ADD-ONLY layer + httpClient factory rebuild + Owner Admin restyle V3 + 7 component shared mới + 4 composable foundation. Bốn frame Owner Admin V3v2 mới (Dashboard cross-tenant, Tenant Lifecycle Confirm Modal, Domain DNS Retry, SSL Pending) cần được implement đầy đủ.

### Scope Chi Tiết Wave A

File dự kiến tạo/sửa (chỉ trong frontend lane):

```txt
frontend/packages/design-tokens/src/v3/color.ts            (mới)
frontend/packages/design-tokens/src/v3/typography.ts       (mới)
frontend/packages/design-tokens/src/v3/spacing.ts          (mới)
frontend/packages/design-tokens/src/v3/radius.ts           (mới)
frontend/packages/design-tokens/src/v3/shadow.ts           (mới)
frontend/packages/design-tokens/src/v3/motion.ts           (mới)
frontend/packages/design-tokens/src/v3/v3.css              (mới — CSS custom property layer)
frontend/packages/design-tokens/src/index.ts               (export v3 namespace, KHÔNG xoá v2 layer)

frontend/packages/api-client/src/httpClient.ts             (rebuild factory pattern, bỏ X-Owner-Role hardcode)
frontend/packages/api-client/src/owner.ts                  (mới — surface owner)
frontend/packages/api-client/src/clinic.ts                 (mới — surface clinic admin)
frontend/packages/api-client/src/public.ts                 (mới — surface public)

frontend/packages/ui/src/composables/useTenantContext.ts   (mới)
frontend/packages/ui/src/composables/useReducedMotion.ts   (mới)
frontend/packages/ui/src/composables/useFocusTrap.ts       (mới)
frontend/packages/ui/src/composables/useViewTransition.ts  (mới)

frontend/packages/ui/src/components/KPITile.vue            (mới — sparkline)
frontend/packages/ui/src/components/ModuleChips.vue        (mới — x/12)
frontend/packages/ui/src/components/PlanBadge.vue          (mới)
frontend/packages/ui/src/components/EmptyState.vue         (mới)
frontend/packages/ui/src/components/TenantSwitcher.vue     (mới)
frontend/packages/ui/src/components/CommandPalette.vue     (mới)
frontend/packages/ui/src/components/DomainStateRow.vue     (mới)

frontend/packages/ui/.histoire/                            (mới — Histoire Vue 3 setup)

frontend/apps/owner-admin/src/components/TenantTable.vue           (token V3 update)
frontend/apps/owner-admin/src/components/TenantDetailDrawer.vue    (token V3 update)
frontend/apps/owner-admin/src/components/CreateTenantWizard.vue    (token V3 update)
frontend/apps/owner-admin/src/components/ConflictState.vue         (token V3 update)
frontend/apps/owner-admin/src/components/MetricCard.vue            (token V3 update — re-export từ ui package)
frontend/apps/owner-admin/src/components/StatusPill.vue            (token V3 update — re-export từ ui package)
frontend/apps/owner-admin/src/pages/CrossTenantDashboardPage.vue   (mới — frame 124:2)
frontend/apps/owner-admin/src/components/TenantLifecycleConfirmModal.vue (mới — frame 124:292)
frontend/apps/owner-admin/src/components/DomainDnsRetryState.vue   (mới — frame 125:2)
frontend/apps/owner-admin/src/components/SslPendingState.vue       (mới — frame 125:122)
```

Frame Figma Wave A bắt buộc đối chiếu (14 frame):

```txt
66:2     Tokens reference
127:2    Component Inventory 38
127:410  Layout Grid + Responsive Rules
127:518  A11y WCAG 2.2 Poster
124:2    Owner Admin Dashboard cross-tenant
124:292  Tenant Lifecycle Confirm Modal
125:2    Domain DNS Retry
125:122  SSL Pending
85:2     Tenant Operations (V3v1 enhance)
87:2     Tenant Detail Drawer (V3v1 enhance)
88:2     Create Tenant Wizard (V3v1 enhance)
88:112   Empty state (V3v1 enhance)
88:119   Conflict slug/domain (V3v1 enhance)
88:127   Command palette (V3v1 enhance)
104:2    OPEN HERE Overview & TOC
```

### Backend Dependency Wave A

```txt
- Phase 4.1 Domain Service: OpenAPI contract (mock OK) — endpoint list/detail/verify dummy.
- Phase 4.2 Template Service stub: registry + apply mode dialog 3-mode contract.
- Phase 4.3 Website CMS Service: settings/sliders contract (CRUD mock OK).
```

Wave A cho phép mock-first; backend implementation thật chỉ bắt buộc trước Wave B/C/D.

### Verify Command Wave A

```powershell
cd frontend
npm run typecheck
npm run build
# Lint script SKIP nếu workspace chưa khai báo (như Phase 3).
npm run dev:owner   # Smoke 4 page Owner Admin sau token migration: /dashboard, /clinics, /clinics/create, /clinics/:tenantId.
# Bổ sung sau scaffold: smoke /cross-tenant-dashboard nếu route đã active trong Wave A.
```

### QA Gate Wave A

```txt
- axe-core CI baseline cho 4 page Owner Admin: /dashboard, /clinics, /clinics/create, /clinics/:tenantId.
- Lighthouse CI baseline cho cùng 4 page (chưa fail-build, chỉ baseline).
- VRT screenshot diff 14 component Phase 3 trước/sau token V3 migration:
  AppButton, AppCard, MetricCard, StatusPill, AdminSidebar, AdminTopbar,
  TenantTable, TenantFilterBar, TenantDetailDrawer, CreateTenantWizard,
  ConflictState, DashboardPage shell, ClinicsPage shell, ClinicDetailPage shell.
```

### Effort Estimate

```txt
~34 dev-day, 4 tuần, 2 FE (parallel theo plan owner duyệt).
```

### Risk Top 3 Wave A

```txt
1. CRITICAL  httpClient hardcode X-Owner-Role: OwnerSuperAdmin
             → rebuild factory tách 3 surface (owner / clinic / public),
             header tenant + role do consumer inject, không hardcode trong client.
2. HIGH      Token V2 hex literal vs V3 CSS var drift
             → ADD-ONLY: giữ V2 layer, V3 CSS custom property mới,
             component restyle dùng var(--color-*) thay vì hex. Phase 3 component
             được migrate dần, không break visual.
3. HIGH      Owner Dashboard cross-tenant API mock-first
             → backend Phase 4 chưa có aggregate cross-tenant; Wave A dùng mock
             trong api-client/owner.ts để unblock UI; switch real khi backend ready.
```

### Hard Rules Giữ

```txt
- KHÔNG tạo Figma file mới.
- KHÔNG sửa V1/V2 Figma baseline.
- KHÔNG sửa backend code trong lane này (ngoại trừ note request OpenAPI contract gửi backend lane).
- KHÔNG commit/push trừ khi owner yêu cầu rõ.
- KHÔNG đụng .env/secret/IP server thật/connection string thật.
- KHÔNG hard-code role/tenant id trong httpClient sau rebuild.
```

### Điểm Dừng

```txt
- Plan ready trong temp/plan.frontend.md §16. Frontend Agent chỉ implement khi
  owner đã duyệt plan rõ.
- 5 owner decision (xem section 7.1.6 roadmap) phải có quyết định trước khi
  start Wave B/C/D/E. Wave A có thể start với token + httpClient + composable
  + Owner Admin restyle ngay khi owner duyệt.
- Sau implement, QA Agent verify; Documentation Agent cập nhật dashboard +
  lane + roadmap; Lead Agent gom report và đề xuất commit split.
```

---

## Phase 3 Lịch Sử (giữ làm tham chiếu)


## Vai Trò File

File này là handoff riêng cho Frontend workstream. Frontend Agent cập nhật file này khi làm Phase 3 Owner Admin Tenant Slice.

Không ghi task backend/devops vào file này. Không overwrite `docs/current-task.md`.

## Trạng Thái

Đang làm: Phase 3 Owner Admin Tenant Slice.

Trạng thái hiện tại: **Mock Functional Smoke PASS - Real API Smoke Pending Wiring** (QA Agent confirm 2026-05-10). Implementation Done + post-smoke fix round 1 + 2 done. QA Agent chạy manual smoke checklist A-F: A/B/C/E/F-mock PASS; D PASS với 1 gap UX nhỏ (wizard step gating - tech-debt Phase 4, không phải data integrity blocker vì HTML5 required ở submit cuối vẫn chặn). F-real API smoke pending vì FE chưa wire env real (Backend Phase 2 đã Done trên server `116.118.47.78`).

UI Redesign V2 đã ready trong Figma source of truth (frame `V2 - Owner Admin Tenant Operations`). Owner đã duyệt plan, frontend đã hoàn thành code Phase 3 và đã xử lý các issue phát sinh từ visual smoke ngày 2026-05-10 trong 2 round.

## Bug Fix Round 1 (post-visual smoke)

Sau khi owner mở `http://localhost:5175` và visual smoke, phát hiện 2 issue. Frontend Agent fix surgical:

```txt
Issue A - Sidebar nav disabled bị lệch logic
  File: frontend/apps/owner-admin/src/components/AdminSidebar.vue
  Vấn đề: 6 nav item disabled (Domains, Templates, Billing, Monitoring, Support, Cài đặt)
          render bằng <RouterLink to="/clinics">. Click vẫn navigate, đồng thời
          dễ trùng router-link-active với mục "Phòng khám" cùng trỏ /clinics.
  Fix:
  - Đổi 6 item disabled sang <button type="button" disabled tabindex="-1"
    aria-disabled="true" title="Sắp ra mắt - Phase 4+">.
  - Đặt `to=""` cho item disabled trong nav config (không còn dùng cho RouterLink).
  - 2 item enabled (Dashboard, Phòng khám) giữ là <RouterLink>.
  - CSS: thêm reset cho <button> (border/background/font-inherit/text-align/width 100%);
    selector :hover :not(.disabled):not(:disabled); selector .nav-item:disabled mirror
    cùng look với .disabled. Item disabled không bao giờ nhận router-link-active
    vì không phải RouterLink.
  Kết quả: chỉ có 1 nav active đúng route hiện tại; click 6 mục disabled không navigate.

Issue B - Chuyển trang giật
  File: frontend/apps/owner-admin/src/layouts/OwnerAdminLayout.vue
  Vấn đề: <RouterView /> raw -> route đổi tức thời, owner cảm giác giật.
  Fix:
  - Bọc bằng <RouterView v-slot="{ Component, route: matchedRoute }">.
  - Thêm <Transition name="page" mode="out-in" appear> + key=matchedRoute.fullPath
    để nested route /clinics -> /clinics/:tenantId vẫn re-trigger transition.
  - CSS: opacity 0 -> 1 + translateY 6px -> 0 (enter), translateY 0 -> -4px (leave),
    duration 200ms, easing cubic-bezier(0.4, 0, 0.2, 1).
  - Không thêm dependency. Chỉ Vue built-in.
  Kết quả: chuyển trang fade + slide-up nhẹ, mượt không lâu, không phá grid sidebar 248px.
```

Verify sau fix:

```txt
cd frontend && npm run typecheck   -> PASS (clinic-admin, owner-admin, public-web)
cd frontend && npm run build       -> PASS (3 app)
  - clinic-admin   62.23 kB (không đổi, ngoài scope)
  - owner-admin    139.97 kB (+6.93 kB so với 133.04 kB do Transition + button reset CSS)
  - public-web     62.56 kB (không đổi, ngoài scope)
Vite HMR     -> dev server 5175 đã reload sau khi save (Lead chạy background, không restart).
Real API     -> chưa smoke, vẫn dùng mock fallback trong session này.
```

## Bug Fix Round 2 (post-visual smoke 2026-05-10)

Sau round 1, owner re-check visual và phát hiện thêm 4 issue. Frontend Agent fix surgical:

```txt
Issue 1 - Badge "OWNERSUPERADMIN" dính chữ
  File: frontend/apps/owner-admin/src/components/AdminSidebar.vue
  Vấn đề: text "OwnerSuperAdmin" + CSS text-transform: uppercase -> render dính "OWNERSUPERADMIN".
  Fix: đổi text thành "Owner Super Admin" để CSS uppercase render "OWNER SUPER ADMIN" có khoảng trắng.
  CSS giữ nguyên (uppercase + letter-spacing 0.04em).

Issue 2 - UX disabled item: Coming soon rõ ràng hơn
  File: frontend/apps/owner-admin/src/components/AdminSidebar.vue
  Vấn đề: <button disabled title="..."> chỉ hint qua hover -> owner không thấy ngay là item Soon.
  Fix:
  - Thêm visible badge "Sắp ra mắt" bên phải label trong nav item disabled (font 10px,
    background rgba(255,255,255,0.1), color #9fb3c8, padding 2-4px 6-8px, border-radius 999px,
    text-transform uppercase, letter-spacing 0.02em).
  - Wrap label trong <span class="nav-label"> với flex: 1 để badge dính sát mép phải.
  - Giữ disabled + aria-disabled + title cho accessibility.
  - 2 item enabled không có badge, vẫn nhận router-link-active style teal khi active.

Issue 3 - Responsive narrow viewport (drawer mode)
  File: frontend/apps/owner-admin/src/layouts/OwnerAdminLayout.vue
        frontend/apps/owner-admin/src/components/AdminTopbar.vue
        frontend/apps/owner-admin/src/components/AdminSidebar.vue
  Vấn đề:
  - Layout grid 248px + 1fr ở < 900px chuyển 1fr nhưng sidebar vẫn render đè trên content.
  - AdminSidebar render full chiều cao 8 nav item -> mobile tốn không gian.
  - Topbar không có cách mở sidebar trên mobile.
  Fix: chuyển sang drawer pattern cho viewport < 1024px:
  - Layout giữ ref `sidebarOpen`. Desktop (>= 1024px) sidebar inline qua grid 248px;
    mobile/tablet (< 1024px) sidebar wrapper position fixed left, width min(280px, 82vw),
    transform translateX(-100%) -> 0 khi open, transition 220ms cubic-bezier(0.4, 0, 0.2, 1).
  - Backdrop fixed inset 0, background rgba(16,42,67,0.55), animation backdrop-fade-in 200ms,
    role="presentation" + aria-hidden="true", click backdrop -> đóng drawer.
  - Hamburger button trong AdminTopbar (display: none ở desktop, inline-flex ở < 1024px),
    aria-label="Mở menu điều hướng", aria-expanded sync với sidebarOpen, click emit toggle-sidebar.
  - AdminSidebar emit `navigate` khi user click RouterLink enabled -> layout đóng drawer.
    Disabled <button> không emit, không navigate.
  - Layout watch route.fullPath: nếu sidebarOpen thì close (cover trường hợp navigate ngoài sidebar).
  - Layout listen window keydown Escape -> đóng drawer (cleanup ở onBeforeUnmount).
  - Body scroll lock khi drawer mở: document.body.style.overflow = "hidden" khi open, "" khi close
    (cleanup ở onBeforeUnmount để không leak khi unmount lúc drawer còn mở).
  - prefers-reduced-motion: tắt slide animation drawer + backdrop fade + page transition slide,
    chỉ giữ opacity transition ngắn 120ms cho user nhạy cảm motion.
  - Topbar trong drawer mode: subtitle ẩn (display: none) để topbar không cao quá; search width
    co từ 320px về min(220px, 30vw); ở < 640px actions stack xuống dòng dưới flex-basis: 100%.
  - Sidebar @media < 1024px: position: static (vì wrapper đã fixed), thêm box-shadow 8px 0 24px
    rgba(16,42,67,0.32) để drawer có depth so với backdrop.

Issue 4 - Doc clarification /dashboard vs /clinics
  Files: docs/current-task.frontend.md, temp/plan.frontend.md
  Vấn đề: chưa làm rõ /dashboard có frame Figma riêng hay không -> dễ hiểu nhầm.
  Fix: thêm section "Mapping Route ↔ Figma Frame" liệt kê rõ source of truth từng route.
```

Verify sau fix round 2:

```txt
cd frontend && npm run typecheck   -> PASS (clinic-admin, owner-admin, public-web)
cd frontend && npm run build       -> PASS (3 app)
  - clinic-admin   62.23 kB (không đổi, ngoài scope)
  - owner-admin    141.36 kB JS (+1.39 kB so với 139.97 kB do drawer state, hamburger,
                   watch route, keydown listener, scroll lock)
                   22.78 kB CSS (+2.41 kB so với 20.37 kB do drawer slide-in,
                   backdrop, hamburger, soon-badge, prefers-reduced-motion)
  - public-web     62.56 kB (không đổi, ngoài scope)
Vite HMR     -> dev server 5175 đã reload sau khi save (Lead chạy background, không restart).
Real API     -> vẫn chưa smoke, vẫn dùng mock fallback.
```

Mental walkthrough viewport:

```txt
1440px desktop : sidebar fixed 248px qua grid, hamburger ẩn, subtitle visible, search 320px.
1024px         : biên trên của drawer mode (media query là max-width: 1023px) -> vẫn desktop.
1023px tablet  : drawer mode bật, hamburger visible, content full width 1023px (gap padding 18px).
900px tablet   : drawer mode, hamburger visible, content full width.
640px          : drawer mode, topbar wrap (search + actions xuống dòng dưới), metrics grid 1 cột.
```

Tự verify behavior:

```txt
- Click nav item enabled (Dashboard/Phòng khám) trong drawer -> RouterLink navigate
  + AdminSidebar emit `navigate` + layout closeSidebar() -> drawer đóng.
- Click nav item disabled (Domains/Templates/...) trong drawer -> <button disabled>
  KHÔNG navigate, KHÔNG emit, drawer giữ mở.
- Click backdrop -> @click="closeSidebar".
- Phím Escape -> window keydown listener đóng drawer.
- Route đổi (ví dụ click "Tạo tenant" trên topbar khi drawer đang mở) -> watch route.fullPath đóng.
- Hamburger có aria-label="Mở menu điều hướng" và aria-expanded sync với sidebarOpen.
- Backdrop có role="presentation" + aria-hidden="true", screen reader bỏ qua.
- Body scroll lock khi drawer mở -> document.body.style.overflow = "hidden".
```

## Mapping Route ↔ Figma Frame

Mapping rõ ràng nguồn gốc UI cho từng route trong owner-admin để các phase sau biết khi nào cần re-check Figma:

```txt
/                       redirect tới /dashboard. Không có frame Figma riêng.

/dashboard              page phụ overview, KHÔNG có frame Figma riêng.
                        Là landing nhanh khi vào app, hiển thị 4 metric tổng quan
                        + preview 3 tenant gần đây + CTA "Mở danh sách phòng khám".
                        UI tận dụng cùng MetricCard + StatusPill + AppCard với /clinics.
                        Nếu Phase 4 cần frame riêng cho Dashboard, sẽ design + add sau.

/clinics                page chính, frame Figma "V2 - Owner Admin Tenant Operations" (36:348).
                        Source of truth UI Owner Admin: sidebar, topbar, metrics, filter bar,
                        tenant table, footer 2 card (Conflict 409 + Wizard CTA).

/clinics/create         wizard 4 bước. Bám phần "Create Tenant Wizard preview" trong
                        frame chính `V2 - Owner Admin Tenant Operations`.
                        Topbar ẩn nút "Tạo tenant" (route đang là chính trang tạo).

/clinics/:tenantId      detail page full-page. Bám phần "Detail Drawer" trong
                        frame chính `V2 - Owner Admin Tenant Operations`,
                        nhưng render full page (không phải drawer overlay) để hỗ trợ
                        deep-link/bookmark vào detail tenant cụ thể.
                        Drawer overlay vẫn được dùng song song trong /clinics khi click row.
```

## Bối Cảnh

Owner duyệt plan tại `temp/plan.frontend.md` và yêu cầu Frontend Agent implement. Code chỉ chạm `frontend/apps/owner-admin`, `frontend/packages/*`, plus 1 chỉnh stub nhỏ trong `clinic-admin/public-web` để giữ build chung pass.

Source đã đọc/inspect:

```txt
AGENTS.md
CLAUDE.md
docs/agent-playbook.md
docs/current-task.frontend.md
temp/plan.frontend.md
rules/coding-rules.md
frontend workspace hiện tại
Figma frame: 36:348 V2 - Owner Admin Tenant Operations (qua MCP get_screenshot + get_metadata)
```

## File Đã Tạo/Sửa

Owner Admin app:

```txt
frontend/apps/owner-admin/src/router/index.ts                       (cập nhật meta title/subtitle)
frontend/apps/owner-admin/src/layouts/OwnerAdminLayout.vue          (lấy title/subtitle từ route meta)
frontend/apps/owner-admin/src/components/AdminSidebar.vue           (brand ClinicOS Owner, nav 8 mục VN)
frontend/apps/owner-admin/src/components/AdminTopbar.vue            (props title/subtitle, search VN, nút "Tạo tenant")
frontend/apps/owner-admin/src/components/TenantTable.vue            (table chuẩn Figma, plan/domain dùng StatusPill, accessibility row keyboard)
frontend/apps/owner-admin/src/components/TenantFilterBar.vue        (label/option VN, giữ logic filter)
frontend/apps/owner-admin/src/components/TenantDetailDrawer.vue     (Escape close, label VN, format ngày vi-VN)
frontend/apps/owner-admin/src/components/CreateTenantWizard.vue     (auto-focus field 409 đầu tiên, jump tới step chứa field, label VN)
frontend/apps/owner-admin/src/components/ConflictState.vue          (banner 409 chuẩn, map field conflict sang label VN)
frontend/apps/owner-admin/src/pages/DashboardPage.vue               (metric VN, empty/error state)
frontend/apps/owner-admin/src/pages/ClinicsPage.vue                 (thêm empty/filter-empty, footer 2 card Conflict + Wizard CTA bám Figma)
frontend/apps/owner-admin/src/pages/CreateClinicPage.vue            (label VN)
frontend/apps/owner-admin/src/pages/ClinicDetailPage.vue            (label VN, error/loading state)
```

Stub fix tối thiểu để build chung pass (không động UI mới):

```txt
frontend/apps/clinic-admin/src/App.vue          (mockTenant.name -> mockTenant.displayName)
frontend/apps/public-web/src/App.vue            (mockTenant.name -> mockTenant.displayName)
```

Docs:

```txt
docs/current-task.frontend.md   (file này)
temp/plan.frontend.md           (tick xong các bước implement, thêm section verify)
```

Các file đã có sẵn từ implement trước (đã đọc và giữ pattern, không tạo file song song):

```txt
frontend/apps/owner-admin/src/main.ts
frontend/apps/owner-admin/src/App.vue
frontend/apps/owner-admin/src/services/tenantClient.ts
frontend/packages/api-client/src/{httpClient,tenantClient,mockTenantClient,index}.ts
frontend/packages/api-client/package.json
frontend/packages/shared-types/src/{tenant,index}.ts
frontend/packages/design-tokens/src/{ownerAdminTokens,index}.ts
frontend/packages/ui/src/components/{AppButton,AppCard,MetricCard,StatusPill}.vue
frontend/packages/ui/src/index.ts
```

## Routes Đã Có

```txt
/                         -> redirect /dashboard
/dashboard                -> DashboardPage.vue
/clinics                  -> ClinicsPage.vue (chính, bám frame Figma)
/clinics/create           -> CreateClinicPage.vue (wizard 4 bước, ẩn nút "Tạo tenant" trên topbar)
/clinics/:tenantId        -> ClinicDetailPage.vue (full-page detail)
```

## States Đã Cài

```txt
loading      : table cell, drawer, dashboard, detail page
empty        : ClinicsPage hiển thị CTA tạo tenant đầu tiên; Dashboard có message empty
error        : banner màu cam + nút Retry/Bỏ qua ở mọi page
conflict 409 : ConflictState component, wizard giữ form data và auto-focus field lỗi đầu, jump step
filter-empty : khi filter không match, có nút Reset filter
```

## Verify Đã Chạy

Build/typecheck:

```txt
cd frontend && npm run typecheck   -> PASS (clinic-admin, owner-admin, public-web)
cd frontend && npm run build       -> PASS (3 app build vite OK, không warning chặn)
npm run lint                       -> SKIP (script không tồn tại trong workspace)
npm install                        -> SKIP (lockfile match, không thêm dep mới)
```

Output build chính:

```txt
clinic-admin  dist/index.js  62.23 kB
owner-admin   dist/index.js  133.04 kB (+ css 20.37 kB)
public-web    dist/index.js  62.56 kB
```

Dev server runtime smoke (Lead chạy 2026-05-10):

```txt
Command   : cd frontend && npm run dev:owner
URL       : http://localhost:5175 (host 0.0.0.0, port 5175 free)
Vite ready: 659ms
```

HTTP route smoke (Invoke-WebRequest):

```txt
GET /                        -> 200, 124B, hasMain=True, hasRootDiv=True
GET /dashboard               -> 200, 124B, hasMain=True, hasRootDiv=True
GET /clinics                 -> 200, 124B, hasMain=True, hasRootDiv=True
GET /clinics/create          -> 200, 124B, hasMain=True, hasRootDiv=True
GET /clinics/aurora-dental   -> 200, 124B, hasMain=True, hasRootDiv=True
```

Vite transform smoke (4 file source key + 1 workspace alias):

```txt
GET /src/main.ts                              -> 200, 684B
GET /src/router/index.ts                      -> 200, 5786B
GET /src/services/tenantClient.ts             -> 200, 1758B (dùng import.meta.env, mode auto, fallback mock)
GET /src/pages/ClinicsPage.vue                -> 200, 33764B
GET /src/components/CreateTenantWizard.vue    -> 200, 53094B
GET /@id/@clinic-saas/api-client              -> 200, 655B (workspace alias resolve OK)
```

Wizard logic verification (regex trong file Vite-served):

```txt
focusFirstConflictField  -> True
fieldToStep              -> True (slug/contactEmail -> step 0; defaultDomainName -> step 2)
ConflictState            -> True
toggleModule             -> True
```

Static smoke flow chính:

```txt
- Mock client trả 3 tenant: aurora-dental Active+verified, river-eye Draft+pending, nova-skin Suspended+failed.
- Dashboard metric khớp mock: Active=1, Verified=1, Suspended=1, Needs support (Draft + failed)=2.
- Clinics table render đủ 6 cột Tenant/Slug/Plan/Domain/Status/Action; click row emit `tenant.id` chuẩn `tenant-{slug}`.
- Drawer chứa RouterLink `/clinics/${tenant.id}` dùng id full -> deep-link detail không 404.
- Wizard form mặc định = aurora-dental -> submit raw sẽ trùng slug + domain + email -> mock throw 409 với 3 conflict field; banner ConflictState hiện, jump tới step chứa field đầu tiên.
- Detail route `/clinics/tenant-aurora-dental` -> mock find by id OK -> render full detail.
- Detail route `/clinics/aurora-dental` (slug, không phải id) -> mock throw "Tenant not found." -> Detail page error state.
- Drawer Escape key + close button + backdrop click -> đóng drawer (kiểm tra source).
```

Không phát hiện bug cần sửa. Code không bị thay đổi trong session smoke này.

Limitations smoke runtime:

```txt
- Môi trường Lead không có Playwright/Puppeteer/headless browser tool.
- WebFetch tự upgrade HTTP -> HTTPS, không fetch được http://localhost.
- Vue SPA: mọi route trả cùng index.html (124B). Không thể phân biệt rendered DOM giữa các route qua HTTP smoke.
- Visual pixel-perfect đối chiếu Figma frame `V2 - Owner Admin Tenant Operations` cần con người mở trình duyệt.
- Smoke real API GET/POST/PATCH chưa thực hiện -> đợi Backend Phase 2 smoke pass và set VITE_API_BASE_URL + VITE_TENANT_API_MODE=real.
```

## Bám Figma

Frame chính `36:348 V2 - Owner Admin Tenant Operations` đã đối chiếu qua Figma MCP get_metadata + get_screenshot:

```txt
Sidebar           : ClinicOS Owner brand, role badge, 8 nav items VN (chỉ Dashboard + Phòng khám active).
Topbar            : title "Tenant Operations" + subtitle, search "Tìm tenant, slug, domain hoặc owner", nút "Tạo tenant".
Metrics           : Active tenants, Domains verified, Suspended, Needs support (4 MetricCard tone success/info/danger/warning).
Filter bar        : 4 select + Reset + Export disabled.
Tenant table      : Tenant, Slug, Plan (StatusPill), Domain (StatusPill + tên domain), Status (StatusPill), Action.
Detail drawer     : Tenant ID, Slug, Plan, Specialty, Owner, Ngày tạo, Domain list, Module list, Open detail + Active/Suspend.
Conflict 409 card : footer page Clinics + ConflictState trong wizard.
Wizard CTA card   : footer page Clinics, nút "Mở wizard".
```

Một điểm khác nhẹ với Figma: detail panel ở Figma là cột inline phải, code dùng drawer overlay theo plan duyệt + thêm route `/clinics/:tenantId` full-page riêng cho deep link.

## Còn Thiếu / Blocker

```txt
- QA tech-debt Phase 4 (gap D nhỏ): CreateTenantWizard.vue next() không gate required validation trước khi sang step kế. UX không sạch (cho phép qua step 1->4 dù field rỗng) nhưng submit cuối vẫn bị HTML5 required chặn -> KHÔNG phải data integrity blocker. Fix gợi ý đã ghi trong docs/testing/owner-admin-tenant-slice-smoke.md section 6.
- FE chưa wire env real (VITE_API_BASE_URL + VITE_TENANT_API_MODE=real) -> chưa smoke GET/POST/GET-by-id/PATCH thật. Backend Phase 2 đã Done trên server 116.118.47.78 -> sẵn sàng wiring khi Lead/owner cấp env.
- Lead/QA env hiện tại không có headless browser tool -> không thể assert pixel/DOM render cho từng route. HTTP + Vite transform + source regex smoke đã PASS thay thế. Visual pixel-perfect owner đã smoke 2 round 2026-05-10.
- Lint script chưa được khai báo trong workspace; nếu owner muốn enforce, cần thêm cấu hình ESLint sau.
- Domain operations (verify DNS, SSL automation, publish gateway) là backend domain-service, ngoài scope Phase 3 frontend.
- Clinic Admin và Public Website apps chỉ có stub placeholder, sẽ làm ở phase sau.
```

## QA Smoke Phase 3 (2026-05-10)

QA Agent (đã update `docs/agents/qa-agent.md` + `.claude/agents/qa-agent.md` thêm trách nhiệm FE Owner Admin Tenant Slice với checklist A-F đầy đủ) chạy manual smoke vì workspace không có vitest/playwright/cypress/jest/testing-library. Lead Agent đã quyết định KHÔNG thêm dependency test lớn ở phase này theo hard rule. Manual checklist + hướng dẫn rerun + hướng dẫn smoke real API tương lai trong:

```txt
docs/testing/owner-admin-tenant-slice-smoke.md
```

Kết quả nhóm:

```txt
A. Navigation/Layout                : PASS
B. Tenant List                      : PASS (test throw-error-on-list SKIP - mock không có hook fail)
C. Tenant Detail                    : PASS (visual DOM SKIP env không có browser - HTTP+Vite+regex thay thế)
D. Create Tenant Wizard             : PASS với 1 gap UX nhỏ (step gating - tech-debt Phase 4)
E. Build / Type Safety              : PASS (typecheck PASS x3 app, build PASS x3 app, lint SKIP đúng guard)
F. API Mode - Mock fallback         : PASS
F. API Mode - Real API smoke        : PENDING WIRING (env chưa set)
```

Lead phán quyết Option A — chuyển state Phase 3 sang **"Mock Functional Smoke PASS"** với caveat gap D ghi nhận thành tech-debt Phase 4 (không fix trong slice hiện tại vì không có data integrity risk; HTML5 required ở submit cuối vẫn chặn payload lỗi). Khi Lead/owner cấp env real, QA chạy lại nhóm F-real để đóng phase.

## Bước Tiếp Theo Đề Xuất

```txt
1. (Optional) Owner mở browser tới http://localhost:5175 để pixel-perfect visual đối chiếu Figma. Lead đã giữ dev server chạy phía background trong phiên smoke; phiên sau sẽ phải khởi lại.
2. (Hoặc) Bổ sung Playwright vào workspace để Lead/QA tự chạy headless visual smoke trong các phiên sau (đề xuất nhưng ngoài scope hard rule hiện tại — cần owner duyệt thêm).
3. Khi backend smoke Phase 2 Done, set VITE_API_BASE_URL và VITE_TENANT_API_MODE=real để smoke real API.
4. Confirm visual + real API pass -> Lead chuyển Phase 3 sang "Implementation Done"; không chuyển Phase 2 sang Done nếu backend smoke chưa pass.
5. Sau verify, plan tiếp Phase 4: domain verification UI hoặc bắt đầu Clinic Admin website builder slice.
```

## Vietnamese UI Copy Polish 2026-05-10

Owner yêu cầu rà soát toàn bộ visible UI copy trong Owner Admin Tenant Slice và chuyển sang tiếng Việt tự nhiên cho thị trường Việt Nam. Frontend Agent đã thực hiện surgical, KHÔNG đổi enum value backend, route name, type, API contract; chỉ đổi label/copy hiển thị cho user.

Helper formatter mới:

```txt
frontend/apps/owner-admin/src/services/labels.ts
  formatTenantStatus(status: TenantStatus): string
    Draft -> "Bản nháp"
    Active -> "Đang hoạt động"
    Suspended -> "Đã tạm ngưng"
    Archived -> "Đã lưu trữ"
  formatDomainStatus(status: TenantDomainStatus): string
    verified -> "Đã xác minh"
    pending -> "Đang chờ xác minh"
    failed -> "Xác minh thất bại"
    unknown -> "Chưa rõ"
  formatModuleCode(code: TenantModuleCode): string
    website -> "Website"
    booking -> "Đặt lịch"
    catalog -> "Danh mục dịch vụ"
    payments -> "Thanh toán"
    reports -> "Báo cáo"
    notifications -> "Thông báo"
```

Nhóm copy đã đổi:

```txt
1. Router meta title/subtitle: "Dashboard" -> "Tổng quan"; "Tenant Operations" -> "Quản lý phòng khám";
   "Tạo tenant mới" -> "Thêm phòng khám"; "Tenant detail" -> "Chi tiết phòng khám".
2. Sidebar nav (AdminSidebar): "Dashboard" -> "Tổng quan"; "Domains" -> "Tên miền";
   "Templates" -> "Mẫu giao diện"; "Billing" -> "Thanh toán"; "Monitoring" -> "Giám sát";
   "Support" -> "Hỗ trợ"; aria-label nav: "Điều hướng Owner Admin".
3. Topbar (AdminTopbar): default title/subtitle VN; placeholder search "Tìm phòng khám theo tên,
   slug hoặc tên miền"; aria-label "Tìm phòng khám"; nút "Thêm phòng khám".
4. Tenant table headers: Tenant -> "Phòng khám"; Plan -> "Gói"; Domain -> "Tên miền";
   Status -> "Trạng thái"; Action -> "Thao tác". Status/Domain pill dùng formatter VN.
5. Filter bar labels + options: Status -> "Trạng thái"; Plan -> "Gói"; Domain -> "Tên miền";
   options "Tất cả trạng thái/gói/tên miền/module"; status & domain options dùng formatter VN;
   Plan options giữ Starter/Growth/Premium (tên thương mại); button "Đặt lại"/"Xuất file".
6. Detail drawer + detail page: heading "Chi tiết phòng khám"; "Tenant ID" -> "Mã phòng khám";
   "Plan" -> "Gói"; "Owner" -> "Chủ sở hữu"; "Domain" -> "Tên miền"; "Email owner" ->
   "Email chủ sở hữu"; nút "Active hóa tenant" -> "Kích hoạt phòng khám"; "Suspend tenant" ->
   "Tạm ngưng phòng khám"; module pill dùng formatModuleCode.
7. Wizard steps: ["Phòng khám", "Gói & Module", "Tên miền", "Xác nhận"]; field "Slug tenant" ->
   "Slug phòng khám"; "Email owner" -> "Email chủ sở hữu"; "Subdomain mặc định" ->
   "Tên miền mặc định"; preview labels VN; submit "Tạo tenant" -> "Tạo phòng khám";
   side preview "Live setup preview" -> "Xem trước thiết lập"; plan description VN.
8. Conflict state: "Slug tenant" -> "Slug phòng khám"; "Domain mặc định" -> "Tên miền mặc định";
   "Email owner" -> "Email chủ sở hữu"; banner "Conflict duplicate slug/domain" ->
   "Trùng slug hoặc tên miền"; HTTP 409 giữ; copy hint Việt hóa.
9. ClinicsPage eyebrow "Resource index" -> "Danh mục phòng khám"; metric labels VN
   ("Phòng khám đang hoạt động", "Tên miền đã xác minh", "Đã tạm ngưng", "Cần hỗ trợ");
   footer cards "Trùng slug hoặc tên miền (HTTP 409)" và "Wizard tạo phòng khám" với
   nút "Mở wizard tạo phòng khám"; empty state CTA "Thêm phòng khám đầu tiên";
   filter empty + reset button VN.
10. DashboardPage h2 "Tenant control room" -> "Trung tâm vận hành phòng khám"; card
    "Tenant gần đây" -> "Phòng khám gần đây"; CTA "Tạo tenant" -> "Thêm phòng khám";
    error/empty message VN.
11. CreateClinicPage eyebrow "Create clinic flow" -> "Luồng thêm phòng khám"; h2
    "Wizard tạo tenant mới" -> "Wizard thêm phòng khám mới".
```

Từ kỹ thuật giữ tiếng Anh (lý do):

```txt
- "Slug": thuật ngữ chuẩn URL identifier, dev/admin Việt đều dùng nguyên.
- "Module": từ phổ biến tiếng Việt kỹ thuật, không cần dịch.
- "HTTP 409": mã lỗi HTTP chuẩn, giữ.
- "ClinicOS Owner" / "Owner Super Admin": brand + role hệ thống, giữ.
- "Starter/Growth/Premium": tên gói thương mại, không dịch.
- "Wizard": quen thuộc với admin Việt và thị trường SaaS VN.
```

File đã sửa:

```txt
frontend/apps/owner-admin/src/services/labels.ts (mới)
frontend/apps/owner-admin/src/router/index.ts
frontend/apps/owner-admin/src/layouts/OwnerAdminLayout.vue
frontend/apps/owner-admin/src/components/AdminSidebar.vue
frontend/apps/owner-admin/src/components/AdminTopbar.vue
frontend/apps/owner-admin/src/components/TenantTable.vue
frontend/apps/owner-admin/src/components/TenantFilterBar.vue
frontend/apps/owner-admin/src/components/TenantDetailDrawer.vue
frontend/apps/owner-admin/src/components/CreateTenantWizard.vue
frontend/apps/owner-admin/src/components/ConflictState.vue
frontend/apps/owner-admin/src/pages/DashboardPage.vue
frontend/apps/owner-admin/src/pages/ClinicsPage.vue
frontend/apps/owner-admin/src/pages/CreateClinicPage.vue
frontend/apps/owner-admin/src/pages/ClinicDetailPage.vue
```

Verify:

```txt
cd frontend && npm run typecheck  -> PASS (clinic-admin, owner-admin, public-web).
cd frontend && npm run build      -> PASS.
  - clinic-admin   62.23 kB JS (không đổi).
  - owner-admin   142.89 kB JS (+1.53 kB so 141.36 kB do thêm formatter helper + option arrays
                   trong TenantFilterBar). 22.78 kB CSS (không đổi).
  - public-web     62.56 kB JS (không đổi).
```

Self-check: grep label="[A-Z]"/aria-label/placeholder/title trong owner-admin -> không còn visible
English ngoài tên kỹ thuật/brand đã liệt kê ở trên. Enum value backend giữ nguyên trong store/filter
state/payload API.

## Real API Adapter Round 2026-05-10

QA Agent smoke real API qua SSH tunnel `localhost:5005 → server gateway` + Vite proxy `localhost:5175/api/* → tunnel`. Phát hiện 2 contract gap CRITICAL chặn Phase 3 trên real mode. Lead phán quyết FE-side fix (backend Phase 2 đã DONE và không sửa). Frontend Agent hoàn thành 2 fix surgical:

```txt
Gap A - TenantSummary/TenantDetail shape lệch backend wire format
  Backend list  : GET /api/tenants trả paged envelope `{items,total,limit,offset}`
                  với mỗi item flat partial: id, slug, displayName, status, planCode,
                  clinicName, createdAtUtc, updatedAtUtc.
                  KHÔNG có planDisplayName (empty string), specialty, defaultDomainName,
                  domainStatus, moduleCodes, contactEmail.
  Backend detail: GET /api/tenants/{id} trả nested với profile{clinicName, contactEmail,
                  phoneNumber, addressLine, specialty}, domains[]{id, domainName,
                  normalizedDomainName, domainType, status, isPrimary, createdAtUtc,
                  verifiedAtUtc}, modules[]{moduleCode, isEnabled, sourcePlanCode,
                  createdAtUtc, updatedAtUtc}, kèm createdAtUtc/updatedAtUtc/
                  activatedAtUtc/suspendedAtUtc/archivedAtUtc.
  Enum casing  : TenantDomainStatus backend trả PascalCase ("Pending"); FE shape lowercase.
                 planDisplayName trống -> phải fallback từ planCode lowercase.
  FE shape     : TenantSummary/TenantDetail flat (planDisplayName, specialty,
                 defaultDomainName, domainStatus, moduleCodes flat string[]).

Fix:
  - Tạo helper `frontend/packages/api-client/src/tenantAdapter.ts` chứa:
    + Type wire format: BackendTenantSummary, BackendTenantListResponse,
      BackendTenantDetail, BackendTenantProfile, BackendTenantDomain, BackendTenantModule.
    + adaptTenantDetail(raw): map nested -> FE shape flat. pickPrimaryDomain ưu tiên
      isPrimary === true, fallback domain[0]. pickEnabledModuleCodes filter isEnabled !== false.
      normalizeDomainStatus map PascalCase -> lowercase, default "unknown".
      fallbackPlanDisplayName Title Case từ planCode khi backend trả empty string.
      Domain id fallback `${tenantId}-${domainName}` cho forward-compat khi list endpoint
      trong tương lai có thể trả domain mà không kèm id.
      ownerName: undefined (backend không trả owner profile hiện tại).
    + adaptTenantSummary(raw): map item list flat partial -> TenantSummary; field thiếu
      fallback "" / [] / "unknown" để UI render được mà không show "undefined".
    + adaptTenantListResponse(raw): unwrap envelope `{items}` (hoặc fallback Array nếu
      endpoint sau này đổi) -> mảng TenantSummary[]; metadata total/limit/offset drop
      vì UI Phase 3 chưa cần phân trang.
  - Sửa `frontend/packages/api-client/src/tenantClient.ts` real path 4 endpoint:
    + listTenants: http.request<BackendTenantListResponse | BackendTenantSummary[]>
                   -> adaptTenantListResponse.
    + getTenant: http.request<BackendTenantDetail> -> adaptTenantDetail.
    + createTenant: request body giữ nguyên FE shape (backend accept đúng), response
                    -> adaptTenantDetail.
    + updateTenantStatus: tương tự -> adaptTenantDetail.
  - Mock client KHÔNG đổi (vẫn match FE shape; UI offline render được).
  - Sửa `frontend/packages/shared-types/src/tenant.ts`: TenantDetail.ownerName chuyển
    optional `ownerName?: string` (backend chưa trả owner profile, adapter để undefined).
  - Sửa `frontend/apps/owner-admin/src/components/TenantDetailDrawer.vue`: dòng "Chủ sở hữu"
    chỉ render khi `tenant.ownerName` có giá trị (v-if), không in "undefined" khi missing.
  - Export adapter qua `frontend/packages/api-client/src/index.ts` (forward-compat).

Gap B - Conflict 409 ProblemDetails RFC 9457 không có `fields`
  Backend trả: { type, title, status:409, detail, traceId } - KHÔNG có `fields` array.
  FE expect : { status:409, message, fields: TenantConflictField[] }.
  Wizard    : focusFirstConflictField dùng fields[0] để jump step + focus input.
  Verified detail từ smoke: detail = "Tenant slug or domain already exists." (chứa
              keyword "slug" + "domain" nên parser sẽ infer cả 2 field).

Fix:
  - Sửa normalizeConflict trong `tenantClient.ts`:
    + Thêm isProblemDetailsPayload: detect title/detail/type/traceId nhưng KHÔNG có fields.
    + Thêm parseConflictFieldsFromDetail: regex /slug/i, /domain/i, /email|contact/i
      case-insensitive, không match -> mảng rỗng (graceful fallback).
    + Forward-compat: nếu backend trả `fields` array thì respect array đó luôn (skip parse).
    + ProblemDetails branch: message = detail (cụ thể) thay vì title chung chung; fields
      do parser infer.
    + Fallback cuối: HttpError 409 không có payload nhận diện được -> fields = [].
  - CreateTenantWizard.focusFirstConflictField đã có `if (!firstField) return` -> graceful
    khi fields rỗng (không jump step, không focus). Verified không cần đụng.
  - ConflictState.vue: thêm copy generic khi fields rỗng:
    "Dữ liệu bị trùng — vui lòng kiểm tra slug, tên miền hoặc email và thử lại."
    (giữ tone tiếng Việt round trước, banner vẫn hiển thị message từ `detail` của backend).
```

File đã sửa Round 3:

```txt
frontend/packages/api-client/src/tenantAdapter.ts          (mới, helper riêng)
frontend/packages/api-client/src/tenantClient.ts           (áp adapter 4 endpoint, fix normalizeConflict cho ProblemDetails)
frontend/packages/api-client/src/index.ts                  (export tenantAdapter)
frontend/packages/shared-types/src/tenant.ts               (ownerName? optional)
frontend/apps/owner-admin/src/components/ConflictState.vue (copy generic khi fields rỗng)
frontend/apps/owner-admin/src/components/TenantDetailDrawer.vue (v-if cho ownerName optional)
```

Verify Round 3:

```txt
cd frontend && npm run typecheck   -> PASS (clinic-admin, owner-admin, public-web).
cd frontend && npm run build       -> PASS.
  - clinic-admin   62.23 kB JS (không đổi, ngoài scope).
  - owner-admin   145.61 kB JS (+2.72 kB so 142.89 kB do tenantAdapter + ProblemDetails
                   branch trong normalizeConflict + ConflictState v-else copy).
                   22.78 kB CSS (không đổi).
  - public-web     62.56 kB JS (không đổi, ngoài scope).

Status-only HTTP smoke qua Vite proxy localhost:5175 (status code only, không print
body chứa tenant data thật):
  GET  /api/tenants                  -> 200 (paged envelope, 1491 bytes).
  GET  /api/tenants/{id}             -> 200 (nested detail, 1031 bytes).
  POST /api/tenants (slug duplicate) -> 409 (ProblemDetails RFC 9457, có detail text).

Wire format đã verify khớp adapter:
  - List item keys (đã match BackendTenantSummary): id, slug, displayName, status,
    planCode, clinicName, createdAtUtc, updatedAtUtc.
  - Detail keys (đã match BackendTenantDetail): id, slug, displayName, status, planCode,
    planDisplayName, profile, domains, modules, createdAtUtc/updatedAtUtc, activatedAtUtc,
    suspendedAtUtc, archivedAtUtc.
  - Domain status raw "Pending" (PascalCase) -> normalizeDomainStatus trả "pending".
  - planDisplayName empty string -> fallbackPlanDisplayName trả "Starter"/"Growth"/"Premium".
  - 409 keys: type, title, status, detail, traceId — KHÔNG có fields, parser sẽ infer
    fields từ regex match trên `detail`.
```

Trạng thái mới Phase 3: **Real API HTTP Smoke 5/5 PASS, Contract Adapter applied, ready for QA F-real round 2**. Adapter chạy ở runtime FE (Vite HMR đã apply, không restart dev server). Verify pixel/DOM render thật cần QA F-real round 2 mở browser.

Edge case còn lại (ghi nhận để QA F-real round 2 xác nhận hoặc track tech-debt sau):

```txt
- list endpoint hiện trả paged envelope `{items,total,limit,offset}`; UI Phase 3 chỉ
  dùng items mảng, drop metadata. Khi nâng phân trang sau, sẽ cần update adapter trả
  `{items: TenantSummary[], total, limit, offset}` và update store/page consumer.
- TenantSummary từ list endpoint không có specialty/defaultDomainName/domainStatus/
  moduleCodes/contactEmail (backend list partial). FE adapter fallback "" / [] /
  "unknown". UI hiện tại render được nhưng filter "By specialty / By domain status /
  By module" trên TenantFilterBar sẽ chỉ work đầy đủ khi user click row mở drawer
  (drawer fetch detail riêng có đủ field). Track UX gap: filter trên list chỉ filter
  được field flat backend trả. Cần backend mở rộng list shape hoặc FE prefetch detail
  cho filter (Phase 4+).
- ownerName backend chưa trả ở mọi endpoint; drawer ẩn dòng "Chủ sở hữu". Khi backend
  bổ sung owner profile, adapter chỉ cần map `raw.profile.ownerName ?? raw.ownerName`
  vào FE shape mà không cần đổi UI.
- Mode `auto` với fallbackToMock=true: nếu real API trả 5xx/network error, FE rớt sang
  mock. Real mode `mode=real` + `VITE_TENANT_API_FALLBACK=false` (env hiện tại) sẽ
  không fallback, lỗi raw bubble ra UI để QA thấy.
- Module fields tương lai (ví dụ backend bổ sung `moduleConfig` hoặc `customLimits`):
  adapter pickEnabledModuleCodes hiện chỉ giữ moduleCode flat. Khi cần extend, mở rộng
  TenantModule type ở shared-types và update adapter cùng lúc.
```

## Guardrail Đã Tuân Thủ

```txt
Không sửa backend.
Không sửa Figma.
Không tạo Figma file mới.
Không commit / git add.
Không hardcode secret/token/IP/connection string.
Không chuyển Phase 2 sang Done.
Không print tenant data thật vào output (smoke chỉ status code + structure flag).
Comment trong code mới viết bằng tiếng Việt khi thuộc public contract.
```

## Codex Wave A Planning Pass 2026-05-10

Trạng thái: 🟡 **Plan Wave A đã được Codex Lead + Frontend Agent đối chiếu lại, chưa code**.

Đã đọc: `AGENTS.md`, `CLAUDE.md`, `rules/coding-rules.md`, `clinic_saas_report.md`, `architech.txt`, `docs/agent-playbook.md`, dashboard/lane current-task, `temp/plan.frontend.md`, roadmap section 7.1, `docs/agents/figma-ui-agent.md`, architecture docs và frontend workspace/package files liên quan.

Đã inspect Figma bằng MCP: `104:2`, `66:2`, `85:2`, `87:2`, `88:2`, `88:112`, `88:119`, `88:127`, `124:2`, `124:292`, `125:2`, `125:122`, `127:2`, `127:410`, `127:518`; page metadata `65:2` xác nhận V3 có 76 frame. Không sửa Figma.

Plan chi tiết mới nằm trong `temp/plan.frontend.md` §17. Điểm quyết định đáng chú ý:

```txt
- /dashboard nên là route canonical cho frame 124:2 Dashboard Cross-Tenant.
- Histoire/axe/Lighthouse cần owner duyệt dependency/package-lock trước khi thêm.
- useDomainVerifyPoll/polling thật cần owner chốt DNS retry tolerance; nếu chưa chốt, chỉ làm UI state + manual retry mock-first.
- Chưa code, chưa build/test. Frontend Agent dừng chờ owner duyệt Wave A.
```

## Wave A Step A5.1 - Shared Display Components 2026-05-11

Trạng thái: 🟡 **A5.1 implementation + verify PASS; A5.2 chờ owner duyệt tiếp**.

Phạm vi đã hoàn thành:
- Thêm 5 shared display component V3 trong `frontend/packages/ui/src/components`: `KPITile`, `ModuleChips`, `PlanBadge`, `EmptyState`, `DomainStateRow`.
- Export 5 component mới qua `frontend/packages/ui/src/index.ts`.
- Giữ `packages/ui` ở mức presentational: không import router, Pinia, api-client, shared-types; không gọi API; không polling DNS; không đọc env/storage; không hardcode tenant id/role.

File đã sửa/tạo trong commit `eca5086 feat(ui): add v3 shared display components`:

```txt
frontend/packages/ui/src/components/KPITile.vue
frontend/packages/ui/src/components/ModuleChips.vue
frontend/packages/ui/src/components/PlanBadge.vue
frontend/packages/ui/src/components/EmptyState.vue
frontend/packages/ui/src/components/DomainStateRow.vue
frontend/packages/ui/src/index.ts
```

Verify đã chạy:

```txt
cd frontend && npm run typecheck -> PASS cả 3 app: clinic-admin, owner-admin, public-web.
cd frontend && npm run build     -> PASS cả 3 app.
Static check 5 component mới không có hex/rgb/rgba literal.
git show --check --stat HEAD -> không có whitespace error.
```

Chưa làm / còn pending:
- `CommandPalette` shared base và `TenantSwitcher` presentational chưa implement, để lại A5.2.
- Chưa integrate các component mới vào Owner Admin route để tránh đổi behavior hiện có trong A5.1.
- Chưa chạy `npm run dev:owner` + manual browser smoke vì A5.1 chỉ thêm/export shared components.
- Không sửa backend, không sửa Figma, không thêm dependency, không push.

Bước resume tiếp theo:
1. Nếu owner duyệt A5.2, implement `CommandPalette` shared base + `TenantSwitcher` presentational theo boundary trong `temp/plan.frontend.md` §19.
2. Nếu owner muốn adopt UI ngay, tạo plan nhỏ cho adoption Owner Admin trước khi sửa route/component app.

## Wave A Step A5.1b - Owner Admin Shared Display Adoption 2026-05-11

Trạng thái: 🟢 **Implementation + verify PASS; chưa commit theo yêu cầu owner**.

Phạm vi đã hoàn thành:
- Source hiện có commit `1e01350 feat(owner-admin): adopt v3 shared display components` đã adopt phần ưu tiên A5.1b vào `DashboardPage.vue`, `ClinicsPage.vue`, `TenantTable.vue`, `TenantDetailDrawer.vue`.
- Lượt này hoàn tất phần còn lại trong scope A5.1b:
  - `CreateTenantWizard.vue`: preview/review dùng `PlanBadge` và `ModuleChips`, không thay button chọn plan/module.
  - `ClinicDetailPage.vue`: detail page dùng `PlanBadge`, `DomainStateRow`, `ModuleChips`, giữ status update/load route/API cũ.
- Không sửa backend, không sửa Figma, không đổi route/API client/store, không thêm dependency, không commit/push.

Verify đã chạy:

```txt
cd frontend && npm run typecheck -> PASS cả 3 app: clinic-admin, owner-admin, public-web.
cd frontend && npm run build     -> PASS cả 3 app.
git diff --check                 -> PASS, chỉ có warning LF/CRLF trên Windows.
rg "KPITile|ModuleChips|PlanBadge|EmptyState|DomainStateRow" frontend/apps/owner-admin/src -> có usage ở Owner Admin.
rg "MetricCard|module-meter|empty-state|domain-card" frontend/apps/owner-admin/src/pages frontend/apps/owner-admin/src/components -> no match.
dev:owner Vite localhost:5175 ready; HTTP route smoke 200 cho /dashboard, /clinics, /clinics/create, /clinics/mock-tenant-id.
```

Artifact/runtime:
- `npm run dev:owner` đang chạy ở `http://localhost:5175/`.
- Log generated untracked: `temp/owner-admin-vite.log`; không stage/commit.
- Chưa chụp screenshot/pixel smoke vì workspace hiện không có Playwright/Vitest/browser test dependency; HTTP route smoke đã chạy thay thế.

Bước tiếp theo đề xuất:
1. Owner review UI trên `http://localhost:5175/`.
2. Sau khi owner review xong, cleanup `temp/owner-admin-vite.log` theo workflow artifact cleanup.
3. Nếu owner duyệt tiếp, chuyển sang A5.2 (`CommandPalette` shared base + `TenantSwitcher` presentational).

## Wave A Step A5 Completion Bundle - Shared UI Foundation Closure 2026-05-11

Trạng thái: 🟢 **A5 Completion Bundle PASS - A5.4/A5.5/A5.6/A5.7 hoàn tất; chưa commit theo yêu cầu owner**.

Phạm vi đã hoàn tất:
- A5.1/A5.1b/A5.2 đã implementation + verify PASS trước đó.
- A5.3 verify PASS theo plan trong `temp/plan.frontend.md`.
- Lượt này hoàn tất feature team A5.4-A5.7:
  - A5.4 Adoption sweep: PASS, Owner Admin đã dùng shared UI A5 đúng các điểm đã plan.
  - A5.5 Boundary cleanup: PASS, `packages/ui` A5 components không kéo router/Pinia/API/shared-types/env/storage.
  - A5.6 Documentation closure: PASS, cập nhật lane docs.
  - A5.7 Completion gate: PASS, verify cuối toàn bộ A5.

Team assembly:

```txt
Lead Agent: điều phối lane frontend, đọc plan/current-task, gom kết quả.
Architect Agent: review boundary shared UI/app boundary/router/API/store/env/storage.
Frontend Agent: rà adoption và duplicate local rõ ràng trong owner-admin.
QA Agent: chạy verify cuối A5.
Documentation Agent: cập nhật docs/current-task.frontend.md và temp/plan.frontend.md.
```

Frontend Agent adoption sweep:

```txt
KPITile: DashboardPage.vue, ClinicsPage.vue.
EmptyState: ClinicsPage.vue.
PlanBadge + ModuleChips: TenantTable.vue, CreateTenantWizard.vue, ClinicDetailPage.vue.
DomainStateRow: TenantDetailDrawer.vue, ClinicDetailPage.vue.
CommandPalette: OwnerCommandPalette.vue wrapper app-local dùng shared base từ @clinic-saas/ui.
TenantSwitcher: chỉ export trong packages/ui, chưa tích hợp vào Topbar/layout nên không đổi behavior hiện tại.
```

Không sửa source implementation vì không phát hiện lỗi type/build/boundary hoặc duplicate rõ ràng cần cleanup surgical. Ghi chú: `DashboardPage.vue` còn empty state local nhỏ trong AppCard recent tenants và `TenantDetailDrawer.vue` còn plan text summary; cả hai được xem là polish sau, không phải blocker A5.

Verify đã chạy:

```txt
git diff --check -> PASS, chỉ warning LF/CRLF trên Windows.
cd frontend && npm run typecheck -> PASS cả 3 app: clinic-admin, owner-admin, public-web.
cd frontend && npm run build -> PASS cả 3 app.
Boundary rg forbidden imports/storage/env trên 7 component A5 -> PASS, no match.
Usage/export rg -> PASS, export trong ui index và usage trong owner-admin source đúng.
```

Skipped đúng scope:

```txt
Không làm A6 Owner Admin V3 restyle.
Không làm A7 state surfaces.
Không thêm route mới.
Không đổi API client/store/shared-types/business logic.
Không tích hợp TenantSwitcher vào Topbar vì có thể tạo hiểu nhầm tenant authority.
Không thêm package/dependency/Histoire/axe/Lighthouse.
Không sửa backend/Figma/package-lock.
```

Dirty/untracked sau completion:

```txt
Dirty tracked:
- temp/plan.frontend.md
- docs/current-task.frontend.md

Untracked artifact:
- temp/owner-admin-vite.log
```

Commit split proposal nếu owner yêu cầu:

```txt
docs(frontend): close a5 shared ui completion gate
```

Commit này chỉ gồm `temp/plan.frontend.md` và `docs/current-task.frontend.md`. Không stage/commit `temp/owner-admin-vite.log`.
## Wave A Step A6 - Owner Admin V3 Restyle / State Foundation 2026-05-11

Trạng thái: **Implementation + verify PASS; chưa commit theo yêu cầu owner**.

Team assembly:

```txt
Lead Agent: điều phối frontend lane, giữ scope A6, tích hợp source và verify.
Architect Agent: subagent runtime thật `Parfit`, review boundary route/API/store/env/package-lock.
Figma UI Agent: subagent runtime thật `Hubble`, trích checklist V3 từ docs/Figma frame notes và MCP read-only.
Frontend Agent: subagent runtime thật `Descartes`, polish shared UI foundation trong `frontend/packages/ui`.
QA Agent: Lead chạy verify cuối lượt theo checklist.
Documentation Agent: Lead cập nhật docs/current-task.frontend.md và temp/plan.frontend.md.
```

Phạm vi đã hoàn thành:

```txt
- Restyle Owner Admin shell/layout/sidebar/topbar theo V3: sidebar slate, topbar compact, focus/hover state rõ.
- Dashboard `/dashboard` chuyển sang cross-tenant cockpit V3: 6 KPI, MRR chart mock presentation-only,
  attention queue và recent tenants, không thêm API aggregate mới.
- Clinics `/clinics`: token V3 sweep cho heading, metric, filter, table, error/empty/filter-empty/footer state.
- TenantFilterBar: compact chip, aria-pressed, focus-visible.
- TenantTable: header tiếng Việt, aria-label, Space/Enter chọn row, row height compact.
- TenantDetailDrawer: sheet 560px, tabs visual Overview/Domains/Modules/Billing/Audit, PlanBadge, DomainStateRow.
- ClinicDetailPage: token/radius/error/loading state thống nhất V3.
- CreateTenantWizard: stepper/card/input/plan/module/preview chuyển sang V3 CSS variables, giữ validation/focus/conflict.
- Shared UI A5: AppButton/AppCard/MetricCard/StatusPill/KPITile/EmptyState/PlanBadge/ModuleChips/
  DomainStateRow/CommandPalette/TenantSwitcher polish radius/spacing/focus/reduced-motion.
```

Boundary đã giữ:

```txt
Không đổi route path.
Không đổi API client behavior.
Không đổi Pinia/store/business logic.
Không đổi shared-types contract.
Không thêm dependency, không sửa package.json/package-lock.
Không sửa backend, không sửa Figma file.
Không hardcode role/tenant id mới.
Không stage/commit/push.
```

Verify:

```txt
git diff --check -> PASS, chỉ warning LF/CRLF trên Windows.
cd frontend && npm run typecheck -> PASS cả 3 app: clinic-admin, owner-admin, public-web.
cd frontend && npm run build -> PASS cả 3 app.
HTTP smoke qua dev server hiện có http://localhost:5175:
  /dashboard -> 200
  /clinics -> 200
  /clinics/create -> 200
  /clinics/mock-tenant-id -> 200
Static boundary packages/ui forbidden imports/storage/env -> PASS, no match.
Static secret/IP check -> không thấy secret/token/IP thật; có match `localhost:` trong
  `frontend/apps/owner-admin/vite.config.ts` comment/config dev có sẵn, không phải A6 source mới.
```

Skipped/blocker:

```txt
- Không làm A7 state surfaces thật cho lifecycle confirm modal, DNS retry, SSL pending.
- Không thêm route mới `/cross-tenant-dashboard`.
- Không integrate TenantSwitcher vào Topbar vì chưa có tenant switching authority.
- Không chạy screenshot/pixel smoke vì phiên hiện không có Playwright/browser screenshot dependency sẵn.
Blocker: không có blocker cho A6 code/verify.
```

Dirty/untracked sau A6:

```txt
Dirty tracked: A6 owner-admin source, shared UI source, docs/current-task.frontend.md, temp/plan.frontend.md.
Untracked artifact còn lại: temp/owner-admin-vite.log (runtime artifact có từ trước, không stage/commit).
```

Commit split proposal nếu owner yêu cầu:

```txt
feat(owner-admin): restyle tenant operations surfaces for v3
feat(ui): polish shared admin components for v3
docs(frontend): record a6 owner admin v3 completion
```

## Wave A Step A6 - QA Visual Review Follow-up 2026-05-11

Trạng thái: **QA visual review PASS sau fix surgical; chưa commit theo yêu cầu owner**.

Phạm vi review:

```txt
Route dev server http://localhost:5175:
- /dashboard
- /clinics
- /clinics/create
- /clinics/mock-tenant-id
```

Issue đã phát hiện và fix:

```txt
1. /clinics/mock-tenant-id hiển thị error "Tenant not found." nhưng heading vẫn là
   "Đang tải phòng khám..." -> sửa ClinicDetailPage để heading/error state nhất quán
   và Việt hóa lỗi not found.
2. Mobile viewport 390px có overflow/cắt topbar CTA và heading dài -> sửa responsive
   guard ở OwnerAdminLayout/AdminTopbar, thêm wrap guard heading page, thêm box-sizing
   cho AppButton/AppCard để control width 100% không vượt viewport do padding.
```

Verify đã chạy:

```txt
HTTP smoke /dashboard, /clinics, /clinics/create, /clinics/mock-tenant-id -> 200, có #app.
Edge headless screenshot desktop 1440x1100 và mobile 390x844 -> đã review.
cd frontend && npm run typecheck -> PASS cả clinic-admin, owner-admin, public-web.
cd frontend && npm run build -> PASS cả clinic-admin, owner-admin, public-web.
git diff --check -> PASS, chỉ warning LF/CRLF trên Windows.
```

Screenshot artifact review, không stage/commit:

```txt
frontend/test-results/owner-dashboard-a6.png
frontend/test-results/owner-clinics-a6.png
frontend/test-results/owner-clinics-create-a6.png
frontend/test-results/owner-clinics-detail-a6.png
frontend/test-results/owner-dashboard-a6-mobile.png
frontend/test-results/owner-clinics-a6-mobile.png
frontend/test-results/owner-clinics-create-a6-mobile.png
frontend/test-results/owner-clinics-detail-a6-mobile.png
```

Guardrail:

```txt
Không stage, không commit, không push.
Không sửa backend/Figma/package-lock/API client/store/shared-types.
Không stage/commit frontend/test-results/ hoặc temp/owner-admin-vite.log.
```

## Wave A Step A7 - Budget Mode Verify 2026-05-11

Trạng thái: **A7 source verify PASS; chưa stage/commit/push theo yêu cầu owner**.

Scope hoàn tất trong worktree hiện tại:

```txt
- Thêm DomainDnsRetryState, SslPendingState, TenantLifecycleConfirmModal.
- Gắn DNS/SSL state surfaces vào DashboardPage, TenantDetailDrawer, ClinicDetailPage.
- Lifecycle confirm modal dùng cho suspend/archive/restore, không đổi route/store/API contract.
```

Verify:

```txt
cd frontend && npm run typecheck -> PASS.
cd frontend && npm run build -> PASS.
git diff --check -> PASS, chỉ warning LF/CRLF trên Windows.
Static boundary packages/ui + 3 A7 components -> PASS, no match.
Secret/IP check owner-admin/ui/design-tokens -> PASS, no match.
HTTP smoke không screenshot:
  /dashboard -> 200, app=True.
  /clinics -> 200, app=True.
  /clinics/aurora-dental -> 200, app=True.
```

Cleanup artifact:

```txt
Đã xóa untracked/generated: frontend/test-results/, .playwright-mcp/,
a7-dashboard-state-surfaces-desktop.png, temp/owner-admin-vite.log,
temp/owner-admin-vite.log.err.
Đã dừng node vite dev server port 5175 để gỡ lock log trước khi xóa.
```

Dirty còn lại:

```txt
Source A7: TenantDetailDrawer.vue, ClinicDetailPage.vue, DashboardPage.vue,
DomainDnsRetryState.vue, SslPendingState.vue, TenantLifecycleConfirmModal.vue.
Docs lane: docs/current-task.frontend.md, temp/plan.frontend.md.
Không stage, không commit, không push A7.
```

## Wave A Step A7 - Budget Contact Sheet Follow-up 2026-05-11

Trạng thái: **visual review FAIL; verify kỹ thuật PASS; chưa commit/push**.

Đã thực hiện:

```txt
- Tạo contact sheet đúng 1 file: frontend/test-results/a7-visual-contact-sheet.png.
- Dev server chạy mock mode để tránh proxy 127.0.0.1:5005 không sẵn sàng.
- Contact sheet gồm desktop/mobile /dashboard và desktop/mobile /clinics/aurora-dental.
- Lifecycle modal không thêm vào contact sheet vì không mở được bằng UI an toàn trong flow hiện có.
```

Kết quả:

```txt
Visual review FAIL: /clinics/aurora-dental hiển thị state "Không tìm thấy phòng khám"
trong mock mode; dashboard hiển thị mock data đúng.
Verify kỹ thuật PASS:
  git diff --check
  cd frontend && npm run typecheck
  cd frontend && npm run build
  static boundary no match
  secret/IP no match
  HTTP smoke /dashboard, /clinics, /clinics/aurora-dental -> 200 + #app
```

Resume next:

```txt
Không stage/commit/push cho đến khi owner quyết định cách xử lý visual fail.
Giữ contact sheet để review: frontend/test-results/a7-visual-contact-sheet.png.
Đã xóa log dev server temp/owner-admin-vite.log và .err; không giữ screenshot rời.
```

## Wave A Step A7 - Budget Visual Fix 2026-05-11

Trạng thái: **visual review PASS sau fix surgical; chưa stage/commit/push**.

Fix:

```txt
- Sửa mock tenant detail lookup để nhận cả tenant id và tenant slug.
- /clinics/aurora-dental trong mock mode render Aurora Dental thay vì "Không tìm thấy phòng khám".
- Không đổi route path, API contract, store, shared-types, backend, package-lock hoặc dependency.
```

Verify:

```txt
git diff --check -> PASS, chỉ warning LF/CRLF trên Windows.
cd frontend && npm run typecheck -> PASS.
cd frontend && npm run build -> PASS.
Static boundary -> PASS, no match.
Secret/IP -> PASS, no match.
HTTP smoke /dashboard, /clinics, /clinics/aurora-dental -> PASS 200 + #app.
Contact sheet PASS: frontend/test-results/a7-visual-contact-sheet.png.
```

## Wave A Step A7 - QA Retest Budget 2026-05-11

Trạng thái: **Owner visual review PASS + QA retest PASS; chưa stage/commit/push**.

```txt
Owner đã review contact sheet frontend/test-results/a7-visual-contact-sheet.png và PASS.
QA retest không tạo thêm screenshot, không gọi Figma/full team, không sửa code.
Verify PASS:
  git diff --check
  cd frontend && npm run typecheck
  cd frontend && npm run build
  static boundary no match
  secret/IP no match
  HTTP smoke /dashboard, /clinics, /clinics/aurora-dental -> 200 + #app
Contact sheet hiện còn tồn tại để owner review/reference.
```

## Wave A Step A7 - Finalize 2026-05-11

Trạng thái: **A7 PASS, sẵn sàng commit split và push theo yêu cầu owner**.

```txt
Owner visual review PASS trên contact sheet A7.
QA retest budget PASS: git diff --check, typecheck, build, static boundary,
secret/IP và HTTP smoke /dashboard, /clinics, /clinics/aurora-dental.
Artifact contact sheet chỉ dùng review tạm và sẽ cleanup trước push.
Commit split:
  feat(owner-admin): add v3 lifecycle state surfaces
  docs(frontend): record a7 budget verification
```

## Wave A Step A8 - Owner Admin Plan/Module Catalog Mock-First 2026-05-12

Trạng thái: **Implementation + verify PASS; chưa stage/commit/push**.

Team assembly:

```txt
Lead Agent: điều phối Frontend lane, xác nhận A7 qua git log/status, ghi plan A8 trước khi code.
Frontend Agent: implement `/plans`, mock data và route/nav/command palette.
Figma UI Agent: read-only, đối chiếu frame 126:2 Plan/Module Catalog và 126:173 Plan Assignment qua Figma MCP.
QA Agent: typecheck/build, HTTP smoke, static source check và contact sheet budget.
Documentation Agent: cập nhật docs/current-task.frontend.md và temp/plan.frontend.md.
```

Scope đã hoàn tất:

```txt
- `/plans` Owner Admin route cho Plan & Module Catalog mock-first.
- `planCatalogMock.ts` chứa plan cards, 12 module matrix row, tenant assignment draft.
- Sidebar bật nav "Gói & module"; command palette có action mở `/plans`.
- `/plans` render Starter/Growth/Premium, module matrix, tenant plan assignment, selected tenants, target plan select, MRR diff và bulk action mock.
- A7 state surfaces vẫn có mặt: dashboard DNS/SSL attention; detail Aurora Dental có lifecycle action buttons; source vẫn có modal open/close/confirm và DNS/SSL preview.
```

Mock data/API dependency:

```txt
Mock data: frontend/apps/owner-admin/src/services/planCatalogMock.ts.
API thật chưa wire. BE handoff request:
- GET /api/owner/plans
- GET /api/owner/modules
- GET /api/owner/tenant-plan-assignments
- POST /api/owner/tenant-plan-assignments/bulk-change
- Domain Service Phase 4.1 status/retry/pending contract cho DNS/SSL real state.
```

Verify:

```txt
git diff --check -> PASS, chỉ warning LF/CRLF trên Windows; backend dirty là lane khác.
cd frontend && npm run typecheck -> PASS.
cd frontend && npm run build -> PASS.
HTTP smoke mock mode:
  /dashboard, /clinics, /clinics/create, /clinics/aurora-dental, /plans -> 200 + #app.
Vite transform smoke:
  /src/main.ts, /src/router/index.ts, /src/pages/PlanModuleCatalogPage.vue,
  /src/services/planCatalogMock.ts -> 200.
CDP interaction smoke /plans:
  checkbox count 6; tick thêm nova-skin -> selected 4;
  đổi target plan nova-skin sang Premium; Apply mock change -> status
  "4 tenant sẽ đổi gói ở chu kỳ kế tiếp. MRR dự kiến tăng $420."
Visual artifact:
  frontend/test-results/a8-fe-contact-sheet.png.
```

Test gaps:

```txt
- Chưa click automation lifecycle modal; plan bulk action interaction smoke đã PASS qua Edge CDP.
- Real API smoke pending BE contract.
- Contact sheet là generated artifact review, không stage/commit nếu owner chưa yêu cầu.
```

Dirty/untracked liên quan FE A8:

```txt
frontend/apps/owner-admin/src/components/AdminSidebar.vue
frontend/apps/owner-admin/src/components/OwnerCommandPalette.vue
frontend/apps/owner-admin/src/router/index.ts
frontend/apps/owner-admin/src/pages/PlanModuleCatalogPage.vue
frontend/apps/owner-admin/src/services/planCatalogMock.ts
docs/current-task.frontend.md
temp/plan.frontend.md
frontend/test-results/a8-fe-contact-sheet.png (artifact)
```

Ghi chú lane khác:

```txt
Worktree có backend dirty/untracked từ tab backend riêng. Frontend A8 không sửa, không stage, không revert.
```

## Wave A Step A8 - /plans Visual Quality Fix 2026-05-12

Trạng thái: **visual polish + verify PASS; chưa stage/commit/push**.

Phạm vi sửa đúng yêu cầu owner:

```txt
- Tăng contrast toàn màn /plans: heading/subtitle/table text đậm hơn, tránh label xám mờ.
- Plan cards Starter/Growth/Premium khác tone rõ; Growth popular nổi bật bằng nền slate nhưng giữ text/price dễ đọc.
- Thêm hierarchy price + tenant count + module summary trên plan cards.
- Module matrix tăng font/row spacing, header rõ hơn, enabled/limited/locked state có chip/icon dễ đọc.
- Plan assignment nổi rõ trên desktop contact sheet; CTA Apply mock change rõ.
- Mobile /plans stack gọn hơn; matrix/assignment không phụ thuộc overflow ngang và contact sheet thấy được assignment/table.
```

Verify sau fix:

```txt
git diff --check -> PASS, chỉ warning LF/CRLF trên Windows.
cd frontend && npm run typecheck -> PASS cả 3 app.
cd frontend && npm run build -> PASS cả 3 app.
HTTP smoke:
  /dashboard, /clinics, /clinics/create, /clinics/aurora-dental, /plans -> 200 + #app.
Interaction smoke /plans:
  checkbox count 6; chọn/bỏ chọn/chọn lại nova-skin; đổi target plan sang Premium;
  Apply mock change -> "4 tenant sẽ đổi gói ở chu kỳ kế tiếp. MRR dự kiến tăng $420."; app không crash.
Visual contact sheet:
  frontend/test-results/a8-fe-contact-sheet.png
```

Test gaps giữ nguyên:

```txt
- Chưa click automation lifecycle modal.
- Real API smoke pending BE contract.
- Contact sheet là generated artifact review, không stage/commit nếu owner chưa yêu cầu.
```

## Wave A Step A9 - Wiring `/plans` Với BE A.2 Real Contract 2026-05-12

Trạng thái: **Implementation + verify PASS; chưa stage/commit/push**.

Scope hoàn tất:

```txt
- `/plans` chuyển từ mock import trực tiếp sang app-level `planCatalogClient`.
- Thêm shared types: `OwnerPlanCatalogItem`, `OwnerModuleCatalogRow`,
  `OwnerTenantPlanAssignment`, bulk-change request/response.
- Thêm typed API client gọi BE A.2:
  GET /api/owner/plans
  GET /api/owner/modules
  GET /api/owner/tenant-plan-assignments
  POST /api/owner/tenant-plan-assignments/bulk-change
- Giữ mock fallback A8 qua `planCatalogMock.ts`.
- Thêm loading/error/retry state cho `/plans`; bulk-change gửi auditReason + effectiveAt next renewal.
```

Verify:

```txt
git diff --check -> PASS, chỉ warning LF/CRLF trên Windows.
cd frontend && npm run typecheck -> PASS.
cd frontend && npm run build -> PASS.
HTTP smoke `http://localhost:5175`:
  /dashboard, /clinics, /clinics/create, /clinics/aurora-dental, /plans -> 200 + #app.
Vite transform smoke:
  /src/main.ts, /src/router/index.ts, /src/pages/PlanModuleCatalogPage.vue,
  /src/services/planCatalogClient.ts, /src/services/planCatalogMock.ts -> 200.
Real API direct smoke:
  /api/owner/plans hiện 500 vì backend runtime/gateway không bật trong phiên này; UI vẫn fallback mock/auto.
```

Song song được:

```txt
Backend/DevOps có thể chạy BE A.3 contract hardening/test guard song song, không chặn FE:
  tenant mismatch, missing X-Tenant-Id, ClinicAdmin forbidden cho `/api/owner/*`,
  OpenAPI/gateway smoke consistency.
Backend persistence/schema plan-module thật vẫn cần owner duyệt riêng, không làm trong FE A9.
```
