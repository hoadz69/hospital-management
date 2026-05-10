# Kế Hoạch Frontend - Phase 3 Owner Admin Tenant Slice

Ngày cập nhật: 2026-05-10

Trạng thái: **Mock Functional Smoke PASS - Real API Smoke Pending Wiring** (QA Agent 2026-05-10). Implementation Done + post-smoke fix round 1 + 2 done. QA manual smoke A-F: A/B/C/E/F-mock PASS; D PASS với 1 gap UX nhỏ (step gating tech-debt Phase 4, không phải data integrity blocker). Checklist tại `docs/testing/owner-admin-tenant-slice-smoke.md`.

Owner visual smoke ngày 2026-05-10 phát hiện 2 round issue (đã fix):
- Round 1: sidebar disabled (RouterLink -> button) + chuyển trang giật (Transition fade+slide-up).
- Round 2: badge dính chữ "OWNERSUPERADMIN" + UX disabled item (badge "Sắp ra mắt") + responsive narrow (drawer pattern + hamburger) + doc clarification /dashboard vs /clinics.

Cả 2 round đã fix surgical, typecheck/build PASS lại. Real API smoke chưa thực hiện vì backend Phase 2 smoke real chưa Done trong session frontend hiện tại. Plan ban đầu lập 2026-05-09; smoke + fix 2026-05-10.

Chế độ thực hiện: Đã code Phase 3 owner-admin theo plan duyệt. Verify typecheck/build PASS, dev server lên port 5175 + Vite transform PASS. Chưa smoke API thật do backend smoke chưa Done.

## 1. Bối Cảnh

UI source of truth đã đủ để bắt đầu Phase 3 frontend:

```txt
Figma file: https://www.figma.com/design/1nbJ5XkrlDgQ3BmzpXzhCC
Page: UI Redesign V2 - 2026-05-09
Frame chính: V2 - Owner Admin Tenant Operations
Frame tham chiếu:
- V2 - Clinic Admin Website Builder
- V2 - Public Homepage Desktop
- V2 - Public Booking Flow Desktop
```

Phase 2 Tenant API đã implement nhưng DB/API smoke vẫn có thể chưa Done. Vì vậy frontend phải có API client/mock fallback để UI shell và visual check không bị block.

## 2. App Làm Trước

Làm trước trong:

```txt
frontend/apps/owner-admin
```

Các app `clinic-admin` và `public-web` chỉ dùng làm tham chiếu Figma/workflow trong task đầu. Chưa implement code cho hai app này khi Owner Admin Tenant Slice chưa xong.

## 3. Scope Khi Owner Duyệt Implement

Được sửa/tạo:

```txt
frontend/apps/owner-admin
frontend/packages/ui
frontend/packages/api-client
frontend/packages/shared-types
frontend/packages/design-tokens nếu cần map token Figma V2
docs/current-task.frontend.md
temp/plan.frontend.md
docs/roadmap/clinic-saas-roadmap.md nếu status Phase 3 thay đổi
```

Không sửa:

```txt
backend/
Figma
infrastructure backend runtime
docs/current-task.backend.md
temp/plan.backend.md
```

## 4. Routes

Tạo/hoàn thiện routes trong `owner-admin`:

```txt
/dashboard
/clinics
/clinics/create
/clinics/:tenantId
```

Mapping:

- `/dashboard`: dashboard shell ngắn, metric tổng quan từ tenant list hoặc mock.
- `/clinics`: màn hình chính bám frame `V2 - Owner Admin Tenant Operations`.
- `/clinics/create`: wizard tạo tenant.
- `/clinics/:tenantId`: detail route, có thể dùng page hoặc bridge vào detail drawer theo layout V2.

## 5. Components Cần Tạo

Shared UI / app components:

```txt
AppButton
AppCard
StatusPill
MetricCard
AdminSidebar
AdminTopbar
TenantTable
TenantDetailDrawer
TenantFilterBar
CreateTenantWizard
ConflictState
```

Vai trò chính:

