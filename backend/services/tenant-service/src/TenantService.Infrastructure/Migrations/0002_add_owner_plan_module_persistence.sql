-- Tạo persistence thật cho Owner Plan & Module Catalog trong Tenant Service.
-- Rollback thủ công nếu cần: chỉ drop các bảng 0002 theo thứ tự phụ thuộc sau khi đã backup dữ liệu audit.

CREATE SCHEMA IF NOT EXISTS platform;

-- Bảng catalog plan do Owner Super Admin quản lý ở phạm vi platform.
CREATE TABLE IF NOT EXISTS platform.plans (
  -- Mã plan ổn định được FE và backend dùng trong contract.
  plan_code text PRIMARY KEY,

  -- Tên plan hiển thị cho Owner Admin.
  name text NOT NULL,

  -- Mô tả ngắn giúp Owner Admin so sánh plan.
  description text NULL,

  -- Giá MRR hiện tại của plan theo currency_code.
  price_monthly numeric(12,2) NOT NULL,

  -- Mã tiền tệ ISO dùng cho giá MRR.
  currency_code text NOT NULL DEFAULT 'USD',

  -- Tone UI hiện tại để giữ contract FE.
  tone text NULL,

  -- Cho biết plan được đánh dấu phổ biến trên UI.
  is_popular boolean NOT NULL DEFAULT false,

  -- Thứ tự hiển thị plan trong catalog.
  display_order integer NOT NULL,

  -- Cho biết plan còn được chọn trong bulk-change hay không.
  is_active boolean NOT NULL DEFAULT true,

  -- Thời điểm tạo plan catalog theo UTC.
  created_at_utc timestamptz NOT NULL,

  -- Thời điểm cập nhật plan catalog theo UTC.
  updated_at_utc timestamptz NOT NULL,

  -- Giá plan không được âm.
  CONSTRAINT ck_plans_price_monthly_non_negative CHECK (price_monthly >= 0),

  -- Currency code không được rỗng.
  CONSTRAINT ck_plans_currency_code_not_empty CHECK (length(trim(currency_code)) > 0)
);

-- Tăng tốc list plan active theo thứ tự hiển thị.
CREATE INDEX IF NOT EXISTS ix_plans_is_active_display_order
  ON platform.plans (is_active, display_order);

-- Bảng module catalog cấp platform, không phải trạng thái bật/tắt riêng của tenant.
CREATE TABLE IF NOT EXISTS platform.modules (
  -- Mã module ổn định dùng trong entitlement matrix.
  module_code text PRIMARY KEY,

  -- Tên module hiển thị cho Owner Admin.
  name text NOT NULL,

  -- Nhóm module trong catalog.
  category text NOT NULL,

  -- Mô tả module nếu cần hiển thị sau này.
  description text NULL,

  -- Thứ tự hiển thị module trong matrix.
  display_order integer NOT NULL,

  -- Cho biết module còn active trong catalog hay không.
  is_active boolean NOT NULL DEFAULT true,

  -- Thời điểm tạo module catalog theo UTC.
  created_at_utc timestamptz NOT NULL,

  -- Thời điểm cập nhật module catalog theo UTC.
  updated_at_utc timestamptz NOT NULL
);

-- Tăng tốc nhóm module theo category và thứ tự hiển thị.
CREATE INDEX IF NOT EXISTS ix_modules_category_display_order
  ON platform.modules (category, display_order);

-- Tăng tốc list module active theo thứ tự hiển thị.
CREATE INDEX IF NOT EXISTS ix_modules_is_active_display_order
  ON platform.modules (is_active, display_order);

-- Bảng entitlement mô tả mỗi plan được dùng module nào và giới hạn ra sao.
CREATE TABLE IF NOT EXISTS platform.plan_module_entitlements (
  -- Plan sở hữu entitlement.
  plan_code text NOT NULL REFERENCES platform.plans (plan_code),

  -- Module được cấu hình trong plan.
  module_code text NOT NULL REFERENCES platform.modules (module_code),

  -- Cho biết module được bật trong plan hay không.
  is_enabled boolean NOT NULL,

  -- Giá trị giới hạn kỹ thuật nếu entitlement là quota.
  limit_value text NULL,

  -- Chuỗi hiển thị giữ nguyên contract FE, ví dụ unlimited hoặc 100/month.
  display_value text NULL,

  -- Thời điểm tạo entitlement theo UTC.
  created_at_utc timestamptz NOT NULL,

  -- Thời điểm cập nhật entitlement theo UTC.
  updated_at_utc timestamptz NOT NULL,

  -- Mỗi plan chỉ có một entitlement cho mỗi module.
  CONSTRAINT pk_plan_module_entitlements PRIMARY KEY (plan_code, module_code)
);

