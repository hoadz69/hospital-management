---
name: plan
description: Tạo kế hoạch implementation cho Clinic SaaS trước khi viết code.
effort: high
user_invocable: true
allowed-tools: Read, Glob, Grep, Write
---

Đọc trước:

1. `AGENTS.md`
2. `CLAUDE.md`
3. `clinic_saas_report.md`
4. `architech.txt`
5. `docs/current-task.md`
6. `docs/agent-playbook.md`

Tạo hoặc cập nhật `/temp/plan.md` cho yêu cầu:

```txt
$ARGUMENTS
```

Plan phải có:

1. Scope và assumptions.
2. Architecture impact: frontend app, backend service, database/cache/event bus.
3. Agent/role phù hợp nếu task cần chia vai.
4. Files dự kiến tạo/sửa.
5. Tenant isolation requirements.
6. Figma/FigJam references nếu UI hoặc architecture bị ảnh hưởng.
7. Implementation order.
8. Success criteria cho từng bước.
9. Manual verification steps.

Chỉ viết plan. Không implement code cho đến khi owner duyệt.