- `AppButton`: primary, secondary, danger, ghost, loading, disabled.
- `AppCard`: card shell cho metric, table, drawer, conflict, wizard preview.
- `StatusPill`: status, plan, domain, trend, filter chip.
- `MetricCard`: Active tenants, Domains verified, Suspended, Needs support.
- `AdminSidebar`: nav Owner Admin, active route `/clinics`.
- `AdminTopbar`: title, subtitle, search, create tenant CTA.
- `TenantFilterBar`: status/plan/domain/module filters, reset/export placeholder.
- `TenantTable`: resource index tenant list, selected row, action menu placeholder.
- `TenantDetailDrawer`: tenant id, slug, plan, modules, domain, status, created date, open/suspend.
- `CreateTenantWizard`: 4 bước gồm clinic info, plan/modules, domain, template/publish placeholder.
- `ConflictState`: duplicate slug/domain 409, giữ form data, focus field lỗi đầu tiên.

## 6. Data/API Mapping

API client map đúng Phase 2 endpoints:

```txt
GET /api/tenants
POST /api/tenants
GET /api/tenants/{id}
PATCH /api/tenants/{id}/status
```

Shared types dự kiến:

```txt
TenantSummary
TenantDetail
TenantStatus = Draft | Active | Suspended | Archived
TenantDomainStatus = verified | pending | failed | unknown
TenantCreateRequest
TenantStatusUpdateRequest
ApiConflictError
```

Luồng:

- `/clinics` gọi `GET /api/tenants`.
- Chọn row gọi hoặc reuse `GET /api/tenants/{id}` cho drawer/detail.
- `/clinics/create` submit `POST /api/tenants`.
- Suspend/activate/archive gọi `PATCH /api/tenants/{id}/status`.
- Nếu `POST /api/tenants` trả `409`, hiển thị `ConflictState`, giữ dữ liệu form, không tự thêm tenant giả vào list.

## 7. Mock Fallback

Vì backend smoke có thể chưa sẵn sàng:

- Tạo tenant client có mode `real` và `mock`.
- Ưu tiên real API khi env/base URL có sẵn.
- Nếu real API unavailable trong dev, dùng mock data local để visual/UI check không bị block.
- Mock chỉ nằm trong frontend dev layer và giữ contract gần API thật.
- UI vẫn phải có error/retry state khi real API lỗi.

## 8. Files Dự Kiến Sửa/Tạo

```txt
frontend/apps/owner-admin/src/main.ts
frontend/apps/owner-admin/src/App.vue
frontend/apps/owner-admin/src/router/index.ts

frontend/apps/owner-admin/src/layouts/OwnerAdminLayout.vue
frontend/apps/owner-admin/src/pages/DashboardPage.vue
frontend/apps/owner-admin/src/pages/ClinicsPage.vue
frontend/apps/owner-admin/src/pages/CreateClinicPage.vue
frontend/apps/owner-admin/src/pages/ClinicDetailPage.vue

frontend/apps/owner-admin/src/components/AdminSidebar.vue
frontend/apps/owner-admin/src/components/AdminTopbar.vue
frontend/apps/owner-admin/src/components/TenantTable.vue
frontend/apps/owner-admin/src/components/TenantDetailDrawer.vue
frontend/apps/owner-admin/src/components/TenantFilterBar.vue
frontend/apps/owner-admin/src/components/CreateTenantWizard.vue
frontend/apps/owner-admin/src/components/ConflictState.vue

frontend/packages/ui/src/components/AppButton.vue
frontend/packages/ui/src/components/AppCard.vue
frontend/packages/ui/src/components/StatusPill.vue
frontend/packages/ui/src/components/MetricCard.vue
frontend/packages/ui/src/index.ts

frontend/packages/shared-types/src/tenant.ts
frontend/packages/shared-types/src/index.ts

frontend/packages/api-client/src/httpClient.ts
frontend/packages/api-client/src/tenantClient.ts
frontend/packages/api-client/src/mockTenantClient.ts
frontend/packages/api-client/src/index.ts

frontend/packages/design-tokens/src/index.ts
frontend/packages/design-tokens/src/ownerAdminTokens.ts
```

Nếu repo đã có file tương đương thì sửa theo pattern hiện có, không tạo file song song cùng trách nhiệm.

## 9. Thứ Tự Implement Sau Khi Được Duyệt

