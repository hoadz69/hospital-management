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
4. Nếu owner gọi ngắn `Lead Agent: bắt đầu <task>` hoặc `Lead Agent: làm tiếp <task>`, xem đó là action trigger thật: chạy `git status --branch --short`, `git diff --stat`, đọc lane current-task/plan/handoff, tự phân lane và tự chọn agents theo Feature Team Execution Workflow; không yêu cầu owner paste "Agents tham gia" và không trả lời acknowledge-only.
5. Nếu task đã có scope rõ trong lane plan/current-task/handoff/roadmap với allowed files/file areas và acceptance/verify rõ, xem là resumable/approved scope và implement/resume đúng scope. Chỉ dừng approval gate khi task mới, scope chưa rõ, cross-lane lớn chưa có plan, có rủi ro destructive/secret/security, hoặc owner nói rõ "chỉ lập plan/chưa code/đợi tôi duyệt".
6. Fast Mode là mặc định cho short prompt làm/tiếp tục/fix/verify; không gọi full subagents/Figma/screenshot nếu không cần, không paste log dài. Full Team Mode chỉ khi owner yêu cầu rõ hoặc task nhiều vùng/rủi ro cao.

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
- Do not stage/commit/push unless owner explicitly asks.
- Do not stage `.claude/settings.local.json`, screenshot/log/generated artifacts.
- Do not delete dirty source/docs/plan files unless ownership is clear.
- Update `docs/current-task.md` if work cannot be completed.

Yêu cầu:

```txt
$ARGUMENTS
```