-- Tăng tốc query matrix theo plan.
CREATE INDEX IF NOT EXISTS ix_plan_module_entitlements_plan_code
  ON platform.plan_module_entitlements (plan_code);

-- Tăng tốc query module nào xuất hiện trong các plan.
CREATE INDEX IF NOT EXISTS ix_plan_module_entitlements_module_code
  ON platform.plan_module_entitlements (module_code);

-- Bảng assignment plan hiện tại của tenant, phục vụ Owner Super Admin bulk-change.
CREATE TABLE IF NOT EXISTS platform.tenant_plan_assignments (
  -- Định danh assignment, wave này backfill bằng tenant_id để idempotent.
  id uuid PRIMARY KEY,

  -- Tenant sở hữu assignment plan.
  tenant_id uuid NOT NULL REFERENCES platform.tenants (id) ON DELETE CASCADE,

  -- Plan hiện tại đang áp dụng cho tenant.
  current_plan_code text NOT NULL REFERENCES platform.plans (plan_code),

  -- Plan mục tiêu gần nhất nếu có thao tác đổi plan.
  target_plan_code text NULL REFERENCES platform.plans (plan_code),

  -- Cách áp dụng thay đổi; wave này chỉ hỗ trợ next_renewal.
  effective_at text NOT NULL DEFAULT 'next_renewal',

  -- Ngày gia hạn kế tiếp nếu Billing phase sau cung cấp.
  next_renewal_date date NULL,

  -- MRR hiện tại của tenant tại thời điểm assignment được lưu.
  current_mrr numeric(12,2) NOT NULL,

  -- Mã tiền tệ của current_mrr.
  currency_code text NOT NULL DEFAULT 'USD',

  -- Trạng thái assignment hiện tại.
  status text NOT NULL DEFAULT 'active',

  -- Lý do audit gần nhất do Owner Super Admin nhập.
  audit_reason text NULL,

  -- Actor/user id gần nhất nếu auth provider cung cấp.
  assigned_by_user_id text NULL,

  -- Thời điểm assignment được tạo hoặc đổi gần nhất theo UTC.
  assigned_at_utc timestamptz NOT NULL,

  -- Thời điểm assignment được cập nhật gần nhất theo UTC.
  updated_at_utc timestamptz NOT NULL,

  -- Version lạc quan để phase sau kiểm soát conflict nếu cần.
  version integer NOT NULL DEFAULT 1,

  -- Mỗi tenant chỉ có một assignment plan hiện tại.
  CONSTRAINT ux_tenant_plan_assignments_tenant_id UNIQUE (tenant_id),

  -- Wave này chỉ hỗ trợ đổi ở kỳ gia hạn kế tiếp.
  CONSTRAINT ck_tenant_plan_assignments_effective_at CHECK (effective_at IN ('next_renewal')),

  -- Trạng thái assignment được giới hạn để tránh giá trị mơ hồ.
  CONSTRAINT ck_tenant_plan_assignments_status CHECK (status IN ('active', 'scheduled', 'archived')),

  -- MRR không được âm.
  CONSTRAINT ck_tenant_plan_assignments_current_mrr_non_negative CHECK (current_mrr >= 0)
);

-- Tăng tốc lọc tenant theo plan hiện tại.
CREATE INDEX IF NOT EXISTS ix_tenant_plan_assignments_current_plan_code
  ON platform.tenant_plan_assignments (current_plan_code);

-- Tăng tốc lọc tenant theo plan mục tiêu.
CREATE INDEX IF NOT EXISTS ix_tenant_plan_assignments_target_plan_code
  ON platform.tenant_plan_assignments (target_plan_code);