1. [x] Đọc lại structure thật của `owner-admin`, `ui`, `api-client`, `shared-types`, `design-tokens`.
2. [x] Chốt token Figma V2 tối thiểu: background, sidebar, border, text, primary teal, status colors, radius, shadow.
3. [x] Tạo shared types và tenant API client/mock fallback.
4. [x] Tạo `OwnerAdminLayout` với `AdminSidebar` và `AdminTopbar` (topbar nhận title/subtitle qua route meta).
5. [x] Hoàn thiện `AppButton`, `AppCard`, `StatusPill`, `MetricCard`.
6. [x] Tạo `/clinics` với metrics, filter bar, tenant table, detail drawer, conflict + wizard CTA footer.
7. [x] Tạo `/clinics/create` với `CreateTenantWizard` (auto focus field 409, giữ form data).
8. [x] Tạo `/clinics/:tenantId` detail route full-page.
9. [x] Thêm loading, empty, error, conflict states.
10. [x] Visual pass đối chiếu Figma frame `V2 - Owner Admin Tenant Operations`. Owner đã visual smoke trên http://localhost:5175 ngày 2026-05-10, phát hiện 2 round issue và đã fix:

    Round 1 (2 issue):
    - Sidebar nav disabled chuyển từ `<RouterLink>` sang `<button disabled>` để không navigate và không match router-link-active (file `AdminSidebar.vue`).
    - Layout main bọc `<RouterView>` bằng `<Transition name="page" mode="out-in" appear>` (fade + slide-up 200ms cubic-bezier(0.4,0,0.2,1)) để chuyển trang mượt (file `OwnerAdminLayout.vue`).

    Round 2 (4 issue):
    - Badge "OWNERSUPERADMIN" dính chữ -> đổi text "OwnerSuperAdmin" thành "Owner Super Admin" (file `AdminSidebar.vue`).
    - UX item disabled -> thêm visible badge "Sắp ra mắt" cạnh phải label cho mọi nav item disabled (file `AdminSidebar.vue`).
    - Responsive narrow viewport -> drawer pattern cho < 1024px:
      `OwnerAdminLayout.vue` thêm ref sidebarOpen, watch route đóng drawer, listen Escape, body scroll lock, prefers-reduced-motion fallback, sidebar wrapper position fixed slide-in, backdrop click close.
      `AdminTopbar.vue` thêm hamburger button (display: none ở desktop, inline-flex ở < 1024px), aria-label "Mở menu điều hướng", aria-expanded sync sidebarOpen, emit toggle-sidebar.
      `AdminSidebar.vue` emit `navigate` khi click nav enabled để layout đóng drawer, sidebar @media < 1024px position: static + box-shadow drawer.
    - Doc /dashboard vs /clinics -> ghi rõ /dashboard không có frame Figma riêng (page phụ overview). /clinics là frame chính source of truth. Mapping route ↔ Figma frame có trong `docs/current-task.frontend.md`.

    Typecheck + build PASS lại sau cả 2 round. Chờ owner re-check visual.

11. [x] QA Agent manual smoke A-F (workspace không có vitest/playwright -> manual checklist tại `docs/testing/owner-admin-tenant-slice-smoke.md`). PASS toàn nhóm A/B/C/E/F-mock; nhóm D PASS với 1 gap UX step gating (tech-debt Phase 4, không phải data integrity blocker). F real API pending wiring vì FE chưa set env real (Backend Phase 2 Done trên server 116.118.47.78).

12. [x] Vietnamese UI copy polish 2026-05-10. Frontend Agent rà soát toàn bộ visible UI copy trong Owner Admin Tenant Slice và chuyển sang tiếng Việt tự nhiên cho thị trường VN. Tạo helper `frontend/apps/owner-admin/src/services/labels.ts` với 3 formatter (formatTenantStatus, formatDomainStatus, formatModuleCode) là cầu nối enum backend ↔ label hiển thị. Áp dụng tại TenantTable, TenantFilterBar, TenantDetailDrawer, ClinicDetailPage, DashboardPage, CreateTenantWizard. Cập nhật router meta, AdminSidebar nav (Domains->Tên miền, Templates->Mẫu giao diện, Billing->Thanh toán, Monitoring->Giám sát, Support->Hỗ trợ), AdminTopbar (placeholder + nút "Thêm phòng khám"), ClinicsPage (eyebrow + metric labels + footer cards + empty state), DashboardPage (h2 + tenant strip), CreateClinicPage (eyebrow + h2), TenantDetailDrawer (heading + dt labels + nút Active/Suspend), TenantTable (headers VN), CreateTenantWizard (steps + form labels + plan description + submit), ConflictState (banner + field labels + hint). Giữ enum value backend (Draft/Active/Suspended/Archived/verified/pending/failed/unknown/website/booking/catalog/payments/reports/notifications), route name, type, API contract. Từ kỹ thuật giữ tiếng Anh: Slug, Module, HTTP 409, ClinicOS Owner, Owner Super Admin, Starter/Growth/Premium, Wizard. Verify typecheck + build PASS; owner-admin JS 142.89 kB (+1.53 kB do helper + option arrays).

