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
