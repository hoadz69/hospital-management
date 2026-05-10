# Kế Hoạch Hiện Hành - Multi-Workstream Index

Ngày cập nhật: 2026-05-10

File này là index tương thích cũ cho plan. Không ghi plan chi tiết của một lane vào đây nữa.

Mọi feature mới phải chạy theo "Feature Team Execution Workflow" (Step 0–10) trong `docs/agent-playbook.md` và `AGENTS.md`. Plan chi tiết phải nằm trong lane file phù hợp (`temp/plan.backend.md`, `temp/plan.frontend.md`, hoặc khi cần tạo lane riêng `temp/plan.database.md`, `temp/plan.devops.md`).

## Plan Lane Hiện Tại

| Workstream | Plan chi tiết | Task handoff | Trạng thái |
|---|---|---|---|
| Backend/DevOps | `temp/plan.backend.md` | `docs/current-task.backend.md` | Phase 2 API Runtime Smoke Gate PASS. Pre-Phase 4 Hardening (§12) committed. **§13 Phase 4 Backend Plan (Wave A unblock contract) 🟡 Planning** — OpenAPI contract Phase 4.1/4.2/4.3 mock-first cho FE Wave A |
| Frontend | `temp/plan.frontend.md` | `docs/current-task.frontend.md` | Phase 3 Done (commit `7f6366d`) + Pre-Phase 4 Hardening (§15) committed. **§16 Phase 4 Wave A V3 Foundation 🟡 Planning** — token V3 ADD-ONLY + httpClient rebuild + Owner Admin restyle + 7 component shared + 4 composable + 4 frame Owner Admin V3v2 |
| DevOps (lane mới) | `temp/plan.devops.md` | `docs/current-task.md` (dashboard) | Pre-Phase 4 Hardening lane committed (P1.4 nginx SPA fallback + X-Forwarded, P1.8 docker-compose dev bind 127.0.0.1). Wave B dep: edge resolver subdomain → tenantId |
| Database (lane riêng nếu cần) | `temp/plan.database.md` | (sẽ tạo khi feature data lớn) | Pre-Phase 4 Hardening (P1.5) committed. Phase 4 chưa cần lane riêng. Dùng khi Wave D Catalog/Customer/Records APSO bắt đầu |
| Docs/Agent Workflow | (file này) | `docs/current-task.md` | Feature Team Execution Workflow committed (`74c29c8`). Pre-Phase 4 Hardening đã apply. V3v2 76 frame source of truth Phase 4+ docs sync DONE 2026-05-10 |

## Pre-Phase 4 Hardening (Cross-Lane Feature Team)

Feature team đầu tiên chạy theo "Feature Team Execution Workflow" sau khi workflow được duyệt và commit. Mục tiêu: vá P1 quick wins trước khi mở Phase 4.

| Issue | Lane | Plan section | Trạng thái |
|---|---|---|---|
| P1.1 test infra rỗng | backend | `temp/plan.backend.md` §12.9 | ✅ implementation done, dotnet test 3/3 PASS |
| P1.3 Swagger/OpenAPI chỉ bật ở Development | backend | `temp/plan.backend.md` §12.9 | ✅ implementation done, build PASS, smoke env runtime pending QA |
| P1.4 nginx SPA fallback + X-Forwarded headers | devops | `temp/plan.devops.md` §10 | ✅ implementation done, compose config PASS, `nginx -t` pending Docker daemon |
| P1.7 httpClient JSON.parse try/catch | frontend | `temp/plan.frontend.md` §15.9 | ✅ implementation done, typecheck/build PASS x3 app |
| P1.8 docker-compose dev bind 127.0.0.1 | devops | `temp/plan.devops.md` §10 | ✅ implementation done, compose config xác nhận host_ip 127.0.0.1 cho 4 service |

Agents đã tham gia: Lead + Architect (review boundary) + Backend (P1.1, P1.3) + Frontend (P1.7) + DevOps (P1.4, P1.8) + QA (verify checklist) + Documentation (cập nhật lane plan, dashboard, roadmap). Web Research/Figma UI không cần.

