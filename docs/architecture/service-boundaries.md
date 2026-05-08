# Service Boundaries

Backend services live under `backend/services/`. Shared backend building blocks, contracts, and observability helpers live under `backend/shared/`.

- `api-gateway`: Gateway/BFF.
- `identity-service`: Auth, RBAC, tenant claims.
- `tenant-service`: Tenant lifecycle, plans, modules.
- `website-cms-service`: CMS settings, menu, slider, page content.
- `template-service`: Template library and apply modes.
- `domain-service`: Subdomain, custom domain, DNS, SSL, publish status.
- `booking-service`: Appointments and slots.
- `catalog-service`: Services, pricing, staff, schedules.
- `customer-service`: Customers and record metadata.
- `billing-service`: Subscriptions, invoices, payments.
- `report-service`: KPI and analytics.
- `notification-service`: Email, SMS, Zalo-ready pipeline.
- `realtime-gateway`: Future SignalR/WebSocket gateway.
