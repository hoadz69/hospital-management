# Owner Admin Tenant Slice - Manual Smoke Checklist

Ngày tạo: 2026-05-10
Owner workstream: Frontend - Phase 3 Owner Admin Tenant Slice
Phụ trách smoke: QA Agent (manual smoke, không có vitest/playwright/cypress trong workspace).

## 1. Bối Cảnh

Frontend Phase 3 (`/dashboard`, `/clinics`, `/clinics/create`, `/clinics/:tenantId`) đã code xong. Backend Phase 2 đã PASS 5 smoke trên server test do owner cung cấp nhưng FE chưa set `VITE_API_BASE_URL` + `VITE_TENANT_API_MODE=real`, nên smoke FE hiện tại chỉ dùng mock fallback.

Workspace KHÔNG có test framework (vitest/playwright/cypress/jest/testing-library). Smoke phải chạy bằng tool có sẵn:

- `npm run typecheck` và `npm run build` từ `frontend/`
- `npm run dev:owner` (Vite dev server port 5175, chạy background)
- HTTP smoke: `curl http://localhost:5175/...` để xác nhận SPA fallback và Vite transform pipeline
- Vite source transform smoke: `curl http://localhost:5175/src/...` xác nhận compile được, kèm regex check key logic
- Vue SPA: mọi route trả cùng `index.html` (140B), không phân biệt được DOM render qua HTTP. Phải dùng source regex thay thế cho assertion DOM.
- Mock client: 3 tenant `aurora-dental` (Active+verified), `river-eye` (Draft+pending), `nova-skin` (Suspended+failed). Default form Wizard = aurora-dental → submit raw sẽ trigger 409 ở slug + domain + email.

## 2. Hướng Dẫn Chạy Lại Smoke

```powershell
cd d:\project\hospital-management\frontend
npm run typecheck
npm run build
npm run dev:owner
# Chờ Vite ready (port 5175). Mở terminal khác hoặc chạy curl trong cùng phiên.
curl http://localhost:5175/
curl http://localhost:5175/dashboard
curl http://localhost:5175/clinics
curl http://localhost:5175/clinics/create
curl http://localhost:5175/clinics/tenant-aurora-dental
```

Khi muốn smoke real API (sau khi backend Phase 2 đã sẵn sàng và owner cung cấp API base URL):

```powershell
# Tạo file frontend/apps/owner-admin/.env.local với:
#   VITE_API_BASE_URL=https://<backend-host>
#   VITE_TENANT_API_MODE=real
#   VITE_TENANT_API_FALLBACK=false
# Sau đó dừng Vite, chạy lại npm run dev:owner để Vite reload env.
# Mở browser tới http://localhost:5175/clinics và xác nhận GET /api/tenants trả 3 row backend (không phải mock).
# Click "Tạo tenant" và submit form aurora-dental → backend phải trả 409, ConflictState hiển thị.
```

Trạng thái checkbox: `[x]` PASS, `[ ]` chưa chạy / fail, `[~]` SKIP có lý do.

## 3. A. Navigation / Layout

- [x] Vite dev server lên đúng port `5175` (`curl http://localhost:5175/` → 200, 140B, `id="app"` + `/src/main.ts` xuất hiện trong index.html shell).
- [x] SPA fallback hoạt động cho 5 navigation route chính: `/`, `/dashboard`, `/clinics`, `/clinics/create`, `/clinics/aurora-dental` (slug, error path), `/clinics/tenant-aurora-dental` (id thật, success path).
- [x] Layout key tokens xuất hiện trong `OwnerAdminLayout.vue` đã transform: `AdminSidebar`, `AdminTopbar`, `sidebarOpen`, `toggle-sidebar`, `closeSidebar` (drawer pattern <1024px).
- [x] Sidebar chứa brand "ClinicOS Owner", role badge "Owner Super Admin" và 8 nav item (`navItems` literal). 6 mục disabled render `<button>` + `soon-badge` "Sắp ra mắt".
- [x] Topbar chứa hamburger với `aria-label="Mở menu điều hướng"`, `aria-expanded`, prop `showCreateAction` và `showSearch`.
- [x] Router meta khai báo đủ 4 route (`DashboardPage`, `ClinicsPage`, `CreateClinicPage`, `ClinicDetailPage`) + `tenantId` props + `showCreateAction: false` cho `/clinics/create`.
- [~] Visual pixel-perfect đối chiếu Figma frame `V2 - Owner Admin Tenant Operations`: SKIP - QA env không có headless browser. Owner đã visual smoke 2 round trên `http://localhost:5175` ngày 2026-05-10 (ghi trong `docs/current-task.frontend.md`).
- [~] Refresh deep-link (browser hard reload) trên route `/clinics/tenant-river-eye`: SKIP qua HTTP smoke vì SPA fallback chỉ trả `index.html` (rendered DOM cần browser thật). Vite transform 200 cho ClinicDetailPage.vue đảm bảo source compile được.

