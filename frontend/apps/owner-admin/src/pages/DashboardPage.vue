<script setup lang="ts">
// Trang Dashboard ngắn cho Owner Super Admin.
// Tận dụng cùng tenantClient để derive metric tổng quan và preview vài tenant gần nhất.
// Không gọi API riêng để tránh trùng endpoint trong Phase 3 backend slice tối thiểu.
import type { TenantSummary } from "@clinic-saas/shared-types";
import { AppButton, AppCard, KPITile, StatusPill } from "@clinic-saas/ui";
import { computed, onMounted, ref } from "vue";
import { formatTenantStatus } from "../services/labels";
import { tenantClient } from "../services/tenantClient";

const tenants = ref<TenantSummary[]>([]);
const loading = ref(false);
const error = ref<string | null>(null);

const activeCount = computed(() => tenants.value.filter((tenant) => tenant.status === "Active").length);
const verifiedDomains = computed(() => tenants.value.filter((tenant) => tenant.domainStatus === "verified").length);
const suspendedCount = computed(() => tenants.value.filter((tenant) => tenant.status === "Suspended").length);
const supportCount = computed(() =>
  tenants.value.filter((tenant) => tenant.domainStatus === "failed" || tenant.status === "Draft").length
);

async function loadDashboard() {
  loading.value = true;
  error.value = null;

  try {
    tenants.value = await tenantClient.listTenants();
  } catch (loadError) {
    error.value = loadError instanceof Error ? loadError.message : "Không tải được dữ liệu tổng quan.";
  } finally {
    loading.value = false;
  }
}

onMounted(loadDashboard);
</script>

<template>
  <div class="dashboard-page">
    <section class="page-heading">
      <div>
        <p class="eyebrow">Owner Super Admin</p>
        <h2>Trung tâm vận hành phòng khám</h2>
      </div>
      <RouterLink to="/clinics/create">
        <AppButton label="Thêm phòng khám" />
      </RouterLink>
    </section>

    <div class="metrics-grid">
      <KPITile label="Phòng khám đang hoạt động" :value="activeCount" meta="Phòng khám đang vận hành" tone="success" />
      <KPITile label="Tên miền đã xác minh" :value="verifiedDomains" meta="Tên miền mặc định đã sẵn sàng" tone="info" />
      <KPITile label="Đã tạm ngưng" :value="suspendedCount" meta="Cần rà soát vòng đời" tone="danger" />
      <KPITile label="Cần hỗ trợ" :value="supportCount" meta="Bản nháp hoặc tên miền lỗi" tone="warning" />
    </div>

    <AppCard>
      <div class="operations-header">
        <div>
          <h3>Phòng khám gần đây</h3>
          <p>{{ loading ? "Đang tải phòng khám..." : `${tenants.length} phòng khám trong hệ thống.` }}</p>
        </div>
        <RouterLink to="/clinics">
          <AppButton label="Mở danh sách phòng khám" variant="secondary" />
        </RouterLink>
      </div>

      <div v-if="error" class="state">
        <p>{{ error }}</p>
        <AppButton label="Thử lại" variant="secondary" @click="loadDashboard" />
      </div>

      <div v-else-if="!loading && tenants.length === 0" class="empty">
        Chưa có phòng khám nào. Hãy thêm phòng khám đầu tiên để bắt đầu vận hành.
      </div>

      <div v-else class="tenant-strip">
        <article v-for="tenant in tenants.slice(0, 3)" :key="tenant.id">
          <div>
            <strong>{{ tenant.displayName }}</strong>
            <span>{{ tenant.defaultDomainName }}</span>
          </div>
          <StatusPill :label="formatTenantStatus(tenant.status)" :tone="tenant.status === 'Active' ? 'success' : 'warning'" />
        </article>
      </div>
    </AppCard>
  </div>
</template>

<style scoped>
.dashboard-page {
  display: grid;
  gap: 22px;
}

.page-heading,
.operations-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
}

.page-heading p,
.page-heading h2,
.operations-header h3,
.operations-header p,
.state p {
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

.metrics-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 16px;
}

.operations-header p {
  margin-top: 6px;
  color: #627d98;
}

.tenant-strip {
  display: grid;
  gap: 10px;
  margin-top: 18px;
}

.tenant-strip article {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  border: 1px solid #edf2f7;
  border-radius: 8px;
  padding: 14px;
}

.tenant-strip strong,
.tenant-strip span {
  display: block;
}

.tenant-strip span {
  margin-top: 4px;
  color: #627d98;
  font-size: 13px;
}

.state {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  margin-top: 18px;
  border-radius: 8px;
  padding: 16px;
  background: #fff7ed;
  color: #9a3412;
}

.empty {
  margin-top: 18px;
  border: 1px dashed #d9e2ec;
  border-radius: 8px;
  padding: 20px;
  color: #627d98;
  text-align: center;
}

a {
  text-decoration: none;
}

@media (max-width: 980px) {
  .metrics-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 640px) {
  .page-heading,
  .operations-header,
  .tenant-strip article,
  .state {
    align-items: stretch;
    flex-direction: column;
  }

  .metrics-grid {
    grid-template-columns: 1fr;
  }
}
</style>
