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

Da co prototype runner, nhung **chua dat day du muc "tu dong lam moi thu"**.

Da dat:

- `scripts/agent-runner.ps1` doc `docs/agent-queue.md`, chon task `READY`, goi `codex exec`, chay verify whitelist, mark `DONE`/`BLOCKED`, ghi checkpoint.
- Co `-AutoPlan`, `-PlanOnly`, `-Continuous`, `-MaxCycles`.
- Co schema queue moi: `priority`, `blocker_type`, `auto_retry`, `attempts`, `max_attempts`.
- Co prompt planner `docs/prompts/AUTO-PLANNER.md`.
- Co task UI smoke that `FE-OWNER-ADMIN-UI-SMOKE`.
- Da tach UI route smoke ra `scripts/verify-owner-admin-ui-smoke.ps1` de runner bot nhiem task-specific logic.
- Da chay UI smoke that qua Vite dev server Owner Admin va PASS.

Chua dat:

- Planner AI ben trong `codex exec` van gap loi shell sandbox `CreateProcessWithLogonW failed: 1326`, nen chua tu doc file/tac nghiep product on dinh.
- Chua co task product implement that nao duoc runner tu sinh tu backlog/plan va tu lam end-to-end.
- Chua co browser click/DOM visual test that bang Playwright/Cypress. Hien moi HTTP route smoke qua Vite dev server.
- Runner dang bi nhiem task-specific verify command `frontend owner-admin route smoke`; ve thiet ke nen tach ra script rieng de runner loi co dinh.

## 3. Verify That Da Chay

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
   - Runner hien chi whitelist command goi script smoke, khong con embed script smoke dai trong switch.
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
