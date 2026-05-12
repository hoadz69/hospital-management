# Kế Hoạch Frontend - Phase 3 Owner Admin Tenant Slice (Done) + Phase 4 Wave A V3 Foundation Planning

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

## 15. Pre-Phase 4 Hardening - Frontend Issues (2026-05-10)

Trạng thái: 🟡 Plan ready, chờ owner duyệt implement.

Phần này thuộc feature team Pre-Phase 4 Hardening do Lead Agent điều phối theo "Feature Team Execution Workflow". Frontend lane phụ trách 1/5 issue: P1.7 httpClient `JSON.parse` try/catch. Các issue còn lại do Backend lane (`temp/plan.backend.md`) và DevOps lane (`temp/plan.devops.md`) phụ trách.

### 15.1 Agents Tham Gia (Lane Frontend)

- Lead Agent: điều phối, gom report, không tự code.
- Architect Agent: review boundary/risk (low risk, chỉ surface error rõ hơn).
- Frontend Agent: thực hiện thay đổi `frontend/packages/api-client/src/httpClient.ts`.
- QA Agent: tạo verify checklist, chạy `npm run typecheck` + `npm run build`, smoke 1 trường hợp non-JSON nếu có thể (mock fetch trả HTML).
- Documentation Agent: cập nhật dashboard, lane plan này, roadmap nếu Pre-Phase 4 Hardening thêm dòng roadmap.

### 15.2 Issue Trong Lane Này

#### P1.7 httpClient `JSON.parse` try/catch

File hiện tại: `frontend/packages/api-client/src/httpClient.ts` line 38:

```typescript
const text = await response.text();
const payload = text ? JSON.parse(text) : undefined;
```

Vấn đề: nếu backend hoặc reverse proxy trả non-JSON (HTML 502 từ nginx, plain text "Bad Gateway", proxy timeout HTML page), `JSON.parse(text)` throw `SyntaxError`. Lỗi này không phải `HttpError`, không có `status`/`payload` context, làm UI tenantClient không biết phải handle thế nào → tất cả error states (loading/error/conflict) sẽ không bắt được.

Mục tiêu:

- Bọc `JSON.parse(text)` bằng try/catch.
- Nếu parse fail và `response.ok` là true: throw `HttpError(response.status, "Invalid JSON response", text)` (giữ raw text làm payload để debug).
- Nếu parse fail và `response.ok` là false: throw `HttpError(response.status, message, text)` với `message = "Request failed with status N"`, payload là raw text. Không nuốt error.
- Không đổi public API của `createHttpClient`/`HttpError`. Chỉ sửa nội bộ `request<T>`.
- Giữ behavior text rỗng (`text === ""`) → `payload = undefined` như cũ.

### 15.3 File Dự Kiến Sửa

```txt
frontend/packages/api-client/src/httpClient.ts
docs/current-task.frontend.md (lane status)
temp/plan.frontend.md (file này, ghi nhận trạng thái sau implement)
```

Không sửa:

```txt
frontend/packages/api-client/src/tenantClient.ts (chỉ consume HttpError)
frontend/packages/api-client/src/tenantAdapter.ts
frontend/packages/api-client/src/mockTenantClient.ts
frontend/apps/owner-admin/**
frontend/packages/ui/**
backend/**
infrastructure/**
.env*
```

### 15.4 Risk

- Low risk: thay đổi cục bộ trong `request<T>`, không đổi public API.
- Test path: tenantClient hiện đã có ProblemDetails branch, sẽ chạy đúng nếu payload là object có `title/detail/...`. Khi payload là raw string (parse fail), `normalizeConflict` cần graceful fallback. Cần kiểm tra `normalizeConflict` trong tenantClient.ts xem có handle non-object payload không. Nếu không, thêm guard `if (typeof payload !== "object") { ... }`.
- Backward compat: tất cả API call hiện tại đều mong đợi JSON từ backend. Nếu backend trả non-JSON đó là tình huống lỗi thực, chuyển từ `SyntaxError` (uncaught) sang `HttpError` (caught) là cải thiện UX.

### 15.5 Verify Command

```powershell
cd frontend
npm run typecheck
npm run build
```

Kỳ vọng: PASS toàn 3 app, không error/warning chặn.

QA Agent có thể smoke 1 trường hợp non-JSON bằng mock fetch tạm trong dev console:

```typescript
// Trong dev tool browser, chạy 1 lần để verify
const original = window.fetch;
window.fetch = async () => new Response("<html>502 Bad Gateway</html>", { status: 502, statusText: "Bad Gateway" });
// Click load tenant list trong UI -> expect ConflictState/ErrorState hiển thị, không white screen
window.fetch = original;
```

Nếu không có browser tool, để dành smoke khi Phase 3 real API rớt thật.

### 15.6 Commit Split Đề Xuất

```txt
fix(frontend): guard JSON.parse in httpClient (P1.7)
```

Không gom với commit Backend/DevOps của Pre-Phase 4 Hardening.

### 15.7 Out Of Scope

- Không thêm retry/backoff/circuit breaker (Phase 4 reliability).
- Không thay fetch sang Axios.
- Không refactor tenantClient/tenantAdapter ngoài 1 guard `typeof !== "object"` nếu cần.
- Không sửa backend.
- Không tạo Figma file.
- Không commit/push.

### 15.8 Điểm Dừng

Plan ready. Frontend Agent chỉ implement khi owner đã duyệt rõ. Sau implement, QA Agent verify, Documentation Agent cập nhật dashboard/lane. Lead Agent gom report và đề xuất commit split.

### 15.9 Implementation Result (2026-05-10)

Trạng thái: ✅ **IMPLEMENTATION DONE** — P1.7 đã sửa, `npm run typecheck` + `npm run build` PASS x3 app.

File đã sửa:

```txt
frontend/packages/api-client/src/httpClient.ts (P1.7: bọc JSON.parse trong try/catch + parseFailed flag; nếu parse fail và response.ok=true throw HttpError với message = 200 ký tự đầu của raw text trim, payload = raw text; nếu parse fail và response.ok=false vẫn throw HttpError như branch cũ với payload = raw text)
```

`tenantClient.ts` không phải sửa: `normalizeConflict` đã có guard `typeof payload === "object"` cho cả 2 branch (forward-compat + ProblemDetails RFC 9457), branch fallback cuối dùng `error.message` từ HttpError nên non-object payload (raw HTML/text) sẽ rơi vào fallback an toàn.

Verify đã chạy:

```powershell
cd frontend
npm run typecheck   # PASS x3 app
npm run build       # PASS x3 app
```

Kết quả:

```txt
clinic-admin:  62.23 kB JS,  0.83 kB CSS
owner-admin:  145.73 kB JS, 22.78 kB CSS (+0.12 kB so 145.61 kB trước, do parseFailed flag + message trim logic)
public-web:    62.56 kB JS,  1.42 kB CSS
```

Smoke non-JSON response chưa chạy (cần browser/headless tool). Logic an toàn theo manual review:

- Body rỗng (`text === ""`) → `payload = undefined`, `parseFailed = false`, không throw thêm. Behavior cũ giữ nguyên.
- Body JSON valid + ok → `payload = parsed object`, `parseFailed = false`, return.
- Body JSON valid + 4xx/5xx → throw HttpError như cũ (branch `!response.ok`).
- Body non-JSON + ok 2xx → `parseFailed = true`, `payload = text`; throw HttpError với status `response.status`, message = 200 ký tự đầu raw text trim. UI thấy ConflictState/ErrorState với context debug.
- Body non-JSON + 4xx/5xx → `parseFailed = true`, `payload = text`; rơi vào branch `!response.ok` đầu tiên, message generic "Request failed with status N". Vẫn throw HttpError đúng pattern.

Commit Split (Step 9):

```txt
fix(frontend): guard JSON.parse in httpClient (P1.7)
```

## 16. Phase 4 Wave A V3 Foundation Plan

Trạng thái: 🟡 **Planning — chờ owner duyệt plan**.

Wave A là wave đầu tiên Phase 4+ theo UI Redesign V3 (page Figma `65:2` 76 frame source of truth). Tham chiếu chi tiết V3 + 5 Wave plan + 5 owner decision risk: `docs/roadmap/clinic-saas-roadmap.md#71-ui-redesign-v3-source-of-truth-phase-4`.

### 16.1 Mục Tiêu Wave A

```txt
- Token V3 ADD-ONLY layer trong design-tokens/v3/* + CSS custom property.
- httpClient factory rebuild (3 surface: owner / clinic / public),
  bỏ hardcode X-Owner-Role.
- Composable foundation: useTenantContext, useReducedMotion,
  useFocusTrap, useViewTransition.
- Owner Admin Phase 3 component restyle V3 (TenantTable, TenantDetailDrawer,
  CreateTenantWizard, ConflictState, MetricCard, StatusPill update token).
- 7 component shared mới: KPITile (sparkline), ModuleChips (x/12),
  PlanBadge, EmptyState, TenantSwitcher, CommandPalette, DomainStateRow.
- 4 frame Owner Admin V3v2 mới: Dashboard cross-tenant (124:2),
  Tenant Lifecycle Confirm Modal (124:292), Domain DNS Retry (125:2),
  SSL Pending (125:122).
- Histoire Vue 3 setup frontend/packages/ui/.histoire/ cho component visual review.
```

### 16.2 Frame Figma Wave A

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

### 16.3 Files Dự Kiến Tạo/Sửa

```txt
frontend/packages/design-tokens/src/v3/color.ts            (NEW)
frontend/packages/design-tokens/src/v3/typography.ts       (NEW)
frontend/packages/design-tokens/src/v3/spacing.ts          (NEW)
frontend/packages/design-tokens/src/v3/radius.ts           (NEW)
frontend/packages/design-tokens/src/v3/shadow.ts           (NEW)
frontend/packages/design-tokens/src/v3/motion.ts           (NEW)
frontend/packages/design-tokens/src/v3/v3.css              (NEW)
frontend/packages/design-tokens/src/index.ts               (MODIFIED — export v3, giữ v2)

frontend/packages/api-client/src/httpClient.ts             (MODIFIED — factory pattern)
frontend/packages/api-client/src/owner.ts                  (NEW)
frontend/packages/api-client/src/clinic.ts                 (NEW)
frontend/packages/api-client/src/public.ts                 (NEW)
frontend/packages/api-client/src/index.ts                  (MODIFIED — export 3 surface)

frontend/packages/ui/src/composables/useTenantContext.ts   (NEW)
frontend/packages/ui/src/composables/useReducedMotion.ts   (NEW)
frontend/packages/ui/src/composables/useFocusTrap.ts       (NEW)
frontend/packages/ui/src/composables/useViewTransition.ts  (NEW)

frontend/packages/ui/src/components/KPITile.vue            (NEW)
frontend/packages/ui/src/components/ModuleChips.vue        (NEW)
frontend/packages/ui/src/components/PlanBadge.vue          (NEW)
frontend/packages/ui/src/components/EmptyState.vue         (NEW)
frontend/packages/ui/src/components/TenantSwitcher.vue     (NEW)
frontend/packages/ui/src/components/CommandPalette.vue     (NEW)
frontend/packages/ui/src/components/DomainStateRow.vue     (NEW)

frontend/packages/ui/.histoire/                            (NEW — config)

frontend/apps/owner-admin/src/components/TenantTable.vue           (token V3)
frontend/apps/owner-admin/src/components/TenantDetailDrawer.vue    (token V3)
frontend/apps/owner-admin/src/components/CreateTenantWizard.vue    (token V3)
frontend/apps/owner-admin/src/components/ConflictState.vue         (token V3)
frontend/apps/owner-admin/src/pages/CrossTenantDashboardPage.vue   (NEW — frame 124:2)
frontend/apps/owner-admin/src/components/TenantLifecycleConfirmModal.vue (NEW — frame 124:292)
frontend/apps/owner-admin/src/components/DomainDnsRetryState.vue   (NEW — frame 125:2)
frontend/apps/owner-admin/src/components/SslPendingState.vue       (NEW — frame 125:122)
frontend/apps/owner-admin/src/router/index.ts                      (MODIFIED — thêm route cross-tenant dashboard)
```

### 16.4 Success Criteria

```txt
[ ] Token V3 ADD-ONLY layer build pass; CSS custom property load đúng tại entry app.
[ ] httpClient factory tách 3 surface (owner/clinic/public);
    không còn hardcode X-Owner-Role; tenant + role do consumer inject.
[ ] 4 composable foundation typecheck pass; có sample usage trong owner-admin.
[ ] 7 component shared mới + Histoire story; visual review pass.
[ ] 6 component Phase 3 Owner Admin restyle V3; visual diff không break layout/UX.
[ ] 4 frame Owner Admin V3v2 mới render được trong owner-admin app:
    - /cross-tenant-dashboard route render đúng KPITile + sparkline mock.
    - TenantLifecycleConfirmModal mở từ TenantDetailDrawer đúng flow.
    - DomainDnsRetry/SslPending state hiển thị đúng khi domain status = pending/failed.
[ ] axe-core CI baseline ≥ 95 pass cho 4 page Owner Admin.
[ ] Lighthouse CI baseline cho 4 page (chưa fail-build).
[ ] VRT screenshot diff 14 component Phase 3 trước/sau token V3 không break.
```

### 16.5 Verify Command

```powershell
cd frontend
npm run typecheck
npm run build
# Lint script SKIP nếu workspace chưa khai báo (như Phase 3).
npm run dev:owner   # Smoke 4 page Owner Admin sau token migration.
```

QA Agent verify:

```txt
- axe-core CI baseline 4 page Owner Admin (/dashboard, /clinics, /clinics/create, /clinics/:tenantId).
- Lighthouse CI baseline cho cùng 4 page + page mới /cross-tenant-dashboard.
- VRT screenshot diff 14 component Phase 3 trước/sau token V3 migration.
- Manual smoke 4 frame V3v2 mới (Dashboard cross-tenant, Lifecycle modal, DNS retry, SSL pending).
```

### 16.6 Rollback Plan

```txt
- Token V3 ADD-ONLY: nếu V3 layer break Phase 3 visual, disable v3.css import
  ở entry → tự động fallback về V2 token. Không cần rollback git.
- httpClient factory: giữ HttpError shape compatible với Phase 3 tenantClient
  (consume HttpError không đổi). Nếu rebuild break Phase 3 real API smoke,
  revert httpClient.ts về commit 7f6366d, giữ owner/clinic/public surface
  như chưa active.
- 4 component frame V3v2 mới: nếu chưa stable, route /cross-tenant-dashboard
  có thể disable trong router meta { hidden: true }; không impact Phase 3 routes.
```

### 16.7 Backend Dependency

```txt
- Phase 4.1 Domain Service: OpenAPI contract (mock OK) — endpoint list/detail/verify dummy.
- Phase 4.2 Template Service: registry + apply mode dialog 3-mode contract (mock OK).
- Phase 4.3 Website CMS Service: settings/sliders/page content contract (mock OK).
```

Wave A cho phép mock-first; backend service Phase 4 không bắt buộc implement thật cho Wave A. Lead Agent gửi request OpenAPI contract qua Backend lane (xem `docs/current-task.backend.md` "Bước Tiếp Theo" + `temp/plan.backend.md` §13).

### 16.8 Effort

```txt
~34 dev-day, 4 tuần với 2 FE.
```

### 16.9 Risk Top 3

```txt
1. CRITICAL  httpClient hardcode X-Owner-Role: OwnerSuperAdmin
             → rebuild factory tách 3 surface, header tenant + role do consumer inject.
2. HIGH      Token V2 hex literal vs V3 CSS var drift
             → ADD-ONLY: giữ V2 layer, V3 CSS custom property mới,
             component restyle dùng var(--color-*).
3. HIGH      Owner Dashboard cross-tenant API mock-first
             → backend Phase 4 chưa có aggregate cross-tenant; Wave A dùng mock
             trong api-client/owner.ts, switch real khi backend ready (Wave E).
```

### 16.10 Hard Rules

```txt
- KHÔNG tạo Figma file mới.
- KHÔNG sửa V1/V2 Figma baseline.
- KHÔNG sửa backend code trong lane này.
- KHÔNG commit/push trừ khi owner yêu cầu.
- KHÔNG đụng .env/secret/IP server thật/connection string thật.
- KHÔNG hard-code role/tenant id trong httpClient sau rebuild.
```

### 16.11 Điểm Dừng