-- Tăng tốc query batch theo trạng thái và ngày gia hạn.
CREATE INDEX IF NOT EXISTS ix_tenant_plan_assignments_status_next_renewal_date
  ON platform.tenant_plan_assignments (status, next_renewal_date);

-- Bảng audit tối thiểu cho mọi bulk-change owner-plan.
CREATE TABLE IF NOT EXISTS platform.tenant_plan_assignment_changes (
  -- Định danh audit row.
  id uuid PRIMARY KEY,

  -- Định danh một lần bulk operation để gom các tenant thay đổi cùng request.
  bulk_operation_id uuid NULL,

  -- Tenant được đổi plan.
  tenant_id uuid NOT NULL REFERENCES platform.tenants (id) ON DELETE CASCADE,

  -- Plan trước khi đổi.
  from_plan_code text NOT NULL,

  -- Plan sau khi đổi.
  to_plan_code text NOT NULL,

  -- Cách áp dụng thay đổi; wave này chỉ hỗ trợ next_renewal.
  effective_at text NOT NULL,

  -- Lý do audit bắt buộc do Owner Super Admin nhập.
  audit_reason text NOT NULL,

  -- Actor/user id nếu auth provider cung cấp.
  actor_user_id text NULL,

  -- Thời điểm tạo audit row theo UTC.
  created_at_utc timestamptz NOT NULL
);

-- Tăng tốc xem lịch sử đổi plan của một tenant.
CREATE INDEX IF NOT EXISTS ix_tenant_plan_assignment_changes_tenant_id_created_at
  ON platform.tenant_plan_assignment_changes (tenant_id, created_at_utc DESC);

-- Tăng tốc truy vấn audit rows theo bulk operation.
CREATE INDEX IF NOT EXISTS ix_tenant_plan_assignment_changes_bulk_operation_id
  ON platform.tenant_plan_assignment_changes (bulk_operation_id);

INSERT INTO platform.plans (
  plan_code,
  name,
  description,
  price_monthly,
  currency_code,
  tone,
  is_popular,
  display_order,
  is_active,
  created_at_utc,
  updated_at_utc)
VALUES
  ('starter', 'Starter', 'Cho phong kham nho, toi da 2 bac si', 49, 'USD', 'info', false, 10, true, now(), now()),
  ('growth', 'Growth', 'Pho bien nhat cho phong kham 3-10 bac si', 129, 'USD', 'neutral', true, 20, true, now(), now()),
  ('premium', 'Premium', 'Phong kham lon, da chi nhanh va van hanh nang cao', 299, 'USD', 'warning', false, 30, true, now(), now())
ON CONFLICT (plan_code) DO UPDATE
SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price_monthly = EXCLUDED.price_monthly,
  currency_code = EXCLUDED.currency_code,
  tone = EXCLUDED.tone,
  is_popular = EXCLUDED.is_popular,
  display_order = EXCLUDED.display_order,
  is_active = EXCLUDED.is_active,
  updated_at_utc = now();

INSERT INTO platform.modules (
  module_code,
  name,
  category,
  description,
  display_order,
  is_active,
  created_at_utc,
  updated_at_utc)
VALUES
  ('public-website', 'Public Website', 'Website', NULL, 10, true, now(), now()),
  ('booking-online', 'Booking Online', 'Booking', NULL, 20, true, now(), now()),
  ('doctor-schedule', 'Doctor Schedule', 'Clinic Ops', NULL, 30, true, now(), now()),
  ('patient-records', 'Patient Records', 'Customer', NULL, 40, true, now(), now()),
  ('apso-eprescribe', 'APSO E-prescribe', 'Records', NULL, 50, true, now(), now()),
  ('custom-domain', 'Custom domain', 'Domain', NULL, 60, true, now(), now()),
  ('ssl-auto-renew', 'SSL auto-renew', 'Domain', NULL, 70, true, now(), now()),
  ('telehealth', 'Telehealth', 'Care', NULL, 80, true, now(), now()),
  ('reports-analytics', 'Reports analytics', 'Analytics', NULL, 90, true, now(), now()),
  ('multi-branch', 'Multi-branch', 'Operations', NULL, 100, true, now(), now()),
  ('api-access', 'API access', 'Platform', NULL, 110, true, now(), now()),
  ('priority-support', 'Priority support', 'Support', NULL, 120, true, now(), now())
