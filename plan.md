# Plan - Chuẩn Hóa Repo Clinic SaaS

## Pha Hiện Tại

Đồng bộ tài liệu, rule và prompt theo structure scaffold hiện tại:

```txt
frontend/
  apps/
  packages/

backend/
  services/
  shared/

infrastructure/
docs/
temp/
```

## Đã Hoàn Thành Trước Đó

- Xác định project identity: Clinic SaaS / Hospital Management Platform.
- Bổ sung luật chung cho Codex/Claude trong `AGENTS.md`.
- Bổ sung entrypoint cho Claude Code trong `CLAUDE.md`.
- Cấu hình Figma MCP trong `.mcp.json`.
- Tạo/cập nhật báo cáo kiến trúc tiếng Việt.
- Scaffold skeleton theo `frontend/`, `backend/`, `infrastructure/`, `docs/`, `temp/`.

## Đã Hoàn Thành Trong Task Này

- Đồng bộ source-of-truth/rules/prompt còn nhắc root-level `apps/`, `packages/`, `services/`.
- Đồng bộ legacy report path, legacy lead-agent file reference và FigJam link cũ sang source hiện tại.
- Ghi nhận Figma/FigJam không còn text structure cũ và đã thêm note structure hiện tại vào Technical Architecture FigJam.

## Pha Tiếp Theo

1. Tạo .NET project files thực tế cho phase 1 backend: `api-gateway`, `identity-service`, `tenant-service`.
2. Mở rộng frontend routing/layout placeholder theo Figma cho 3 apps.
3. Thêm smoke tests/checklists cho tenant isolation, routing và compose config.

## Không Được Làm Khi Chưa Có Duyệt

- Không tạo connection thật tới server/database.
- Không commit.
- Không xóa file lịch sử nếu owner chưa yêu cầu.
- Không triển khai business logic ngoài scope.
