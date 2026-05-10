# Current Task - Project Coordination Dashboard

Ngày cập nhật: 2026-05-10

File này là dashboard điều phối project. Không ghi plan chi tiết của một lane vào đây.
Lead Agent chỉ dùng file này để tóm tắt trạng thái tổng quan và trỏ sang task/plan lane riêng.

Mọi feature mới phải chạy theo "Feature Team Execution Workflow" (Step 0–10) trong `docs/agent-playbook.md` và `AGENTS.md`. Lead Agent assemble team theo loại feature (UI / API / full-stack / deployment / data) và phân lane đúng.

## Workstream Đang Chạy Song Song

| Workstream | Task file | Plan file | Trạng thái ngắn | Bước tiếp theo |
|---|---|---|---|---|
| Backend/DevOps | `docs/current-task.backend.md` | `temp/plan.backend.md` | ✅ Phase 2 API Runtime Smoke Gate PASS. ✅ Pre-Phase 4 Hardening backend lane (P1.1 + P1.2 + P1.3) committed. Sẵn sàng giao Phase 4.1/4.2/4.3 OpenAPI contract cho FE Wave A | Owner duyệt OpenAPI contract Phase 4.1 Domain + 4.2 Template + 4.3 CMS (mock OK) cho Wave A unblock. Sau đó implement Phase 4.1 Domain DNS verify async + SSL ACME |
| Frontend | `docs/current-task.frontend.md` | `temp/plan.frontend.md` | 🟡 Phase 3 Implementation Done (commit `7f6366d`) + Pre-Phase 4 Hardening (P1.6 + P1.7) committed. **V3v2 76 frame ready cho Phase 4 Wave A.** Chờ owner duyệt plan + 5 owner decision | Owner duyệt plan Wave A trong `temp/plan.frontend.md` §16. Owner duyệt 5 decision (audit retention, PII rule, autosave conflict, DNS retry, tenant suspended fallback) trước Wave B/C/D/E |
| DevOps | (dashboard này) | `temp/plan.devops.md` | ✅ Pre-Phase 4 Hardening DevOps lane (P1.4 + P1.8) committed. Sẵn sàng support Wave A backend mock + Wave B edge resolver subdomain → tenantId | Wave B backend dep: nginx hoặc Cloudflare Worker meta tag inject tenant id |
| Database | (dashboard này) | (note trong dashboard) | ✅ Pre-Phase 4 Hardening database lane (P1.5) committed. Phase 4 chưa cần lane riêng. P2 candidate: chuyển sang DbUp/FluentMigrator runner | Theo dõi schema mới khi Wave D Catalog/Customer/Records APSO bắt đầu |
| Docs/Agent Workflow | (dashboard này) | `temp/plan.md` | ✅ Feature Team Execution Workflow committed (`74c29c8`). ✅ Pre-Phase 4 Hardening DONE. 🟡 V3v2 76 frame source of truth Phase 4+ ready, docs sync DONE 2026-05-10 | Theo dõi 5 owner decision; cập nhật roadmap khi từng wave Phase 4+ chuyển trạng thái |

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
- 2026-05-10 (chiều): Pre-Phase 4 Hardening **wave 2** — owner duyệt thêm 3 P1 medium từ Full Project Review: P1.2 ProblemDetails 409 attach `extensions.fields` (BE), P1.5 init.sql header drift warning + sync với migration 0001 (DB), P1.6 CreateTenantWizard isStepValid gate (FE). Feature team đã chạy đủ Step 0-7. Verify: dotnet build 0/0, dotnet test 12/12 [Fact] PASS, typecheck PASS x3, build PASS x3, docker compose config PASS. **WAVE 2 IMPLEMENTATION DONE**. Tổng 8/8 P1 hardening DONE, đã commit split theo lane (Step 9).
- 2026-05-10: UI Redesign V3 expand từ 16 → **76 frame** (60 frame V3v2 mới). 7 section trên page Figma `65:2`. Source of truth Phase 4+. Chi tiết: `docs/roadmap/clinic-saas-roadmap.md#71-ui-redesign-v3-source-of-truth-phase-4`.
- 2026-05-10: 5 owner decision cần duyệt trước Wave A: audit retention scope, PII patient rule, builder autosave conflict policy, DNS retry tolerance, tenant suspended fallback policy. Frame Figma `129:197` Open Questions & Risks ghi đầy đủ.
- 2026-05-10: Lead Agent bổ sung Crash Recovery & Checkpoint Protocol để chống mất context khi Codex/Claude compact/chết session. Rule đã đồng bộ vào `docs/session-continuity.md`, `AGENTS.md`, `CLAUDE.md`, `docs/agent-playbook.md`, Lead/Documentation agent docs và `docs/commands/implement-phase.md`. Frontend lane đã có checkpoint cho Owner Admin V3 restyle đang dirty.
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
