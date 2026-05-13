-- =============================================================================
-- File này là MIRROR (sao y) của
--   backend/services/tenant-service/src/TenantService.Infrastructure/Migrations/0001_create_tenant_mvp.sql
-- Mục đích: docker-compose dev mount file này vào /docker-entrypoint-initdb.d/
--          để postgres container init lần đầu có sẵn schema Phase 2.
--
-- ⚠️ CẢNH BÁO DRIFT:
--   - Khi backend/database thêm migration mới (0002_*.sql, 0003_*.sql, ...)
--     PHẢI sync nội dung mới vào file này thủ công, hoặc convert sang
--     migration runner (DbUp / FluentMigrator) ở Phase 4+.
--   - Server production KHÔNG dùng file này — chỉ dùng migration file thật.
--     Nếu init.sql lệch server, dev local sẽ phát sinh bug "schema mismatch"
--     khó debug.
--
-- Quy ước:
--   - Khi sync, copy y nguyên migration file (kể cả COMMENT ON, IF NOT EXISTS).
--   - KHÔNG thêm seed data vào file này — đó là việc của
--     infrastructure/postgres/seed/ hoặc migration data script.
--   - KHÔNG thêm DROP/TRUNCATE/DELETE — postgres image chỉ chạy init khi
--     volume rỗng, mọi destructive sẽ phá database hiện hữu.
--
-- Khác biệt cho phép so với migration 0001:
--   - Giữ thêm `CREATE SCHEMA IF NOT EXISTS tenant` để bootstrap schema
--     `tenant` cho service khác sẽ mount sau (Phase 3+). Đây là concern
--     của dev container, không thuộc Tenant Service migration.
-- =============================================================================

-- Tạo schema Tenant MVP cho Owner Super Admin quản lý tenant/phòng khám.
-- Migration này tạo tenant root, hồ sơ phòng khám, domain/subdomain và module enablement ban đầu.

CREATE SCHEMA IF NOT EXISTS platform;
CREATE SCHEMA IF NOT EXISTS tenant;

-- Bảng tenant gốc phục vụ Owner Super Admin quản lý vòng đời phòng khám.
CREATE TABLE IF NOT EXISTS platform.tenants (
  -- Định danh tenant dùng trong API và các bảng con.
  id uuid PRIMARY KEY,

  -- Slug tenant đã chuẩn hóa, dùng cho quản trị và subdomain mặc định.
  slug text NOT NULL,

  -- Tên hiển thị tenant trong Owner Super Admin.
  display_name text NOT NULL,

  -- Trạng thái vòng đời tenant: Draft, Active, Suspended hoặc Archived.
  status text NOT NULL,

  -- Mã gói dịch vụ được lưu tại thời điểm tạo tenant.
  plan_code text NOT NULL,

  -- Tên hiển thị của gói dịch vụ nếu caller gửi snapshot.
  plan_display_name text NULL,

  -- Thời điểm tạo tenant theo UTC.
  created_at_utc timestamptz NOT NULL,

  -- Thời điểm cập nhật tenant gần nhất theo UTC.
  updated_at_utc timestamptz NULL,

  -- Thời điểm tenant được active lần đầu theo UTC.
  activated_at_utc timestamptz NULL,

  -- Thời điểm tenant bị suspend gần nhất theo UTC.
  suspended_at_utc timestamptz NULL,

  -- Thời điểm tenant được archive theo UTC.
  archived_at_utc timestamptz NULL,

  -- Giới hạn status theo lifecycle đã chốt cho Phase 2.
  CONSTRAINT ck_tenants_status
    CHECK (status IN ('Draft', 'Active', 'Suspended', 'Archived')),

  -- Đảm bảo slug tenant duy nhất trên toàn platform.
  CONSTRAINT uq_tenants_slug UNIQUE (slug)
);

-- Bảng hồ sơ phòng khám thuộc một tenant.
CREATE TABLE IF NOT EXISTS platform.tenant_profiles (
  -- Tenant sở hữu hồ sơ; đồng thời là khóa chính one-to-one với platform.tenants.
  tenant_id uuid PRIMARY KEY REFERENCES platform.tenants (id) ON DELETE CASCADE,

  -- Tên phòng khám hiển thị trong hệ thống.
  clinic_name text NOT NULL,

  -- Email liên hệ của phòng khám nếu có.
  contact_email text NULL,

  -- Số điện thoại liên hệ của phòng khám nếu có.
  phone_number text NULL,

  -- Địa chỉ phòng khám nếu có.
  address_line text NULL,

  -- Chuyên khoa chính của phòng khám nếu có.
  specialty text NULL
);