Command + kết quả:

```txt
GET /                                  -> 200, 140B, app=1, main=1
GET /dashboard                         -> 200, 140B, app=1, main=1
GET /clinics                           -> 200, 140B, app=1, main=1
GET /clinics/create                    -> 200, 140B, app=1, main=1
GET /clinics/aurora-dental             -> 200, 140B, app=1, main=1   (slug, không phải id - mock sẽ throw not-found)
GET /clinics/tenant-aurora-dental      -> 200, 140B, app=1, main=1   (id mock thật)
GET /clinics/tenant-river-eye          -> 200, 140B, app=1, main=1
GET /clinics/tenant-nova-skin          -> 200, 140B, app=1, main=1
GET /clinics/tenant-does-not-exist     -> 200, 140B, app=1, main=1   (id ko tồn tại - mock throw)
```

## 4. B. Tenant List (`/clinics`)

- [x] Source `ClinicsPage.vue` transform 200 (33764B), chứa `loadTenants`, `tenantClient.listTenants`, `filteredTenants`, `isFiltered`, `isEmpty`.
- [x] Loading state: `loading.value = true` set trước khi await `tenantClient.listTenants()`. Table render `Đang tải danh sách tenant...` khi `loading=true`.
- [x] Table 6 cột render trong `TenantTable.vue`: Tenant, Slug, Plan, Domain, Status, Action (đã grep header `<th>`).
- [x] Filter bar có 4 select: status, plan, domainStatus, moduleCode (`TenantFilterBar.vue` source 13188B). Reset + Export disabled.
- [x] Filter logic trong `ClinicsPage.vue`: `filteredTenants = tenants.filter(...)`, `resetFilters` reset 4 trường về `"all"`.
- [x] Filter-empty state: `<div v-if="!isEmpty && !loading && filteredTenants.length === 0 && isFiltered" class="filter-empty">` với nút Reset filter.
- [x] Empty state: `<div v-else class="empty-state">` với CTA "Tạo tenant đầu tiên" khi mock list rỗng.
- [x] Error state: `<div v-if="error" class="error-state">` với nút "Thử lại" gọi `loadTenants` lại. Mock client không throw cho list nhưng path tồn tại đầy đủ trong source.
- [~] Test thực sự throw error trên list path: SKIP - mock client `listTenants` không có hook fail. Để test thực, Lead/owner có thể tạm chuyển `mode=real` với baseUrl invalid trong env.
- [x] Metric tổng quan: `activeCount` (Active=1: aurora), `verifiedDomains` (verified=1: aurora), `suspendedCount` (Suspended=1: nova), `supportCount` (Draft+failed=2: river+nova).
- [x] Footer 2 card: Conflict 409 + Wizard CTA, render trong `<div class="footer-grid">`.

## 5. C. Tenant Detail

- [x] Source `ClinicDetailPage.vue` transform 200 (27627B), chứa `tenantClient.getTenant`, `loadTenant`, dt `Tenant ID`, `Module đang bật`, button `Quay lại danh sách`.
- [x] Drawer overlay: `TenantDetailDrawer.vue` source 26930B chứa `Teleport` to body, `drawer-backdrop`, `aria-modal`, `handleEscape` listener (Escape close).
- [x] Click row trong `TenantTable.vue` emit `select` → `ClinicsPage` gọi `selectTenant(tenantId)` → drawer mở.
- [x] Drawer chứa link `<RouterLink :to="`/clinics/${tenant.id}`">` để mở full page detail (id full `tenant-{slug}` không 404).
- [x] Detail page hiển thị: Tenant ID, Slug, Plan, Phòng khám (clinicName), Email owner, Số điện thoại, Địa chỉ, Domain list, Module list (StatusPill).
- [x] `nextStatus` computed: nếu tenant Active → next "Suspended", ngược lại → "Active". Button label đổi tương ứng (`Active hóa tenant` / `Suspend tenant`).
- [x] Not-found: mock client `getTenant` throw `"Tenant not found."` khi id không match (test với id `tenant-does-not-exist` hoặc slug `aurora-dental` thay vì id `tenant-aurora-dental`). Detail page bắt và set `error.value` → render error banner + "Thử lại".
- [~] Visual confirm rendered DOM cho từng tenant id: SKIP env không có browser. Source compile + mock contract khớp đảm bảo path đúng.