ON CONFLICT (module_code) DO UPDATE
SET
  name = EXCLUDED.name,
  category = EXCLUDED.category,
  description = EXCLUDED.description,
  display_order = EXCLUDED.display_order,
  is_active = EXCLUDED.is_active,
  updated_at_utc = now();

INSERT INTO platform.plan_module_entitlements (
  plan_code,
  module_code,
  is_enabled,
  limit_value,
  display_value,
  created_at_utc,
  updated_at_utc)
VALUES
  ('starter', 'public-website', true, NULL, 'limit-3-pages', now(), now()),
  ('growth', 'public-website', true, NULL, 'unlimited', now(), now()),
  ('premium', 'public-website', true, NULL, 'unlimited', now(), now()),
  ('starter', 'booking-online', true, NULL, '100/month', now(), now()),
  ('growth', 'booking-online', true, NULL, '1000/month', now(), now()),
  ('premium', 'booking-online', true, NULL, 'unlimited', now(), now()),
  ('starter', 'doctor-schedule', true, NULL, NULL, now(), now()),
  ('growth', 'doctor-schedule', true, NULL, NULL, now(), now()),
  ('premium', 'doctor-schedule', true, NULL, NULL, now(), now()),
  ('starter', 'patient-records', false, NULL, NULL, now(), now()),
  ('growth', 'patient-records', true, NULL, NULL, now(), now()),
  ('premium', 'patient-records', true, NULL, NULL, now(), now()),
  ('starter', 'apso-eprescribe', false, NULL, NULL, now(), now()),
  ('growth', 'apso-eprescribe', false, NULL, NULL, now(), now()),
  ('premium', 'apso-eprescribe', true, NULL, NULL, now(), now()),
  ('starter', 'custom-domain', false, NULL, NULL, now(), now()),
  ('growth', 'custom-domain', true, NULL, NULL, now(), now()),
  ('premium', 'custom-domain', true, NULL, NULL, now(), now()),
  ('starter', 'ssl-auto-renew', true, NULL, NULL, now(), now()),
  ('growth', 'ssl-auto-renew', true, NULL, NULL, now(), now()),
  ('premium', 'ssl-auto-renew', true, NULL, NULL, now(), now()),
  ('starter', 'telehealth', false, NULL, NULL, now(), now()),
  ('growth', 'telehealth', true, NULL, 'limit-50/m', now(), now()),
  ('premium', 'telehealth', true, NULL, 'unlimited', now(), now()),
  ('starter', 'reports-analytics', true, NULL, 'basic', now(), now()),
  ('growth', 'reports-analytics', true, NULL, 'advanced', now(), now()),
  ('premium', 'reports-analytics', true, NULL, 'advanced + export', now(), now()),
  ('starter', 'multi-branch', false, NULL, NULL, now(), now()),
  ('growth', 'multi-branch', false, NULL, NULL, now(), now()),
  ('premium', 'multi-branch', true, NULL, NULL, now(), now()),
  ('starter', 'api-access', false, NULL, NULL, now(), now()),
  ('growth', 'api-access', false, NULL, NULL, now(), now()),
  ('premium', 'api-access', true, NULL, NULL, now(), now()),
  ('starter', 'priority-support', false, NULL, NULL, now(), now()),
  ('growth', 'priority-support', true, NULL, 'email', now(), now()),
  ('premium', 'priority-support', true, NULL, 'phone+email 24/7', now(), now())
ON CONFLICT (plan_code, module_code) DO UPDATE
SET
  is_enabled = EXCLUDED.is_enabled,
  limit_value = EXCLUDED.limit_value,
  display_value = EXCLUDED.display_value,
  updated_at_utc = now();

INSERT INTO platform.tenant_plan_assignments (
  id,
  tenant_id,
  current_plan_code,
  target_plan_code,
  effective_at,
  next_renewal_date,
  current_mrr,
  currency_code,
  status,
  audit_reason,
  assigned_by_user_id,
  assigned_at_utc,
  updated_at_utc,
  version)