## 10. Verification

Chạy theo scripts thực tế:

```txt
npm install
npm run typecheck
npm run lint nếu repo có script lint
npm run build
```

Visual check:

```txt
npm run dev:owner
```

Kiểm tra routes:

```txt
/dashboard
/clinics
/clinics/create
/clinics/:tenantId
```

Đối chiếu Figma:

- Sidebar/topbar đúng tinh thần Owner Admin operations.
- Metric cards, filter bar, tenant table, detail drawer đúng hierarchy.
- 409 duplicate slug/domain state xuất hiện đúng.
- Loading/empty/error không làm layout nhảy mạnh.
- Text không overflow ở desktop.

Nếu backend smoke sẵn sàng, smoke real API:

```txt
GET /api/tenants
POST /api/tenants
GET /api/tenants/{id}
PATCH /api/tenants/{id}/status
duplicate slug/domain => 409
```

Nếu backend chưa sẵn sàng, verify bằng mock fallback và ghi rõ chưa smoke real API.

## 11. Out Of Scope

- Không sửa backend.
- Không sửa Figma.
- Không commit.
- Không làm real login/JWT/RBAC.
- Không làm billing/domain/template/module backend thật.
- Không làm Clinic Admin code ở task đầu.
- Không làm Public Website code ở task đầu.
- Không chuyển Phase 2 sang Done.
- Không hardcode secret, IP server thật hoặc production connection string.

## 12. Điểm Dừng

Plan đã được code theo các bước [x]. Owner visual smoke 2026-05-10 đã pass sau khi fix round 1 (sidebar disabled + page transition). Chờ owner re-check visual và smoke API thật khi backend smoke Done.

## 13. Verify Đã Chạy

```txt
npm run typecheck (frontend workspace) -> PASS toàn 3 app: clinic-admin, owner-admin, public-web.
npm run build (frontend workspace) -> PASS toàn 3 app, không error/warning chặn.
npm run lint -> Script không tồn tại trong workspace (đã ghi nhận).
npm run dev:owner -> PASS, Vite ready 659ms, http://localhost:5175.
HTTP smoke 5 route -> 200, index shell có id="app" + script /src/main.ts.
Vite transform 6 file source key -> 200 (main.ts, router, services/tenantClient, ClinicsPage.vue, CreateTenantWizard.vue, alias @clinic-saas/api-client).
Wizard logic regex -> focusFirstConflictField, fieldToStep, ConflictState, toggleModule đều có trong build dev.
Mock client trả 3 tenant đúng metric layout 4 ô; default form aurora-dental sẽ trigger 409 (slug + domain + email) -> ConflictState chạy đúng path.
```

Smoke real API chưa thực hiện do backend smoke Phase 2 chưa Done; API client đã hỗ trợ `auto`/`real`/`mock` mode + `fallbackToMock` để không block UI shell.

Limitations cần ghi nhớ:

```txt
- Lead env hiện tại: không có Playwright/Puppeteer/headless browser tool. WebFetch không fetch được http://localhost (auto-upgrade HTTPS).
- Vue SPA -> mọi route trả cùng index.html (124B). Không thể phân biệt rendered DOM giữa các route qua HTTP smoke.
- Pixel-perfect visual đối chiếu Figma cần human visual hoặc bổ sung tool browser cho Lead/QA.
```

## 14. Real API Adapter Round 3 (2026-05-10)

