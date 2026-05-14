# Current Task - Project Coordination Dashboard

Ngay cap nhat: 2026-05-14

File nay la dashboard dieu phoi project. Khong ghi plan chi tiet cua mot lane vao day; chi tro sang `docs/current-task.backend.md`, `docs/current-task.frontend.md` va plan lane tuong ung.

## Backend Domain DNS/SSL API - 2026-05-13

Trang thai: **Implemented backend, runtime smoke pending**.

Ket qua ngan:
- Owner da go Domain blocker cho scope toi thieu de FE thay mock.
- Domain Service hien van scaffold/stub; backend implement trong Tenant Service vi da co `platform.tenant_domains`.
- API Gateway forward-only cho 3 endpoint, khong truy DB:
  - `GET /api/tenants/{tenantId}/domains`
  - `POST /api/tenants/{tenantId}/domains/{domainId}/dns-retry`
  - `GET /api/tenants/{tenantId}/domains/{domainId}/ssl-status`
- Response item co `domainId`, `domainName`, `dnsStatus`, `dnsRecords`, `lastCheckedAt`, `retryCount`, `nextRetryAt`, `sslStatus`, `sslIssuer`, `expiresAt`, `message`.
- Persistence implemented bang migration `0003_add_tenant_domain_dns_ssl_state.sql` + Dapper/Npgsql repository trong Tenant Service.
- Verify local: `git diff --check` PASS, restore/build/test PASS 35/35.
- Runtime smoke qua gateway chua chay duoc vi Docker daemon local offline va session khong co server smoke env; khong danh dau DB Done cho Domain DNS/SSL.
- QA/checkpoint 2026-05-14: rerun restore/build/test PASS 35/35; diff secret scan khong thay private key/token/connection string that. QA doc lap danh dau **FAIL push-gate** vi Docker/server smoke va migration 0003 runtime van pending.

Chi tiet lane: `docs/current-task.backend.md`, `temp/plan.backend.md`.

## Backend Owner Plan Persistence PASS - 2026-05-13

Trang thai: **PASS DB that**. Backend/DevOps da thay owner-plan contract stub bang PostgreSQL persistence trong Tenant Service va API Gateway forward-only.

Ket qua ngan:
- FE inventory: FE real client hien goi Tenant APIs va Owner Plan APIs.
- DB/migration: apply `0002_add_owner_plan_module_persistence.sql` tren server test; seed 3 plans, 12 modules, 36 entitlement rows; backfill 7 tenant assignments; audit table san sang.
- Runtime: publish lai Tenant Service va API Gateway len runtime smoke, PostgreSQL container van khong publish public port.
- API smoke qua gateway that: plans/modules/assignments 200; bulk-change 200 `accepted-db`; re-GET thay plan persisted khop DB; audit row tang 1.
- Negative: ClinicAdmin 403, invalid payload 400, missing assignment 404.

## Workstream Dang Chay Song Song

| Workstream | Task file | Plan file | Trang thai ngan | Buoc tiep theo |
|---|---|---|---|---|
| Backend/DevOps | `docs/current-task.backend.md` | `temp/plan.backend.md` | Tenant APIs DB that; Owner Plan APIs DB that; Domain DNS/SSL code+migration PASS local, runtime smoke pending | Apply migration 0003 va smoke 3 Domain endpoint qua gateway khi co Docker/server env |
| Frontend | `docs/current-task.frontend.md` | `temp/plan.frontend.md` | Phase 3 Owner Admin Tenant Slice va Wave A FE A5-A9/A6/A7 Done; Domain DNS/SSL panel da API-first wiring voi mock fallback | Runtime real smoke Domain DNS/SSL qua gateway khi backend env/migration 0003 san sang |
| DevOps | `temp/plan.devops.md` | `temp/plan.devops.md` | Server test/dev smoke la runtime chinh cho backend/DB/API smoke khi local Docker offline | Dung `deploy.local.ps1`/env local, khong ghi secret/IP/key vao repo/docs |
| Database | `docs/current-task.backend.md` | `temp/plan.backend.md` | Tenant Service PostgreSQL 0001 + owner-plan 0002 da apply server test; domain 0003 code ready, apply pending | Migration tiep theo phai idempotent, SQL-first, Dapper/Npgsql, khong EF migrations |