SELECT
  t.id,
  t.id,
  CASE WHEN p.plan_code IS NULL THEN 'starter' ELSE t.plan_code END,
  CASE WHEN p.plan_code IS NULL THEN 'starter' ELSE t.plan_code END,
  'next_renewal',
  (current_date + interval '30 days')::date,
  coalesce(p.price_monthly, 49),
  coalesce(p.currency_code, 'USD'),
  'active',
  'Initial backfill from platform.tenants plan snapshot.',
  NULL,
  coalesce(t.created_at_utc, now()),
  now(),
  1
FROM platform.tenants t
LEFT JOIN platform.plans p ON p.plan_code = t.plan_code
ON CONFLICT (tenant_id) DO UPDATE
SET
  current_plan_code = EXCLUDED.current_plan_code,
  target_plan_code = EXCLUDED.target_plan_code,
  current_mrr = EXCLUDED.current_mrr,
  currency_code = EXCLUDED.currency_code,
  updated_at_utc = now();

COMMENT ON TABLE platform.plans IS 'Lưu catalog plan của nền tảng cho Owner Super Admin.';
COMMENT ON COLUMN platform.plans.plan_code IS 'Mã plan ổn định dùng trong API và bulk-change.';
COMMENT ON COLUMN platform.plans.name IS 'Tên plan hiển thị cho Owner Admin.';
COMMENT ON COLUMN platform.plans.description IS 'Mô tả ngắn của plan.';
COMMENT ON COLUMN platform.plans.price_monthly IS 'Giá MRR hiện tại của plan.';
COMMENT ON COLUMN platform.plans.currency_code IS 'Mã tiền tệ của giá plan.';
COMMENT ON COLUMN platform.plans.tone IS 'Tone UI để giữ contract với frontend hiện tại.';
COMMENT ON COLUMN platform.plans.is_popular IS 'Cho biết plan được đánh dấu phổ biến.';
COMMENT ON COLUMN platform.plans.display_order IS 'Thứ tự hiển thị plan trong catalog.';
COMMENT ON COLUMN platform.plans.is_active IS 'Cho biết plan còn được chọn trong bulk-change hay không.';
COMMENT ON CONSTRAINT ck_plans_price_monthly_non_negative ON platform.plans IS 'Đảm bảo giá plan không âm.';
COMMENT ON CONSTRAINT ck_plans_currency_code_not_empty ON platform.plans IS 'Đảm bảo currency code không rỗng.';
COMMENT ON INDEX platform.ix_plans_is_active_display_order IS 'Tăng tốc list plan active theo thứ tự hiển thị.';

COMMENT ON TABLE platform.modules IS 'Lưu module catalog cấp platform cho entitlement matrix.';
COMMENT ON COLUMN platform.modules.module_code IS 'Mã module ổn định dùng trong API và entitlement.';
COMMENT ON COLUMN platform.modules.name IS 'Tên module hiển thị cho Owner Admin.';
COMMENT ON COLUMN platform.modules.category IS 'Nhóm module trong catalog.';
COMMENT ON COLUMN platform.modules.display_order IS 'Thứ tự hiển thị module trong matrix.';
COMMENT ON COLUMN platform.modules.is_active IS 'Cho biết module còn active trong catalog hay không.';
COMMENT ON INDEX platform.ix_modules_category_display_order IS 'Tăng tốc nhóm module theo category.';
COMMENT ON INDEX platform.ix_modules_is_active_display_order IS 'Tăng tốc list module active theo thứ tự hiển thị.';

COMMENT ON TABLE platform.plan_module_entitlements IS 'Lưu quyền sử dụng module theo từng plan.';
COMMENT ON COLUMN platform.plan_module_entitlements.plan_code IS 'Plan sở hữu entitlement.';
COMMENT ON COLUMN platform.plan_module_entitlements.module_code IS 'Module được cấu hình trong plan.';
COMMENT ON COLUMN platform.plan_module_entitlements.is_enabled IS 'Cho biết module được bật trong plan hay không.';
COMMENT ON COLUMN platform.plan_module_entitlements.limit_value IS 'Giá trị quota kỹ thuật nếu có.';
COMMENT ON COLUMN platform.plan_module_entitlements.display_value IS 'Chuỗi hiển thị giữ contract FE.';
COMMENT ON CONSTRAINT pk_plan_module_entitlements ON platform.plan_module_entitlements IS 'Mỗi plan chỉ có một entitlement cho mỗi module.';
COMMENT ON INDEX platform.ix_plan_module_entitlements_plan_code IS 'Tăng tốc query entitlement theo plan.';
COMMENT ON INDEX platform.ix_plan_module_entitlements_module_code IS 'Tăng tốc query entitlement theo module.';

