# Backend Testing Rules - Clinic SaaS

Use these rules when API services exist.

## Before Testing

- Confirm which services exist and which ports they use.
- Check `/health` before testing feature endpoints.
- Do not use old copied tokens, users, ports, or service names.
- Do not connect to real production databases unless the owner explicitly asks.

## Required Test Focus

- Tenant resolution by domain/subdomain/header/JWT.
- Clinic Admin cannot access another tenant.
- Owner Super Admin can perform approved cross-tenant actions.
- Booking flow creates correct appointment state.
- Template apply modes do not overwrite the wrong fields.
- Domain verification handles pending, failed, and verified states.

## Reporting

For each manual/API test, report:

- endpoint,
- request summary,
- expected result,
- actual result,
- tenant/user context,
- pass/fail status.

If services do not exist yet, state that API testing is not applicable.
