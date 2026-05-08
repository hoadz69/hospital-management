---
name: coding
description: Implement plan Clinic SaaS đã được owner duyệt trong /temp/plan.md.
effort: medium
user_invocable: true
allowed-tools: Read, Write, Glob, Grep, Bash
---

Trước khi code:

1. Read `/temp/plan.md`.
2. Read `AGENTS.md`, `CLAUDE.md`, `clinic_saas_report.md`, `architech.txt`, `docs/current-task.md`, and `rules/coding-rules.md`.
3. Confirm owner đã duyệt plan.

Luật implementation:

- Chỉ làm đúng approved scope.
- Đổi ít nhất có thể, không refactor ngoài scope.
- Mỗi dòng thay đổi phải trace được về request hoặc approved plan.
- Nếu plan thiếu success criteria, bổ sung/nhắc lại trước khi implement.
- Preserve tenant isolation.
- Keep frontend aligned with Figma when UI is involved.
- Use Vue 3 + Vite + TypeScript for frontend.
- Use Clean Architecture for backend services.
- Security context thiếu/invalid phải fail rõ ràng, không fallback hardcoded.
- Do not add real server/database secrets.
- Update `docs/current-task.md` if work cannot be completed.

Yêu cầu:

```txt
$ARGUMENTS
```
