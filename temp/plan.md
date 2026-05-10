# Kế Hoạch Hiện Hành - Multi-Workstream Index

Ngày cập nhật: 2026-05-10

File này là index tương thích cũ cho plan. Không ghi plan chi tiết của một lane vào đây nữa.

Mọi feature mới phải chạy theo "Feature Team Execution Workflow" (Step 0–10) trong `docs/agent-playbook.md` và `AGENTS.md`. Plan chi tiết phải nằm trong lane file phù hợp (`temp/plan.backend.md`, `temp/plan.frontend.md`, hoặc khi cần tạo lane riêng `temp/plan.database.md`, `temp/plan.devops.md`).

## Plan Lane Hiện Tại

| Workstream | Plan chi tiết | Task handoff | Trạng thái |
|---|---|---|---|
| Backend/DevOps | `temp/plan.backend.md` | `docs/current-task.backend.md` | Phase 2 API Runtime Smoke Gate đã PASS, chờ Phase 3 backend support khi FE cần real API |
| Frontend | `temp/plan.frontend.md` | `docs/current-task.frontend.md` | Phase 3 Owner Admin Tenant Slice ready for commit, chờ owner duyệt commit + plan Phase 4 |
| Database (lane riêng nếu cần) | `temp/plan.database.md` | (sẽ tạo khi feature data lớn) | Chưa active, dùng khi Database Agent có task lớn riêng |
| DevOps (lane riêng nếu cần) | `temp/plan.devops.md` | (sẽ tạo khi feature deployment lớn) | Chưa active, dùng khi DevOps task tách khỏi backend lane |
| Docs/Agent Workflow | (file này) | `docs/current-task.md` | Feature Team Execution Workflow đã thêm vào agent docs, sẵn sàng commit docs riêng |

## Rule Sử Dụng

- Lead Agent có thể cập nhật file này như index ngắn nếu cần.
- Backend Agent và DevOps Agent không ghi plan chi tiết vào `temp/plan.md`; dùng `temp/plan.backend.md`.
- Frontend Agent không ghi plan chi tiết vào `temp/plan.md`; dùng `temp/plan.frontend.md`.
- Nếu task mới chưa rõ lane, Lead Agent ghi note ngắn tại `docs/current-task.md` rồi tạo plan lane phù hợp.

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
