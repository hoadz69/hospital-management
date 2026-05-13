# Kế Hoạch Hiện Hành - Multi-Workstream Index

Ngày cập nhật: 2026-05-13

File này chỉ là index tương thích cũ. Không ghi plan chi tiết, log verify, lịch sử phase hoặc implementation checklist vào đây.

## Lane Active Plans

| Workstream | Active plan | Handoff | Ghi chú |
|---|---|---|---|
| Backend/DevOps | `temp/plan.backend.md` | `docs/current-task.backend.md` | Living active plan backend/runtime. |
| Frontend | `temp/plan.frontend.md` | `docs/current-task.frontend.md` | Living active plan frontend. |
| DevOps riêng | `temp/plan.devops.md` | `docs/current-task.md` | Chỉ dùng khi task DevOps tách lane rõ. |
| Database riêng | `temp/plan.database.md` | `docs/current-task.md` | Chỉ tạo khi có feature data/schema lớn. |
| Docs/Workflow | `docs/agent-playbook.md` / `docs/codex-setup.md` | `docs/current-task.md` | Workflow, tooling, GitNexus, MCP. |

## Rule Sử Dụng

- Backend/DevOps Agent không ghi plan chi tiết vào file này; dùng `temp/plan.backend.md`.
- Frontend Agent không ghi plan chi tiết vào file này; dùng `temp/plan.frontend.md`.
- Lead Agent chỉ cập nhật file này khi thêm/bỏ lane plan hoặc đổi vị trí source-of-truth.
- GitNexus workflow chi tiết nằm trong `docs/codex-setup.md`; không ghi plan GitNexus dài vào file này.
- Roadmap phase/wave dài hạn nằm trong `docs/roadmap/clinic-saas-roadmap.md`.

## Prompt Ngắn

```txt
Backend Agent: đọc docs/current-task.backend.md và temp/plan.backend.md, làm đúng active slice.
Frontend Agent: đọc docs/current-task.frontend.md và temp/plan.frontend.md, làm đúng active slice.
Lead Agent: nếu task mới chưa rõ lane, phân lane rồi cập nhật active plan lane phù hợp.
```
