---
name: plan
description: Create an implementation plan for the Clinic SaaS platform before coding.
effort: high
user_invocable: true
allowed-tools: Read, Glob, Grep, Write
---

Read first:

1. `AGENTS.md`
2. `CLAUDE.md`
3. `clinic_saas_report_vi.md`
4. `docs/current-task.md`

Create or update `/temp/plan.md` for this request:

```txt
$ARGUMENTS
```

The plan must include:

1. Scope and assumptions.
2. Architecture impact: frontend app, backend service, database/cache/event bus.
3. Files to create/change.
4. Tenant isolation requirements.
5. Figma/FigJam references if UI or architecture is affected.
6. Implementation order.
7. Manual verification steps.

Only write the plan. Do not implement code until the owner approves.
