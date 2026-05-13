-- Bo sung state DNS retry va SSL cho domain cua tenant.
-- Migration idempotent, khong tao background worker/DNS provider that; API ghi nhan retry state de FE bo mock.

-- Cac cot moi phuc vu GET domain state, POST dns-retry va GET ssl-status.
ALTER TABLE platform.tenant_domains
  ADD COLUMN IF NOT EXISTS dns_status text NULL,
  ADD COLUMN IF NOT EXISTS dns_records jsonb NULL,
  ADD COLUMN IF NOT EXISTS last_checked_at_utc timestamptz NULL,
  ADD COLUMN IF NOT EXISTS retry_count integer NOT NULL DEFAULT 0,
  ADD COLUMN IF NOT EXISTS next_retry_at_utc timestamptz NULL,
  ADD COLUMN IF NOT EXISTS ssl_status text NULL,
  ADD COLUMN IF NOT EXISTS ssl_issuer text NULL,
  ADD COLUMN IF NOT EXISTS expires_at_utc timestamptz NULL,
  ADD COLUMN IF NOT EXISTS status_message text NULL;

-- Backfill domain cu tu status Phase 2 sang contract DNS/SSL moi.
UPDATE platform.tenant_domains
SET
  dns_status = COALESCE(
    dns_status,
    CASE
      WHEN status = 'Active' THEN 'verified'
      WHEN status = 'Suspended' THEN 'failed'
      ELSE 'pending'
    END),
  ssl_status = COALESCE(
    ssl_status,
    CASE
      WHEN status = 'Active' THEN 'issued'
      WHEN status = 'Suspended' THEN 'failed'
      ELSE 'pending'
    END),
  last_checked_at_utc = COALESCE(last_checked_at_utc, verified_at_utc, created_at_utc),
  status_message = COALESCE(
    status_message,
    CASE
      WHEN status = 'Active' THEN 'Domain DNS verified and SSL state is available.'
      ELSE 'Domain is waiting for DNS propagation and SSL provisioning.'
    END),
  dns_records = COALESCE(dns_records, jsonb_build_array(jsonb_build_object(
    'recordType', 'CNAME',
    'host', domain_name,
    'expectedValue', 'cname.clinicos.local',
    'actualValue', null,
    'status', CASE WHEN status = 'Active' THEN 'verified' ELSE 'pending' END,
    'message', 'CNAME should point to the Clinic SaaS gateway.'
  )));

ALTER TABLE platform.tenant_domains
  ALTER COLUMN dns_status SET NOT NULL,
  ALTER COLUMN ssl_status SET NOT NULL;

DO $$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'ck_tenant_domains_dns_status'
      AND conrelid = 'platform.tenant_domains'::regclass
  ) THEN
    ALTER TABLE platform.tenant_domains
      ADD CONSTRAINT ck_tenant_domains_dns_status
        CHECK (dns_status IN ('pending', 'propagating', 'failed', 'verified'));
  END IF;

  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'ck_tenant_domains_ssl_status'
      AND conrelid = 'platform.tenant_domains'::regclass
  ) THEN
    ALTER TABLE platform.tenant_domains
      ADD CONSTRAINT ck_tenant_domains_ssl_status
        CHECK (ssl_status IN ('none', 'pending', 'issued', 'failed'));
  END IF;

  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'ck_tenant_domains_retry_count'
      AND conrelid = 'platform.tenant_domains'::regclass
  ) THEN
    ALTER TABLE platform.tenant_domains
      ADD CONSTRAINT ck_tenant_domains_retry_count
        CHECK (retry_count >= 0);
  END IF;
END $$;

-- Query path chinh: list domain theo tenant va loc domain dang can retry/poll.
CREATE INDEX IF NOT EXISTS ix_tenant_domains_tenant_dns_status
  ON platform.tenant_domains (tenant_id, dns_status);

CREATE INDEX IF NOT EXISTS ix_tenant_domains_tenant_ssl_status
  ON platform.tenant_domains (tenant_id, ssl_status);

COMMENT ON COLUMN platform.tenant_domains.dns_status IS 'Trang thai DNS retry hien tai cua domain: pending, propagating, failed hoac verified.';
COMMENT ON COLUMN platform.tenant_domains.dns_records IS 'Danh sach CNAME/TXT dang JSONB de FE hien thi diagnostic DNS.';
COMMENT ON COLUMN platform.tenant_domains.last_checked_at_utc IS 'Thoi diem he thong kiem tra DNS gan nhat theo UTC.';
COMMENT ON COLUMN platform.tenant_domains.retry_count IS 'So lan owner yeu cau retry DNS cho domain.';
COMMENT ON COLUMN platform.tenant_domains.next_retry_at_utc IS 'Thoi diem du kien co the retry/poll DNS tiep theo.';
COMMENT ON COLUMN platform.tenant_domains.ssl_status IS 'Trang thai SSL cua domain: none, pending, issued hoac failed.';
COMMENT ON COLUMN platform.tenant_domains.ssl_issuer IS 'Issuer cua certificate neu SSL da duoc cap.';
COMMENT ON COLUMN platform.tenant_domains.expires_at_utc IS 'Thoi diem certificate SSL het han theo UTC neu co.';
COMMENT ON COLUMN platform.tenant_domains.status_message IS 'Thong diep ngan an toan cho FE hien thi trang thai DNS/SSL.';
COMMENT ON CONSTRAINT ck_tenant_domains_dns_status ON platform.tenant_domains IS 'Gioi han trang thai DNS ma Domain DNS/SSL API tra cho FE.';
COMMENT ON CONSTRAINT ck_tenant_domains_ssl_status ON platform.tenant_domains IS 'Gioi han trang thai SSL ma Domain DNS/SSL API tra cho FE.';
COMMENT ON CONSTRAINT ck_tenant_domains_retry_count ON platform.tenant_domains IS 'Dam bao retry_count khong am.';
COMMENT ON INDEX platform.ix_tenant_domains_tenant_dns_status IS 'Tang toc truy van domain theo tenant va trang thai DNS.';
COMMENT ON INDEX platform.ix_tenant_domains_tenant_ssl_status IS 'Tang toc truy van domain theo tenant va trang thai SSL.';
