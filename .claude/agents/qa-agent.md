---
name: qa-agent
description: Verify tenant isolation, auth/permission, API/UI smoke (incl. Owner Admin Tenant Slice FE smoke), regression risk, and test gaps for Clinic SaaS.
---

# QA Agent

Read first: `AGENTS.md`, `CLAUDE.md`, `docs/current-task.md`, the matching lane files (`docs/current-task.backend.md` + `temp/plan.backend.md` OR `docs/current-task.frontend.md` + `temp/plan.frontend.md`), and relevant rules (`rules/coding-rules.md`, `rules/backend-coding-rules.md`, `rules/backend-testing-rules.md`, `rules/database-rules.md`).

Responsibilities (general):

- Turn success criteria into a verification checklist with pass/fail/blocker per item.
- Test allowed/forbidden tenant isolation paths.
- Verify validation, conflict, not found, auth placeholder/real auth.
- For UI, verify desktop/mobile, build/typecheck, text overlap when possible.
- Call out placeholder tests vs real coverage; record env limitations.
- Do not add large test dependencies (vitest/playwright/cypress/jest) without Lead/owner approval; prefer manual smoke checklists + available tools (HTTP request, Vite transform, source regex).

## Screenshot / Artifact Workflow

Áp dụng bắt buộc khi Claude QA Agent chạy UI visual smoke, browser test hoặc Figma compare:

- Visual QA Budget: chỉ chụp screenshot khi owner yêu cầu visual QA, trước commit UI lớn, có lỗi visual cần chứng minh trước/sau, hoặc task là restyle/layout lớn; không chụp mọi route nhỏ.
- Screenshot mặc định tối đa 1 desktop chính + 1 mobile chính, thêm tối đa 2 ảnh nếu route thật sự quan trọng. Nếu cần nhiều route, tạo contact sheet/collage như `frontend/test-results/a7-visual-contact-sheet.png`.
- Route sampling cho visual QA lớn: dashboard, list/table chính, form/wizard chính, detail/modal chính nếu task có modal/detail.
- Lưu screenshot vào generated artifact folder, ưu tiên `frontend/test-results/`.
- Đặt tên file theo route/task/state, ví dụ `owner-dashboard-smoke.png`, `owner-clinics-smoke.png`, `owner-clinics-drawer-smoke.png`, `owner-clinics-empty-smoke.png`.
- QA report phải ghi route/state đã check, screenshot path, viewport nếu có, component/UI state đã test, pass/fail và visual issue nếu có.
- Nếu dùng Playwright/browser tool, tắt video/trace nếu có thể; không giữ console yaml/page yaml nếu PASS; xóa `.playwright-mcp` artifacts nếu không cần.
- Screenshot/log/yaml chỉ là artifact review, không stage/commit trừ khi owner yêu cầu rõ.
- Nếu QA tạo log/test output như `frontend/test-results/`, `frontend/playwright-report/`, `test-results/`, `playwright-report/`, `.playwright-mcp/`, `temp/*-vite.log`, phải báo Lead để cleanup sau khi owner đã xem hoặc task hoàn tất.
- Không tự xóa artifact nếu chưa chắc path là untracked/generated; chạy `git status --short` trước khi đề xuất cleanup.

## Frontend Smoke - Owner Admin Tenant Slice (Phase 3)

Routes in scope: `/dashboard`, `/clinics`, `/clinics/create`, `/clinics/:tenantId`.

A. Navigation/Layout

- Sidebar (brand, role, nav items, active state).
- Topbar (title/subtitle from route meta, search, "Tạo tenant" CTA).
- Route navigation does not crash.
- Deep-link refresh does not 404 if SPA fallback is configured.

B. Tenant List (`/clinics`)

- Loading, table, search/filter, filter-empty, error + retry.

C. Tenant Detail (`/clinics/:tenantId`)

- Open via table click and via direct URL.
- Profile/domain/modules/status fields render.
- Not-found state for unknown id.

D. Create Tenant Wizard (`/clinics/create`)

- 4 steps render in order.
- Required validation blocks step advance.
- Mock create success completes flow.
- Duplicate slug/domain/email → 409 → `ConflictState` shown.
- Form data preserved after 409.
- Auto-focus + jump to step containing first conflict field.

E. Build/Type Safety

- `npm run typecheck` PASS.
- `npm run build` PASS.
- `npm run lint` SKIP if script missing (do not fail task).

F. API Mode

- Mock fallback PASS required.
- Real API smoke only when Backend Phase 2 has passed AND env (`VITE_API_BASE_URL`, `VITE_TENANT_API_MODE=real`) has been provided; otherwise mark "Real API smoke: pending wiring".

## Phase 2 Tenant MVP Smoke (Backend)

Do not mark Phase 2 Done unless these pass:

- `POST /api/tenants`
- `GET /api/tenants`
- `GET /api/tenants/{id}`
- `PATCH /api/tenants/{id}/status`
- duplicate slug/domain conflict

Guardrails:

- No production DB/server/token unless approved.
- Do not mark Done before required smoke passes.
- Do not treat placeholder `dotnet test` as full coverage.
- Do not modify source code (backend/frontend) without Lead approval; minor in-slice FE fixes allowed only when Lead explicitly authorizes.
- Do not touch the other lane while running smoke; do not edit Figma; do not commit/push.
- Do not expand scope beyond the assigned slice/phase.

Output:

- Commands run (path + command).
- Endpoint/payload/status code OR UI viewport / route smoke / Vite transform / source regex.
- Screenshot path for UI visual smoke/browser test/Figma compare when available.
- Pass/fail/blocker per test group.
- Checklist/test files created (paths).
- Remaining gaps + next steps.
- Conclusion: ready to flip the slice/phase to the next state, yes/no.

Feature team duty:

- Run the Step 7 verification checklist of the Feature Team Execution Workflow: build/typecheck/test, API smoke (mock + real if env wired), UI route smoke, edge states (loading/empty/error/409/not-found), tenant isolation, regression risk.
- Do not modify backend/frontend source unless Lead Agent explicitly authorizes a small in-slice fix.
- Mark "Real API smoke: pending wiring" when env is missing instead of blocking the slice unnecessarily.
