# FE-TENANT-LIFECYCLE-API-WIRING

Lead Agent: implement frontend-only tenant lifecycle API wiring cho Owner Admin.

## Mode

- Lane frontend.
- Khong commit, push, stage.
- Khong sua backend, database, infrastructure, Figma, screenshot, hoac generated artifact.
- Chi lam trong Allowed Files.

## Read

- `AGENTS.md`
- `docs/current-task.md`
- `docs/current-task.frontend.md`
- `temp/plan.frontend.md`
- `rules/coding-rules.md`
- Source frontend lien quan trong Allowed Files.

## Context

`/clinics/:tenantId` da co `TenantLifecycleConfirmModal`, nhung `ClinicDetailPage.vue` van dung `lifecycleTimer = window.setTimeout(...)`, local status mutation, va audit metadata `mock-local`. Shared API client da co `tenantClient.updateTenantStatus(tenantId, { status })`, backend/gateway da co `PATCH /api/tenants/{tenantId}/status`.

## Scope

1. Sua `frontend/apps/owner-admin/src/pages/ClinicDetailPage.vue` de `confirmLifecycle(action, reason)` goi `tenantClient.updateTenantStatus(tenant.value.id, { status })` thay cho local timer/mock status mutation.
2. Giu behavior modal: loading, success, error, tu dong dong sau success, reason text trong audit event local.
3. Dung `TenantDetail` tra ve tu `updateTenantStatus` lam source of truth cho `tenant.value`.
4. Doi copy va audit metadata trong lifecycle path tu mock/local sang API-first. Khong gui reason len backend vi contract hien tai chi ho tro `{ status }`.
5. Chi xoa lifecycle timer cleanup neu no thanh dead code. Khong dong vao audit/domain timers neu khong can cho typecheck.
6. Sua copy trong `TenantLifecycleConfirmModal.vue` neu con noi "mock lifecycle" hoac "Khong goi API lifecycle moi", de phu hop hanh vi status update qua API ma khong claim audit persistence.
7. Neu verify pass, cap nhat ngan `docs/current-task.frontend.md` va `temp/plan.frontend.md`.

## Allowed Files

- `frontend/apps/owner-admin/src/pages/ClinicDetailPage.vue`
- `frontend/apps/owner-admin/src/components/TenantLifecycleConfirmModal.vue`
- `docs/current-task.frontend.md`
- `temp/plan.frontend.md`

## Verify

Outer runner se chay:

```txt
git diff --check
cd frontend && npm run typecheck
cd frontend && npm run build
```

## Stop If

- `tenantClient.updateTenantStatus` thieu hoac khong goi `/api/tenants/{tenantId}/status`.
- Backend status route khong ton tai.
- Scope bat buoc persist lifecycle reason server-side. Scope hien tai chi la status API wiring.
- Can sua backend/API contract.

## Report

```txt
Lane:
Action:
Files changed:
Verify:
Skipped/blocker:
Dirty:
Next:
```
