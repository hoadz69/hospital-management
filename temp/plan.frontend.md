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