## 6. D. Create Tenant Wizard (`/clinics/create`)

- [x] Source `CreateTenantWizard.vue` transform 200 (53094B). Source `CreateClinicPage.vue` (10495B) wrap wizard, gọi `tenantClient.createTenant` với conflict catch.
- [x] 4 step labels: `["Phòng khám", "Plan & Module", "Domain", "Xác nhận"]` literal trong source serve.
- [x] `fieldToStep` mapping: `slug: 0, contactEmail: 0, defaultDomainName: 2`. Đúng theo Figma.
- [x] `focusFirstConflictField` set `activeStep` rồi `nextTick` focus đúng `slugInput` / `domainInput` / `emailInput` theo first conflict field.
- [x] `watch(() => props.conflict, ...)` tự động trigger `focusFirstConflictField` khi parent đẩy 409 xuống.
- [x] Form data mặc định = aurora-dental (slug + domain + email trùng mock) → submit raw sẽ trigger 409 cho cả 3 field.
- [x] Required validation HTML5: 8 input chính có attribute `required` (displayName, clinicName, slug, specialty, contactEmail, phoneNumber, addressLine, defaultDomainName).
- [x] Form submit `<form @submit.prevent="submit">` emit payload `{ ...form, moduleCodes: [...form.moduleCodes] }` lên parent.
- [x] `setPlan` đổi modules theo plan (starter→[website], growth→[website,booking,catalog], premium→full 5).
- [x] `toggleModule` add/remove module trong `form.moduleCodes`.
- [x] `ConflictState.vue` source 8118B chứa `ApiConflictError`, `HTTP 409`, `fieldLabel` map sang VN (Slug tenant, Domain mặc định, Email owner) và liệt kê `conflict.fields`.
- [x] `CreateClinicPage` lưu conflict vào `conflict.value` khi catch error, không clear form data.
- [~] Test thực sự click submit để observe runtime state: SKIP env không có browser. Logic compile được + path đầy đủ → mock 409 path hợp lệ. Để test runtime, mở browser → `/clinics/create` → bấm "Tạo tenant" tới step 4 → bấm "Tạo tenant" submit → quan sát ConflictState banner xuất hiện và step jump về step 1 với slug/email focus.

### Gap Phát Hiện (D): Step Validation Không Gate

Hàm `next()` trong `CreateTenantWizard.vue` không kiểm tra required validation trước khi sang step kế:

```ts
function next() {
  activeStep.value = Math.min(activeStep.value + 1, steps.length - 1);
}
```

Nút "Tiếp tục" là `<AppButton @click="next">` (không phải `type="submit"`), nên user có thể nhảy qua step 1 → 2 → 3 → 4 dù field bắt buộc rỗng. HTML5 `required` chỉ chặn ở step 4 khi `<form @submit>` được kích hoạt.

Tác động: nếu user clear field bắt buộc rồi bấm "Tiếp tục", wizard vẫn cho qua. Validation cuối cùng vẫn chặn submit, nhưng UX không sạch theo checklist QA item D.2.

Đánh giá: lỗi nhỏ về validation gating. KHÔNG fix tự ý theo hard rule. Đã report cho Lead trong handoff.

Cách fix gợi ý (Lead duyệt thì Frontend Agent thực hiện):

```ts
function isStepValid(step: number): boolean {
  if (step === 0) {
    return Boolean(form.displayName && form.clinicName && form.slug && form.specialty && form.contactEmail && form.phoneNumber && form.addressLine);
  }
  if (step === 1) {
    return form.moduleCodes.length > 0;
  }
  if (step === 2) {
    return Boolean(form.defaultDomainName);
  }
  return true;
}

function next() {
  if (!isStepValid(activeStep.value)) {
    return;
  }
  activeStep.value = Math.min(activeStep.value + 1, steps.length - 1);
}
```