Verify đã chạy:

```txt
backend: dotnet restore PASS, build PASS (0 warning, 0 error), test PASS 3/3
frontend: npm typecheck PASS x3 app, build PASS x3 app
devops: docker compose -f docker-compose.dev.yml config PASS (host_ip 127.0.0.1 cho 4 service), prod config PASS
devops pending: nginx -t blocked do Docker daemon offline trong session
```

Commit split đề xuất (Step 9):

```txt
chore(backend): pre-phase 4 hardening (xunit infra, swagger gating)
fix(frontend): pre-phase 4 hardening (httpClient JSON.parse guard)
chore(devops): pre-phase 4 hardening (compose 127.0.0.1, nginx scaffold)
docs: update Pre-Phase 4 Hardening lane plans + dashboard
```

Hoặc 5 commit nhỏ theo issue (P1.8 → P1.7 → P1.3 → P1.4 → P1.1) cho dễ revert từng issue. Owner chọn khi commit. Không push.

## Rule Sử Dụng

- Lead Agent có thể cập nhật file này như index ngắn nếu cần.
- Backend Agent và DevOps Agent không ghi plan chi tiết vào `temp/plan.md`; dùng `temp/plan.backend.md`.
- Frontend Agent không ghi plan chi tiết vào `temp/plan.md`; dùng `temp/plan.frontend.md`.
- Nếu task mới chưa rõ lane, Lead Agent ghi note ngắn tại `docs/current-task.md` rồi tạo plan lane phù hợp.

## V3 Source of Truth Phase 4+

UI source of truth Phase 4+ là Figma page `65:2` UI Redesign V3 - 2026-05-10 (file key `1nbJ5XkrlDgQ3BmzpXzhCC`), 76 frame, 7 section grouping.

Chi tiết V3 + 5 Wave plan (Wave A token foundation, Wave B Public, Wave C Booking, Wave D Clinic Admin, Wave E A11y/Performance/Reports/Audit/Monitoring/Billing) + 7 risk + 5 owner decision: `docs/roadmap/clinic-saas-roadmap.md#71-ui-redesign-v3-source-of-truth-phase-4`.

Quy tắc chung Phase 4+:

```txt
- KHÔNG tạo Figma file mới.
- KHÔNG sửa V1 (page 0:1, 40 frame UI Kit historical).
- KHÔNG sửa V2 (page 36:2, empty historical).
- V3 (page 65:2) là active source of truth.
- Frontend Agent đọc Figma V3 frame đúng surface trước khi code Phase 4+.
- Frame wireframe annotated chỉ là placeholder Phase 6-7;
  cần upgrade detailed trước khi code wave đó.
```

## Nội Dung Cũ Đã Move

- Plan Server Bootstrap PostgreSQL và Phase 2 API smoke được chuyển sang `temp/plan.backend.md`.
- Plan Phase 3 Owner Admin Tenant Slice được chuyển sang `temp/plan.frontend.md`.

## Prompt Ngắn

Backend/DevOps:

```txt
Backend Agent: đọc temp/plan.backend.md và docs/current-task.backend.md, tiếp tục Phase 2 API Runtime Smoke Gate. Không sửa frontend, không commit.
```

Frontend:

```txt
Frontend Agent: đọc temp/plan.frontend.md và docs/current-task.frontend.md, tiếp tục Phase 3 Owner Admin Tenant Slice. Chỉ code khi owner duyệt rõ.
```

Feature Team:

```txt
Lead Agent: bắt đầu feature team cho [feature name]. Tự chọn agents cần tham gia, lập plan trước, chưa code.
Lead Agent: owner duyệt plan, cho feature team implement [feature name]. Không commit.
Lead Agent: verify feature team output cho [feature name]. QA Agent chạy checklist.
Lead Agent: chia commit theo lane cho [feature name]. Không push.
```
