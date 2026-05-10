<script setup lang="ts">
// Trang chi tiết tenant tương ứng route `/clinics/:tenantId`.
// Page này có cùng dữ liệu nguồn với drawer ở `/clinics`, nhưng được trình bày dạng full-page
// để Owner Admin có thể bookmark/chia sẻ link riêng từng tenant.
import type { TenantDetail, TenantStatus } from "@clinic-saas/shared-types";
import { AppButton, AppCard, StatusPill } from "@clinic-saas/ui";
import { computed, onMounted, ref, watch } from "vue";
import { formatDomainStatus, formatModuleCode, formatTenantStatus } from "../services/labels";
import { tenantClient } from "../services/tenantClient";

const props = defineProps<{
  /** Tenant ID lấy từ route params, dùng để fetch chi tiết. */
  tenantId: string;
}>();

const tenant = ref<TenantDetail | undefined>();
const loading = ref(false);
const error = ref<string | null>(null);

const nextStatus = computed<TenantStatus>(() => {
  if (!tenant.value || tenant.value.status !== "Active") {
    return "Active";
  }

  return "Suspended";
});

function statusTone(status: TenantStatus) {
  if (status === "Active") {
    return "success";
  }

  if (status === "Suspended" || status === "Archived") {
    return "danger";
  }

  return "warning";
}

async function loadTenant() {
  loading.value = true;
  error.value = null;

  try {
    tenant.value = await tenantClient.getTenant(props.tenantId);
  } catch (loadError) {
    error.value = loadError instanceof Error ? loadError.message : "Không tải được phòng khám.";
  } finally {
    loading.value = false;
  }
}

async function updateStatus() {
  if (!tenant.value) {
    return;
  }

  loading.value = true;

  try {
    tenant.value = await tenantClient.updateTenantStatus(tenant.value.id, { status: nextStatus.value });
  } catch (updateError) {
    error.value = updateError instanceof Error ? updateError.message : "Không cập nhật được trạng thái phòng khám.";
  } finally {
    loading.value = false;
  }
}

onMounted(loadTenant);
watch(() => props.tenantId, loadTenant);
</script>

<template>
  <div class="detail-page">
    <section class="page-heading">
      <div>
        <p class="eyebrow">Chi tiết phòng khám</p>
        <h2>{{ tenant?.displayName ?? "Đang tải phòng khám..." }}</h2>
      </div>
      <div class="heading-actions">
        <RouterLink to="/clinics">
          <AppButton label="Quay lại danh sách" variant="secondary" />
        </RouterLink>
        <AppButton
          v-if="tenant"
          :label="nextStatus === 'Active' ? 'Kích hoạt phòng khám' : 'Tạm ngưng phòng khám'"
          :variant="nextStatus === 'Active' ? 'primary' : 'danger'"
          :loading="loading"
          @click="updateStatus"
        />
      </div>
    </section>

    <div v-if="error" class="error-state">
      <span>{{ error }}</span>
      <AppButton label="Thử lại" variant="secondary" @click="loadTenant" />
    </div>

    <AppCard v-if="loading && !tenant">
      <div class="loading-state">Đang tải hồ sơ phòng khám...</div>
    </AppCard>

    <template v-else-if="tenant">
      <div class="detail-grid">
        <AppCard>
          <div class="status-row">
            <StatusPill :label="formatTenantStatus(tenant.status)" :tone="statusTone(tenant.status)" />
            <StatusPill :label="formatDomainStatus(tenant.domainStatus)" tone="info" />
          </div>
          <dl>
            <div>
              <dt>Mã phòng khám</dt>
              <dd>{{ tenant.id }}</dd>
            </div>
            <div>
              <dt>Slug</dt>
              <dd>{{ tenant.slug }}</dd>
            </div>
            <div>
              <dt>Gói</dt>
              <dd>{{ tenant.planDisplayName }}</dd>
            </div>
            <div>
              <dt>Tên phòng khám</dt>
              <dd>{{ tenant.clinicName }}</dd>
            </div>
            <div>
              <dt>Email chủ sở hữu</dt>
              <dd>{{ tenant.contactEmail }}</dd>
            </div>
            <div>
              <dt>Số điện thoại</dt>
              <dd>{{ tenant.phoneNumber }}</dd>
            </div>
            <div class="wide">
              <dt>Địa chỉ</dt>
              <dd>{{ tenant.addressLine }}</dd>
            </div>
          </dl>
        </AppCard>

        <AppCard>
          <h3>Tên miền</h3>
          <ul class="domain-list">
            <li v-for="domain in tenant.domains" :key="domain.id">
              <span>{{ domain.domainName }}</span>
              <StatusPill :label="formatDomainStatus(domain.status)" tone="info" />
            </li>
          </ul>
        </AppCard>
      </div>

      <AppCard>
        <h3>Module đang bật</h3>
        <div class="module-list">
          <StatusPill
            v-for="moduleCode in tenant.moduleCodes"
            :key="moduleCode"
            :label="formatModuleCode(moduleCode)"
            tone="neutral"
          />
        </div>
      </AppCard>
    </template>
  </div>
</template>

<style scoped>
.detail-page {
  display: grid;
  gap: 20px;
}

.page-heading,
.heading-actions,
.status-row,
.module-list {
  display: flex;
  align-items: center;
  gap: 12px;
}

.page-heading {
  justify-content: space-between;
}

.page-heading p,
.page-heading h2,
h3,
dl {
  margin: 0;
}

.eyebrow {
  color: #0e7c86;
  font-size: 12px;
  font-weight: 800;
  text-transform: uppercase;
}

.page-heading h2 {
  margin-top: 6px;
  font-size: 28px;
}

a {
  text-decoration: none;
}

.detail-grid {
  display: grid;
  grid-template-columns: minmax(0, 1fr) 380px;
  gap: 20px;
}

dl {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 16px;
  margin-top: 18px;
}

dt {
  color: #627d98;
  font-size: 12px;
  font-weight: 800;
}

dd {
  margin: 4px 0 0;
  overflow-wrap: anywhere;
}

.wide {
  grid-column: 1 / -1;
}

.domain-list {
  display: grid;
  gap: 10px;
  margin: 14px 0 0;
  padding: 0;
  list-style: none;
}

.domain-list li {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.module-list {
  flex-wrap: wrap;
}

.error-state,
.loading-state {
  border-radius: 8px;
  padding: 16px;
}

.error-state {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  background: #fff7ed;
  color: #9a3412;
}

.loading-state {
  color: #627d98;
  text-align: center;
}

@media (max-width: 980px) {
  .detail-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 640px) {
  .page-heading,
  .heading-actions,
  .error-state {
    align-items: stretch;
    flex-direction: column;
  }

  dl {
    grid-template-columns: 1fr;
  }
}
</style>