## Agent Queue / Auto Runner V2 - 2026-05-14

Trang thai: **Implemented + auto-plan smoke PASS**.

Ket qua ngan:
- Tao queue `docs/agent-queue.md`, prompt templates `docs/prompts/`, runner docs `docs/agent-runner.md`.
- Tao PowerShell runner `scripts/agent-runner.ps1` co `-DryRun`, `-Once`, `-MaxTasks`, `-AutoPlan`, `-PlanOnly`, `-Continuous`, `-MaxCycles`.
- Runner V2 co planner prompt `docs/prompts/AUTO-PLANNER.md` va fallback deterministic smoke task khi Codex planner bi sandbox/shell loi.
- Runner da chay `codex exec` that cho 2 task docs-only tam thoi; ca 2 task tu update queue/checkpoint va verify `git diff --check` PASS.
- Auto-plan test mot lenh: `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/agent-runner.ps1 -AutoPlan -Once` PASS; runner goi planner, fallback tao `AUTO-RUNNER-V2-SMOKE`, worker chay task, outer verify `git diff --check` PASS, queue mark `DONE`.
- UI smoke thật: task `FE-OWNER-ADMIN-UI-SMOKE` chạy qua runner, `npm run typecheck` PASS, `npm run build` PASS, Vite Owner Admin route smoke PASS cho `/dashboard`, `/clinics`, `/clinics/create`, `/clinics/tenant-aurora-dental`.
- Runner hardening 2026-05-14: them timeout cho planner/worker/verify, kill process tree khi qua timeout, normalize queue EOF, them `executor: verify_only` cho smoke/env recheck khong can `codex exec`.
- Verify-only smoke: task `AUTO-RUNNER-SELF-CHECK` chay that, skip Codex worker, `git diff --check` PASS, queue mark `DONE`, checkpoint ghi ben duoi.
- Planner-timeout fallback: `-AutoPlan -Once -PlannerTimeoutSeconds 1` da timeout planner, runner tu reactivate `AUTO-RUNNER-SELF-CHECK`, chay verify-only va mark `DONE`; vong auto khong treo.
- Codex worker smoke: task `AUTO-RUNNER-CODEX-WORKER-SMOKE` chay qua `codex exec` that trong 79 giay, worker sua `docs/current-task.md`, runner verify `git diff --check` PASS va mark `DONE`.
- Deterministic product planner: khi AI planner timeout/fail/khong sua queue, runner co rule doc source de phat hien Tenant Lifecycle modal con mock/local timer + FE/BE da co Tenant status API, tu tao task `FE-TENANT-LIFECYCLE-API-WIRING`.
- Product worker smoke that: runner tao/chay `FE-TENANT-LIFECYCLE-API-WIRING`, Codex worker noi `/clinics/:tenantId` lifecycle confirm sang `tenantClient.updateTenantStatus`, outer verify `git diff --check`, `npm run typecheck`, `npm run build` PASS, queue mark `DONE`.
- Deterministic verify fallback: khi khong con product rule an toan nhung `frontend/` dang dirty, runner tao/chay `FE-DIRTY-VERIFY` dang `verify_only`; verify `git diff --check`, frontend `typecheck`, frontend `build`, `gitnexus detect-changes` PASS/report.
- Task BE-DOMAIN-0003-RUNTIME-SMOKE bi mark `DONE` sai da duoc go khoi queue; log runtime smoke cu chi la artifact va khong con dung lam trang thai.
- Queue hien khong co product implement task READY; task runtime/backend/frontend tiep theo phai do planner sinh tu dashboard/plan hoac owner them vao queue.

## Server Test Runtime Rule

