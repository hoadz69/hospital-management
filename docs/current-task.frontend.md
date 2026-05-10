# Current Task Frontend - Phase 3 Done + Phase 4 Wave A V3 Foundation Planning

Ngày cập nhật: 2026-05-10

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
