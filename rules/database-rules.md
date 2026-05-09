# Database Rules - Clinic SaaS

Áp dụng khi tạo/sửa schema, migration, SQL, repository persistence, index, seed data, database config hoặc data access.

## Bắt Buộc Đọc Trước Khi Đụng Database

- `AGENTS.md`
- `clinic_saas_report.md`
- `architech.txt`
- `docs/current-task.md`
- `temp/plan.md`
- `rules/coding-rules.md`
- `rules/backend-coding-rules.md` nếu liên quan backend
- File này

Không sửa database nếu scope chưa có trong `temp/plan.md` hoặc owner chưa yêu cầu rõ.

## Data Ownership

- PostgreSQL dùng cho relational/transactional data.
- MongoDB dùng cho CMS/page/template/layout JSON.
- Redis dùng cho cache, tenant config, domain mapping, rate limit, temporary lock.
- Kafka/Event Bus dùng cho async platform events khi cần scale.
- Mỗi service sở hữu schema/bảng/collection thuộc bounded context của nó.
- Không để service A query trực tiếp bảng private của service B nếu chưa có quyết định architecture; dùng API/event/contract.
- API Gateway không truy cập database.

## Multi-Tenant Bắt Buộc

- Mọi tenant-owned table phải có `tenant_id`.
- Mọi tenant-owned collection/document phải có `tenant_id`.
- Query tenant-owned data phải filter theo tenant context hợp lệ.
- Không query tenant-owned data nếu thiếu tenant context, trừ use case platform-scoped explicit của Owner Super Admin.
- Clinic Admin chỉ được đọc/ghi dữ liệu tenant của họ.
- Owner Super Admin cross-tenant phải là use case explicit.
- Cache key liên quan tenant phải prefix bằng `tenant:{tenant_id}:...`.

## Naming Convention

Chọn một convention và giữ nhất quán. Repo này chốt:

```txt
schema: lowercase snake_case
table: lowercase snake_case, số nhiều
column: lowercase snake_case
primary key: pk_<table>
foreign key: fk_<table>_<ref_or_column>
index: idx_<table>_<columns>
unique index/constraint: ux_<table>_<columns>
check constraint: ck_<table>_<rule>
```

Ví dụ đúng:

```txt
platform.tenants
platform.tenant_profiles
tenant_id
created_at_utc
pk_tenants
fk_tenant_profiles_tenant_id
idx_tenant_domains_tenant_id
ux_tenant_domains_normalized_domain_name
ck_tenants_status
```

Tránh:

```txt
Users
TenantId
"CreatedAt"
"Order"
```

Không dùng quoted PascalCase trong SQL. Tránh đặt tên trùng hoặc dễ nhầm với SQL keywords như `user`, `order`, `table`, `select`. Nếu domain bắt buộc có từ dễ trùng, dùng tên rõ hơn như `platform_users`, `purchase_orders`, `service_orders`.

## Comment Bằng Tiếng Việt

- SQL file phải có comment tiếng Việt ở phần đầu nói mục đích migration.
- Mỗi `CREATE TABLE` phải có comment ngay trước block nói bảng phục vụ nghiệp vụ gì.
- Mỗi column quan trọng trong `CREATE TABLE` phải có comment ngay phía trên dòng column để người đọc thấy ý nghĩa tại chỗ.
- Mỗi constraint/index quan trọng phải có comment ngay phía trên dòng định nghĩa constraint/index, nêu invariant/query path tương ứng.
- Table/column/index/function/procedure/view quan trọng vẫn phải có PostgreSQL metadata comment bằng `COMMENT ON` để database lưu được mô tả.
- Không đưa secret, connection string, IP server thật hoặc dữ liệu nhạy cảm vào comment.
- Comment phải giải thích ý nghĩa nghiệp vụ hoặc lý do kỹ thuật, không chỉ lặp lại tên cột.
- Với SQL function/procedure/view có input/output, comment phải nêu rõ đầu vào dùng để làm gì và đầu ra có ý nghĩa gì.

Ví dụ:

```sql
-- Tạo bảng tenant gốc phục vụ Owner Super Admin quản lý vòng đời phòng khám.
create table platform.tenants (
    -- Định danh tenant dùng trong API và các bảng con.
    id uuid primary key,

    -- Slug ngắn đã chuẩn hóa, dùng cho subdomain mặc định và thao tác quản trị.
    slug text not null,

    -- Trạng thái vòng đời tenant trong platform.
    status text not null
);

comment on table platform.tenants is 'Lưu tenant gốc của từng phòng khám trên nền tảng SaaS.';
comment on column platform.tenants.slug is 'Mã định danh ngắn dùng cho subdomain mặc định và thao tác quản trị.';
comment on column platform.tenants.status is 'Trạng thái vòng đời tenant: Draft, Active, Suspended hoặc Archived.';
```

## PostgreSQL Schema Design

- Schema phải explicit, ví dụ `platform`, không tạo bảng lung tung trong `public` nếu service đã có schema riêng.
- Primary key ưu tiên `uuid` cho entity public/API-facing.
- Timestamp dùng `created_at_utc`, `updated_at_utc`, `deleted_at_utc` nếu có soft delete.
- Dữ liệu tiền tệ không dùng `float/double`; dùng `numeric(precision, scale)` và lưu currency code nếu cần.
- Status dùng `text` + check constraint ở MVP nếu cần đơn giản; enum DB chỉ dùng khi đã chốt lifecycle ổn định.
- Domain/email/slug nên có normalized column nếu cần unique không phân biệt chữ hoa/thường.
- Unique constraint/index phải thể hiện đúng business invariant.
- Không thêm soft delete nếu chưa có use case.
- Không thêm audit/history table nếu chưa có scope, nhưng không thiết kế làm mất khả năng thêm sau.

