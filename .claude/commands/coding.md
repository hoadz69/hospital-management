---
name: coding
description: Implement the approved Clinic SaaS plan from /temp/plan.md.
effort: medium
user_invocable: true
allowed-tools: Read, Write, Glob, Grep, Bash
---

Before coding:

1. Read `/temp/plan.md`.
2. Read `AGENTS.md`, `CLAUDE.md`, and `clinic_saas_report_vi.md`.
3. Confirm the plan is approved by the owner.

Implementation rules:

- Follow the approved scope only.
- Preserve tenant isolation.
- Keep frontend aligned with Figma when UI is involved.
- Use Vue 3 + Vite + TypeScript for frontend.
- Use Clean Architecture for backend services.
- Do not add real server/database secrets.
- Update `docs/current-task.md` if work cannot be completed.

Request:

```txt
$ARGUMENTS
```
