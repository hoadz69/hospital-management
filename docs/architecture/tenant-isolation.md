# Tenant Isolation

Tenant isolation is mandatory.

- Tenant-owned commands and queries require a valid tenant context.
- Clinic Admin can only access their own tenant.
- Owner Super Admin is the only cross-tenant role.
- Public Website resolves tenant by domain or subdomain.
- Tenant-owned SQL tables and MongoDB collections must include `tenant_id`.