```txt
- Plan ready. Frontend Agent chỉ implement khi owner đã duyệt plan rõ.
- 5 owner decision (xem section 7.1.6 roadmap) phải có quyết định trước khi
  start Wave B/C/D/E. Wave A có thể start ngay sau khi owner duyệt plan +
  nhận xác nhận cho 1 trong 5 decision: "DNS retry tolerance" (block
  useDomainVerifyPoll trong Wave A composable).
- Sau implement, QA verify, Documentation Agent cập nhật dashboard + lane +
  roadmap; Lead Agent gom report, đề xuất commit split theo lane.
```

### 16.12 Commit Split Đề Xuất Wave A

```txt
feat(design-tokens): add v3 token layer + CSS custom property
feat(api-client): rebuild httpClient factory (owner/clinic/public)
feat(ui): add composable foundation (4 composables)
feat(ui): add 7 shared components for Owner Admin V3
feat(owner-admin): restyle Phase 3 components with V3 token
feat(owner-admin): add 4 V3v2 frames (cross-tenant dashboard, lifecycle modal, DNS retry, SSL pending)
chore(ui): scaffold Histoire Vue 3 for component review
```

## 17. Codex Lead + Frontend Planning Pass - Wave A (2026-05-10)

Trạng thái: 🟡 **Plan đã đối chiếu lại với docs + Figma V3, chờ owner duyệt trước khi code**.

### 17.1 Đã Đọc

```txt
AGENTS.md
CLAUDE.md
rules/coding-rules.md
clinic_saas_report.md
architech.txt
docs/agent-playbook.md
docs/current-task.md
docs/current-task.frontend.md
temp/plan.frontend.md
docs/roadmap/clinic-saas-roadmap.md (section 7.1 UI Redesign V3 / Phase 4)
docs/agents/figma-ui-agent.md
docs/architecture/overview.md
docs/architecture/service-boundaries.md
docs/architecture/tenant-isolation.md
frontend/package.json
frontend/packages/design-tokens/src/index.ts
frontend/packages/design-tokens/src/ownerAdminTokens.ts
frontend/packages/api-client/src/httpClient.ts
frontend/packages/api-client/src/tenantClient.ts
frontend/packages/api-client/src/index.ts
frontend/packages/ui/src/index.ts
frontend/apps/owner-admin/src/router/index.ts
```

Repo đang có nhiều file modified sẵn trước planning pass này, gồm `AGENTS.md`, `docs/current-task.frontend.md`, `temp/plan.frontend.md`, `.claude/settings.local.json` và nhiều docs/agent files. Wave A implementation sau này chỉ được chạm file frontend lane đã duyệt, không stage file ngoài scope.

### 17.2 Frame Figma Đã Inspect

```txt
65:2    Page UI Redesign V3 - 2026-05-10 metadata, xác nhận 76 frame.
104:2   V3 Overview & TOC.
66:2    V3 Design Tokens Reference.
85:2    Owner Admin Tenant Operations.
87:2    Tenant Detail Drawer.
88:2    Create Tenant Wizard.
88:112  Empty State.
88:119  Conflict slug/domain.
88:127  Command Palette.
124:2   Dashboard Cross-Tenant.
124:292 Tenant Lifecycle Confirm Modal.
125:2   Domain DNS Retry State.
125:122 SSL Pending State.
127:2   Component Inventory.
127:410 Layout Grid + Responsive Rules.
127:518 A11y WCAG 2.2 Poster.
```

Kết luận inspect: tất cả frame owner/system Wave A owner yêu cầu đều đọc được bằng Figma MCP. Không sửa Figma.

### 17.3 Step Wave A Nên Làm

