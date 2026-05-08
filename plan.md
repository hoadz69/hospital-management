# Plan - Chuẩn Hóa Repo Clinic SaaS

## Pha Hiện Tại

Chuẩn hóa tài liệu, rule, script và scaffold để toàn bộ repo đi theo hướng Clinic SaaS / Hospital Management.

## Đã Hoàn Thành

- Xác định project identity: Clinic SaaS / Hospital Management Platform.
- Bổ sung luật chung cho Codex/Claude trong `AGENTS.md`.
- Bổ sung entrypoint cho Claude Code trong `CLAUDE.md`.
- Cấu hình Figma MCP trong `.mcp.json`.
- Tạo/cập nhật báo cáo kiến trúc tiếng Việt.
- Chuẩn hóa Docker Compose local cho PostgreSQL, MongoDB, Redis, Kafka.

## Đang Làm

- Khôi phục các rule bắt buộc kiểu “BẮT BUỘC ĐỌC TRƯỚC KHI VIẾT CODE”.
- Chuyển các file còn lệch domain sang nội dung Clinic SaaS bằng tiếng Việt.
- Ghi rõ giới hạn Figma MCP của phiên hiện tại.

## Pha Tiếp Theo

1. Restart Codex/Claude Code để Figma MCP tools hiện ra.
2. Đọc hai board Figma/FigJam bằng MCP.
3. Cập nhật `clinic_saas_report.md` bằng nội dung board thật.
4. Tạo `temp/plan.md` cho bước implementation đầu tiên.
5. Chờ owner duyệt trước khi scaffold frontend/backend.

## Không Được Làm Khi Chưa Có Duyệt

- Không tạo connection thật tới server/database.
- Không commit.
- Không xóa file lịch sử nếu owner chưa yêu cầu.
- Không triển khai code runtime khi chưa có `temp/plan.md` được duyệt.
