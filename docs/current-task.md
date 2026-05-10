# Current Task - Project Coordination Dashboard

Ngày cập nhật: 2026-05-10

File này là dashboard điều phối project. Không ghi plan chi tiết của một lane vào đây.
Lead Agent chỉ dùng file này để tóm tắt trạng thái tổng quan và trỏ sang task/plan lane riêng.

Mọi feature mới phải chạy theo "Feature Team Execution Workflow" (Step 0–10) trong `docs/agent-playbook.md` và `AGENTS.md`. Lead Agent assemble team theo loại feature (UI / API / full-stack / deployment / data) và phân lane đúng.

## Workstream Đang Chạy Song Song

| Workstream | Task file | Plan file | Trạng thái ngắn | Bước tiếp theo |
|---|---|---|---|---|
| Backend/DevOps | `docs/current-task.backend.md` | `temp/plan.backend.md` | ✅ Phase 2 API Runtime Smoke Gate PASS. ✅ Pre-Phase 4 Hardening backend lane (P1.1 + P1.3 + **P1.2**) DONE: dotnet test 12 [Fact] PASS (3+3+6), build 0 warning 0 error. P1.2 ProblemDetails 409 attach `extensions.fields` từ PostgresException ConstraintName | Chờ owner duyệt commit backend lane. QA verify 409 fields end-to-end khi có Testcontainers (Phase 4 scope) |
| Frontend | `docs/current-task.frontend.md` | `temp/plan.frontend.md` | Phase 3 committed (`7f6366d`). ✅ Pre-Phase 4 Hardening frontend lane (P1.7 + **P1.6**) DONE: httpClient JSON.parse guard + CreateTenantWizard isStepValid gate. Bundle owner-admin 146.04 kB JS (+0.43), typecheck/build PASS x3 | Chờ owner duyệt commit frontend lane. Tech-debt P3: stepper number-click bypass gate (Phase 4) |
| DevOps | (dashboard này) | `temp/plan.devops.md` | ✅ Pre-Phase 4 Hardening DevOps lane (P1.4 + P1.8) DONE: docker compose config PASS (host_ip 127.0.0.1 cho 4 service), nginx scaffold 56 dòng (try_files + 4 forward header + proxy_pass resolver pattern). `nginx -t` pending Docker daemon | Chờ owner duyệt commit DevOps lane |
| Database | (dashboard này) | (note trong dashboard) | ✅ Pre-Phase 4 Hardening database lane (**P1.5**) DONE: `infrastructure/postgres/init.sql` thêm header drift warning + body khớp migration 0001 (cho phép thêm `CREATE SCHEMA tenant`). P2 candidate: chuyển sang DbUp/FluentMigrator runner Phase 4+ | Chờ owner duyệt commit database lane |
| Docs/Agent Workflow | (dashboard này) | `temp/plan.md` | ✅ Feature Team Execution Workflow committed (`74c29c8`). ✅ Pre-Phase 4 Hardening là feature team đầu tiên áp dụng workflow, implementation DONE | Update roadmap nếu Pre-Phase 4 Hardening cần dòng riêng |

## Rule Điều Phối

- `docs/current-task.md` chỉ do Lead Agent cập nhật dạng dashboard.
- Backend Agent và DevOps Agent chỉ cập nhật `docs/current-task.backend.md` và `temp/plan.backend.md`.
- Frontend Agent chỉ cập nhật `docs/current-task.frontend.md` và `temp/plan.frontend.md`.
- Không agent nào overwrite `docs/current-task.md` bằng task chi tiết của một lane.
- Nếu task mới chưa rõ thuộc lane nào, Lead Agent ghi một dòng ngắn ở phần Notes/Unclassified rồi phân lane sau.
- `temp/plan.md` là index tương thích cũ, không dùng làm plan chi tiết cho backend hoặc frontend.

## Notes / Unclassified

- Nội dung backend cũ từ `docs/current-task.md` đã được chuyển sang `docs/current-task.backend.md`.
- Nội dung frontend/UI cũ từ `docs/current-task.md` đã được chuyển sang `docs/current-task.frontend.md`.
- 2026-05-10: thêm "Feature Team Execution Workflow" vào `AGENTS.md`, `docs/agent-playbook.md`, `.claude/agents/*`, `docs/agents/*`, `agents/*`, và `docs/commands/*`. Workflow áp dụng cho cả Claude Code và Codex; Lead Agent giả lập role khi tool không có subagent runtime thật. Commit `74c29c8`.
- 2026-05-10 (sáng): feature team đầu tiên áp dụng workflow là "Pre-Phase 4 Hardening" wave 1 — 5 P1 quick wins (P1.1 test infra, P1.3 Swagger gating, P1.4 nginx SPA fallback, P1.7 httpClient JSON.parse, P1.8 docker-compose dev bind). Plan cross-lane đã ghi vào `temp/plan.devops.md` (mới), `temp/plan.backend.md` §12, `temp/plan.frontend.md` §15, index trong `temp/plan.md`. **IMPLEMENTATION DONE**.
- 2026-05-10 (chiều): Pre-Phase 4 Hardening **wave 2** — owner duyệt thêm 3 P1 medium từ Full Project Review: P1.2 ProblemDetails 409 attach `extensions.fields` (BE), P1.5 init.sql header drift warning + sync với migration 0001 (DB), P1.6 CreateTenantWizard isStepValid gate (FE). Feature team đã chạy đủ Step 0-7. Verify: dotnet build 0/0, dotnet test 12/12 [Fact] PASS, typecheck PASS x3, build PASS x3, docker compose config PASS. **WAVE 2 IMPLEMENTATION DONE**. Tổng 8/8 P1 hardening DONE, sẵn sàng commit split theo lane (Step 9). Owner chưa yêu cầu commit.
- Không có task chi tiết chưa phân loại tại thời điểm cập nhật này.

## Prompt Ngắn Từ Giờ

Backend/DevOps:

```txt
Backend Agent: tiếp tục Phase 2 API Runtime Smoke Gate theo docs/current-task.backend.md và temp/plan.backend.md. Không sửa frontend, không commit, không đánh dấu Done nếu API smoke chưa pass.
```

Frontend:

```txt
Frontend Agent: tiếp tục Phase 3 Owner Admin Tenant Slice theo docs/current-task.frontend.md và temp/plan.frontend.md. Chỉ implement khi owner đã duyệt plan rõ, không sửa backend, không sửa Figma.
```

Lead:

```txt
Lead Agent: cập nhật dashboard multi-workstream, đồng bộ roadmap và phân lane task mới nếu cần.
```

Feature Team (mọi feature mới):

```txt
Lead Agent: bắt đầu feature team cho [feature name]. Tự chọn agents cần tham gia, lập plan trước, chưa code.
Lead Agent: owner duyệt plan, cho feature team implement [feature name]. Không commit.
Lead Agent: verify feature team output cho [feature name]. QA Agent chạy checklist.
Lead Agent: chia commit theo lane cho [feature name]. Không push.
```

## Guardrail Chung

- Không sửa source code nếu task chỉ là docs/task management.
- Không sửa Figma nếu owner không yêu cầu.
- Không commit hoặc push nếu owner chưa yêu cầu rõ.
- Không ghi secret, IP server thật, private key, token hoặc connection string thật vào repo.
- Phase 2 đã PASS 5 smoke pass đủ trên server, đủ điều kiện Done. Roadmap đã chuyển Phase 2 Done ngày 2026-05-10.
