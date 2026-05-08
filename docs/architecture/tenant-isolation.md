# Tenant Isolation

Tenant isolation is mandatory.

- Tenant-owned commands and queries require a valid tenant context.
- Clinic Admin can only access their own tenant.
- Owner Super Admin is the only cross-tenant role.
- Public Website resolves tenant by domain or subdomain.
- Tenant-owned SQL tables and MongoDB collections must include `tenant_id`.
- Frontend tenant-aware logic belongs in `frontend/apps/*` and shared client helpers in `frontend/packages/api-client`.
- Backend tenant enforcement belongs in each `backend/services/*` application/data-access path, with reusable primitives only under `backend/shared/`.
