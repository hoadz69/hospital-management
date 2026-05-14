# Auto Agent Workflow Handoff - Clinic SaaS

Ngay cap nhat: 2026-05-14

File nay la handoff that cho co che Agent Queue / Runner. Muc tieu la de tiep tuc o may khac ma khong phai doc lai log dai.

## 1. Yeu Cau Goc Cua Owner

Owner khong can mot script chi sua markdown. Owner can co che:

- Tu doc trang thai project.
- Tu lap task tiep theo.
- Tu chay task that.
- Tu verify that.
- Tu dung khi gap blocker that.
- Co the tien toi lam UI/parse/Figma/product task, khong chi queue smoke.

## 2. Trang Thai That Hien Tai

Da co runner chay duoc queue task va da harden them ngay 2026-05-14, nhung **chua dat day du muc "tu dong lam moi thu product end-to-end"**.

Da dat:

- `scripts/agent-runner.ps1` doc `docs/agent-queue.md`, chon task `READY`, goi `codex exec`, chay verify whitelist, mark `DONE`/`BLOCKED`, ghi checkpoint.
- Co `-AutoPlan`, `-PlanOnly`, `-Continuous`, `-MaxCycles`.
- Co schema queue moi: `priority`, `blocker_type`, `auto_retry`, `attempts`, `max_attempts`.
- Co prompt planner `docs/prompts/AUTO-PLANNER.md`.
- Co task UI smoke that `FE-OWNER-ADMIN-UI-SMOKE`.
- Da tach UI route smoke ra `scripts/verify-owner-admin-ui-smoke.ps1` de runner bot nhiem task-specific logic.
- Da chay UI smoke that qua Vite dev server Owner Admin va PASS.
- Runner co timeout chong treo:
  - `-PlannerTimeoutSeconds` mac dinh `300`.
  - `-WorkerTimeoutSeconds` mac dinh `900`.
  - `-VerifyTimeoutSeconds` mac dinh `1800`.
- Runner co `executor: verify_only` de chay smoke/env recheck khong can `codex exec`.
- Da chay task `AUTO-RUNNER-SELF-CHECK` bang `verify_only`; runner skip worker, verify `git diff --check` PASS, queue mark `DONE`, checkpoint ghi vao `docs/current-task.md`.
- Da test `-AutoPlan -Once -PlannerTimeoutSeconds 1`: planner timeout, runner tu reactivate fallback `AUTO-RUNNER-SELF-CHECK`, chay verify-only, mark `DONE`; khong treo vo han.
- Da test worker path that: task `AUTO-RUNNER-CODEX-WORKER-SMOKE` chay qua `codex exec` trong khoang 79 giay, worker them checkpoint vao `docs/current-task.md`, outer verify `git diff --check` PASS, queue mark `DONE`.
- Da them deterministic product planner trong runner: khi AI planner timeout/fail/khong sua queue, runner doc source de phat hien `/clinics/:tenantId` lifecycle con mock/local timer, FE client co `updateTenantStatus`, backend/gateway co status endpoint, roi tao task `FE-TENANT-LIFECYCLE-API-WIRING`.
- Da chay runner that cho task `FE-TENANT-LIFECYCLE-API-WIRING`: worker goi `codex exec`, sua lifecycle modal sang Tenant status API, outer verify `git diff --check`, `npm run typecheck`, `npm run build` PASS, queue mark `DONE`.
- Da them deterministic verify planner: khi khong con product rule an toan nhung `frontend/` dang dirty, runner tao `FE-DIRTY-VERIFY` dang `verify_only` va chay `git diff --check`, frontend `typecheck`, frontend `build`, `gitnexus detect-changes`.

Chua dat:

- Planner AI ben trong `codex exec` van gap loi shell sandbox `CreateProcessWithLogonW failed: 1326`, nen chua tu doc file/tac nghiep product on dinh.
- Worker Codex co the ket thuc thao tac file nhung khong thoat kip; runner da co timeout/kill tree de mark `BLOCKED` thay vi treo vo han.
- Da co mot task product frontend nho duoc runner deterministic planner tu sinh va worker lam xong. Chua phai he thong auto lam moi loai product task end-to-end; moi co 1 rule deterministic cu the cho Tenant lifecycle API wiring.
- Khi het product task an toan, runner se chay verify deterministic thay vi tiep tuc smoke docs cu; day la behavior dung de khong nhin nhu chet im nhung cung khong invent task ngoai scope.
- Chua co browser click/DOM visual test that bang Playwright/Cypress. Hien moi HTTP route smoke qua Vite dev server.
- Runner van dung verify whitelist; muon chay task moi phai them command verify vao whitelist hoac script rieng.

## 3. Verify That Da Chay

Task: `AUTO-RUNNER-SELF-CHECK`

Command runner:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\agent-runner.ps1 -Once
```

Ket qua:

```txt
executor verify_only: skip codex exec
git diff --check PASS
queue mark DONE
checkpoint ghi docs/current-task.md
```

Task: `AUTO-RUNNER-SELF-CHECK` qua fallback planner timeout

Command runner:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\agent-runner.ps1 -AutoPlan -Once -PlannerTimeoutSeconds 1
```

Ket qua:

```txt
auto planner timed out after 1 seconds
fallback self-check task reactivated
executor verify_only: skip codex exec
git diff --check PASS
queue mark DONE
```

Task: `AUTO-RUNNER-CODEX-WORKER-SMOKE`