Hoặc dùng `<AppButton :disabled="!isStepValid(activeStep) || submitting">` cho nút "Tiếp tục".

## 7. E. Build / Type Safety

- [x] `npm run typecheck` (frontend workspace) PASS toàn 3 app: `clinic-admin`, `owner-admin`, `public-web`.
- [x] `npm run build` (frontend workspace) PASS toàn 3 app:
  - clinic-admin: `dist/index.js` 62.23 kB (gzip 24.98 kB), CSS 0.83 kB
  - owner-admin: `dist/index.js` 141.36 kB (gzip 50.97 kB), CSS 22.78 kB
  - public-web: `dist/index.js` 62.56 kB (gzip 25.10 kB), CSS 1.42 kB
- [~] `npm run lint`: SKIP (script không tồn tại trong root `frontend/package.json`). Đúng theo guard QA - không fail task.

Command + kết quả tóm tắt:

```txt
cd frontend && npm run typecheck    -> PASS x3
cd frontend && npm run build        -> PASS x3, không error/warning chặn
cd frontend && npm run lint         -> SCRIPT NOT FOUND (skip theo guard)
```

## 8. F. API Mode

- [x] `services/tenantClient.ts` đọc 3 env: `VITE_API_BASE_URL`, `VITE_TENANT_API_MODE`, `VITE_TENANT_API_FALLBACK`. Mặc định `mode='auto'` + `fallbackToMock=true`.
- [x] Không có file `.env*` trong `frontend/apps/owner-admin/` → `import.meta.env.VITE_API_BASE_URL` = undefined → resolved sang mock client (đúng path mock fallback).
- [x] `createTenantClient` trong `packages/api-client/src/tenantClient.ts`:
  - mode='mock' → mock
  - mode='auto' + không baseUrl → mock
  - mode='real' + không baseUrl → throw `"Tenant API baseUrl is required in real mode."`
  - mode='real' với baseUrl → http client + optional fallback
- [x] `normalizeConflict(error)` đảm bảo HTTP 409 từ backend chuẩn hóa thành `ApiConflictError` để wizard UI bắt và hiển thị `ConflictState`.
- [x] Mock client serve qua Vite alias `@id/@clinic-saas/api-client` → re-export `mockTenantClient.ts`. Source 16023B chứa 3 tenant + `createConflict` + `conflictFields.push("slug" | "defaultDomainName" | "contactEmail")`.
- [x] Real API smoke (GET /api/tenants, POST /api/tenants, GET /api/tenants/{id}, PATCH /api/tenants/{id}/status, duplicate slug 409): PASS theo HTTP layer (5/5 status code đúng) qua chain `localhost:5175` (Vite proxy) → `127.0.0.1:5005` (SSH tunnel) → gateway server test do owner cung cấp. Round 1 phát hiện 2 contract mismatch — Round 2 sau khi Frontend Agent áp dụng adapter layer thì cả HTTP lẫn contract đều PASS 5/5. Xem section "F-real Round 2 Smoke 2026-05-10".

### F-real Smoke 2026-05-10

Base URL smoke: `http://localhost:5175` (đi qua Vite dev proxy, KHÔNG gọi trực tiếp `127.0.0.1:5005`). Env real mode đã set qua `frontend/apps/owner-admin/.env.local` (`VITE_TENANT_API_MODE=real`, `VITE_API_BASE_URL=http://localhost:5175`, `VITE_TENANT_API_FALLBACK=false`).

| Case | Method + Path | Status | Notes |
|---|---|---|---|
| F.1 | GET /api/tenants | 200, 939B | items count = 3, item[0] có field id, slug, status |
| F.2 | GET /api/tenants/{id} | 200, 1279B | có id, slug, displayName, status, domains(array). KHÔNG có moduleCodes (backend trả `modules` array of object) |
| F.3 | POST /api/tenants (slug `qa-fe-smoke-<ts>`) | 201, 940B | id format = GUID 36-char, slug khớp request |
| F.4 | POST /api/tenants (duplicate) | 409, application/problem+json | Body keys = type,title,status,detail,traceId. KHÔNG có `fields` array |
| F.5 | PATCH /api/tenants/{id}/status | 200 (Active) + 200 (Suspended) | response.status field khớp body request cho cả 2 transition |

