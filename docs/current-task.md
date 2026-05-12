# Current Task - Project Coordination Dashboard

Ngày cập nhật: 2026-05-12

File này là dashboard điều phối project. Không ghi plan chi tiết của một lane vào đây.
Lead Agent chỉ dùng file này để tóm tắt trạng thái tổng quan và trỏ sang task/plan lane riêng.

Mọi feature mới phải chạy theo "Feature Team Execution Workflow" (Step 0–10) trong `docs/agent-playbook.md` và `AGENTS.md`. Lead Agent assemble team theo loại feature (UI / API / full-stack / deployment / data) và phân lane đúng.

## Workstream Đang Chạy Song Song

| Workstream | Task file | Plan file | Trạng thái ngắn | Bước tiếp theo |
|---|---|---|---|---|
| Backend/DevOps | `docs/current-task.backend.md` | `temp/plan.backend.md` | ✅ BE A.2/A.3 `/api/owner/*` readiness QA PASS cho Tenant Service + API Gateway local runtime. Contract giữ nguyên; persistence owner đề xuất là Tenant Service, Billing Service phase sau | Chờ owner duyệt `temp/plan.backend.md` §16 trước khi tạo migration/schema/code persistence. Có thể smoke real API với FE A9 khi cần |
| Frontend | `docs/current-task.frontend.md` | `temp/plan.frontend.md` | ✅ FE A9 `/plans` wired với BE A.2 contract ở client layer, mock/auto fallback giữ nguyên. typecheck/build PASS, HTTP smoke 5 route PASS | Smoke real API khi backend runtime/gateway bật; nếu owner yêu cầu commit thì split FE A9 riêng, không kèm artifact/log |
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

- 2026-05-12: BE A.3 Contract Hardening + FE A9 support **QA PASS** trong Backend/DevOps lane. Đã bổ sung Tenant Service validation/route metadata tests; local Development smoke Tenant Service `:5006` và API Gateway `:5018` PASS cho 4 endpoint `/api/owner/*`, ClinicAdmin 403, invalid role 403, và bulk-change validation 400. Không migration/schema, không persistence, không Billing, không sửa frontend. Chi tiết: `docs/current-task.backend.md`, `temp/plan.backend.md` §17.

- 2026-05-12: Backend Phase 4 Wave A.2 `/api/owner/*` readiness verify bổ sung PASS. Docker CLI không có trong PATH nên không verify container local; đã dựng runtime local bằng dotnet cho Tenant Service `:5006` và API Gateway `:5018`. OpenAPI đủ 4 route, response shape giữ nguyên, OwnerSuperAdmin happy path PASS, ClinicAdmin/invalid role 403 PASS, validation 400 PASS. 409 conflict chưa áp dụng cho stub, sẽ cover khi owner duyệt persistence §16.

- 2026-05-12: Backend Phase 4 Wave B Owner Plan/Module persistence preparation đã cập nhật plan-only trong lane Backend/DevOps. Decision đề xuất: Tenant Service sở hữu plan catalog/module entitlement/tenant-plan assignment vì gắn tenant lifecycle; Billing Service để phase sau cho subscription/invoice/payment. Schema dự kiến: `platform.plans`, `platform.modules`, `platform.plan_module_entitlements`, `platform.tenant_plan_assignments`, audit tối thiểu `platform.tenant_plan_assignment_changes`. Không migration/schema/code ở lượt này; chờ owner duyệt `temp/plan.backend.md` §16.

- 2026-05-12: FE A9 đã wire `/plans` với BE A.2 real contract qua typed client + app wrapper `auto|real|mock`, vẫn fallback mock khi backend runtime chưa bật. Verify frontend PASS; direct `/api/owner/plans` qua dev proxy hiện 500 do backend/gateway runtime không sẵn trong phiên này. Backend song song phù hợp nhất: BE A.3 contract hardening/test guard, không schema/persistence.

- 2026-05-12: Backend Phase 4 Wave A.2 Owner Plan Module contract/stub **QA PASS** trong lane Backend/DevOps. Service owner: Tenant Service; API Gateway expose `/api/owner/plans`, `/api/owner/modules`, `/api/owner/tenant-plan-assignments`, `POST /api/owner/tenant-plan-assignments/bulk-change`. Không migration/schema, không persistence thật. Verify: `git diff --check`, restore/build/test PASS, local Development smoke Tenant Service + API Gateway PASS, `X-Owner-Role=ClinicAdmin` bulk-change 403 PASS. Chi tiết: `docs/current-task.backend.md` và `temp/plan.backend.md` §15.

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
