CREATE SCHEMA IF NOT EXISTS platform;
CREATE SCHEMA IF NOT EXISTS tenant;

CREATE TABLE IF NOT EXISTS platform.tenants (
  id uuid PRIMARY KEY,
  name text NOT NULL,
  subdomain text NOT NULL UNIQUE,
  status text NOT NULL,
  created_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS tenant.placeholder_records (
  id uuid PRIMARY KEY,
  tenant_id uuid NOT NULL,
  record_type text NOT NULL,
  created_at timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS ix_placeholder_records_tenant_id
  ON tenant.placeholder_records (tenant_id);