Tenant đã tạo qua F.3 (cần cleanup): id GUID 36-char, slug `qa-fe-smoke-1778382871`, hiện ở trạng thái `Suspended` sau F.5.

#### Gap FE-Backend Contract Mismatch (Critical cho Phase 3)

**Gap 1 — F.2 / F.3 detail response shape không khớp `TenantDetail` TS type:**

| Field FE expect (`shared-types/src/tenant.ts`) | Backend trả thực tế |
|---|---|
| `moduleCodes: TenantModuleCode[]` | `modules: Array<{ moduleCode, isEnabled, sourcePlanCode, createdAtUtc, updatedAtUtc }>` |
| `clinicName, contactEmail, phoneNumber, addressLine, specialty` (flat) | nested trong `profile: { clinicName, contactEmail, phoneNumber, addressLine, specialty }` |
| `defaultDomainName, domainStatus` (flat trên summary) | suy ra từ `domains[]` (object có `domainName, status, isPrimary`) |
| `createdAt: string` | `createdAtUtc: string` |
| `ownerName: string` | không có trong response |
| `planDisplayName, planCode, status, displayName, slug, id` | Trùng (ok) |

Tác động: `ClinicDetailPage.vue`, `TenantDetailDrawer.vue`, `TenantTable.vue` đang đọc `tenant.moduleCodes`, `tenant.clinicName`, `tenant.contactEmail`, `tenant.phoneNumber`, `tenant.addressLine`, `tenant.specialty`, `tenant.defaultDomainName`, `tenant.domainStatus`, `tenant.createdAt` — tất cả sẽ undefined trên real mode → render rỗng hoặc lỗi runtime.

**Gap 2 — F.4 conflict body không có `fields` array:**

FE expect `ApiConflictError = { status: 409, message: string, fields: TenantConflictField[] }`. Backend trả ProblemDetails RFC 9457:

```json
{"type":"...","title":"Tenant conflict","status":409,"detail":"Tenant slug or domain already exists.","traceId":"..."}
```

Tác động: `normalizeConflict` trong `tenantClient.ts` set `fields: payload?.fields ?? []` → mảng rỗng. `CreateTenantWizard.focusFirstConflictField` không có gì để focus → wizard không jump về step 1 và highlight đúng input. UX conflict rộng (slug/email/domain) không hoạt động — chỉ banner generic xuất hiện.

Đề xuất Lead phân xử:

1. Backend bổ sung field `fields: ["slug" | "defaultDomainName" | "contactEmail"]` vào ProblemDetails 409 (best fix), hoặc
2. FE thêm adapter trong `tenantClient.ts`/`mockTenantClient.ts` để map `modules[].moduleCode → moduleCodes`, flatten `profile`, và parse `detail` string từ ProblemDetails để suy ra fields, hoặc
3. Cập nhật `shared-types/src/tenant.ts` để khớp wire format backend (breaking cho mock + UI nhưng ít lệch contract dài hạn).

QA chỉ report; không tự sửa source theo hard rule.

### F-real Round 2 Smoke 2026-05-10

Round 2 chạy sau khi Frontend Agent đã apply adapter layer (`tenantAdapter.ts` mới + `tenantClient.ts` cập nhật `normalizeConflict` + `shared-types/src/tenant.ts` đặt `ownerName?` optional + UI component fallback). KHÔNG sửa backend, KHÔNG sửa Figma.

Base URL smoke: `http://localhost:5175` (Vite dev proxy → SSH tunnel `127.0.0.1:5005` → gateway server test do owner cung cấp). HMR đã apply adapter trước khi chạy lại smoke.

