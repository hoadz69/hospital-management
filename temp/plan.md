# Kế Hoạch Hiện Hành - Multi-Workstream Index

Ngày cập nhật: 2026-05-09

File này là index tương thích cũ cho plan. Không ghi plan chi tiết của một lane vào đây nữa.

## Plan Lane Hiện Tại

| Workstream | Plan chi tiết | Task handoff | Trạng thái |
|---|---|---|---|
| Backend/DevOps | `temp/plan.backend.md` | `docs/current-task.backend.md` | Phase 2 API Runtime Smoke Gate, PostgreSQL server đã bootstrap, chờ API smoke |
| Frontend | `temp/plan.frontend.md` | `docs/current-task.frontend.md` | Phase 3 Owner Admin Tenant Slice, plan xong, chờ owner duyệt implement |

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