COMMENT ON TABLE platform.tenant_plan_assignments IS 'Lưu plan assignment hiện tại của mỗi tenant.';
COMMENT ON COLUMN platform.tenant_plan_assignments.id IS 'Định danh assignment plan.';
COMMENT ON COLUMN platform.tenant_plan_assignments.tenant_id IS 'Tenant sở hữu assignment plan.';
COMMENT ON COLUMN platform.tenant_plan_assignments.current_plan_code IS 'Plan hiện tại đang áp dụng cho tenant.';
COMMENT ON COLUMN platform.tenant_plan_assignments.target_plan_code IS 'Plan mục tiêu gần nhất nếu có thao tác đổi plan.';
COMMENT ON COLUMN platform.tenant_plan_assignments.effective_at IS 'Cách áp dụng thay đổi plan.';
COMMENT ON COLUMN platform.tenant_plan_assignments.next_renewal_date IS 'Ngày gia hạn kế tiếp nếu có.';
COMMENT ON COLUMN platform.tenant_plan_assignments.current_mrr IS 'MRR hiện tại của tenant.';
COMMENT ON COLUMN platform.tenant_plan_assignments.audit_reason IS 'Lý do audit gần nhất do Owner Super Admin nhập.';
COMMENT ON CONSTRAINT ux_tenant_plan_assignments_tenant_id ON platform.tenant_plan_assignments IS 'Mỗi tenant chỉ có một assignment plan hiện tại.';
COMMENT ON CONSTRAINT ck_tenant_plan_assignments_effective_at ON platform.tenant_plan_assignments IS 'Wave này chỉ hỗ trợ next_renewal.';
COMMENT ON CONSTRAINT ck_tenant_plan_assignments_status ON platform.tenant_plan_assignments IS 'Giới hạn trạng thái assignment.';
COMMENT ON CONSTRAINT ck_tenant_plan_assignments_current_mrr_non_negative ON platform.tenant_plan_assignments IS 'Đảm bảo MRR không âm.';
COMMENT ON INDEX platform.ix_tenant_plan_assignments_current_plan_code IS 'Tăng tốc lọc tenant theo plan hiện tại.';
COMMENT ON INDEX platform.ix_tenant_plan_assignments_target_plan_code IS 'Tăng tốc lọc tenant theo plan mục tiêu.';
COMMENT ON INDEX platform.ix_tenant_plan_assignments_status_next_renewal_date IS 'Tăng tốc batch theo trạng thái và ngày gia hạn.';

COMMENT ON TABLE platform.tenant_plan_assignment_changes IS 'Lưu audit history cho các lần Owner Super Admin đổi plan tenant.';
COMMENT ON COLUMN platform.tenant_plan_assignment_changes.bulk_operation_id IS 'Định danh gom các tenant thay đổi cùng request.';
COMMENT ON COLUMN platform.tenant_plan_assignment_changes.tenant_id IS 'Tenant được đổi plan.';
COMMENT ON COLUMN platform.tenant_plan_assignment_changes.from_plan_code IS 'Plan trước khi đổi.';
COMMENT ON COLUMN platform.tenant_plan_assignment_changes.to_plan_code IS 'Plan sau khi đổi.';
COMMENT ON COLUMN platform.tenant_plan_assignment_changes.effective_at IS 'Cách áp dụng thay đổi plan.';
COMMENT ON COLUMN platform.tenant_plan_assignment_changes.audit_reason IS 'Lý do audit bắt buộc do Owner Super Admin nhập.';
COMMENT ON COLUMN platform.tenant_plan_assignment_changes.actor_user_id IS 'Actor/user id nếu auth provider cung cấp.';
COMMENT ON INDEX platform.ix_tenant_plan_assignment_changes_tenant_id_created_at IS 'Tăng tốc xem lịch sử đổi plan của một tenant.';
COMMENT ON INDEX platform.ix_tenant_plan_assignment_changes_bulk_operation_id IS 'Tăng tốc truy vấn audit rows theo bulk operation.';
