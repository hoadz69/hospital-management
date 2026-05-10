# Architecture Overview

Clinic SaaS is a multi-tenant platform with three frontend apps, an API Gateway/BFF, .NET microservices, and infrastructure for PostgreSQL, MongoDB, Redis, and Kafka.

Current approved source structure:

```txt
frontend/
  apps/
    public-web/
    clinic-admin/
    owner-admin/
  packages/
    ui/
    design-tokens/
    api-client/
    shared-types/
    config/

backend/
  services/
  shared/

infrastructure/
docs/
temp/
```

Do not add root-level `apps/`, `packages/`, or `services/` directories. Frontend app/package work belongs under `frontend/`; backend service/shared work belongs under `backend/`.

## UI Source of Truth

UI source of truth Phase 4+ là Figma page `65:2` UI Redesign V3 - 2026-05-10 trong file `1nbJ5XkrlDgQ3BmzpXzhCC` (76 frame, 7 section grouping: Overview / Public / Booking / Clinic Admin / Owner Admin / Design System / Handoff). Chi tiết V3 + 5 Wave plan + 5 owner decision: `docs/roadmap/clinic-saas-roadmap.md#71-ui-redesign-v3-source-of-truth-phase-4`.

V1 baseline (page `0:1`, 40 frame UI Kit historical) và V2 (page `36:2`, empty historical) không sửa, không xóa. V3 (page `65:2`) là active source of truth cho mọi phase từ Phase 4 trở đi. Frontend Agent phải đọc V3 frame đúng surface trước khi code Phase 4+; không tự invent layout khi V3 đã có frame detailed visual.