```txt
Step A0 - Approval gate:
  Chỉ bắt đầu khi owner duyệt plan rõ. Không code trước gate này.

Step A1 - Token V3 foundation:
  Tạo layer add-only `frontend/packages/design-tokens/src/v3/*` theo frame 66:2.
  Export token object + CSS custom properties. Không xóa V2/ownerAdminTokens cũ.
  Component code dùng CSS var/token name, không hardcode hex rải rác.

Step A2 - Token adoption entry:
  Import V3 CSS ở owner-admin entry/layout sau khi token compile ổn.
  Giữ fallback V2 để rollback nhanh nếu visual Phase 3 vỡ.

Step A3 - API client factory:
  Rebuild `createHttpClient` theo factory/options pattern.
  Bỏ hardcode `"X-Owner-Role": "OwnerSuperAdmin"`.
  Tách surface `owner / clinic / public`; tenant/role/header do consumer inject.
  Giữ `HttpError` compatible với tenantClient hiện tại.

Step A4 - Shared composables foundation:
  Tạo `useReducedMotion`, `useViewTransition`, `useFocusTrap`, `useTenantContext`.
  `useDomainVerifyPoll` chỉ implement khi owner chốt DNS retry tolerance/max attempts;
  nếu chưa chốt thì Wave A chỉ làm manual retry UI + cleanup-safe hook skeleton nếu cần.

Step A5 - Shared UI components:
  Tạo component V3 phục vụ Owner Admin trước: KPITile, ModuleChips, PlanBadge,
  EmptyState, TenantSwitcher, CommandPalette, DomainStateRow.
  Tái dùng AppButton/AppCard/StatusPill/MetricCard nếu sửa được mà không phá Phase 3.

Step A6 - Owner Admin V3 restyle:
  Restyle sidebar/topbar/table/drawer/wizard/conflict/empty theo 85:2, 87:2, 88:*.
  `/dashboard` nên trở thành route canonical cho frame 124:2 Dashboard Cross-Tenant.
  Chỉ tạo route phụ `/cross-tenant-dashboard` nếu owner muốn so sánh side-by-side.

Step A7 - Owner Admin V3 state surfaces:
  Thêm/hoàn thiện TenantLifecycleConfirmModal, Domain DNS Retry State, SSL Pending State.
  Dữ liệu mock-first trong owner surface, không sửa backend.

Step A8 - Component review tooling:
  Histoire chỉ thêm nếu owner duyệt thêm dependency/package-lock change.
  Nếu chưa duyệt dependency, dùng owner-admin route/story page tạm để visual review.

Step A9 - QA + docs:
  Typecheck/build, route smoke, desktop/tablet/mobile visual smoke, a11y keyboard pass.
  Sau implement mới cập nhật current-task lane/roadmap trạng thái thật.
```

### 17.4 File Dự Kiến Sửa/Tạo

```txt
frontend/packages/design-tokens/src/v3/color.ts
frontend/packages/design-tokens/src/v3/typography.ts
frontend/packages/design-tokens/src/v3/spacing.ts
frontend/packages/design-tokens/src/v3/radius.ts
frontend/packages/design-tokens/src/v3/shadow.ts
frontend/packages/design-tokens/src/v3/motion.ts
frontend/packages/design-tokens/src/v3/v3.css
frontend/packages/design-tokens/src/index.ts

frontend/packages/api-client/src/httpClient.ts
frontend/packages/api-client/src/owner.ts
frontend/packages/api-client/src/clinic.ts
frontend/packages/api-client/src/public.ts
frontend/packages/api-client/src/index.ts

frontend/packages/ui/src/composables/useTenantContext.ts
frontend/packages/ui/src/composables/useReducedMotion.ts
frontend/packages/ui/src/composables/useFocusTrap.ts
frontend/packages/ui/src/composables/useViewTransition.ts
frontend/packages/ui/src/components/KPITile.vue
frontend/packages/ui/src/components/ModuleChips.vue
frontend/packages/ui/src/components/PlanBadge.vue
frontend/packages/ui/src/components/EmptyState.vue
frontend/packages/ui/src/components/TenantSwitcher.vue
frontend/packages/ui/src/components/CommandPalette.vue
frontend/packages/ui/src/components/DomainStateRow.vue
frontend/packages/ui/src/index.ts

frontend/apps/owner-admin/src/main.ts
frontend/apps/owner-admin/src/router/index.ts
frontend/apps/owner-admin/src/layouts/OwnerAdminLayout.vue
frontend/apps/owner-admin/src/components/AdminSidebar.vue
frontend/apps/owner-admin/src/components/AdminTopbar.vue
frontend/apps/owner-admin/src/components/TenantTable.vue
frontend/apps/owner-admin/src/components/TenantDetailDrawer.vue
frontend/apps/owner-admin/src/components/CreateTenantWizard.vue
frontend/apps/owner-admin/src/components/ConflictState.vue
frontend/apps/owner-admin/src/pages/DashboardPage.vue
frontend/apps/owner-admin/src/components/TenantLifecycleConfirmModal.vue
frontend/apps/owner-admin/src/components/DomainDnsRetryState.vue
frontend/apps/owner-admin/src/components/SslPendingState.vue

frontend/package.json và frontend/package-lock.json chỉ sửa nếu owner duyệt Histoire/axe/lighthouse dependency.
docs/current-task.frontend.md và temp/plan.frontend.md chỉ cập nhật trạng thái/handoff.
```

Không sửa: `backend/`, `infrastructure/`, `.env*`, `.claude/settings.local.json`, Figma, Public Website, Booking, Clinic Admin Builder.

### 17.5 Verify Command

```powershell
cd frontend
npm run typecheck
npm run build
npm run dev:owner
```

Smoke route sau khi dev server chạy:

```txt
/dashboard
/clinics
/clinics/create
/clinics/:tenantId
```

Nếu thêm route phụ hoặc state page:

```txt
/cross-tenant-dashboard
DNS retry state entry
SSL pending state entry
```

QA bổ sung:

```txt
- Keyboard nav: sidebar, table row, drawer, modal, command palette.
- Reduced motion: drawer/modal/page transition không gây motion mạnh.
- Responsive: 375, 414, 768, 1280, 1440+ theo frame 127:410.
- A11y: focus-visible, aria-label/aria-sort, modal focus trap, tap target >= 24px, primary mobile target >= 44px.
- VRT/manual visual diff 14 component Phase 3 trước/sau token V3.
- axe-core/Lighthouse baseline chỉ chạy nếu tooling dependency được owner duyệt.
```

### 17.6 Risk / Blocker

```txt
CRITICAL - httpClient hardcode OwnerSuperAdmin:
  Bắt buộc xử lý trong A3, không để shared client tự gắn role cố định.

HIGH - Token drift:
  V3 phải add-only, có fallback V2, và component code không dùng hex literal rải rác.

HIGH - Route /dashboard:
  Phase 3 dashboard cũ không có Figma riêng; V3 đã có frame 124:2.
  Đề xuất dùng /dashboard làm canonical để tránh route phụ không cần thiết.

MEDIUM - DNS retry tolerance:
  Owner cần chốt retry max attempts/interval/rate limit trước khi implement polling thật.
  Nếu chưa chốt, chỉ làm UI state + manual retry mock-first.

MEDIUM - Histoire/axe/Lighthouse:
  Hiện frontend package chưa có các dependency này. Cần owner duyệt nếu thêm package-lock change.

MEDIUM - Backend contract:
  Wave A mock-first được, nhưng Domain/Template/CMS OpenAPI contract vẫn cần backend lane giao trước Wave B/D/E.
```

### 17.7 Commit Split Đề Xuất

```txt
feat(design-tokens): add v3 token layer
feat(api-client): split http client surfaces
feat(ui): add v3 composables and owner admin components
feat(owner-admin): restyle tenant operations with v3 tokens
feat(owner-admin): add v3 dashboard and domain lifecycle states
chore(ui): add component review tooling
docs(frontend): update wave a handoff
```

Không commit/push nếu owner chưa yêu cầu rõ.

### 17.8 Điểm Dừng

```txt
Dừng tại đây để owner duyệt plan Wave A.
Chưa code, chưa sửa Figma, chưa chạy build/test vì chưa có implementation.
Sau khi owner nói rõ "duyệt plan / bắt đầu implement", Frontend Agent mới bắt đầu Step A1.
```

## 18. Wave A Step A4 Shared Composables Foundation (2026-05-11)

Trạng thái: ✅ **DONE** — commit `b87dad4` (`feat(ui): add shared composable foundation`).

Verify:

```txt
cd frontend && npm run typecheck -> PASS
cd frontend && npm run build     -> PASS
```

Step A4 chỉ làm composable foundation trong `frontend/packages/ui`, không làm shared components, không restyle UI, không sửa backend/Figma.

### 18.1 Composable Hiện Trạng

```txt
frontend/packages/ui/src hiện chỉ có:
- components/
- index.ts

Chưa có:
- frontend/packages/ui/src/composables/
- useReducedMotion
- useViewTransition
- useFocusTrap
- useTenantContext
```

Ghi chú liên quan:

```txt
- OwnerAdminLayout.vue hiện có CSS prefers-reduced-motion riêng cho drawer/page transition.
- frontend/packages/api-client/src/clinic.ts đã có khái niệm ClinicTenantContext ở API client layer.
- Owner Admin hiện inject owner role tại app service tenantClient.ts; Step A4 không đổi API client.
```

### 18.2 Scope A4

```txt
IN:
- Tạo shared composable foundation:
  1. useReducedMotion
  2. useViewTransition
  3. useFocusTrap
  4. useTenantContext
- Export composable qua ui package index hoặc composables barrel.
- Có thể thêm usage rất nhỏ trong owner-admin nếu cần chứng minh typecheck/sample usage,
  nhưng không đổi visual behavior và không restyle.

OUT:
- Không tạo 7 shared components.
- Không restyle Owner Admin.
- Không sửa backend.
- Không sửa Figma.
- Không thêm Histoire/axe/Lighthouse dependency.
- Không implement useDomainVerifyPoll/polling thật vì DNS retry tolerance chưa chốt.
- Không commit/push.
```

### 18.3 File Dự Kiến Tạo/Sửa

```txt
frontend/packages/ui/src/composables/useReducedMotion.ts   (NEW)
frontend/packages/ui/src/composables/useViewTransition.ts  (NEW)
frontend/packages/ui/src/composables/useFocusTrap.ts       (NEW)
frontend/packages/ui/src/composables/useTenantContext.ts   (NEW)
frontend/packages/ui/src/composables/index.ts              (NEW)
frontend/packages/ui/src/index.ts                          (MODIFIED - export composables)
```

Optional nếu cần sample usage nhỏ, chỉ sau khi owner duyệt:

```txt
frontend/apps/owner-admin/src/layouts/OwnerAdminLayout.vue
frontend/apps/owner-admin/src/components/OwnerCommandPalette.vue
```

### 18.4 API / Interface Đề Xuất

```ts
// useReducedMotion.ts
export type ReducedMotionPreference = {
  prefersReducedMotion: Readonly<Ref<boolean>>;
  isSupported: Readonly<Ref<boolean>>;
};

export function useReducedMotion(): ReducedMotionPreference;
```

```ts
// useViewTransition.ts
export type ViewTransitionOptions = {
  disabled?: boolean | Ref<boolean>;
};

export type ViewTransitionControls = {
  isRunning: Readonly<Ref<boolean>>;
  run<T>(callback: () => T | Promise<T>): Promise<T>;
};

export function useViewTransition(options?: ViewTransitionOptions): ViewTransitionControls;
```

```ts
// useFocusTrap.ts
export type FocusTrapOptions = {
  active?: Ref<boolean> | boolean;
  initialFocus?: Ref<HTMLElement | null> | HTMLElement | null;
  restoreFocus?: boolean;
  escapeDeactivates?: boolean;
  onEscape?: () => void;
};

export type FocusTrapControls = {
  activate(): void;
  deactivate(): void;
  isActive: Readonly<Ref<boolean>>;
};

export function useFocusTrap(
  container: Ref<HTMLElement | null>,
  options?: FocusTrapOptions
): FocusTrapControls;
```

```ts
// useTenantContext.ts
export type TenantContextRole = "owner" | "clinic" | "public";

export type TenantContextState = {
  tenantId: Ref<string | undefined>;
  tenantSlug: Ref<string | undefined>;
  role: Ref<TenantContextRole>;
};

export type TenantContextControls = TenantContextState & {
  setTenantContext(context: Partial<Pick<TenantContextState, "tenantId" | "tenantSlug" | "role">>): void;
  clearTenantContext(): void;
  requireTenantId(): string;
};

export function useTenantContext(initial?: Partial<{
  tenantId: string;
  tenantSlug: string;
  role: TenantContextRole;
}>): TenantContextControls;
```

Interface có thể điều chỉnh khi implement để khớp Vue typing thực tế, nhưng nguyên tắc giữ:

```txt
- Không phụ thuộc app cụ thể.
- Không đọc secret/env trực tiếp.
- Không hardcode tenant id/role.
- Fail rõ khi requireTenantId() thiếu tenant context.
- SSR-safe / test-safe: guard window/document trước khi dùng matchMedia, document.activeElement, View Transition API.
```

### 18.5 Component Sẽ Dùng Sau Này

```txt
useReducedMotion:
- OwnerAdminLayout page/drawer transition.
- TenantLifecycleConfirmModal.
- OwnerCommandPalette.
- DomainDnsRetryState / SslPendingState animation.

useViewTransition:
- OwnerAdminLayout route transition.
- TenantTable row -> TenantDetailDrawer/detail route.
- Dashboard cross-tenant KPI refresh sau này.

useFocusTrap:
- OwnerCommandPalette.
- TenantLifecycleConfirmModal.
- TenantDetailDrawer nếu chuyển sang modal/drawer focus trap đầy đủ.
- CreateTenantWizard confirm/conflict focus flow nếu cần.

useTenantContext:
- Clinic Admin API surface.
- Public Website tenant/domain resolver.
- TenantSwitcher.
- DomainStateRow hoặc future domain verification UI.
```

### 18.6 Verify Command

```powershell
cd frontend
npm run typecheck
npm run build
npm run dev:owner
```

Smoke tối thiểu sau khi dev server chạy:

```txt
/dashboard
/clinics
/clinics/create
/clinics/:tenantId với tenant id thật từ list
```

Manual checks:

```txt
- prefers-reduced-motion không gây lỗi khi browser không hỗ trợ matchMedia.
- View Transition API không làm crash Chromium/Firefox khi document.startViewTransition không tồn tại.
- Focus trap không giữ focus sau khi modal/palette unmount.
- Tenant context thiếu tenant id thì requireTenantId() throw lỗi rõ, không fallback hardcoded.
```

### 18.7 Risk

```txt
LOW/MEDIUM - useFocusTrap có thể can thiệp keyboard nav nếu activate/deactivate không cleanup đúng.
Mitigation: add/remove keydown listener theo lifecycle, restore focus optional, guard HTMLElement null.

MEDIUM - useViewTransition browser support không đồng nhất.
Mitigation: progressive enhancement, fallback chạy callback trực tiếp.

MEDIUM - useTenantContext dễ bị hiểu nhầm là auth/permission source.
Mitigation: chỉ là UI context helper; không thay thế backend tenant isolation hay token claims.

LOW - useReducedMotion duplicate với CSS hiện có.
Mitigation: dùng composable cho JS decision, giữ CSS media query làm baseline.
```

### 18.8 Điểm Dừng

```txt
A4 đã hoàn tất và đã commit riêng b87dad4.
Chưa sửa backend/Figma.
Chưa push.
Không stage .claude/settings.local.json.
```

## 19. Wave A Step A5 - Shared UI Components Foundation (2026-05-11)

Trạng thái: 🟡 **A5.1 đã implement + verify PASS; A5.2 còn pending**.

Lead Agent đã điều phối Frontend Agent, Figma UI Agent, Architect Agent và QA Agent theo workflow owner yêu cầu. Step A5 được tách nhỏ để giảm risk: A5.1 chỉ thêm 5 display component shared UI trong `packages/ui`; A5.2 còn lại cho `CommandPalette`, `TenantSwitcher` và adoption vào Owner Admin wrapper nếu owner duyệt tiếp.

### 19.1 Resume / Git Check

```txt
git status --branch --short -> ## main...origin/main
git log -12 --oneline ->
  9489124 docs(frontend): mark wave a composables complete
  b87dad4 feat(ui): add shared composable foundation
  025fa8f feat(api-client): split http client surfaces
  f85ac23 style(owner-admin): polish v3 tenant operations smoke fixes
  d91d19f style(owner-admin): restyle tenant operations with v3 tokens
  6876576 docs: add crash recovery checkpoint protocol
  9eb0487 feat(design-tokens): add v3 token foundation
  67beeab docs: sync UI Redesign V3 handoff for Phase 4
  98d2513 docs: sync pre-phase 4 hardening status
  6d65dcb chore(database): pre-phase 4 hardening
  ee4ce03 chore(devops): pre-phase 4 hardening
  ee0c773 fix(frontend): pre-phase 4 hardening
```

Kết luận: Wave A progress đã push/commit đủ các mốc A1-A4 liên quan:

```txt
- Token V3 foundation: commit 9eb0487.
- Owner Admin V3 restyle: commit d91d19f.
- Owner Admin polish smoke fixes: commit f85ac23.
- httpClient surface split: commit 025fa8f.
- Shared composables foundation: commit b87dad4 + handoff 9489124.
```

### 19.2 Source Đã Đọc / Inspect

Docs/lane:

```txt
AGENTS.md
docs/session-continuity.md
docs/current-task.md
docs/current-task.frontend.md
temp/plan.frontend.md
docs/roadmap/clinic-saas-roadmap.md section 7.1 UI Redesign V3 / Phase 4
docs/agent-playbook.md
docs/agents/lead-agent.md
docs/agents/figma-ui-agent.md
```

Code:

```txt
frontend/packages/ui/src/components
frontend/packages/ui/src/index.ts
frontend/apps/owner-admin/src/components
frontend/apps/owner-admin/src/pages
frontend/packages/design-tokens/src/v3
frontend/packages/ui/src/composables
frontend/package.json
```

Figma MCP read-only inspect đã đọc được:

```txt
66:2   Design Tokens Reference
85:2   Owner Admin Tenant Operations
87:2   Tenant Detail Drawer
88:112 Empty State
88:127 Command Palette
127:2  Component Inventory
```

Không sửa Figma. Các frame `124:*` / `125:*` thuộc A6/A7 state surfaces, không đưa vào implement A5 trừ khi owner mở scope sau.

### 19.3 Hiện Trạng Component

`frontend/packages/ui` hiện có:

```txt
AppButton
AppCard
MetricCard
StatusPill
useReducedMotion
useViewTransition
useFocusTrap
useTenantContext
```

Sau A5.1, component A5 trong `packages/ui`:

```txt
DONE:
- KPITile
- ModuleChips
- PlanBadge
- EmptyState
- DomainStateRow

PENDING A5.2:
- TenantSwitcher
- CommandPalette
```

Component tương đương / nguồn extract:

| Component A5 | Tương đương hiện có | Quyết định |
|---|---|---|
| `KPITile` | `MetricCard` trong `packages/ui`, đang dùng ở `DashboardPage` và `ClinicsPage` | Tạo mới trong `packages/ui` nếu cần sparkline/trend đúng V3; không thay `MetricCard` ngay. |
| `ModuleChips` | `module-meter` trong `TenantTable`, `module-list` trong `TenantDetailDrawer`/`ClinicDetailPage`, chip chọn module trong wizard | Tạo mới trong `packages/ui`, extract phần display module. Props generic, không import tenant type. |
| `PlanBadge` | `StatusPill` + `planTone()` trong `TenantTable`, plan option cards trong wizard | Tạo mới trong `packages/ui` nếu dùng visual V3 riêng cho plan; props nhận `label`/`tone`. |
| `EmptyState` | Inline `.empty-state` ở `ClinicsPage`, `.empty` ở `DashboardPage`, `state-cell` ở `TenantTable` | Tạo mới trong `packages/ui`, dùng props/slot/action. |
| `TenantSwitcher` | Chưa có component tương đương; có `useTenantContext` foundation | **Không đưa shared behavior tenant thật vào A5**. Nếu vẫn phải có tên A5, tạo presentational controlled component trong `packages/ui`, không route/API/storage. Architect khuyến nghị tên generic hơn về sau. |
| `CommandPalette` | `OwnerCommandPalette.vue` app-local | Extract shared base vào `packages/ui`; app giữ wrapper/data/route/action. |
| `DomainStateRow` | `domainCards` + `.domain-card` trong `TenantDetailDrawer`, domain list trong `ClinicDetailPage` | Tạo mới trong `packages/ui`; chỉ render status/helper/action và emit event, không polling. |

### 19.4 Boundary / Behavior Rule

Step A5 là **presentational shared UI foundation**:

```txt
- Không đổi API client, route, store, tenant lifecycle, DNS retry/polling, auth/RBAC.
- Không import tenantClient, vue-router, Pinia, @clinic-saas/api-client trong packages/ui.
- Không import @clinic-saas/shared-types trong packages/ui; app map enum/type -> label/tone trước.
- Không hardcode tenant id/role; không đọc env/localStorage/sessionStorage.
- CommandPalette shared chỉ quản lý overlay/search/list/focus; route action do Owner Admin wrapper truyền.
- DomainStateRow chỉ emit retry/view/copy; không gọi API, không poll DNS.
- TenantSwitcher nếu tạo shared chỉ là controlled UI props/events, không là tenant authority.
```

### 19.5 File Dự Kiến Tạo/Sửa

Shared UI A5.1 đã tạo mới:

```txt
frontend/packages/ui/src/components/KPITile.vue
frontend/packages/ui/src/components/ModuleChips.vue
frontend/packages/ui/src/components/PlanBadge.vue
frontend/packages/ui/src/components/EmptyState.vue
frontend/packages/ui/src/components/DomainStateRow.vue
```

Shared UI sửa:

```txt
frontend/packages/ui/src/index.ts
```

Shared UI còn pending A5.2:

```txt
frontend/packages/ui/src/components/TenantSwitcher.vue
frontend/packages/ui/src/components/CommandPalette.vue
```

Owner Admin chỉ sửa nếu integrate usage ngay trong A5:

```txt
frontend/apps/owner-admin/src/pages/DashboardPage.vue
frontend/apps/owner-admin/src/pages/ClinicsPage.vue
frontend/apps/owner-admin/src/components/TenantTable.vue
frontend/apps/owner-admin/src/components/TenantDetailDrawer.vue
frontend/apps/owner-admin/src/components/OwnerCommandPalette.vue
frontend/apps/owner-admin/src/layouts/OwnerAdminLayout.vue
frontend/apps/owner-admin/src/pages/ClinicDetailPage.vue
```

Không sửa trong A5:

```txt
backend/
frontend/apps/public-web/
frontend/apps/clinic-admin/
frontend/packages/api-client/
frontend/package.json
frontend/package-lock.json
Figma
.env*
.claude/settings.local.json
```

### 19.6 Token V3 Theo Component

Tất cả component A5 dùng CSS variables từ `frontend/packages/design-tokens/src/v3/v3.css`; không thêm hex literal mới.

| Component | Token chính |
|---|---|
| `KPITile` | `--color-surface-elevated`, `--color-border-subtle`, `--color-text-primary`, `--color-text-muted`, `--color-status-*`, `--radius-card`, `--shadow-elevation-1`, `--motion-duration-xs`. |
| `ModuleChips` | `--color-status-success`, `--color-surface-muted`, `--color-text-secondary`, `--radius-input`, `--space-1/2/3`. |
| `PlanBadge` | `--color-brand-primary`, `--color-status-warning`, `--color-status-specialty`, `--color-surface-muted`, `--radius-input` hoặc `--radius-pill`. |
| `EmptyState` | `--color-surface-elevated`, `--color-border-subtle`, `--color-brand-primary`, `--color-text-primary/secondary/muted`, `--radius-card`, `--space-6/10/14`. |
| `TenantSwitcher` | `--color-surface-elevated`, `--color-surface-muted`, `--color-border-subtle`, `--color-text-primary/secondary`, `--radius-input`, `--shadow-elevation-2`. |
| `CommandPalette` | `--color-text-primary` overlay mix, `--color-surface-elevated`, `--color-surface-muted`, `--color-border-subtle`, `--color-brand-primary`, `--radius-card`, `--shadow-elevation-3`, `--motion-duration-md`, `--motion-ease-standard`. |
| `DomainStateRow` | `--color-status-success/info/warning/danger/specialty`, `--color-surface-elevated`, `--color-surface-muted`, `--color-border-subtle`, `--radius-card`, `--radius-pill`. |

### 19.7 Acceptance Criteria

```txt
1. A5.1: `packages/ui/src/index.ts` export đủ 5 display component: `KPITile`, `ModuleChips`, `PlanBadge`, `EmptyState`, `DomainStateRow`.
2. A5.2: `packages/ui/src/index.ts` export thêm `CommandPalette`, `TenantSwitcher` khi owner duyệt scope tiếp.
3. Shared components dùng props/events/slot, không biết route/API/tenant business enum.
4. Owner Admin wrapper có thể dùng shared CommandPalette mà không đổi shortcut/close behavior hiện tại sau A5.2.
5. Domain/module/plan display nhất quán với Figma V3 frame 85:2, 87:2, 88:112, 88:127.
6. Không thêm dependency, không thêm Histoire/axe/Lighthouse trong A5.
7. Không đổi behavior hiện có trên /dashboard, /clinics, /clinics/create, /clinics/:tenantId.
8. Không tạo polling DNS thật; DNS retry tolerance vẫn chờ owner decision.
```

### 19.8 Verify Đã Chạy Cho A5.1

```powershell
cd frontend
npm run typecheck
npm run build
```

Kết quả:

```txt
- npm run typecheck -> PASS cả 3 app: clinic-admin, owner-admin, public-web.
- npm run build -> PASS cả 3 app.
  - clinic-admin JS 62.23 kB.
  - owner-admin JS 150.57 kB, CSS 37.24 kB.
  - public-web JS 62.57 kB.
- git diff --check -> không có whitespace error; chỉ warning CRLF cho `temp/plan.frontend.md`.
```

Static check A5.1:

```powershell
rg "#[0-9a-fA-F]{3,8}|rgb\(|rgba\(" 5 file component A5.1 -> no match.
git show --check --stat HEAD -> no whitespace error.
```

Chưa chạy:

```txt
- npm run dev:owner và manual browser smoke, vì A5.1 chỉ thêm/export shared components, chưa integrate vào route hiện tại.
- Manual QA CommandPalette/TenantSwitcher, vì 2 component này thuộc A5.2 pending.
```

### 19.9 Risk

```txt
MEDIUM - KPITile trùng MetricCard:
  Mitigation: giữ MetricCard, tạo KPITile chỉ cho V3 sparkline/trend; không migrate rộng nếu chưa cần.

MEDIUM - PlanBadge trùng StatusPill:
  Mitigation: PlanBadge chỉ dùng cho plan visual riêng; status vẫn dùng StatusPill.

HIGH - Shared UI bị lẫn nghiệp vụ tenant:
  Mitigation: packages/ui không import shared-types/api-client/router; app map data trước.

MEDIUM - CommandPalette extract làm đổi behavior hiện có:
  Mitigation: OwnerCommandPalette giữ wrapper/data/shortcut; shared base chỉ nhận props/events.

MEDIUM - TenantSwitcher naming dễ hiểu nhầm là tenant authority:
  Mitigation: A5 chỉ presentational controlled component; tenant isolation/auth vẫn thuộc app/API/backend.

MEDIUM - Token drift:
  Mitigation: chỉ dùng CSS var V3; không thêm hex literal mới trong shared components.

LOW/MEDIUM - Không có test runner/a11y automation:
  Mitigation: typecheck/build + manual keyboard/a11y smoke; không thêm dependency khi owner chưa duyệt.
```

### 19.10 Commit / Worktree

A5.1 source code đã nằm trong commit:

```txt
eca5086 feat(ui): add v3 shared display components
```

Worktree sau verify chỉ còn dirty `temp/plan.frontend.md` để cập nhật handoff. Không push.

### 19.11 Điểm Dừng / Owner Approval Gate

```txt
Dừng tại đây sau A5.1.
Đã code 5 display component shared UI, đã verify typecheck/build.
Chưa sửa Figma, chưa sửa backend, chưa thêm dependency, chưa integrate Owner Admin route.
Còn pending A5.2: CommandPalette shared base + TenantSwitcher presentational + optional Owner Admin adoption.
Frontend Agent chỉ làm A5.2 khi owner duyệt tiếp scope A5.2.
```

## 20. Wave A Step A5.1b - Adopt Shared Display Components vào Owner Admin (2026-05-11)

Trạng thái: 🟡 **Plan ready - dừng tại owner approval gate, chưa code**.

Lead Agent điều phối theo workflow owner yêu cầu:
- Frontend Agent: đã review usage hiện tại trong Owner Admin.
- Architect Agent: kiểm tra boundary để adoption không đổi API/router/store/business logic.
- Figma UI Agent: inspect read-only frame V3 `85:2`, `87:2`, `88:2`, `88:112`.
- QA Agent: đề xuất smoke UI/manual visual sau khi owner duyệt implement.
- Documentation Agent: chưa cần checkpoint riêng vì plan chỉ cập nhật `temp/plan.frontend.md`; `docs/current-task.frontend.md` đang dirty từ handoff trước, không đụng trong lượt plan này.

### 20.1 Mục Tiêu

Gắn 5 component A5.1 đã có vào Owner Admin để có UI thật cho manual visual smoke/Figma review:

```txt
KPITile
ModuleChips
PlanBadge
EmptyState
DomainStateRow
```

Phạm vi là adoption presentation-only trong `frontend/apps/owner-admin`. Không đổi contract API, route, store, tenant lifecycle, status update, wizard validation, filter logic hoặc data fetching.

### 20.2 Source Đã Đọc

Shared UI:

```txt
frontend/packages/ui/src/components/KPITile.vue
frontend/packages/ui/src/components/ModuleChips.vue
frontend/packages/ui/src/components/PlanBadge.vue
frontend/packages/ui/src/components/EmptyState.vue
frontend/packages/ui/src/components/DomainStateRow.vue
```

Owner Admin:

```txt
frontend/apps/owner-admin/src/pages/DashboardPage.vue
frontend/apps/owner-admin/src/pages/ClinicsPage.vue
frontend/apps/owner-admin/src/components/TenantTable.vue
frontend/apps/owner-admin/src/components/TenantDetailDrawer.vue
frontend/apps/owner-admin/src/components/CreateTenantWizard.vue
frontend/apps/owner-admin/src/pages/ClinicDetailPage.vue
```

Figma read-only:

```txt
85:2   V3 - Owner Admin Tenant Operations
87:2   V3 - Owner Admin Tenant Detail Drawer
88:2   V3 - Owner Admin Create Tenant Wizard
88:112 V3 - Owner Admin Empty state
```

### 20.3 Component Nên Adopt Trước

Thứ tự khuyến nghị:

1. `KPITile` vào `DashboardPage.vue` và `ClinicsPage.vue`.
   - Lý do: hiện hai page đang dùng `MetricCard` cho KPI strip, map trực tiếp sang props `KPITile` gần frame `85:2`.
   - Risk thấp vì chỉ thay component hiển thị, giữ computed `activeCount`, `verifiedDomains`, `suspendedCount`, `supportCount`.

2. `PlanBadge` và `ModuleChips` vào `TenantTable.vue`.
   - Lý do: frame `85:2` table có `PlanBadge` và module 8-dot indicator; hiện table dùng `StatusPill` cho plan và inline `.module-meter`.
   - Risk trung bình thấp vì chỉ thay cell render, không đổi row click/keyboard/select.

3. `EmptyState` vào `ClinicsPage.vue`.
   - Lý do: inline `.empty-state` hiện khớp frame `88:112`, component shared đã có slot action.
   - Risk thấp nếu giữ nguyên CTA `RouterLink` và điều kiện `isEmpty`.

4. `DomainStateRow` vào `TenantDetailDrawer.vue`.
   - Lý do: frame `87:2` domain cards chính là domain state rows; hiện drawer có `domainCards` computed, helper/action/status sẵn.
   - Risk trung bình vì có disabled action button trong row; phải giữ action disabled/placeholder, không gọi API/poll DNS.

5. `ModuleChips` và `PlanBadge` vào `CreateTenantWizard.vue` preview/summary nhẹ.
   - Lý do: frame `88:2` nhấn mạnh plan/module ở step 2 và summary; hiện wizard dùng inline plan cards, module option button và `StatusPill` trong preview.
   - Risk trung bình vì wizard có validation/focus/conflict; adoption chỉ nên áp dụng vào preview/sidebar hoặc non-interactive module display trước, tránh thay module option button interactive trong lượt đầu.

6. `DomainStateRow`, `ModuleChips`, `PlanBadge` vào `ClinicDetailPage.vue` sau drawer.
   - Lý do: page detail là surface phụ cùng data với drawer.
   - Risk trung bình thấp; nên chỉ thay domain list/module display/plan display, giữ update status và load route nguyên.

### 20.4 File Dự Kiến Sửa

Ưu tiên sửa:

```txt
frontend/apps/owner-admin/src/pages/DashboardPage.vue
frontend/apps/owner-admin/src/pages/ClinicsPage.vue
frontend/apps/owner-admin/src/components/TenantTable.vue
frontend/apps/owner-admin/src/components/TenantDetailDrawer.vue
```

Sửa nếu còn trong timebox và không phát sinh behavior risk:

```txt
frontend/apps/owner-admin/src/components/CreateTenantWizard.vue
frontend/apps/owner-admin/src/pages/ClinicDetailPage.vue
```

Không sửa:

```txt
frontend/packages/ui/src/components/*
frontend/packages/api-client/*
frontend/packages/shared-types/*
frontend/apps/public-web/*
frontend/apps/clinic-admin/*
backend/*
Figma
package.json
package-lock.json
.claude/settings.local.json
```

### 20.5 Mapping Data Hiện Tại Sang Props Component

`KPITile`:

```txt
DashboardPage / ClinicsPage:
- label: text label hiện đang truyền vào MetricCard.
- value: activeCount / verifiedDomains / suspendedCount / supportCount.
- helper hoặc meta: copy meta hiện tại.
- tone:
  activeCount -> success
  verifiedDomains -> primary hoặc info
  suspendedCount -> danger
  supportCount -> warning
- sparkline: optional static demo array nhỏ nếu muốn nhìn giống frame 85:2; chỉ dùng presentation, không derive business logic mới.
```

`PlanBadge`:

```txt
TenantTable:
- label: tenant.planDisplayName.
- tone: reuse `planTone(tenant.planCode)` hiện có.
- helper: optional `Gói ${tenant.planDisplayName}` nếu cần aria-label rõ hơn.

CreateTenantWizard:
- label: form.planCode hoặc display label Starter/Growth/Premium.
- value: optional summary value nếu không thêm pricing data mới.
- tone: map planCode bằng helper local, không đổi setPlan().

ClinicDetailPage / TenantDetailDrawer:
- label: tenant.planDisplayName.
- tone: map từ tenant.planCode nếu field có sẵn ở TenantDetail; nếu chưa chắc type detail có planCode thì giữ text cũ hoặc kiểm tra type trước khi implement.
```

`ModuleChips`:

```txt
TenantTable:
- items: tạo từ tenant.moduleCodes map thành { key: code, label: formatModuleCode(code), enabled: true, tone: "success" }.
- total: moduleTotal hiện tại, đang là 6.
- compact: true để giữ table row gọn như dot indicator.
- showCount: true.

TenantDetailDrawer / ClinicDetailPage:
- items: tenant.moduleCodes map label bằng formatModuleCode.
- total: moduleTotal nếu có constant local; nếu không có, dùng tenant.moduleCodes.length để tránh nói sai số tổng.
- compact: false cho chip label đầy đủ.

CreateTenantWizard:
- preview/side preview: form.moduleCodes map label bằng formatModuleCode.
- total: moduleOptions.length.
- compact: false.
- Không thay button chọn module ở `.module-options` trong lượt đầu để không đổi toggle behavior.
```

`EmptyState`:

```txt
ClinicsPage:
- label: "Chưa có phòng khám nào trong hệ thống."
- helper: copy paragraph hiện tại.
- tone: primary.
- action slot: giữ RouterLink + AppButton "Thêm phòng khám đầu tiên".

DashboardPage:
- Có thể thay `.empty` bằng EmptyState nhỏ, nhưng nên để sau nếu cần vì dashboard empty copy đang nằm trong AppCard recent tenants, không phải frame 88:112 chính.
```

`DomainStateRow`:

```txt
TenantDetailDrawer:
- label: domain.domainName.
- value: formatDomainStatus(domain.status).
- helper: domain.helper từ computed hiện tại.
- tone: domainTone(domain.status), cần đảm bảo return type nằm trong DomainStateTone.
- actions: [{ key: domain.action, label: domain.action, disabled: true, tone: "secondary" }]
- @action: không cần handler hoặc handler no-op vì action disabled.

ClinicDetailPage:
- label: domain.domainName.
- value: formatDomainStatus(domain.status).
- helper: domain.isPrimary ? "Default subdomain" : domain status hint nếu có helper local.
- tone: map domain.status tương tự drawer.
```

### 20.6 Có Đổi Behavior Không

Không đổi behavior nếu implement đúng scope:

```txt
- Không đổi `tenantClient.listTenants/getTenant/updateTenantStatus`.
- Không đổi computed filter, selection, drawerOpen/detailLoading, conflict focus, step validation.
- Không đổi route `/dashboard`, `/clinics`, `/clinics/create`, `/clinics/:tenantId`.
- Không đổi payload submit `TenantCreateRequest`.
- Không đổi status update emit/handler.
- Không thêm DNS retry/polling; DomainStateRow action trong drawer vẫn disabled/placeholder.
- Không thêm dependency và không sửa package/lockfile.
```

Behavior được phép thay đổi chỉ ở presentation:

```txt
- Card/chip/row markup class thay đổi để dùng shared components.
- Một số CSS inline cũ như `.module-meter`, `.empty-state`, `.domain-card` có thể bị xoá nếu không còn dùng.
- Visible appearance sẽ gần Figma V3 hơn, nhưng interaction phải giữ nguyên.
```

### 20.7 Risk Từng File

`DashboardPage.vue`:
- Risk: `KPITile` height/min-height khác `MetricCard`, có thể làm grid cao hơn.
- Mitigation: giữ `.metrics-grid` hiện tại, không đổi layout ngoài import/component.

`ClinicsPage.vue`:
- Risk: thay `MetricCard` và `EmptyState` cùng lúc có thể làm visual diff rộng.
- Mitigation: làm KPI trước, EmptyState sau; giữ điều kiện `isEmpty`, `isFiltered`.

`TenantTable.vue`:
- Risk: `ModuleChips compact` có thể làm cell width/height lệch table `min-width: 1040px`.
- Mitigation: giữ `moduleTotal`, kiểm tra row height 64px, không đổi table columns/click/keydown.

`TenantDetailDrawer.vue`:
- Risk: `DomainStateRow` emit action có thể vô tình tạo click behavior mới.
- Mitigation: action disabled hoặc không truyền handler; không gọi API; giữ `domainCards` computed.

`CreateTenantWizard.vue`:
- Risk: thay interactive module buttons bằng `ModuleChips` có thể phá toggle/validation/focus.
- Mitigation: chỉ adopt `ModuleChips` trong preview/sidebar và review step; không thay `.module-options` trong implement đầu.

`ClinicDetailPage.vue`:
- Risk: detail page hiện thiếu helper/action domain như drawer, nếu tự thêm logic có thể drift.
- Mitigation: dùng mapper đơn giản hoặc reuse local helper tương tự drawer nhưng không gọi API.

### 20.8 Verify Command

Sau khi owner duyệt implement:

```powershell
cd frontend
npm run typecheck
npm run build
npm run dev:owner
```

Static checks:

```powershell
rg "KPITile|ModuleChips|PlanBadge|EmptyState|DomainStateRow" frontend\apps\owner-admin\src
rg "MetricCard|module-meter|empty-state|domain-card" frontend\apps\owner-admin\src\pages frontend\apps\owner-admin\src\components
git diff --check
git status --short
```

Expected:

```txt
- typecheck PASS cả 3 app.
- build PASS cả 3 app.
- Các component A5.1 xuất hiện trong Owner Admin source.
- Không có thay đổi package/lockfile/backend/api-client/shared-types.
```

### 20.9 Khi Nào Run Frontend/Backend Để Test UI

Plan phase:

```txt
- Không run UI visual test trong plan phase theo yêu cầu owner.
- Không cần backend trong plan phase.
```

Implementation phase sau owner approval:

```txt
1. Run `npm run typecheck` và `npm run build` ngay sau khi sửa.
2. Run `npm run dev:owner` sau build PASS để manual visual smoke.
3. Nếu test bằng mock/offline: không cần backend, dùng mock fallback hiện có.
4. Nếu owner muốn smoke real data: chỉ run frontend dev server sau khi backend/gateway/tunnel real API đã sẵn sàng; không tự sửa backend.
5. Manual route smoke:
   - /dashboard: KPI tile render, recent tenant strip không vỡ.
   - /clinics: KPI tile, table PlanBadge/ModuleChips, empty state nếu mock data rỗng.
   - /clinics/create: wizard preview/sidebar module chips không phá next/back/submit/conflict.
   - /clinics/:tenantId: plan/module/domain display render, update status vẫn hoạt động.
   - /clinics row click: drawer mở, DomainStateRow render, close/Escape/status update vẫn giữ.
```

### 20.10 Out Of Scope

```txt
- Không CommandPalette.
- Không TenantSwitcher.
- Không sửa API client.
- Không đổi route/store/business logic.
- Không sửa backend/Figma.
- Không thêm dependency.
- Không commit/push.
- Không run UI visual test trong plan phase.
```

### 20.11 Owner Approval Gate

```txt
Dừng tại đây.
Chưa code, chưa sửa source, chưa sửa Figma, chưa run UI visual test.
Sau khi owner nói rõ "duyệt A5.1b" hoặc "bắt đầu implement A5.1b", Frontend Agent mới adopt shared display components theo plan trên.
```

### 20.12 Implementation Result 2026-05-11

Trạng thái: 🟢 **A5.1b implementation + verify PASS; chưa commit theo yêu cầu owner**.

Ghi chú quan trọng:
- Commit có sẵn `1e01350 feat(owner-admin): adopt v3 shared display components` đã adopt phần ưu tiên vào `DashboardPage.vue`, `ClinicsPage.vue`, `TenantTable.vue`, `TenantDetailDrawer.vue`.
- Lượt implement này hoàn tất phần còn lại trong scope A5.1b:
  - `CreateTenantWizard.vue`: review/sidebar dùng `PlanBadge` + `ModuleChips`; không thay button chọn plan/module nên không đổi toggle/validation/focus.
  - `ClinicDetailPage.vue`: detail page dùng `PlanBadge`, `DomainStateRow`, `ModuleChips`; không đổi load/update status/route/API.

File source sửa trong lượt này:

```txt
frontend/apps/owner-admin/src/components/CreateTenantWizard.vue
frontend/apps/owner-admin/src/pages/ClinicDetailPage.vue
```

Verify:

```txt
cd frontend && npm run typecheck -> PASS cả 3 app.
cd frontend && npm run build     -> PASS cả 3 app.
git diff --check                 -> PASS, chỉ warning LF/CRLF trên Windows.
rg "KPITile|ModuleChips|PlanBadge|EmptyState|DomainStateRow" frontend/apps/owner-admin/src -> PASS, có usage.
rg "MetricCard|module-meter|empty-state|domain-card" frontend/apps/owner-admin/src/pages frontend/apps/owner-admin/src/components -> PASS, no match.
npm run dev:owner -> Vite ready http://localhost:5175/.
HTTP smoke:
- /dashboard -> 200
- /clinics -> 200
- /clinics/create -> 200
- /clinics/mock-tenant-id -> 200
```

Chưa làm:

```txt
- Chưa screenshot/pixel smoke vì workspace hiện không có Playwright/Vitest/browser test dependency.
- Chưa cleanup `temp/owner-admin-vite.log` vì dev server đang chạy cho owner review.
- Chưa commit/push theo yêu cầu owner.
```

## 21. Wave A Step A5.2 - CommandPalette Shared Base + TenantSwitcher Presentational (2026-05-11)

Trạng thái: 🟡 **Plan ready - dừng tại owner approval gate, chưa code**.

Lead Agent điều phối theo feature team frontend/UI:

```txt
Frontend Agent: review OwnerCommandPalette hiện tại, OwnerAdminLayout, AdminTopbar, AdminSidebar.
Architect Agent: kiểm boundary packages/ui không kéo router/API/business tenant authority.
Figma UI Agent: inspect read-only frame V3 88:127, 85:2, 127:2.
QA Agent: đề xuất keyboard/focus/visual smoke sau implement.
Documentation Agent: chưa checkpoint current-task vì đây là plan nhỏ; chỉ cập nhật plan lane.
```

### 21.1 Mục Tiêu

Hoàn tất phần pending của A5:

```txt
1. Tạo `CommandPalette` shared base trong `frontend/packages/ui`.
2. Refactor `OwnerCommandPalette` thành wrapper app-local giữ data/action/navigation/shortcut.
3. Tạo `TenantSwitcher` presentational trong `frontend/packages/ui` nếu owner duyệt A5.2 full scope.
4. Không đổi route/store/API/business logic.
```

### 21.2 Source Đã Đọc

```txt
frontend/apps/owner-admin/src/components/OwnerCommandPalette.vue
frontend/apps/owner-admin/src/layouts/OwnerAdminLayout.vue
frontend/apps/owner-admin/src/components/AdminTopbar.vue
frontend/apps/owner-admin/src/components/AdminSidebar.vue
frontend/packages/ui/src/components/*
frontend/packages/ui/src/composables/useFocusTrap.ts
frontend/packages/ui/src/composables/useReducedMotion.ts
frontend/packages/ui/src/composables/useTenantContext.ts
frontend/packages/ui/src/index.ts
```

Figma read-only:

```txt
88:127  V3 - Owner Admin · Command palette ⌘K
85:2    V3 - Owner Admin Tenant Operations
127:2   V3 - Design System · Component Inventory
```

### 21.3 CommandPalette Hiện Tại Đang Làm Gì

`OwnerCommandPalette.vue` hiện là component app-local nhưng đang gom cả UI shell và app behavior:

```txt
- Nhận prop `open` và emit `close`.
- Teleport overlay vào body.
- Render dialog overlay, panel 640px, search header, recent section, actions section.
- Hardcode recent tenants: `mat-saigon`, `rhm-hadong`.
- Hardcode actions gồm label/hint/to/icon.
- Dùng `RouterLink` trực tiếp để điều hướng.
- Đóng palette khi click backdrop, click item hoặc phím Escape.
- Lock `document.body.style.overflow` khi mở.
- Tự register window keydown Escape.
- Input search chỉ visual, chưa có v-model query/filter thực.
- Chưa có focus trap Tab, chưa restore focus, chưa keyboard arrow navigation.
```

`OwnerAdminLayout.vue` hiện giữ shortcut global `Ctrl/Cmd+K` để mở palette và state `commandPaletteOpen`. Đây là đúng vị trí cho app-level shortcut vì layout biết shell route/topbar.

### 21.4 Phần Nên Extract Sang Shared Base

Tạo `frontend/packages/ui/src/components/CommandPalette.vue` làm presentational controlled component:

```txt
- Overlay/backdrop/panel/search/list/empty/loading UI.
- Input search controlled bằng `query` + emit `update:query`.
- Nhóm item theo section (`recent`, `actions`, `matching` hoặc label truyền vào).
- Visual active row, hover/focus row, keyboard row focus optional.
- Escape emit `close`.
- Backdrop click emit `close`.
- Enter/click item emit `select`.
- Dùng `useFocusTrap` để trap focus trong panel khi open.
- Dùng `useReducedMotion` hoặc CSS prefers-reduced-motion để giảm motion.
- Body scroll lock có thể nằm trong shared base vì overlay là presentational behavior chung, nhưng phải cleanup đúng khi unmount/close.
- Không dùng `RouterLink`, không import `vue-router`, không biết `to`.
- Không biết tenant API, module, plan, status enum.
```

Không extract:

```txt
- Dữ liệu recent tenants/action list.
- Điều hướng route.
- Shortcut Ctrl/Cmd+K ở layout.
- Quyết định action nào enabled/disabled theo product phase.
- Query business filtering phức tạp nếu cần đọc tenant list thật.
```

### 21.5 OwnerCommandPalette Wrapper Giữ Gì

`frontend/apps/owner-admin/src/components/OwnerCommandPalette.vue` nên giữ vai trò adapter:

```txt
- Import `CommandPalette` từ `@clinic-saas/ui`.
- Khai báo item app-local với route/action metadata.
- Giữ copy Owner Admin hiện tại: recent tenants, actions, hints.
- Quản lý `query` local nếu cần.
- Map query -> grouped items, ví dụ Recent/Actions/Tenants matching.
- Khi `CommandPalette` emit `select`, wrapper dùng `useRouter().push(item.to)` hoặc render slot/RouterLink nếu chọn slot strategy.
- Sau select thành công emit `close`.
- Không tự render toàn bộ overlay CSS nữa, chỉ map data và event.
```

Khuyến nghị implementation: dùng event `select` + `router.push()` trong wrapper thay vì slot `RouterLink` cho từng row. Lý do: shared base giữ được keyboard `Enter` chọn item nhất quán, wrapper chỉ xử lý route ở boundary app.

### 21.6 TenantSwitcher Có Nên Implement Ngay Hay Defer

Khuyến nghị: **implement presentational component ngay trong A5.2, nhưng defer Owner Admin integration**.

Lý do nên implement:

```txt
- A5.1 plan đã để TenantSwitcher là pending A5.2.
- `packages/ui` có `useTenantContext`, nhưng chưa có component controlled để hiển thị tenant context.
- Component presentational không cần API/router/storage nên không vi phạm boundary.
- Có thể dùng lại sau cho Clinic Admin/Public preview/Owner Admin context switch khi backend/auth tenant context rõ hơn.
```

Lý do chưa integrate:

```txt
- Owner Admin hiện là cross-tenant list, chưa có tenant authority/currentTenant real.
- Nếu gắn vào topbar bây giờ dễ tạo hiểu nhầm rằng chọn tenant trong UI là đổi security context.
- Không có route/API/permission flow rõ cho tenant switching.
- `useTenantContext` hiện chỉ là composable local; không phải nguồn quyền truy cập.
```

Vì vậy A5.2 nên tạo/export component, có thể thêm smoke static/source check; chưa render vào `AdminTopbar` hoặc route trừ khi owner duyệt rõ usage.

### 21.7 File Dự Kiến Tạo/Sửa

Được phép sửa/tạo trong A5.2:

```txt
frontend/packages/ui/src/components/CommandPalette.vue
frontend/packages/ui/src/components/TenantSwitcher.vue
frontend/packages/ui/src/index.ts
frontend/apps/owner-admin/src/components/OwnerCommandPalette.vue
```

Chỉ sửa nếu implementation thật sự cần và trong scope:

```txt
frontend/apps/owner-admin/src/layouts/OwnerAdminLayout.vue
```

Lý do có thể cần chạm layout:

```txt
- Nếu muốn wrapper nhận shortcut hint/platform key hoặc close callback rõ hơn.
- Nếu shared base thay đổi close timing/focus restore cần layout button/search trigger không đổi behavior.
```

Không sửa:

```txt
frontend/apps/owner-admin/src/components/AdminTopbar.vue
frontend/apps/owner-admin/src/components/AdminSidebar.vue
frontend/packages/api-client/*
frontend/packages/shared-types/*
frontend/apps/public-web/*
frontend/apps/clinic-admin/*
backend/*
Figma
package.json
package-lock.json
.claude/settings.local.json
docs/current-task.frontend.md nếu task không kéo dài hoặc không bị block
```

### 21.8 Props / Events Đề Xuất

`CommandPalette.vue`:

```ts
type CommandPaletteTone = "default" | "primary" | "success" | "warning" | "danger" | "info" | "neutral";

type CommandPaletteItem = {
  id: string;
  label: string;
  meta?: string;
  hint?: string;
  icon?: string;
  disabled?: boolean;
  tone?: CommandPaletteTone;
};

type CommandPaletteSection = {
  id: string;
  label: string;
  items: CommandPaletteItem[];
};

props:
- open: boolean
- query: string
- sections: CommandPaletteSection[]
- placeholder?: string
- loading?: boolean
- emptyLabel?: string
- emptyHelper?: string
- closeLabel?: string
- autofocus?: boolean

emits:
- "close": []
- "select": [item: CommandPaletteItem]
- "update:query": [value: string]
```

Slot mở rộng nếu cần nhưng không bắt buộc ở lượt đầu:

```txt
- #item="{ item, active }" để app custom row.
- #empty để app custom empty copy.
- #footer nếu sau này cần shortcut help.
```

`TenantSwitcher.vue`:

```ts
type TenantSwitcherItem = {
  id: string;
  label: string;
  slug?: string;
  meta?: string;
  avatarLabel?: string;
  disabled?: boolean;
};

props:
- tenants: TenantSwitcherItem[]
- currentTenantId?: string
- open?: boolean
- label?: string
- helper?: string
- loading?: boolean
- placeholder?: string
- emptyLabel?: string

emits:
- "select": [tenantId: string]
- "open": []
- "close": []
- "update:open": [open: boolean]
```

Implementation style: controlled dropdown/listbox. Component không đọc localStorage/session/env và không gọi API.

### 21.9 Acceptance Criteria

```txt
A5.2.1 `packages/ui/src/index.ts` export `CommandPalette` và `TenantSwitcher`.
A5.2.2 `CommandPalette` không import `vue-router`, Pinia, `@clinic-saas/api-client`, `@clinic-saas/shared-types`.
A5.2.3 `TenantSwitcher` không import router/API/storage/env và không dùng `useTenantContext` như authority.
A5.2.4 `OwnerCommandPalette` vẫn mở bằng search trigger topbar và Ctrl/Cmd+K.
A5.2.5 Escape đóng palette; click backdrop đóng palette; click/Enter chọn item đóng palette và điều hướng như trước.
A5.2.6 Tab focus không thoát khỏi command panel khi palette mở; đóng xong restore focus về trigger hoặc focused element trước đó nếu khả thi.
A5.2.7 Không đổi route `/dashboard`, `/clinics`, `/clinics/create`, `/clinics/:tenantId`.
A5.2.8 Không sửa backend/Figma/package/lockfile; không thêm dependency.
```

### 21.10 Risk Keyboard / Focus / Scroll Lock

```txt
HIGH - Focus trap conflict với global Escape:
  OwnerAdminLayout đang bắt Escape cho sidebar, OwnerCommandPalette cũng bắt Escape.
  Mitigation: khi palette mở, shared CommandPalette tự xử lý Escape và emit close; layout Escape sidebar vẫn chỉ đóng sidebar. Không thêm listener Escape trùng trong wrapper nếu shared đã xử lý.

HIGH - Body scroll lock bị ghi đè giữa sidebar drawer và command palette:
  Layout hiện set document.body.style.overflow cho sidebar, OwnerCommandPalette cũng set overflow.
  Mitigation: A5.2 không mở sidebar và palette đồng thời; khi mở palette có thể close sidebar trước hoặc shared base cleanup không reset sai nếu sidebar vẫn open. Nếu không sửa layout, giữ scroll lock trong CommandPalette nhưng test case mở sidebar mobile rồi Cmd+K cần được QA kiểm.

MEDIUM - Focus restore:
  Nếu trigger là button search topbar, restore OK. Nếu mở bằng keyboard global khi focus ở body/content, restore có thể quay về element cũ hoặc noop.
  Mitigation: dùng `useFocusTrap({ restoreFocus: true })`; fallback focus panel nếu không có focusable.

MEDIUM - Keyboard row navigation chưa có composable riêng:
  Implement tối thiểu ArrowDown/ArrowUp/Enter trong shared base hoặc giữ Tab/click only.
  Khuyến nghị A5.2 implement ArrowUp/ArrowDown/Enter vì Figma handoff 85:2 nhắc kbd nav.

MEDIUM - Router navigation nằm trong wrapper:
  Nếu shared emit disabled item, wrapper có thể navigate nhầm.
  Mitigation: shared không emit select cho disabled item; wrapper cũng guard `if (item.disabled) return`.

LOW - Query filter làm mất active index:
  Khi query đổi, activeIndex phải reset về item đầu tiên enabled.
```

### 21.11 Verify Command

Sau khi owner duyệt implement:

```powershell
cd frontend
npm run typecheck
npm run build
git diff --check
git status --short
```

Static boundary checks:

```powershell
rg "vue-router|pinia|@clinic-saas/api-client|@clinic-saas/shared-types|localStorage|sessionStorage|import\\.meta\\.env" frontend\packages\ui\src\components\CommandPalette.vue frontend\packages\ui\src\components\TenantSwitcher.vue
rg "CommandPalette|TenantSwitcher" frontend\packages\ui\src\index.ts frontend\apps\owner-admin\src\components\OwnerCommandPalette.vue
```

Expected:

```txt
- typecheck PASS cả 3 app.
- build PASS cả 3 app.
- boundary rg không match forbidden imports/storage/env trong 2 shared components.
- Staged/dirty chỉ nằm trong A5.2 files được phép.
```

### 21.12 Khi Nào Run `dev:owner` Để Visual Test

Plan phase hiện tại:

```txt
- Không run UI visual test.
- Không cần backend.
- Không sửa Figma.
```

Implementation phase sau owner approval:

```txt
1. Chạy typecheck/build trước.
2. Nếu PASS, chạy `cd frontend && npm run dev:owner`.
3. Smoke mock trên `http://localhost:5175`:
   - /dashboard: click search trigger topbar mở palette.
   - Ctrl/Cmd+K mở palette từ content.
   - gõ query "m" thấy section matching hoặc item filter đúng.
   - ArrowDown/ArrowUp đổi active row, Enter chọn action/tenant.
   - Escape đóng palette.
   - click backdrop đóng palette.
   - click "Tạo phòng khám mới" đi `/clinics/create`.
   - click tenant/action `/clinics` không crash.
   - Tab/Shift+Tab không thoát panel khi palette mở.
   - Mobile width < 640: panel không overflow ngang, search text không đè kbd.
4. Nếu TenantSwitcher chỉ tạo/export, không cần route visual smoke; có thể inspect bằng typecheck/build + source/static boundary. Nếu owner yêu cầu preview, cần một route/story/demo riêng và phải có plan bổ sung vì hiện chưa có Histoire.
```

### 21.13 Commit Split / Rollback

Commit đề xuất nếu owner duyệt và verify PASS:

```txt
feat(ui): add command palette and tenant switcher base
```

Nếu adoption wrapper thay đổi đủ đáng tách, có thể chia:

```txt
feat(ui): add command palette and tenant switcher base
feat(owner-admin): adopt shared command palette
```

Rollback:

```txt
- Revert commit A5.2.
- OwnerCommandPalette cũ có thể khôi phục độc lập vì A5.2 không đổi API client/router/store.
```

### 21.14 Out Of Scope

```txt
- Không implement real tenant switching authority.
- Không đọc/ghi localStorage/sessionStorage.
- Không gọi API tenant list thật trong TenantSwitcher.
- Không thêm route preview/storybook/histoire.
- Không thêm dependency.
- Không sửa package/lockfile.
- Không sửa backend/Figma.
- Không đổi Owner Admin route/store/API/business logic.
- Không commit/push trong plan phase.
```

### 21.15 Owner Approval Gate

```txt
Dừng tại đây.
Chưa code, chưa sửa source, chưa sửa Figma, chưa chạy UI visual test.
File plan đã sẵn sàng để owner duyệt.
Frontend Agent chỉ implement A5.2 khi owner nói rõ: "duyệt A5.2", "bắt đầu implement A5.2" hoặc tương đương.
```

### 21.16 Implementation Result 2026-05-11

Trạng thái: 🟢 **A5.2 implementation + verify PASS + committed**.

Commit:

```txt
c5426b9 feat(ui): add shared command palette and tenant switcher
```

Phạm vi đã hoàn thành:
- `CommandPalette` shared base đã nằm trong `frontend/packages/ui`.
- `OwnerCommandPalette` là wrapper app-local, giữ data/action/navigation/shortcut hiện tại.
- `TenantSwitcher` là presentational controlled component trong `frontend/packages/ui`.
- `packages/ui` không import `vue-router`, Pinia, `@clinic-saas/api-client`, `@clinic-saas/shared-types`, env/storage.
- Không đổi route/API/store/business logic.

Verify đã chạy:

```txt
cd frontend && npm run typecheck -> PASS cả 3 app.
cd frontend && npm run build     -> PASS cả 3 app.
git diff --check                 -> PASS, chỉ warning LF/CRLF trên Windows.
Boundary rg forbidden imports    -> PASS, không có match.
Export/wrapper rg                -> PASS.
```

Artifact còn lại:
- `temp/owner-admin-vite.log` là generated runtime artifact, không commit.

## 22. Wave A Step A5.3 - Shared UI Hardening & A5 Completion Gate (2026-05-11)

Trạng thái: 🟡 **Plan ready - dừng tại owner approval gate, chưa code**.

### 22.1 Lý Do Tạo Plan

Owner gọi short prompt `Lead Agent: bắt đầu A5.3`. Source of truth hiện chưa có section A5.3 rõ trong lane plan/current-task/roadmap. Theo Short Lead Prompt Rule mới, Lead Agent không acknowledge-only và không tự invent implementation; vì vậy A5.3 được định nghĩa thành bước hardening/closure nhỏ cho toàn bộ A5 trước khi chuyển sang A6 Owner Admin V3 restyle/state surfaces.

### 22.2 Agents Tự Chọn

```txt
Lead Agent: phân lane frontend, giữ scope và approval gate.
Architect Agent: kiểm tra boundary shared UI không kéo router/API/store/env/storage.
Frontend Agent: chỉ sửa surgical nếu verify phát hiện lỗi trong A5 shared UI hoặc wrapper.
QA Agent: chạy typecheck/build/static boundary checks và smoke source-level.
Documentation Agent: cập nhật lane plan/current-task nếu A5.3 được implement hoặc bị block.
```

Không cần Figma UI Agent trong A5.3 mặc định vì đây là hardening/source-boundary step, không đổi layout mới. Chỉ đọc Figma nếu owner mở rộng scope sang visual/pixel review.

### 22.3 Scope

A5.3 là bước completion gate cho A5 shared UI:

```txt
1. Review lại toàn bộ shared UI A5 đã có:
   - KPITile
   - ModuleChips
   - PlanBadge
   - EmptyState
   - DomainStateRow
   - CommandPalette
   - TenantSwitcher
2. Review Owner Admin adoption/wrapper đã có:
   - DashboardPage, ClinicsPage, ClinicDetailPage, CreateTenantWizard.
   - TenantTable, TenantDetailDrawer.
   - OwnerCommandPalette wrapper.
3. Chạy verify và boundary checks cho toàn bộ A5.
4. Chỉ sửa code surgical nếu phát hiện lỗi type/build/boundary hoặc regression rõ trong phạm vi A5.
5. Nếu không có lỗi, không code thêm.
6. Cập nhật docs lane ngắn nếu A5.3 verify xong hoặc phát hiện blocker.
```

### 22.4 Out Of Scope

```txt
- Không thêm component mới ngoài 7 shared UI A5.
- Không tích hợp TenantSwitcher vào AdminTopbar hoặc route thật.
- Không đổi route `/dashboard`, `/clinics`, `/clinics/create`, `/clinics/:tenantId`.
- Không đổi API client/store/shared-types/business logic.
- Không làm A6 Owner Admin V3 restyle.
- Không làm A7 state surfaces: CrossTenantDashboardPage, TenantLifecycleConfirmModal, DomainDnsRetryState, SslPendingState.
- Không thêm Histoire/axe/Lighthouse dependency hoặc sửa package-lock.
- Không sửa backend/Figma.
- Không commit/push nếu owner chưa yêu cầu.
```

### 22.5 Allowed Files / File Areas

Chỉ được sửa nếu verify phát hiện lỗi rõ trong A5:

```txt
frontend/packages/ui/src/components/KPITile.vue
frontend/packages/ui/src/components/ModuleChips.vue
frontend/packages/ui/src/components/PlanBadge.vue
frontend/packages/ui/src/components/EmptyState.vue
frontend/packages/ui/src/components/DomainStateRow.vue
frontend/packages/ui/src/components/CommandPalette.vue
frontend/packages/ui/src/components/TenantSwitcher.vue
frontend/packages/ui/src/index.ts
frontend/apps/owner-admin/src/components/OwnerCommandPalette.vue
frontend/apps/owner-admin/src/components/TenantTable.vue
frontend/apps/owner-admin/src/components/TenantDetailDrawer.vue
frontend/apps/owner-admin/src/components/CreateTenantWizard.vue
frontend/apps/owner-admin/src/pages/DashboardPage.vue
frontend/apps/owner-admin/src/pages/ClinicsPage.vue
frontend/apps/owner-admin/src/pages/ClinicDetailPage.vue
docs/current-task.frontend.md
temp/plan.frontend.md
```

Không được stage/commit artifact/log, đặc biệt:

```txt
temp/owner-admin-vite.log
.claude/settings.local.json
frontend/test-results/
frontend/playwright-report/
frontend/blob-report/
```

### 22.6 Acceptance Criteria

```txt
1. `packages/ui` A5 components không import router, Pinia, api-client, shared-types, env/storage.
2. Owner Admin A5 wrapper/adoption không đổi route/API/store/business behavior.
3. Typecheck PASS cả 3 app.
4. Build PASS cả 3 app.
5. `git diff --check` PASS.
6. Nếu có source diff, dirty files chỉ nằm trong allowed files A5.3.
7. Artifact/log không được stage/commit.
```

### 22.7 Verify Commands

```powershell
git status --branch --short
git diff --stat
git diff --check

cd frontend
npm run typecheck
npm run build

rg "vue-router|pinia|@clinic-saas/api-client|@clinic-saas/shared-types|localStorage|sessionStorage|import\\.meta\\.env" frontend/packages/ui/src/components/KPITile.vue frontend/packages/ui/src/components/ModuleChips.vue frontend/packages/ui/src/components/PlanBadge.vue frontend/packages/ui/src/components/EmptyState.vue frontend/packages/ui/src/components/DomainStateRow.vue frontend/packages/ui/src/components/CommandPalette.vue frontend/packages/ui/src/components/TenantSwitcher.vue
rg "KPITile|ModuleChips|PlanBadge|EmptyState|DomainStateRow|CommandPalette|TenantSwitcher" frontend/packages/ui/src/index.ts frontend/apps/owner-admin/src
```

Expected:

```txt
- Forbidden boundary rg không có match trong packages/ui A5 components.
- Usage/export rg có match đúng.
- Không có staged files trừ khi owner yêu cầu commit.
```

### 22.8 Rollback / Cleanup Notes

```txt
- Nếu A5.3 chỉ verify và không sửa source: rollback không cần thiết.
- Nếu sửa surgical và lỗi phát sinh: revert riêng diff A5.3, không revert commit A5.1/A5.1b/A5.2.
- `temp/owner-admin-vite.log` là artifact runtime; cleanup được nếu owner yêu cầu, không commit.
```

### 22.9 Owner Approval Gate

```txt
Dừng tại đây.
Chưa code, chưa sửa source implementation, chưa chạy build/typecheck cho A5.3.
Frontend Agent chỉ implement/verify A5.3 khi owner nói rõ "duyệt A5.3", "bắt đầu implement A5.3", "verify A5.3" hoặc tương đương.
```

### 22.10 Verify Result 2026-05-11

Trạng thái: 🟢 **A5.3 verify PASS, không sửa source implementation**.

Owner prompt:

```txt
Lead Agent: verify A5.3 theo plan trong temp/plan.frontend.md
```

Agents thực thi:

```txt
Lead Agent: phân lane frontend, chạy checklist A5.3.
Architect Agent: boundary check packages/ui không kéo router/API/store/env/storage.
Frontend Agent: không sửa code vì không phát hiện lỗi type/build/boundary.
QA Agent: chạy typecheck/build/static rg theo plan.
Documentation Agent: cập nhật kết quả verify vào lane plan.
```

Lệnh đã chạy:

```powershell
git status --branch --short
git diff --stat
git diff --check

cd frontend
npm run typecheck
npm run build

rg "vue-router|pinia|@clinic-saas/api-client|@clinic-saas/shared-types|localStorage|sessionStorage|import\\.meta\\.env" frontend/packages/ui/src/components/KPITile.vue frontend/packages/ui/src/components/ModuleChips.vue frontend/packages/ui/src/components/PlanBadge.vue frontend/packages/ui/src/components/EmptyState.vue frontend/packages/ui/src/components/DomainStateRow.vue frontend/packages/ui/src/components/CommandPalette.vue frontend/packages/ui/src/components/TenantSwitcher.vue
rg "KPITile|ModuleChips|PlanBadge|EmptyState|DomainStateRow|CommandPalette|TenantSwitcher" frontend/packages/ui/src/index.ts frontend/apps/owner-admin/src
```

Kết quả:

```txt
git diff --check -> PASS, chỉ warning LF/CRLF trên Windows.
npm run typecheck -> PASS cả 3 app: clinic-admin, owner-admin, public-web.
npm run build -> PASS cả 3 app.
Boundary rg forbidden imports/storage/env -> PASS, no match.
Usage/export rg -> PASS, có match đúng ở ui index và Owner Admin source.
```

Worktree sau verify:

```txt
Dirty tracked:
- temp/plan.frontend.md (plan + verify result)

Untracked artifact:
- temp/owner-admin-vite.log
```

Guardrail:

```txt
Không sửa source implementation.
Không stage.
Không commit.
Không push.
Không stage/commit temp/owner-admin-vite.log.
```

## 23. A5 Completion Bundle - Shared UI Foundation Closure (2026-05-11)

Trạng thái: 🟢 **A5 Completion Bundle PASS - A5.4/A5.5/A5.6/A5.7 hoàn tất**.

Owner prompt:

```txt
Lead Agent: chạy Feature Team hoàn tất A5 Completion Bundle.
```

### 23.1 Team Assembly

```txt
Lead Agent: điều phối lane frontend, đọc plan/current-task, gom kết quả và giữ guardrail.
Architect Agent: review boundary shared UI/app boundary/router/API/store/env/storage.
Frontend Agent: rà adoption, duplicate local rõ ràng và TenantSwitcher behavior.
QA Agent: chạy verify cuối toàn bộ A5.
Documentation Agent: cập nhật docs/current-task.frontend.md và temp/plan.frontend.md.
```

Subagent runtime đã được dùng cho Architect/Frontend/QA. Documentation Agent do Lead thực hiện trực tiếp trong lane docs.

### 23.2 A5.4 Adoption Sweep

Kết quả: ✅ **PASS**.

Frontend Agent rà read-only:

```txt
KPITile: DashboardPage.vue, ClinicsPage.vue.
EmptyState: ClinicsPage.vue.
PlanBadge + ModuleChips: TenantTable.vue, CreateTenantWizard.vue, ClinicDetailPage.vue.
DomainStateRow: TenantDetailDrawer.vue, ClinicDetailPage.vue.
CommandPalette: OwnerCommandPalette.vue wrapper app-local dùng shared base từ @clinic-saas/ui.
TenantSwitcher: chỉ export trong packages/ui, chưa tích hợp vào Topbar/layout nên không đổi behavior hiện tại.
```

Findings:

```txt
- Không còn duplicate rõ ràng bắt buộc cleanup trong A5.4.
- DashboardPage.vue vẫn có empty state local nhỏ trong AppCard recent tenants; không xem là blocker vì không phải target chính của frame.
- TenantDetailDrawer.vue vẫn render plan trong summary dạng text; đổi sang PlanBadge là polish, không đủ rõ để sửa surgical trong A5.4.
```

Quyết định: không sửa source implementation.

### 23.3 A5.5 Boundary Cleanup

Kết quả: ✅ **PASS**.

Architect Agent review:

```txt
Forbidden import/usage trong 7 component A5:
vue-router, pinia, @clinic-saas/api-client, @clinic-saas/shared-types,
localStorage, sessionStorage, import.meta.env -> no match.

packages/ui chỉ dùng Vue core và composable nội bộ UI như useFocusTrap/useReducedMotion.
Không thấy route/API/store/business tenant authority trong shared components.
TenantSwitcher là presentational controlled component.
CommandPalette là base UI, không biết navigation/router/action business.
```

Risk còn lại:

```txt
CommandPalette có Teleport và document.body.style.overflow để khóa scroll khi modal mở.
Đây là UI overlay behavior hợp lệ, không phải vi phạm route/API/store/env/storage.
```

### 23.4 A5.6 Documentation Closure

Kết quả: ✅ **PASS**.

Docs cập nhật:

```txt
temp/plan.frontend.md
docs/current-task.frontend.md
```

Nội dung cập nhật:

```txt
- Ghi A5.2 implementation result.
- Ghi A5.3 verify result.
- Ghi A5 Completion Bundle A5.4-A5.7 pass/fail.
- Ghi dirty/untracked/artifact và commit split proposal.
```

### 23.5 A5.7 Completion Gate Verify

Kết quả: ✅ **PASS**.

Lệnh đã chạy trong main rollout và QA Agent:

```powershell
git status --branch --short
git diff --stat
git diff --check

cd frontend
npm run typecheck
npm run build

rg "vue-router|pinia|@clinic-saas/api-client|@clinic-saas/shared-types|localStorage|sessionStorage|import\\.meta\\.env" frontend/packages/ui/src/components/KPITile.vue frontend/packages/ui/src/components/ModuleChips.vue frontend/packages/ui/src/components/PlanBadge.vue frontend/packages/ui/src/components/EmptyState.vue frontend/packages/ui/src/components/DomainStateRow.vue frontend/packages/ui/src/components/CommandPalette.vue frontend/packages/ui/src/components/TenantSwitcher.vue
rg "KPITile|ModuleChips|PlanBadge|EmptyState|DomainStateRow|CommandPalette|TenantSwitcher" frontend/packages/ui/src/index.ts frontend/apps/owner-admin/src
```

Kết quả verify:

```txt
git diff --check -> PASS, chỉ warning LF/CRLF trên Windows.
npm run typecheck -> PASS cho clinic-admin, owner-admin, public-web.
npm run build -> PASS cho clinic-admin, owner-admin, public-web.
Boundary rg forbidden imports/storage/env -> PASS, no match.
Usage/export rg -> PASS, export trong ui index và usage trong owner-admin source đúng.
```

### 23.6 Scope Skipped / Blocker

Skipped đúng out-of-scope:

```txt
- Không làm A6 Owner Admin V3 restyle.
- Không làm A7 state surfaces.
- Không thêm route mới.
- Không đổi API client/store/shared-types/business logic.
- Không tích hợp TenantSwitcher vào Topbar vì có thể tạo hiểu nhầm tenant authority.
- Không thêm package/dependency/Histoire/axe/Lighthouse.
- Không sửa backend/Figma/package-lock.
```

Blocker: không có blocker cho A5 completion.

### 23.7 Files Sửa / Tạo

Không sửa source implementation trong A5 Completion Bundle.

Docs cập nhật:

```txt
temp/plan.frontend.md
docs/current-task.frontend.md
```

Artifact còn lại:

```txt
temp/owner-admin-vite.log
```

## 27. Wave A Step A9 - Wiring `/plans` Với BE A.2 Real Contract (Plan-Only 2026-05-12)

Trạng thái: **Plan ready - chờ owner duyệt implement, chưa code**.

### 27.1 Mục Tiêu

```txt
- Nối Owner Admin `/plans` từ mock-first sang API thật BE A.2.
- Dùng API Gateway/Tenant Service contract:
  GET /api/owner/plans
  GET /api/owner/modules
  GET /api/owner/tenant-plan-assignments
  POST /api/owner/tenant-plan-assignments/bulk-change
- Giữ mock fallback khi không có env real, API lỗi, hoặc chạy mode mock/auto không có baseUrl.
- Không sửa backend nếu contract hiện tại đủ.
```

### 27.2 Team / Lane

```txt
Lane: Frontend.
Lead Agent: điều phối plan, kiểm tra source of truth và approval gate.
Architect Agent: review boundary FE/API client, không làm lệch tenant/owner role context.
Frontend Agent: sau khi được duyệt, implement client/adapter và page wiring.
QA Agent: verify mock fallback, real contract smoke nếu runtime có sẵn, typecheck/build.
Documentation Agent: cập nhật docs/current-task.frontend.md và temp/plan.frontend.md sau implement.
```

### 27.3 Contract Đã Kiểm Tra

```txt
BE A.2 contract đủ để FE wiring, chưa cần sửa backend:
- Response list đều có envelope `{ items: [...] }`.
- Plan item: code, name, price, description, tenantCount, tone, popular.
- Module item: id, name, category, starter, growth, premium.
- Assignment item: id, slug, currentPlan, currentPlanName, currentMrr, nextRenewal, selected, targetPlan.
- Bulk-change request: selectedTenantIds, targetPlan, effectiveAt="next_renewal", auditReason.
- Bulk-change response: changedCount, mrrDiff, status, message, effectiveAt, auditReason.
- Endpoint yêu cầu Owner Super Admin context qua `X-Owner-Role: OwnerSuperAdmin`; ClinicAdmin bị 403 theo backend smoke.
```

### 27.4 Scope Implement Sau Khi Duyệt

```txt
1. Tạo typed client cho Owner Plan Catalog trong frontend package hoặc owner-admin service layer theo pattern tenantClient hiện có.
2. Tạo adapter/normalizer cho BE response:
   - unwrap `{ items }`,
   - coerce `price/currentMrr/mrrDiff` về number,
   - validate plan code về `starter | growth | premium`,
   - fallback field thiếu sang mock-safe default thay vì crash UI.
3. Tách page `/plans` khỏi import trực tiếp mock constants:
   - load plans/modules/assignments qua client,
   - giữ loading/error/retry state,
   - fallback sang `planCatalogMock.ts` khi mode mock hoặc real fail trong auto/fallback mode.
4. Bulk-change thật:
   - gửi selected tenant ids, targetPlan, effectiveAt `next_renewal`, auditReason mặc định hiển thị rõ trong UI hoặc constant an toàn.
   - status hiển thị message/mrrDiff từ backend khi real path thành công.
   - fallback mock message khi real path không khả dụng và fallback được bật.
5. Giữ UI visual/layout A8, chỉ đổi data/request flow.
6. Cập nhật docs lane sau verify.
```

### 27.5 Out Of Scope

```txt
- Không sửa backend, không sửa docs backend.
- Không sửa schema/migration/persistence thật.
- Không sửa Figma, không tạo Figma file.
- Không thêm dependency/package-lock nếu không thật sự bắt buộc.
- Không đổi route `/plans`, sidebar, command palette trừ khi cần cho trạng thái loading/error nhỏ.
- Không làm billing thật, audit log thật, permission UI thật ngoài contract hiện có.
- Không stage/commit/push.
```

### 27.6 Allowed Files / Areas

```txt
frontend/packages/api-client/src/ownerPlanCatalogClient.ts        (mới nếu chọn package client)
frontend/packages/api-client/src/mockOwnerPlanCatalogClient.ts    (mới nếu mock fallback đặt trong package)
frontend/packages/api-client/src/index.ts                         (export client mới)
frontend/packages/shared-types/src/ownerPlanCatalog.ts            (mới nếu cần shared FE types)
frontend/packages/shared-types/src/index.ts                       (export types mới)
frontend/apps/owner-admin/src/services/planCatalogClient.ts       (mới, app-level env/mode wrapper)
frontend/apps/owner-admin/src/services/planCatalogMock.ts         (giữ làm fallback hoặc chuyển mock package nếu cần)
frontend/apps/owner-admin/src/pages/PlanModuleCatalogPage.vue     (wire data/loading/error/bulk-change)
frontend/apps/owner-admin/src/env.d.ts                            (chỉ nếu cần type env mới)
docs/current-task.frontend.md
temp/plan.frontend.md
```

Không sửa:

```txt
backend/**
docs/current-task.backend.md
temp/plan.backend.md
docs/roadmap/clinic-saas-roadmap.md
generated artifacts/log/screenshot
```

### 27.7 Env / Mode Strategy

```txt
- Dùng `VITE_API_BASE_URL` và same-origin `/api` proxy giống tenantClient.
- Thêm mode riêng `VITE_OWNER_PLAN_API_MODE=auto|real|mock` nếu cần tách khỏi tenant API mode.
- Thêm fallback flag riêng `VITE_OWNER_PLAN_API_FALLBACK` nếu cần; default fallback=true trong auto mode.
- Owner role dùng `OwnerSuperAdmin`, nhưng không hardcode trong `httpClient`; chỉ truyền qua owner plan client config như tenantClient đang làm.
```

### 27.8 Acceptance Criteria

```txt
- `/plans` vẫn render đủ 3 plan card, 12 module row, assignment table trong mock mode.
- Khi có API real, `/plans` load data từ 3 GET endpoint BE A.2 và không đọc trực tiếp mock constants làm source chính.
- Bulk-change gửi POST đúng payload gồm selectedTenantIds, targetPlan, effectiveAt="next_renewal", auditReason.
- UI hiển thị backend message/mrrDiff khi POST real thành công.
- Khi API lỗi hoặc không có baseUrl ở auto mode, mock fallback giữ trải nghiệm A8 không crash.
- Khi real mode không có baseUrl, fail rõ ràng như tenantClient pattern.
- `X-Owner-Role: OwnerSuperAdmin` được gửi qua owner client; không dùng role hardcoded trong generic httpClient.
- Không regression `/dashboard`, `/clinics`, `/clinics/create`, `/clinics/aurora-dental`.
```

### 27.9 Verify Commands

```powershell
git diff --check
cd frontend
npm run typecheck
npm run build
npm run dev:owner
```

HTTP smoke mock/auto fallback:

```txt
/dashboard -> 200 + #app
/clinics -> 200 + #app
/clinics/create -> 200 + #app
/clinics/aurora-dental -> 200 + #app
/plans -> 200 + #app
```

Real contract smoke nếu backend/gateway runtime sẵn:

```txt
GET /api/owner/plans -> 200, items.length=3
GET /api/owner/modules -> 200, items.length=12
GET /api/owner/tenant-plan-assignments -> 200, items.length=6
POST /api/owner/tenant-plan-assignments/bulk-change -> 200, changedCount/mrrDiff/message có giá trị
Negative smoke: X-Owner-Role=ClinicAdmin -> 403 nếu gọi trực tiếp API contract.
```

Interaction smoke `/plans`:

```txt
- tick/bỏ tick assignment không crash.
- đổi target plan.
- click Apply change:
  - mock/fallback path hiển thị status hợp lý;
  - real path hiển thị message/mrrDiff từ backend.
```

### 27.10 Rollback / Cleanup

```txt
- Nếu real wiring lỗi nhưng UI mock vẫn cần giữ, revert phần client/page wiring A9 về import mock A8.
- Không xóa `planCatalogMock.ts` trong A9; giữ làm fallback và rollback source.
- Không tạo screenshot mặc định; nếu có artifact do smoke/browser tạo thì đặt dưới frontend/test-results/ và không stage/commit.
```

### 27.11 Commit Split Proposal Khi Owner Yêu Cầu Commit

```txt
feat(owner-admin): wire plans page to owner plan catalog api
docs(frontend): record a9 plan catalog api wiring
```

Điểm dừng: **chờ owner duyệt plan A9 trước khi implement**.

### 27.12 Implementation Result - 2026-05-12

Trạng thái: **Implementation + verify PASS; chưa stage/commit/push**.

Scope đã làm:

```txt
- Thêm shared types cho Owner Plan Catalog ở `frontend/packages/shared-types/src/ownerPlanCatalog.ts`.
- Thêm typed real API client `createOwnerPlanCatalogClient` trong `frontend/packages/api-client`.
- Thêm app-level `planCatalogClient.ts` cho Owner Admin với mode `auto|real|mock`,
  `VITE_OWNER_PLAN_API_MODE`, `VITE_OWNER_PLAN_API_FALLBACK`, role `OwnerSuperAdmin`,
  và mock fallback giữ nguyên dữ liệu A8.
- Chuyển `/plans` khỏi import trực tiếp mock constants; page load plans/modules/assignments
  qua client, có loading/error/retry state.
- Bulk-change gửi payload BE A.2: `selectedTenantIds`, `targetPlan`, `effectiveAt="next_renewal"`,
  `auditReason`; UI hiển thị message/MRR diff từ response hoặc fallback mock.
- Giữ `planCatalogMock.ts` làm fallback/rollback source, không xóa mock.
```

Verify:

```txt
git diff --check -> PASS, chỉ warning LF/CRLF trên Windows.
cd frontend && npm run typecheck -> PASS cả 3 app.
cd frontend && npm run build -> PASS cả 3 app.
HTTP smoke qua dev server đang có `http://localhost:5175`:
  /dashboard -> 200 + #app
  /clinics -> 200 + #app
  /clinics/create -> 200 + #app
  /clinics/aurora-dental -> 200 + #app
  /plans -> 200 + #app
Vite transform:
  /src/main.ts -> 200
  /src/router/index.ts -> 200
  /src/pages/PlanModuleCatalogPage.vue -> 200
  /src/services/planCatalogClient.ts -> 200
  /src/services/planCatalogMock.ts -> 200
Real API direct smoke:
  /api/owner/plans qua dev proxy hiện 500 vì backend runtime/gateway không bật trong phiên này.
  A9 vẫn giữ auto/mock fallback nên UI không bị chặn.
```

Song song Backend/DevOps:

```txt
Backend wave không chặn FE A9:
  - BE A.3 Contract Hardening + FE A9 Support có thể chạy song song, không schema/persistence.
  - Scope: tăng test guard tenant mismatch, missing X-Tenant-Id, ClinicAdmin forbidden cho `/api/owner/*`,
    smoke OpenAPI/gateway consistency.

Backend wave cần approval riêng:
  - Plan/Module persistence thật, migration 0002, repository Dapper, Billing/subscription integration.
  - Đã có plan preparation trong backend lane; không implement trong lượt FE A9.
```

Gaps:

```txt
- Chưa smoke real browser flow với backend runtime thật vì `/api/owner/plans` hiện không sẵn.
- Chưa chạy interaction automation click Apply sau wiring; typecheck/build/HTTP smoke đã PASS.
```

Artifact này là runtime log, không stage/commit.

### 23.8 Commit Split Proposal

Commit đề xuất cho A5 completion docs nếu owner yêu cầu:

```txt
docs(frontend): close a5 shared ui completion gate
```

Commit này chỉ nên gồm:

```txt
temp/plan.frontend.md
docs/current-task.frontend.md
```

Không đưa vào commit:

```txt
temp/owner-admin-vite.log
.claude/settings.local.json
screenshot/log/test/generated artifacts
```

### 23.9 Guardrail

```txt
Không sửa source implementation.
Không stage.
Không commit.
Không push.
Không stage/commit temp/owner-admin-vite.log.
```
## 24. Wave A Step A6 - Owner Admin V3 Restyle / State Foundation (2026-05-11)

Trạng thái: **Implementation + verify PASS; chưa commit theo yêu cầu owner**.

### 24.1 Owner Prompt

```txt
Lead Agent: chạy Feature Team implement toàn bộ A6 Owner Admin V3.
```

Action: `implement/resume`, không plan-only vì owner override rõ và A6 có đủ scope frontend trong lane docs/checkpoint.

### 24.2 Team Assembly

```txt
Lead Agent: điều phối frontend lane, đọc source of truth, giữ scope, tích hợp app source, chạy verify.
Architect Agent: subagent runtime thật `Parfit`, read-only boundary route/API/store/env/package-lock.
Figma UI Agent: subagent runtime thật `Hubble`, read-only V3 implementation checklist từ docs/Figma frame notes/MCP.
Frontend Agent: subagent runtime thật `Descartes`, sửa shared UI trong phạm vi `frontend/packages/ui/src/components`.
QA Agent: Lead chạy checklist typecheck/build/diff/static/smoke cuối lượt.
Documentation Agent: Lead cập nhật `docs/current-task.frontend.md` và `temp/plan.frontend.md`.
```

Subagent runtime thật đã dùng cho Architect, Figma UI và Frontend. Lead không spawn QA riêng vì cần chạy verify sau integration cuối.

### 24.3 Dirty Classification Trước Khi Sửa

```txt
Dirty tracked trước A6:
- docs/current-task.frontend.md: A5 docs closure / lane checkpoint.
- temp/plan.frontend.md: A5 docs closure / lane plan.

Untracked artifact:
- temp/owner-admin-vite.log: runtime artifact có từ trước, không stage/commit.

A6 source dirty trước khi sửa: không có.
File ngoài scope: không thấy dirty tracked ngoài docs lane/artifact.
```

### 24.4 Source Of Truth / Figma Handoff

Figma MCP read-only đã đối chiếu:

```txt
85:2   Owner Admin Tenant Operations
87:2   Tenant Detail Drawer
88:2   Create Tenant Wizard
88:127 Command Palette
124:2  Dashboard Cross-Tenant
```

Checklist áp dụng:

```txt
- muted/ivory app background, elevated white cards, slate sidebar, compact topbar.
- card radius 12-16, drawer/sheet 560px, table row compact.
- wizard card + horizontal stepper polish.
- command palette overlay polish.
- shared UI A5 reuse: KPITile, EmptyState, PlanBadge, ModuleChips, DomainStateRow, CommandPalette.
- giữ route/API/store/business behavior hiện tại.
```

### 24.5 Implementation Result

Owner Admin app:

```txt
frontend/apps/owner-admin/src/components/AdminSidebar.vue
  - Thêm visible "Sắp ra mắt" badge cho disabled nav, focus-visible cho nav/brand/create.

frontend/apps/owner-admin/src/components/AdminTopbar.vue
  - Search trigger có aria-label, hover/focus-visible V3, icon button focus state.

frontend/apps/owner-admin/src/components/TenantFilterBar.vue
  - Compact filter chip, aria-pressed, focus-visible rõ.

frontend/apps/owner-admin/src/components/TenantTable.vue
  - Header tiếng Việt, aria-label table, Space/Enter mở row, row height compact.

frontend/apps/owner-admin/src/components/TenantDetailDrawer.vue
  - Drawer z-index/sheet polish, header muted, visual tabs, PlanBadge trong summary.

frontend/apps/owner-admin/src/components/CreateTenantWizard.vue
  - Stepper/card/input/plan/module/preview chuyển sang CSS variables V3.
  - Giữ nguyên validation, conflict focus, payload submit.

frontend/apps/owner-admin/src/pages/DashboardPage.vue
  - Rebuild dashboard thành cross-tenant cockpit: 6 KPI, MRR chart presentation-only,
    attention queue, recent tenants table.
  - Chỉ derive từ `tenantClient.listTenants()`, không thêm API aggregate.

frontend/apps/owner-admin/src/pages/ClinicsPage.vue
  - Token/radius/error/filter/footer state sweep theo V3.

frontend/apps/owner-admin/src/pages/ClinicDetailPage.vue
  - Token/radius/error/loading/detail grid sweep theo V3.
```

Shared UI:

```txt
frontend/packages/ui/src/components/AppButton.vue
frontend/packages/ui/src/components/AppCard.vue
frontend/packages/ui/src/components/MetricCard.vue
frontend/packages/ui/src/components/StatusPill.vue
frontend/packages/ui/src/components/KPITile.vue
frontend/packages/ui/src/components/EmptyState.vue
frontend/packages/ui/src/components/PlanBadge.vue
frontend/packages/ui/src/components/ModuleChips.vue
frontend/packages/ui/src/components/DomainStateRow.vue
frontend/packages/ui/src/components/CommandPalette.vue
frontend/packages/ui/src/components/TenantSwitcher.vue
```

Nội dung shared UI: radius/fallback theo hướng 12-16, muted/ivory, compact spacing, hover/focus-visible, reduced motion, aria/listbox polish cho `CommandPalette`/`TenantSwitcher`. Không đổi props/emits và không kéo router/Pinia/API/shared-types/env/storage vào `packages/ui`.

### 24.6 Verify Result

```txt
git diff --check -> PASS, chỉ warning LF/CRLF trên Windows.
cd frontend && npm run typecheck -> PASS:
  - clinic-admin PASS
  - owner-admin PASS
  - public-web PASS
cd frontend && npm run build -> PASS:
  - clinic-admin PASS
  - owner-admin PASS
  - public-web PASS
HTTP smoke qua dev server hiện có http://localhost:5175:
  /dashboard -> 200
  /clinics -> 200
  /clinics/create -> 200
  /clinics/mock-tenant-id -> 200
rg "vue-router|pinia|@clinic-saas/api-client|@clinic-saas/shared-types|localStorage|sessionStorage|import\\.meta\\.env" frontend/packages/ui/src/components
  -> PASS, no match.
rg "116\\.118|localhost:|TOKEN|SECRET|PASSWORD|PRIVATE KEY" frontend/apps/owner-admin frontend/packages/ui frontend/packages/design-tokens
  -> không thấy secret/token/IP thật; có match `localhost:` trong `frontend/apps/owner-admin/vite.config.ts`
     dev comment/config có sẵn, không phải A6 source mới.
```

### 24.7 Scope Skipped / Blocker

Skipped đúng boundary:

```txt
- Không đổi route path.
- Không đổi API client behavior.
- Không đổi Pinia/store/business logic.
- Không đổi shared-types contract.
- Không thêm dependency; không sửa package.json/package-lock.
- Không sửa backend/Figma.
- Không implement A7 state surfaces thật: lifecycle confirm modal, DNS retry, SSL pending.
- Không thêm route `/cross-tenant-dashboard`.
- Không integrate TenantSwitcher vào Topbar vì chưa có tenant switching authority.
- Không chạy screenshot/pixel smoke vì workspace hiện không có Playwright/browser screenshot dependency sẵn.
```

Blocker: không có blocker cho A6 implementation/verify.

### 24.8 Artifact / Cleanup Note

```txt
Untracked artifact còn lại: temp/owner-admin-vite.log.
Không stage/commit artifact này.
Không cleanup vì đây là runtime artifact có từ trước lượt A6 và có thể còn phục vụ owner review dev server.
```

### 24.9 Commit Split Proposal

Nếu owner yêu cầu commit, đề xuất chia:

```txt
feat(owner-admin): restyle tenant operations surfaces for v3
  - frontend/apps/owner-admin/src/components/*
  - frontend/apps/owner-admin/src/pages/*

feat(ui): polish shared admin components for v3
  - frontend/packages/ui/src/components/*

docs(frontend): record a6 owner admin v3 completion
  - docs/current-task.frontend.md
  - temp/plan.frontend.md
```

Không đưa vào commit:

```txt
temp/owner-admin-vite.log
frontend/test-results/
frontend/playwright-report/
frontend/blob-report/
screenshot/log/generated artifacts
```

## 25. Wave A Step A7 - Budget Mode Result 2026-05-11

Status: **verify PASS; chưa stage/commit/push A7**.

Scope đã xử lý:

```txt
- DomainDnsRetryState.vue: DNS retry queue presentation/state surface.
- SslPendingState.vue: SSL issuing pipeline presentation/state surface.
- TenantLifecycleConfirmModal.vue: confirm modal cho suspend/archive/restore.
- TenantDetailDrawer.vue, ClinicDetailPage.vue, DashboardPage.vue: gắn state surfaces vào Owner Admin hiện có.
```

Verify:

```txt
cd frontend && npm run typecheck -> PASS.
cd frontend && npm run build -> PASS.
git diff --check -> PASS, chỉ warning LF/CRLF trên Windows.
Static boundary -> PASS, no match.
Secret/IP -> PASS, no match.
HTTP smoke /dashboard, /clinics, /clinics/aurora-dental -> PASS 200 + #app.
```

Cleanup:

```txt
Đã xóa untracked/generated artifacts: frontend/test-results/, .playwright-mcp/,
a7-dashboard-state-surfaces-desktop.png, temp/owner-admin-vite.log,
temp/owner-admin-vite.log.err.
Đã stop vite dev server port 5175 để xóa log bị lock.
```

Commit proposal khi owner yêu cầu:

```txt
feat(owner-admin): add v3 budget state surfaces
docs(frontend): record a7 budget verification
```

### 24.10 Guardrail

```txt
Không stage.
Không commit.
Không push.
Không sửa backend/Figma/package-lock.
Không revert dirty docs/source không rõ chủ sở hữu.
```

### 25.1 Budget Contact Sheet Follow-up 2026-05-11

Trạng thái: **visual review FAIL; verify kỹ thuật PASS; chưa commit/push**.

```txt
Contact sheet: frontend/test-results/a7-visual-contact-sheet.png.
View đã chụp: desktop/mobile /dashboard và desktop/mobile /clinics/aurora-dental.
Lifecycle modal bỏ qua vì không mở được bằng UI an toàn/state hiện có.
```

```txt
Visual blocker:
- /dashboard mock mode hiển thị data.
- /clinics/aurora-dental mock mode hiển thị "Không tìm thấy phòng khám".
```

```txt
Verify kỹ thuật vẫn PASS:
- git diff --check
- npm run typecheck
- npm run build
- static boundary no match
- secret/IP no match
- HTTP smoke /dashboard, /clinics, /clinics/aurora-dental -> 200 + #app
```

```txt
Decision needed:
- Không stage/commit/push A7 khi visual review đang fail.
- Giữ contact sheet để owner review; đã xóa log dev server và screenshot rời.
```

### 25.2 Budget Visual Fix 2026-05-11

Trạng thái: **visual review PASS sau fix surgical; chưa stage/commit/push**.

```txt
Fix:
- Mock tenant detail lookup nhận cả id và slug.
- /clinics/aurora-dental render Aurora Dental trong mock mode.
- Không đổi route path, API/store/shared-types/business contract, backend, package-lock hoặc dependency.
```

```txt
Verify PASS:
- git diff --check
- npm run typecheck
- npm run build
- static boundary no match
- secret/IP no match
- HTTP smoke /dashboard, /clinics, /clinics/aurora-dental -> 200 + #app
- Contact sheet: frontend/test-results/a7-visual-contact-sheet.png
```

### 25.3 QA Retest Budget 2026-05-11

Trạng thái: **Owner visual review PASS + A7 QA retest PASS; chưa stage/commit/push**.

```txt
Không tạo thêm screenshot. Không gọi Figma/full team. Không sửa code.
Verify PASS:
- git diff --check
- npm run typecheck
- npm run build
- static boundary no match
- secret/IP no match
- HTTP smoke /dashboard, /clinics, /clinics/aurora-dental -> 200 + #app
- Contact sheet hiện có: frontend/test-results/a7-visual-contact-sheet.png
```

### 25.4 Finalize A7 2026-05-11

Trạng thái: **A7 PASS, commit split + push theo yêu cầu owner**.

```txt
Owner visual review PASS.
QA retest budget PASS.
Cleanup artifact trước push: frontend/test-results/, .playwright-mcp/,
temp/owner-admin-vite.log, temp/owner-admin-vite.log.err nếu tồn tại.
Không stage/commit screenshot/log/generated artifact.
Commit split:
- feat(owner-admin): add v3 lifecycle state surfaces
- docs(frontend): record a7 budget verification
```

### 24.11 QA Visual Review Follow-up 2026-05-11

Trạng thái: **PASS sau fix surgical**.

Owner prompt:

```txt
Thực hiện ngay QA visual review A6 trong worktree hiện tại.
```

## 26. Wave A Step A8 - Owner Admin Plan/Module Catalog Mock-First (2026-05-12)

Trạng thái: **Implementation + verify PASS; chưa stage/commit/push theo yêu cầu owner**.

### 26.1 Xác Nhận Đầu Vào

```txt
A7 đã xong qua git log:
- 6d54282 style(owner-admin): polish clinics operations page
- b76122e fix(owner-admin): remove duplicate clinics create cta
- d1e92c3 docs(frontend): record a7 budget verification
- 2a070b9 feat(owner-admin): add v3 lifecycle state surfaces

git status trước A8: worktree sạch, branch main ahead origin/main 2.
git pull --ff-only: Already up to date.
```

### 26.2 Team / Lane

```txt
Lane: Frontend.
Lead Agent: điều phối scope A8, giữ boundary không backend/Figma write.
Frontend Agent: implement Owner Admin mock UI và route.
Figma UI Agent: read-only, đã đối chiếu frame 126:2 Plan/Module Catalog và 126:173 Plan Assignment qua Figma MCP.
QA Agent: typecheck/build, HTTP smoke route, visual contact sheet budget.
Documentation Agent: cập nhật docs/current-task.frontend.md và temp/plan.frontend.md.
```

### 26.3 Scope A8 FE

```txt
1. Thêm route Owner Admin `/plans` cho Plan/Module Catalog mock-first theo Figma frame 126:2.
2. Thêm mock data/adapter frontend rõ ràng cho plan cards, module matrix, tenant assignment draft.
3. Bật nav/sidebar + command palette tới `/plans`, không còn để Plan/Module Catalog chỉ là coming soon.
4. Expose đầy đủ state surfaces sau A7 bằng UI route thật:
   - Dashboard tiếp tục hiển thị DNS retry + SSL pending.
   - Detail `/clinics/aurora-dental` tiếp tục mở lifecycle confirm modal bằng action mock.
   - Detail/drawer tiếp tục có DNS Retry / SSL Pending preview khi domain action mock được bấm.
5. Thêm Plan Assignment mock trong cùng `/plans` nếu scope hợp lý: bảng selected tenants, bulk change draft, MRR diff, effective next renewal.
6. Cập nhật docs lane: mock data ở đâu, route/state đã verify, BE handoff request và test gaps.
```

### 26.4 Out Of Scope

```txt
- Không real backend wiring nếu BE chưa sẵn.
- Không sửa backend, không sửa docs backend.
- Không sửa Figma, không tạo Figma file.
- Không thêm dependency/package-lock.
- Không stage/commit/push.
- Không làm Wave B/C/D/E hoặc các frame audit/monitoring/billing/settings sâu.
```

### 26.5 Allowed Files / Areas

```txt
frontend/apps/owner-admin/src/router/index.ts
frontend/apps/owner-admin/src/components/AdminSidebar.vue
frontend/apps/owner-admin/src/components/OwnerCommandPalette.vue
frontend/apps/owner-admin/src/pages/PlanModuleCatalogPage.vue
frontend/apps/owner-admin/src/services/planCatalogMock.ts
docs/current-task.frontend.md
temp/plan.frontend.md
frontend/test-results/a8-fe-contact-sheet.png (artifact review, không commit)
```

### 26.6 Acceptance Criteria

```txt
- `/plans` render được 3 plan card Starter/Growth/Premium và module matrix 12 row x 3 plan.
- `/plans` có bảng Plan Assignment mock: selected tenants, target plan, MRR diff, next renewal, bulk action mock.
- Sidebar và command palette đi tới `/plans` không crash.
- A7 states vẫn render ở `/dashboard` và `/clinics/aurora-dental`; lifecycle confirm modal vẫn mở/đóng/action mock qua detail action.
- HTTP smoke PASS cho `/dashboard`, `/clinics`, `/clinics/create`, `/clinics/aurora-dental`, `/plans`.
- typecheck/build PASS trong frontend workspace.
```

### 26.7 Verify Commands

```powershell
git diff --check
cd frontend
npm run typecheck
npm run build
npm run dev:owner
HTTP smoke:
- /dashboard
- /clinics
- /clinics/create
- /clinics/aurora-dental
- /plans
Visual budget:
- frontend/test-results/a8-fe-contact-sheet.png (1 contact sheet, không giữ ảnh rời)
```

### 26.8 BE Handoff Request

```txt
BE/API contract cần sau mock:
- GET /api/owner/plans: plan catalog, price, tenantCount, module entitlements.
- GET /api/owner/modules: module metadata, category, default availability per plan.
- GET /api/owner/tenant-plan-assignments: tenant current plan, MRR, renewal date.
- POST /api/owner/tenant-plan-assignments/bulk-change: selected tenant ids, target plan, effectiveAt=next_renewal, audit reason.
- Domain/DNS/SSL real wiring vẫn cần Domain Service contract Phase 4.1 cho retry/pending status.
```

### 26.9 Implementation Result

```txt
Scope đã làm:
- Thêm `/plans` Owner Admin route cho Plan & Module Catalog mock-first.
- Thêm `planCatalogMock.ts` chứa plan cards, 12 module matrix row, tenant plan assignment mock.
- Sidebar bật nav "Gói & module"; command palette thêm action mở `/plans`.
- `/plans` render 3 plan card Starter/Growth/Premium, module matrix 12 x 3, bảng plan assignment, checkbox/select target plan, mock bulk change status.
- A7 surfaces vẫn được expose: dashboard DNS/SSL attention, detail Aurora Dental có lifecycle action buttons, source vẫn có modal open/close/confirm và DNS/SSL preview state.
```

File thay đổi trong FE/docs:

```txt
frontend/apps/owner-admin/src/pages/PlanModuleCatalogPage.vue
frontend/apps/owner-admin/src/services/planCatalogMock.ts
frontend/apps/owner-admin/src/router/index.ts
frontend/apps/owner-admin/src/components/AdminSidebar.vue
frontend/apps/owner-admin/src/components/OwnerCommandPalette.vue
docs/current-task.frontend.md
temp/plan.frontend.md
frontend/test-results/a8-fe-contact-sheet.png (artifact review, không commit)
```

Verify đã chạy:

```txt
git diff --check -> PASS, chỉ warning LF/CRLF trên Windows; có file backend dirty từ lane khác, không thuộc FE A8.
cd frontend && npm run typecheck -> PASS cả 3 app.
cd frontend && npm run build -> PASS cả 3 app.
HTTP smoke mock mode:
  /dashboard -> 200 + #app
  /clinics -> 200 + #app
  /clinics/create -> 200 + #app
  /clinics/aurora-dental -> 200 + #app
  /plans -> 200 + #app
Vite transform smoke:
  /src/main.ts, /src/router/index.ts, /src/pages/PlanModuleCatalogPage.vue,
  /src/services/planCatalogMock.ts -> 200
CDP interaction smoke /plans:
  checkbox count 6; tick thêm nova-skin -> selected 4;
  đổi target plan nova-skin sang Premium; Apply mock change -> status
  "4 tenant sẽ đổi gói ở chu kỳ kế tiếp. MRR dự kiến tăng $420."
Visual contact sheet:
  frontend/test-results/a8-fe-contact-sheet.png
```

Test gaps:

```txt
- Chưa có browser automation click thật cho lifecycle modal; plan bulk action interaction smoke đã PASS qua Edge CDP.
- Chưa có real API smoke vì BE contract cho plan/module/assignment chưa sẵn.
- Contact sheet là generated artifact review, không stage/commit trừ khi owner yêu cầu.
```

### 26.10 /plans Visual Quality Fix 2026-05-12

Trạng thái: **PASS sau owner contact sheet feedback; chưa stage/commit/push**.

Visual polish đã làm:

```txt
- Tăng contrast /plans: heading/subtitle/table text/label rõ hơn, giảm cảm giác nhạt.
- Plan cards Starter/Growth/Premium có tone riêng; Growth popular nổi bật hơn nhưng price/text vẫn đọc được.
- Plan cards thêm hierarchy price, tenant count và module summary.
- Module matrix tăng font/row spacing, header rõ, enabled/limited/locked state nhìn như state thật.
- Assignment card/CTA nổi hơn; desktop contact sheet thấy được bảng assignment.
- Mobile /plans stack gọn hơn; matrix/assignment không còn phụ thuộc overflow ngang để đọc nội dung chính.
```

Verify sau fix:

```txt
git diff --check -> PASS, chỉ warning LF/CRLF trên Windows.
cd frontend && npm run typecheck -> PASS cả 3 app.
cd frontend && npm run build -> PASS cả 3 app.
HTTP smoke:
  /dashboard -> 200 + #app
  /clinics -> 200 + #app
  /clinics/create -> 200 + #app
  /clinics/aurora-dental -> 200 + #app
  /plans -> 200 + #app
Interaction smoke /plans:
  checkbox count 6; chọn/bỏ chọn/chọn lại nova-skin; đổi target plan sang Premium;
  Apply mock change -> "4 tenant sẽ đổi gói ở chu kỳ kế tiếp. MRR dự kiến tăng $420."; app không crash.
Visual contact sheet:
  frontend/test-results/a8-fe-contact-sheet.png
```

Gaps/guardrail:

```txt
- Lifecycle modal click automation vẫn chưa chạy trong lượt này.
- Real API smoke vẫn pending BE contract; BE handoff section 26.8 giữ nguyên.
- Không sửa backend/docs backend/Figma/API/mock scope; không stage/commit/push.
```

Đã kiểm tra:

```txt
git status --branch --short
git diff --stat
HTTP smoke http://localhost:5175/dashboard, /clinics, /clinics/create, /clinics/mock-tenant-id
Edge headless screenshot desktop 1440x1100 và mobile 390x844 cho 4 route
cd frontend && npm run typecheck
cd frontend && npm run build
git diff --check
```

Fix surgical trong lượt QA:

```txt
- ClinicDetailPage: sửa heading/error state khi tenant không tồn tại, Việt hóa lỗi "Tenant not found.".
- AdminTopbar/OwnerAdminLayout: chặn overflow topbar/layout ở mobile.
- DashboardPage/ClinicsPage/CreateClinicPage/ClinicDetailPage: thêm wrap guard cho heading dài.
- AppButton/AppCard: thêm box-sizing border-box để width 100% không vượt viewport do padding.
```

Kết quả verify:

```txt
HTTP smoke 4 route -> PASS 200, SPA shell có #app.
Screenshot review -> PASS sau fix, không còn blocker visual rõ ở desktop/mobile route được yêu cầu.
npm run typecheck -> PASS cả 3 app.
npm run build -> PASS cả 3 app.
git diff --check -> PASS, chỉ warning LF/CRLF trên Windows.
```

Artifact không commit:

```txt
frontend/test-results/
temp/owner-admin-vite.log
```