Command runner:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\agent-runner.ps1 -Once -WorkerTimeoutSeconds 240
```

Ket qua:

```txt
codex exec ran for about 79 seconds
worker edited docs/current-task.md
git diff --check PASS
queue mark DONE
```

Task: `FE-TENANT-LIFECYCLE-API-WIRING`

Command runner:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\agent-runner.ps1 -AutoPlan -PlanOnly -PlannerTimeoutSeconds 1
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\agent-runner.ps1 -Once -WorkerTimeoutSeconds 420 -VerifyTimeoutSeconds 1800
```

Ket qua:

```txt
planner timeout -> deterministic product task created
codex worker edited frontend lifecycle files
git diff --check PASS
cd frontend && npm run typecheck PASS
cd frontend && npm run build PASS
queue mark DONE
```

Task: `FE-DIRTY-VERIFY`

Command runner:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\agent-runner.ps1 -AutoPlan -PlanOnly -PlannerTimeoutSeconds 1
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\agent-runner.ps1 -Once -VerifyTimeoutSeconds 1800
```

Ket qua:

```txt
planner timeout -> deterministic frontend verify task created
git diff --check PASS
cd frontend && npm run typecheck PASS
cd frontend && npm run build PASS
gitnexus detect-changes --scope all PASS/report medium risk
queue mark DONE
```

Task: `FE-OWNER-ADMIN-UI-SMOKE`

Command runner:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\agent-runner.ps1 -Once
```

Ket qua trong `temp/agent-runner/FE-OWNER-ADMIN-UI-SMOKE.verify.log` truoc khi cleanup:

```txt
git diff --check                         PASS
cd frontend && npm run typecheck          PASS
cd frontend && npm run build              PASS
frontend owner-admin route smoke          PASS
```

Route smoke Vite Owner Admin:

```txt
PASS /dashboard HTTP 200 + #app
PASS /clinics HTTP 200 + #app
PASS /clinics/create HTTP 200 + #app
PASS /clinics/tenant-aurora-dental HTTP 200 + #app
Owner Admin route smoke PASS.
```

Dev server port `5175` da duoc dung sau smoke.

## 4. File Chinh Trong Slice Nay

Runner/core:

- `scripts/agent-runner.ps1`
- `docs/agent-queue.md`
- `docs/agent-runner.md`

Prompts:

- `docs/prompts/AUTO-PLANNER.md`
- `docs/prompts/AUTO-RUNNER-V2-SMOKE.md`
- `docs/prompts/FE-OWNER-ADMIN-UI-SMOKE.md`
- `docs/prompts/TEMPLATE.backend.md`
- `docs/prompts/TEMPLATE.frontend.md`
- `docs/prompts/TEMPLATE.fullstack.md`
- `docs/prompts/TEMPLATE.docs.md`
- `docs/prompts/README.md`

Dashboards/checkpoints:

- `docs/current-task.md`
- `docs/current-task.frontend.md`
- `docs/current-task.backend.md`
- `docs/agent-playbook.md`
- `docs/codex-setup.md`
- `temp/auto-agent-workflow-plan.md`

Artifacts:

- `temp/agent-runner/**` la log verify/jsonl tam thoi. Khong commit/push.

## 5. Sai O Dau / No Can Sua Truoc Khi Dung Dai Han

1. Runner da bi sua trong luc test task.
   - Day la sai ky luat trong qua trinh lam.
   - Da giam no bang cach tach verify UI smoke ra `scripts/verify-owner-admin-ui-smoke.ps1`.
   - Van nen review lai runner core truoc khi dung cho product task dai.

2. Auto planner chua dang tin.
   - `AUTO-PLANNER` tren ly thuyet dung, nhung worker Codex bi sandbox shell error nen khong doc/sua file on dinh.
   - Fallback smoke chi chung minh pipeline, khong chung minh product planning.

3. Queue dang co task `DONE`.
   - Queue khong nen thanh history dai han.
   - Sau khi commit/push, nen chuyen task smoke Done vao summary va de queue cho active task that.

4. UI smoke con nong.
   - Hien chi check SPA shell `HTTP 200 + #app`.
   - Chua check DOM rendered content, user interaction, modal/drawer, screenshot, hay visual regression.

## 6. Buoc Tiep Theo O Nha

Nen lam theo thu tu:

1. Cleanup thiet ke runner:
   - Review `scripts/agent-runner.ps1` core.
   - Runner da co timeout/kill tree, `verify_only`, va deterministic product fallback cho lifecycle API wiring; tiep tuc giu verify command o script rieng khi co task-specific logic.
   - Cleanup queue chi giu active task.

2. Fix Codex planner shell issue:
   - Chay thu `codex exec` voi prompt doc file don gian.
   - Neu van `CreateProcessWithLogonW failed: 1326`, can sua cau hinh Codex/Windows sandbox truoc khi tin planner AI.

3. Chon mot product task nho that:
   - Vi du frontend task: route/component cu the, allowed_paths ro, verify bang typecheck/build + route/browser smoke.
   - Khong chon task backend runtime DB neu chua co env server/Docker san sang.

4. Nang UI verify:
   - Them script browser/DOM smoke neu co Playwright hoac dung tool browser san co.
   - Check rendered text/controls, khong chi `#app`.

## 7. Command Huu Ich

Dry run queue:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\agent-runner.ps1 -DryRun
```

Auto plan dry-run:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\agent-runner.ps1 -AutoPlan -DryRun
```

Run one task:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\agent-runner.ps1 -Once
```

Run bounded auto:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\agent-runner.ps1 -AutoPlan -Continuous -MaxCycles 2
```

## 8. Ket Luan

Trang thai nen hieu dung:

- **Runner prototype co chay duoc task UI smoke that.**
- **Chua phai he thong auto lam product end-to-end.**
- **Can on dinh runner va tach verify scripts truoc khi dung de lam task that tiep theo.**