- Runtime chinh cho backend/DB/API smoke la server test/dev smoke do owner cung cap qua `DEPLOY_HOST`, `DEPLOY_USER`, `SSH_KEY_PATH` trong shell/session hoac `deploy.local.ps1` da ignore.
- Neu local Windows thieu Docker/.NET hoac Docker daemon khong chay, khong coi do la blocker backend; chuyen sang SSH/SCP va chay PostgreSQL, Tenant Service, API Gateway, API smoke tren server test.
- FE real API smoke tro Vite proxy toi API Gateway that tren server test hoac qua SSH tunnel.
- Stub chi dung fallback cuoi cung cho contract path; khong dung de danh dau E2E Done khi server test chay duoc API that.
- Khong ghi private key, token, secret, IP server that hoac connection string that vao repo/docs/log.

## Agent Runner V2 Smoke - 2026-05-14

- Result: generated queue task executed by runner.
- Scope: docs-only smoke, no source runtime changes.
- Guardrail: no commit/push/stage.

## Guardrail Chung

- Khong sua source code neu task chi la docs/task management.
- Khong sua Figma neu owner khong yeu cau.
- Khong commit/stage/push neu owner chua yeu cau ro.
- Khong stage/commit screenshot/log/generated artifacts.
- Dashboard chi ghi tong quan; chi tiet active nam trong lane files.

## Agent Runner Checkpoint - 2026-05-14 16:49

- Task id: `AUTO-RUNNER-V2-SMOKE`
- Lane: `docs`
- Result: `DONE`
- Reason: none
- Log: artifact reviewed and cleaned from `temp/agent-runner/`
- Verify: PASS
- Guardrail: runner khong commit/push/stage.

## Agent Runner Codex Worker Smoke - 2026-05-14

- Result: codex worker task executed through runner.
- Scope: docs-only smoke, no runtime source changes.
- Guardrail: no commit/push/stage.

## Agent Runner Checkpoint - 2026-05-14 21:07

- Task id: `AUTO-RUNNER-V2-SMOKE`
- Lane: `docs`
- Result: `BLOCKED`
- Reason: Verify failed: git diff --check
- Log: artifact reviewed and cleaned from `temp/agent-runner/`
- Verify: Verify failed: git diff --check
- Guardrail: runner khong commit/push/stage.

## Agent Runner Checkpoint - 2026-05-14 21:16

- Task id: `AUTO-RUNNER-SELF-CHECK`
- Lane: `docs`
- Result: `DONE`
- Reason: none
- Log: `n/a`
- Verify: PASS
- Guardrail: runner khong commit/push/stage.

## Agent Runner Checkpoint - 2026-05-14 21:21

- Task id: `AUTO-RUNNER-SELF-CHECK`
- Lane: `docs`
- Result: `DONE`
- Reason: none
- Log: `n/a`
- Verify: PASS
- Guardrail: runner khong commit/push/stage.

## Agent Runner Checkpoint - 2026-05-14 21:37

- Task id: `AUTO-RUNNER-CODEX-WORKER-SMOKE`
- Lane: `docs`
- Result: `DONE`
- Reason: none
- Log: artifact reviewed and cleaned from `temp/agent-runner/`
- Verify: PASS
- Guardrail: runner khong commit/push/stage.

## Agent Runner Checkpoint - 2026-05-14 23:00

- Task id: `AUTO-RUNNER-V2-SMOKE`
- Lane: `docs`
- Result: `BLOCKED`
- Reason: Worker output indicates blocker in Skipped/blocker report.
- Log: `temp/agent-runner/AUTO-RUNNER-V2-SMOKE.jsonl`
- Verify: not run
- Guardrail: runner khong commit/push/stage.

## Agent Runner Checkpoint - 2026-05-14 23:00

- Task id: `AUTO-RUNNER-SELF-CHECK`
- Lane: `docs`
- Result: `DONE`
- Reason: none
- Log: `n/a`
- Verify: PASS
- Guardrail: runner khong commit/push/stage.