| Case | Method + Path | Status | Wire shape check | Result |
|---|---|---|---|---|
| F.1 | GET /api/tenants | 200 | Top keys = items, total, limit, offset; items là array; count = 5; total = 5; limit = 50; offset = 0; first_id_present = true | PASS |
| F.2 | GET /api/tenants/{id} (id từ F.1) | 200 | Top keys = activatedAtUtc, archivedAtUtc, createdAtUtc, displayName, domains, id, modules, planCode, planDisplayName, profile, slug, status, suspendedAtUtc, updatedAtUtc; có `profile`, `domains`(1), `modules`(1), `createdAtUtc` đúng nested | PASS |
| F.3 | POST /api/tenants (slug `qa-fe-smoke-r2-1778383645`) | 201 | id GUID `87ed44d4-86d3-497c-8c99-71d8a004de80`, slug khớp request | PASS |
| F.4 | POST /api/tenants (duplicate cùng payload) | 409, `application/problem+json` | Body keys = type,title,status,detail,traceId; KHÔNG có `fields` (expected); detail = `"Tenant slug or domain already exists."` chứa keyword "slug" + "domain" → regex parser FE infer `["slug","defaultDomainName"]` | PASS |
| F.5a | PATCH /api/tenants/{id}/status `{status:"Active"}` | 200 | response.status = "Active" | PASS |
| F.5b | PATCH /api/tenants/{id}/status `{status:"Suspended"}` | 200 | response.status = "Suspended" | PASS |

Bonus UI shell smoke (qua Vite SPA fallback, không mở browser):

| URL | Status | Có `id="app"` | Có `/src/main.ts` | Result |
|---|---|---|---|---|
| GET /clinics | 200 | true | true | PASS |
| GET /clinics/87ed44d4-86d3-497c-8c99-71d8a004de80 (id thật từ F.3) | 200 | true | true | PASS |

So sánh Round 1 vs Round 2:

| Layer | Round 1 (2026-05-10 sáng) | Round 2 (2026-05-10 chiều, sau adapter) |
|---|---|---|
| HTTP status code 5/5 | PASS | PASS |
| Contract `TenantDetail` flat (clinicName/contactEmail/specialty/defaultDomainName/domainStatus/moduleCodes/createdAt) | FAIL — wire shape nested + `modules`/`createdAtUtc` không khớp TS type | PASS — `adaptTenantDetail` flatten `profile`, pick primary domain, normalize domain status, map `modules[].moduleCode → moduleCodes`, alias `createdAtUtc → createdAt` |
| List envelope `{items,total,limit,offset}` unwrap | FAIL — UI assume mảng phẳng | PASS — `adaptTenantListResponse` unwrap |
| Conflict 409 `fields` array để wizard focus đúng input | FAIL — backend trả ProblemDetails không có `fields` | PASS — `normalizeConflict` parse `detail` text qua regex `/slug/i`, `/domain/i`, `/email\|contact/i` → infer `TenantConflictField[]` |
| `ownerName` undefined trên detail real | FAIL — TS type required, UI render `undefined` | PASS — `ownerName?` optional + drawer ẩn dòng "Chủ sở hữu" khi undefined |

Tenant đã tạo Round 2 (cần cleanup): id `87ed44d4-86d3-497c-8c99-71d8a004de80`, slug `qa-fe-smoke-r2-1778383645`, hiện `Suspended` sau F.5b. Tenant Round 1 (`qa-fe-smoke-1778382871`) cũng vẫn còn trên gateway nếu dataset không reset.

#### Smoke thực hiện ra sao

Pattern PowerShell tóm tắt (không in tenant data):

```powershell
$base = "http://localhost:5175"
$ts = [int][double]::Parse((Get-Date -UFormat %s))
$slug = "qa-fe-smoke-$ts"
$body = @{ slug=$slug; displayName="QA FE Smoke $ts"; clinicName="QA FE Smoke Clinic"; planCode="starter"; specialty="general"; contactEmail="qa-fe-$ts@example.local"; phoneNumber="0900000099"; addressLine="QA FE smoke address"; defaultDomainName="qa-fe-smoke-$ts.local"; moduleCodes=@("website") } | ConvertTo-Json -Compress
$h = @{ "Content-Type"="application/json"; "X-Owner-Role"="OwnerSuperAdmin" }
Invoke-WebRequest "$base/api/tenants" -UseBasicParsing                         # F.1
Invoke-WebRequest "$base/api/tenants/{id}" -UseBasicParsing                    # F.2
Invoke-WebRequest "$base/api/tenants" -Method POST -Headers $h -Body $body     # F.3
Invoke-WebRequest "$base/api/tenants" -Method POST -Headers $h -Body $body     # F.4 (409)
Invoke-WebRequest "$base/api/tenants/{id}/status" -Method PATCH -Headers $h -Body '{"status":"Active"}'    # F.5a
Invoke-WebRequest "$base/api/tenants/{id}/status" -Method PATCH -Headers $h -Body '{"status":"Suspended"}' # F.5b
```