-- Bảng domain/subdomain gắn với tenant để phục vụ routing website về sau.
CREATE TABLE IF NOT EXISTS platform.tenant_domains (
  -- Định danh domain record.
  id uuid PRIMARY KEY,

  -- Tenant sở hữu domain/subdomain.
  tenant_id uuid NOT NULL REFERENCES platform.tenants (id) ON DELETE CASCADE,

  -- Domain hiển thị được owner nhập hoặc hệ thống cấp.
  domain_name text NOT NULL,

  -- Domain đã chuẩn hóa để lookup và enforce unique không lệch hoa thường.
  normalized_domain_name text NOT NULL,

  -- Loại domain: DefaultSubdomain hoặc Custom.
  domain_type text NOT NULL,

  -- Trạng thái domain trong Tenant MVP.
  status text NOT NULL,

  -- Cho biết domain này là domain chính của tenant.
  is_primary boolean NOT NULL DEFAULT false,

  -- Thời điểm domain được gắn vào tenant theo UTC.
  created_at_utc timestamptz NOT NULL,

  -- Thời điểm domain được xác minh theo UTC nếu có.
  verified_at_utc timestamptz NULL,

  -- Giới hạn loại domain được Tenant Service hỗ trợ trong Phase 2.
  CONSTRAINT ck_tenant_domains_type
    CHECK (domain_type IN ('DefaultSubdomain', 'Custom')),

  -- Giới hạn trạng thái domain được Tenant Service hỗ trợ trong Phase 2.
  CONSTRAINT ck_tenant_domains_status
    CHECK (status IN ('Pending', 'Active', 'Suspended')),

  -- Đảm bảo một domain đã chuẩn hóa chỉ thuộc một tenant.
  CONSTRAINT uq_tenant_domains_normalized_domain_name UNIQUE (normalized_domain_name)
);

-- Tăng tốc truy vấn domain theo tenant khi load tenant detail.
CREATE INDEX IF NOT EXISTS ix_tenant_domains_tenant_id
  ON platform.tenant_domains (tenant_id);

-- Tăng tốc truy vấn domain chính của tenant.
CREATE INDEX IF NOT EXISTS ix_tenant_domains_tenant_id_is_primary
  ON platform.tenant_domains (tenant_id, is_primary);

-- Bảng module được bật/tắt cho từng tenant.
CREATE TABLE IF NOT EXISTS platform.tenant_modules (
  -- Tenant sở hữu cấu hình module.
  tenant_id uuid NOT NULL REFERENCES platform.tenants (id) ON DELETE CASCADE,

  -- Mã module được bật/tắt.
  module_code text NOT NULL,

  -- Cho biết module đang bật hay tắt.
  is_enabled boolean NOT NULL DEFAULT true,

  -- Mã gói là nguồn bật module nếu có.
  source_plan_code text NULL,

  -- Thời điểm cấu hình module được tạo theo UTC.
  created_at_utc timestamptz NOT NULL,

  -- Thời điểm cấu hình module được cập nhật theo UTC.
  updated_at_utc timestamptz NULL,

  -- Mỗi tenant chỉ có một cấu hình cho mỗi module code.
  CONSTRAINT pk_tenant_modules PRIMARY KEY (tenant_id, module_code)
);

-- Tăng tốc truy vấn module theo tenant khi load tenant detail.
CREATE INDEX IF NOT EXISTS ix_tenant_modules_tenant_id
  ON platform.tenant_modules (tenant_id);

COMMENT ON TABLE platform.tenants IS 'Lưu tenant gốc của từng phòng khám trên nền tảng Clinic SaaS.';
COMMENT ON COLUMN platform.tenants.id IS 'Định danh tenant dùng trong API và các bảng con.';
COMMENT ON COLUMN platform.tenants.slug IS 'Slug tenant đã chuẩn hóa, dùng cho quản trị và subdomain mặc định.';
COMMENT ON COLUMN platform.tenants.display_name IS 'Tên hiển thị tenant trong Owner Super Admin.';
COMMENT ON COLUMN platform.tenants.status IS 'Trạng thái vòng đời tenant: Draft, Active, Suspended hoặc Archived.';
COMMENT ON COLUMN platform.tenants.plan_code IS 'Mã gói dịch vụ được lưu tại thời điểm tạo tenant.';
COMMENT ON COLUMN platform.tenants.plan_display_name IS 'Tên hiển thị của gói dịch vụ nếu có snapshot từ caller.';
COMMENT ON COLUMN platform.tenants.created_at_utc IS 'Thời điểm tạo tenant theo UTC.';
COMMENT ON COLUMN platform.tenants.updated_at_utc IS 'Thời điểm cập nhật tenant gần nhất theo UTC.';
COMMENT ON COLUMN platform.tenants.activated_at_utc IS 'Thời điểm tenant được active lần đầu theo UTC.';
COMMENT ON COLUMN platform.tenants.suspended_at_utc IS 'Thời điểm tenant bị suspend gần nhất theo UTC.';
COMMENT ON COLUMN platform.tenants.archived_at_utc IS 'Thời điểm tenant được archive theo UTC.';
COMMENT ON CONSTRAINT ck_tenants_status ON platform.tenants IS 'Giới hạn trạng thái tenant trong lifecycle đã chốt cho Phase 2.';
COMMENT ON CONSTRAINT uq_tenants_slug ON platform.tenants IS 'Đảm bảo mỗi tenant có slug duy nhất trên platform.';