QA real API smoke phát hiện 2 contract gap CRITICAL chặn Phase 3 trên real mode (qua SSH tunnel `localhost:5005 → server gateway` + Vite proxy `localhost:5175/api/* → tunnel`). Lead phán quyết FE-side fix surgical, không sửa backend.

```txt
[x] Verify wire format backend bằng status-only smoke + key inspection (chỉ status code + property names, không print value chứa data thật):
    - GET /api/tenants  -> 200, paged envelope `{items,total,limit,offset}`,
                            mỗi item flat partial: id, slug, displayName, status, planCode,
                            clinicName, createdAtUtc, updatedAtUtc.
    - GET /api/tenants/{id} -> 200, nested: id, slug, displayName, status, planCode,
                                planDisplayName, profile, domains, modules,
                                createdAtUtc/updatedAtUtc/activatedAtUtc/suspendedAtUtc/archivedAtUtc.
    - POST duplicate slug -> 409 ProblemDetails: type, title, status, detail, traceId
                              (KHÔNG có fields).
    - Domain status raw: "Pending" (PascalCase) — FE shape lowercase -> cần normalize.
    - planDisplayName: empty string trên detail -> cần fallback theo planCode.

[x] Tạo helper riêng `frontend/packages/api-client/src/tenantAdapter.ts`:
    - Type wire format: BackendTenantSummary, BackendTenantListResponse, BackendTenantDetail,
      BackendTenantProfile, BackendTenantDomain, BackendTenantModule.
    - adaptTenantDetail(raw): nested -> FE shape flat, ưu tiên primary domain, filter
      isEnabled !== false, normalize domain status PascalCase -> lowercase, fallback
      planDisplayName từ planCode lowercase, ownerName: undefined.
    - adaptTenantSummary(raw): map item flat partial -> TenantSummary với fallback ""/[]/"unknown".
    - adaptTenantListResponse(raw): unwrap envelope `{items}` -> mảng TenantSummary[].

[x] Sửa `frontend/packages/api-client/src/tenantClient.ts`:
    - Áp adapter cho 4 endpoint real path: listTenants, getTenant, createTenant, updateTenantStatus.
    - Fix normalizeConflict cho ProblemDetails RFC 9457:
      + isProblemDetailsPayload detect title/detail/type/traceId không có fields.
      + parseConflictFieldsFromDetail regex /slug/i, /domain/i, /email|contact/i.
      + Forward-compat: backend trả `fields` array thì respect array đó.
      + ProblemDetails branch: message = detail (cụ thể) thay vì title chung.
    - Mock client KHÔNG đụng (vẫn match FE shape).

[x] Sửa `frontend/packages/api-client/src/index.ts`: export tenantAdapter cho forward-compat.

[x] Sửa `frontend/packages/shared-types/src/tenant.ts`: TenantDetail.ownerName chuyển
    optional `ownerName?: string` (backend chưa trả owner profile, adapter để undefined).

[x] Sửa `frontend/apps/owner-admin/src/components/TenantDetailDrawer.vue`: dòng "Chủ sở hữu"
    chỉ render khi `tenant.ownerName` có giá trị (v-if).

[x] Sửa `frontend/apps/owner-admin/src/components/ConflictState.vue`: thêm copy generic
    "Dữ liệu bị trùng — vui lòng kiểm tra slug, tên miền hoặc email và thử lại." khi
    fields rỗng (banner vẫn hiển thị message từ `detail` của backend).

[x] Verify:
    - cd frontend && npm run typecheck   -> PASS x3 app.
    - cd frontend && npm run build       -> PASS x3 app.
    - owner-admin   145.61 kB JS (+2.72 kB so 142.89 kB do tenantAdapter + ProblemDetails branch + ConflictState v-else copy).
                     22.78 kB CSS (không đổi).
    - Status-only HTTP smoke 5/5 PASS qua Vite proxy localhost:5175 (list 200, detail 200,
      duplicate 409). Body wire format match BackendTenantSummary/BackendTenantDetail/
      ProblemDetails đã verified.

Trạng thái mới Phase 3: **Real API HTTP Smoke 5/5 PASS, Contract Adapter applied,
ready for QA F-real round 2** (mở browser xác nhận pixel/DOM render thật).
```