Hướng dẫn smoke real API khi env sẵn sàng (làm sau, ngoài scope smoke hiện tại):

```powershell
# 1. Tạo .env.local trong frontend/apps/owner-admin/ với:
#    VITE_API_BASE_URL=https://<backend-host>
#    VITE_TENANT_API_MODE=real
#    VITE_TENANT_API_FALLBACK=false
# 2. Restart Vite: dừng process cũ, chạy lại npm run dev:owner.
# 3. Mở browser tới http://localhost:5175/clinics:
#    - DevTools Network tab phải show GET /api/tenants (không phải mock localhost).
#    - Response phải là JSON từ backend, list 3 tenant đã seed (Aurora/River/Nova).
# 4. Click "Tạo tenant" → submit form aurora-dental raw:
#    - DevTools phải show POST /api/tenants với 409 response.
#    - ConflictState banner hiển thị, wizard jump step 1, focus slug input.
# 5. Đổi slug + email + domain unique, submit lại → POST 200/201, redirect về /clinics, list refresh.
# 6. Click row tenant mới → drawer mở qua GET /api/tenants/{id}.
# 7. Click "Suspend tenant" → PATCH /api/tenants/{id}/status với { status: "Suspended" }, drawer cập nhật.
```

## 9. Conclusion

Mock fallback smoke A-D + E build/typecheck PASS đầy đủ. F real API smoke Round 2 (2026-05-10) đạt 5/5 PASS ở cả 2 layer: HTTP status (200/201/409/200x2) **và** contract mapping sau khi Frontend Agent áp dụng adapter layer (`tenantAdapter.ts`, `normalizeConflict` parse ProblemDetails, `ownerName` optional).

Trạng thái gap:

1. (Cũ, minor, vẫn open) Wizard `next()` không gate required validation trước khi sang step kế. Không chặn smoke; có thể fix follow-up.
2. (Round 1 critical) Detail response shape không khớp `TenantDetail` TS type — Round 2 RESOLVED qua `adaptTenantDetail` flatten `profile`, map `modules[].moduleCode → moduleCodes`, alias `createdAtUtc → createdAt`, normalize domain status, pick primary domain.
3. (Round 1 critical) Conflict 409 thiếu `fields` array — Round 2 RESOLVED qua `normalizeConflict` parse `detail` ProblemDetails bằng regex `/slug/i`, `/domain/i`, `/email|contact/i`. Detail thực tế từ smoke có "slug" + "domain" → focusFirstConflictField hoạt động được.

Không phát hiện gap mới ở Round 2.

QA đề xuất chuyển trạng thái FE Phase 3 sang **"Ready For Commit"** dựa trên: (a) smoke A-D mock PASS, (b) E build/typecheck PASS, (c) F-real Round 2 cả HTTP + contract đều PASS 5/5, (d) bonus UI shell SPA fallback PASS cho cả `/clinics` và `/clinics/{id}` thật. Lead phân xử có commit hay không (QA không tự commit theo hard rule).

## 10. File Liên Quan

QA đã tham chiếu/đọc các file sau (không sửa source code FE/Backend):

```txt
docs/current-task.md
docs/current-task.frontend.md
temp/plan.frontend.md
docs/agents/qa-agent.md
AGENTS.md
CLAUDE.md
frontend/package.json
frontend/apps/owner-admin/src/main.ts
frontend/apps/owner-admin/src/App.vue
frontend/apps/owner-admin/src/router/index.ts
frontend/apps/owner-admin/src/services/tenantClient.ts
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
frontend/packages/api-client/src/index.ts
frontend/packages/api-client/src/httpClient.ts
frontend/packages/api-client/src/tenantClient.ts
frontend/packages/api-client/src/mockTenantClient.ts
frontend/packages/api-client/src/tenantAdapter.ts (round 2, đọc adapter)
frontend/packages/shared-types/src/tenant.ts (round 2, verify ownerName? optional)
```

QA tạo duy nhất 1 file: `docs/testing/owner-admin-tenant-slice-smoke.md` (file này).