COMMENT ON TABLE platform.tenant_profiles IS 'Lưu hồ sơ phòng khám thuộc một tenant.';
COMMENT ON COLUMN platform.tenant_profiles.tenant_id IS 'Tenant sở hữu hồ sơ phòng khám.';
COMMENT ON COLUMN platform.tenant_profiles.clinic_name IS 'Tên phòng khám hiển thị trong hệ thống.';
COMMENT ON COLUMN platform.tenant_profiles.contact_email IS 'Email liên hệ của phòng khám nếu có.';
COMMENT ON COLUMN platform.tenant_profiles.phone_number IS 'Số điện thoại liên hệ của phòng khám nếu có.';
COMMENT ON COLUMN platform.tenant_profiles.address_line IS 'Địa chỉ phòng khám nếu có.';
COMMENT ON COLUMN platform.tenant_profiles.specialty IS 'Chuyên khoa chính của phòng khám nếu có.';

COMMENT ON TABLE platform.tenant_domains IS 'Lưu domain hoặc subdomain gắn với tenant.';
COMMENT ON COLUMN platform.tenant_domains.id IS 'Định danh domain record.';
COMMENT ON COLUMN platform.tenant_domains.tenant_id IS 'Tenant sở hữu domain/subdomain.';
COMMENT ON COLUMN platform.tenant_domains.domain_name IS 'Domain hiển thị được owner nhập hoặc hệ thống cấp.';
COMMENT ON COLUMN platform.tenant_domains.normalized_domain_name IS 'Domain đã chuẩn hóa để lookup và enforce unique.';
COMMENT ON COLUMN platform.tenant_domains.domain_type IS 'Loại domain: DefaultSubdomain hoặc Custom.';
COMMENT ON COLUMN platform.tenant_domains.status IS 'Trạng thái domain trong Tenant MVP.';
COMMENT ON COLUMN platform.tenant_domains.is_primary IS 'Cho biết domain này là domain chính của tenant.';
COMMENT ON COLUMN platform.tenant_domains.created_at_utc IS 'Thời điểm domain được gắn vào tenant theo UTC.';
COMMENT ON COLUMN platform.tenant_domains.verified_at_utc IS 'Thời điểm domain được xác minh theo UTC nếu có.';
COMMENT ON CONSTRAINT ck_tenant_domains_type ON platform.tenant_domains IS 'Giới hạn loại domain được Tenant Service hỗ trợ trong Phase 2.';
COMMENT ON CONSTRAINT ck_tenant_domains_status ON platform.tenant_domains IS 'Giới hạn trạng thái domain được Tenant Service hỗ trợ trong Phase 2.';
COMMENT ON CONSTRAINT uq_tenant_domains_normalized_domain_name ON platform.tenant_domains IS 'Đảm bảo một domain đã chuẩn hóa chỉ thuộc một tenant.';
COMMENT ON INDEX platform.ix_tenant_domains_tenant_id IS 'Tăng tốc truy vấn domain theo tenant.';
COMMENT ON INDEX platform.ix_tenant_domains_tenant_id_is_primary IS 'Tăng tốc truy vấn domain chính của tenant.';

COMMENT ON TABLE platform.tenant_modules IS 'Lưu module được bật/tắt cho từng tenant.';
COMMENT ON COLUMN platform.tenant_modules.tenant_id IS 'Tenant sở hữu cấu hình module.';
COMMENT ON COLUMN platform.tenant_modules.module_code IS 'Mã module được bật/tắt.';
COMMENT ON COLUMN platform.tenant_modules.is_enabled IS 'Cho biết module đang bật hay tắt.';
COMMENT ON COLUMN platform.tenant_modules.source_plan_code IS 'Mã gói là nguồn bật module nếu có.';
COMMENT ON COLUMN platform.tenant_modules.created_at_utc IS 'Thời điểm cấu hình module được tạo theo UTC.';
COMMENT ON COLUMN platform.tenant_modules.updated_at_utc IS 'Thời điểm cấu hình module được cập nhật theo UTC.';
COMMENT ON INDEX platform.ix_tenant_modules_tenant_id IS 'Tăng tốc truy vấn module theo tenant.';
-- Addendum mirror cho migration 0003_add_tenant_domain_dns_ssl_state.sql.
-- Phuc vu local bootstrap DNS retry/SSL state; server runtime van apply migration file that.
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