## Index Rule

Khi tạo bảng hoặc thêm query mới, phải ghi rõ đường truy vấn chính và index tương ứng trong plan hoặc migration comment.

Phải cân nhắc index cho:

- Foreign key thường join/filter.
- `tenant_id` của tenant-owned table/collection.
- Unique business key như slug, normalized domain.
- Cột dùng trong filter/sort thường xuyên.
- Composite index cho query có nhiều điều kiện cố định.

Không tạo index bừa:

- Mỗi index làm chậm write và tốn storage.
- Không tạo index cho cột ít selectivity nếu không có query rõ.
- Không tạo composite index quá dài nếu chưa chứng minh cần; ưu tiên 2-3 cột quan trọng theo query path.
- Thứ tự cột trong composite index phải theo query thực tế: equality filter trước, range/sort sau.

Ví dụ query path:

```txt
List tenants: order by created_at_utc desc, filter status/search.
Lookup domain: normalized_domain_name unique.
Tenant child lookup: tenant_id + module_code.
```

## Database Migrations - Dapper / No ORM

Tenant Service hiện chốt:

```txt
Dapper + Npgsql
Không EF Core
Không EF migrations
SQL migration files
```

Quy tắc:

- Migration là SQL thuần, review được.
- Không auto-generate migration bằng ORM.
- File migration đặt trong service sở hữu schema.
- Với Tenant Service:

```txt
backend/services/tenant-service/src/TenantService.Infrastructure/Migrations/
```

- Tên file dạng:

```txt
0001_create_tenant_mvp.sql
0002_add_tenant_billing_snapshot.sql
```

hoặc nếu dùng timestamp:

```txt
20260509_create_tenant_mvp.sql
```

- `infrastructure/postgres/init.sql` chỉ là bootstrap/mirror local, không thay thế migration history của service.
- Nếu cần migration runner, ưu tiên `DbUp` vì phù hợp SQL-first. Chưa thêm runner nếu scope chưa yêu cầu.
- Migration phải có hướng rollback hoặc ghi rõ vì sao rollback thủ công/không an toàn.
- Không chạy `DROP`, destructive `DELETE`, truncate hoặc rewrite dữ liệu lớn nếu chưa có owner duyệt riêng.

## Dapper / SQL Rules

- Mọi SQL nhận input phải dùng parameterized query.
- Không nối chuỗi SQL trực tiếp từ input người dùng.
- Chỉ build dynamic SQL từ whitelist nội bộ đã validate, ví dụ sort field enum.
- Repository method có I/O phải async và nhận `CancellationToken`.
- Multi-table write phải dùng transaction boundary rõ ràng.
- Map PostgreSQL unique violation/check violation sang domain/application error rõ ràng.
- Không leak SQL exception/raw stack trace ra API response ngoài Development.
- Không log parameter chứa secret/token/password/connection string.

## MongoDB Rules

- Tenant-owned document phải có `tenant_id`.
- Collection CMS/template/page config phải có index theo `tenant_id`.
- Draft/published content nên có field trạng thái rõ nếu có publish flow.
- Không lưu relational transactional core data vào MongoDB nếu PostgreSQL phù hợp hơn.
- JSON layout/template cần version field để migrate sau này.

## Seed Data

- Seed chỉ dùng dữ liệu placeholder/dev.
- Không seed thông tin bệnh nhân, số điện thoại, email, IP, domain thật nếu owner chưa cung cấp rõ.
- Seed tenant phải có slug/domain giả, ví dụ `demo-clinic.local`.
- Seed phải idempotent nếu chạy nhiều lần trong local/dev.

## Verify Database Change

Tối thiểu phải chạy hoặc ghi rõ vì sao chưa chạy:

```powershell
docker compose -f infrastructure/docker/docker-compose.dev.yml config
```

Nếu có PostgreSQL local:

```txt
1. Apply migration/init SQL.
2. Kiểm tra schema/table/constraint/index tồn tại.
3. Chạy service liên quan.
4. Smoke API create/list/get/update nếu có endpoint.
5. Test duplicate unique key trả 409 hoặc lỗi domain tương ứng.
6. Test tenant isolation với tenant_id đúng/sai nếu endpoint tenant-scoped.
```

Với Phase 2 Tenant MVP Backend, chưa đánh dấu Done nếu chưa verify được:

```txt
POST /api/tenants
GET /api/tenants
GET /api/tenants/{id}
PATCH /api/tenants/{id}/status
duplicate slug/domain trả 409
```

## Checklist Trước Khi Submit Database Change

- [ ] Đúng scope `temp/plan.md`.
- [ ] Đọc `rules/database-rules.md`.
- [ ] Không dùng secret/connection string production.
- [ ] Naming `snake_case`, lowercase, không quoted PascalCase.
- [ ] Table/column/constraint/index quan trọng có comment tiếng Việt ngay trong block SQL.
- [ ] PostgreSQL object metadata có `COMMENT ON` cho table/column/constraint/index quan trọng.
- [ ] Tenant-owned data có `tenant_id`.
- [ ] Index khớp query path thật, không index bừa.
- [ ] Unique/check constraint thể hiện business rule.
- [ ] Migration SQL rõ ràng, review được.
- [ ] Dapper SQL parameterized.
- [ ] Multi-table write có transaction.
- [ ] `infrastructure/postgres/init.sql` đồng bộ nếu thay đổi local bootstrap.
- [ ] Verify command đã chạy hoặc blocker đã ghi rõ.
