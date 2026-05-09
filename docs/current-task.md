# Current Task - Project Coordination Dashboard

Ngày cập nhật: 2026-05-10

File này là dashboard điều phối project. Không ghi plan chi tiết của một lane vào đây.
Lead Agent chỉ dùng file này để tóm tắt trạng thái tổng quan và trỏ sang task/plan lane riêng.

## Workstream Đang Chạy Song Song

| Workstream | Task file | Plan file | Trạng thái ngắn | Bước tiếp theo |
|---|---|---|---|---|
| Backend/DevOps | `docs/current-task.backend.md` | `temp/plan.backend.md` | ✅ Phase 2 API Runtime Smoke Gate PASS đủ 5 case trên server `116.118.47.78` sau 2 vòng fix (Dapper type handler + reorder positional record `TenantListRow`); roadmap chuyển Phase 2 Done | (Optional) commit backend fix + lane docs khi owner duyệt; chuẩn bị Phase 3 backend support khi frontend cần real API |
| Frontend | `docs/current-task.frontend.md` | `temp/plan.frontend.md` | Phase 3 Owner Admin Tenant Slice: implementation Done, typecheck/build/dev-server PASS, HTTP+Vite smoke 5 route PASS, đang dùng mock fallback do backend smoke chưa Done | Bổ sung browser/Playwright cho headless visual hoặc owner mở thủ công; smoke real API khi backend Phase 2 pass |

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

## Guardrail Chung

- Không sửa source code nếu task chỉ là docs/task management.
- Không sửa Figma nếu owner không yêu cầu.
- Không commit hoặc push nếu owner chưa yêu cầu rõ.
- Không ghi secret, IP server thật, private key, token hoặc connection string thật vào repo.
- Phase 2 đã PASS 5 smoke pass đủ trên server, đủ điều kiện Done. Roadmap đã chuyển Phase 2 Done ngày 2026-05-10.
