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

Nếu `$ARGUMENTS` là prompt ngắn dạng `Lead Agent: bắt đầu <task>`, `Lead Agent: làm tiếp <task>`, `Lead Agent: verify <task>` hoặc `Lead Agent: chia commit <task>`, phải dùng Feature Team Execution Workflow trong `AGENTS.md`/`docs/agent-playbook.md`: đây là action trigger thật, không acknowledge-only; chạy `git status --branch --short`, `git diff --stat`, đọc lane current-task/plan/handoff, tự phân lane, tự chọn agents theo scope, quyết định action và không yêu cầu owner liệt kê "Agents tham gia".

Nếu task đã có scope rõ trong lane plan/current-task/handoff/roadmap với allowed files/file areas và acceptance/verify rõ, không tạo plan lại chỉ để chờ duyệt; xem là resumable/approved scope và implement/resume/verify/commit-split theo prompt. Chỉ plan-only khi task mới, scope chưa rõ, cross-lane lớn chưa có plan, có rủi ro destructive/secret/security, hoặc owner nói rõ "chỉ lập plan", "chưa code", "đợi tôi duyệt".

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

Chỉ viết plan khi action đã được xác định là plan-only. Không stage/commit/push; không stage artifact/log/screenshot/generated files hoặc `.claude/settings.local.json`.
