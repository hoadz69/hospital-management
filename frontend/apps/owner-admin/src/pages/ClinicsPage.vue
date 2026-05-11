<script setup lang="ts">
// Trang chính Owner Admin Tenant Slice. Bám frame Figma `V2 - Owner Admin Tenant Operations`.
// Trách nhiệm:
//  - Load list tenant qua tenantClient (real hoặc mock fallback).
//  - Quản lý filter theo status/plan/domain/module và metric tổng hợp.
//  - Mở drawer detail và cho phép cập nhật trạng thái suspend/activate.
//  - Hiển thị placeholder cho conflict 409 và CTA mở wizard tạo tenant ở footer page.
import type { TenantDetail, TenantStatus, TenantSummary } from "@clinic-saas/shared-types";
import { AppButton, AppCard, EmptyState, KPITile } from "@clinic-saas/ui";
import { computed, onMounted, reactive, ref } from "vue";
import TenantDetailDrawer from "../components/TenantDetailDrawer.vue";
import TenantFilterBar, { type TenantFilters } from "../components/TenantFilterBar.vue";
import TenantTable from "../components/TenantTable.vue";
import { tenantClient } from "../services/tenantClient";

const tenants = ref<TenantSummary[]>([]);
const selectedTenant = ref<TenantDetail | undefined>();
const selectedTenantId = ref<string | undefined>();
const loading = ref(false);
const detailLoading = ref(false);
const drawerOpen = ref(false);
const error = ref<string | null>(null);

const filters = reactive<TenantFilters>({
  status: "all",
  plan: "all",
  domainStatus: "all",
  moduleCode: "all"
});

const filteredTenants = computed(() =>
  tenants.value.filter((tenant) => {
    const statusMatch = filters.status === "all" || tenant.status === filters.status;
    const planMatch = filters.plan === "all" || tenant.planCode === filters.plan;
    const domainMatch = filters.domainStatus === "all" || tenant.domainStatus === filters.domainStatus;
    const moduleMatch = filters.moduleCode === "all" || tenant.moduleCodes.includes(filters.moduleCode);

    return statusMatch && planMatch && domainMatch && moduleMatch;
  })
);

const activeCount = computed(() => tenants.value.filter((tenant) => tenant.status === "Active").length);
const verifiedDomains = computed(() => tenants.value.filter((tenant) => tenant.domainStatus === "verified").length);
const suspendedCount = computed(() => tenants.value.filter((tenant) => tenant.status === "Suspended").length);
const supportCount = computed(() =>
  tenants.value.filter((tenant) => tenant.domainStatus === "failed" || tenant.status === "Draft").length
);

const isEmpty = computed(() => !loading.value && !error.value && tenants.value.length === 0);
const isFiltered = computed(
  () =>
    filters.status !== "all" ||
    filters.plan !== "all" ||
    filters.domainStatus !== "all" ||
    filters.moduleCode !== "all"
);

function resetFilters() {
  filters.status = "all";
  filters.plan = "all";
  filters.domainStatus = "all";
  filters.moduleCode = "all";
}

async function loadTenants() {
  loading.value = true;
  error.value = null;

  try {
    tenants.value = await tenantClient.listTenants();
  } catch (loadError) {
    error.value = loadError instanceof Error ? loadError.message : "Không tải được danh sách phòng khám.";
  } finally {
    loading.value = false;
  }
}

async function selectTenant(tenantId: string) {
  selectedTenantId.value = tenantId;
  drawerOpen.value = true;
  detailLoading.value = true;

  try {
    selectedTenant.value = await tenantClient.getTenant(tenantId);
  } catch (loadError) {
    error.value = loadError instanceof Error ? loadError.message : "Không tải được hồ sơ phòng khám.";
  } finally {
    detailLoading.value = false;
  }
}

async function updateStatus(status: TenantStatus) {
  if (!selectedTenant.value) {
    return;
  }

  detailLoading.value = true;

  try {
    selectedTenant.value = await tenantClient.updateTenantStatus(selectedTenant.value.id, { status });
    tenants.value = tenants.value.map((tenant) =>
      tenant.id === selectedTenant.value?.id ? { ...tenant, status } : tenant
    );
  } catch (updateError) {
    error.value = updateError instanceof Error ? updateError.message : "Không cập nhật được trạng thái phòng khám.";
  } finally {
    detailLoading.value = false;
  }
}

onMounted(loadTenants);
</script>

<template>
  <div class="clinics-page">
    <section class="page-heading">
      <div>
        <p class="eyebrow">Danh mục phòng khám</p>
        <h2>Danh sách phòng khám</h2>
      </div>
      <div class="heading-actions">
        <AppButton
          label="Tải lại"
          variant="secondary"
          :disabled="loading"
          :loading="loading"
          @click="loadTenants"
        />
        <RouterLink to="/clinics/create">
          <AppButton label="Thêm phòng khám" />
        </RouterLink>
      </div>
    </section>

    <div class="metrics-grid">
      <KPITile label="Phòng khám đang hoạt động" :value="activeCount" meta="Phòng khám đang vận hành" tone="success" />
      <KPITile label="Tên miền đã xác minh" :value="verifiedDomains" meta="DNS và routing đã sẵn sàng" tone="info" />
      <KPITile label="Đã tạm ngưng" :value="suspendedCount" meta="Cần rà soát vòng đời" tone="danger" />
      <KPITile label="Cần hỗ trợ" :value="supportCount" meta="Bản nháp hoặc tên miền lỗi" tone="warning" />
    </div>

    <AppCard>
      <TenantFilterBar v-model="filters" @reset="resetFilters" />
    </AppCard>

    <AppCard :padded="false">
      <div v-if="error" class="error-state">
        <span>{{ error }}</span>
        <AppButton label="Thử lại" variant="secondary" @click="loadTenants" />
      </div>

      <TenantTable
        v-if="!isEmpty"
        :loading="loading"
        :selected-tenant-id="selectedTenantId"
        :tenants="filteredTenants"
        @select="selectTenant"
      />

      <EmptyState
        v-else
        label="Chưa có phòng khám nào trong hệ thống."
        helper="Thêm phòng khám đầu tiên để bắt đầu vận hành nền tảng Clinic SaaS."
      >
        <template #action>
          <RouterLink to="/clinics/create">
            <AppButton label="Thêm phòng khám đầu tiên" />
          </RouterLink>
        </template>
      </EmptyState>

      <div v-if="!isEmpty && !loading && filteredTenants.length === 0 && isFiltered" class="filter-empty">
        Không có phòng khám phù hợp bộ lọc hiện tại.
        <AppButton label="Đặt lại bộ lọc" variant="ghost" @click="resetFilters" />
      </div>
    </AppCard>

    <div class="footer-grid">
      <AppCard tone="danger">
        <div class="footer-card">
          <div class="footer-card-head">
            <strong>Trùng slug hoặc tên miền (HTTP 409)</strong>
            <span class="status-tag">HTTP 409</span>
          </div>
          <p>
            Khi tạo phòng khám trùng slug, tên miền hoặc email chủ sở hữu, hệ thống sẽ trả 409 và
            wizard giữ nguyên dữ liệu đã nhập, đồng thời focus vào trường bị trùng đầu tiên.
          </p>
        </div>
      </AppCard>

      <AppCard>
        <div class="footer-card">
          <div class="footer-card-head">
            <strong>Wizard tạo phòng khám</strong>
            <span class="status-tag info">Xem trước</span>
          </div>
          <p>4 bước: thông tin phòng khám, gói &amp; module, tên miền mặc định và xác nhận trước khi tạo.</p>
          <RouterLink to="/clinics/create" class="footer-cta">
            <AppButton label="Mở wizard tạo phòng khám" />
          </RouterLink>
        </div>
      </AppCard>
    </div>

    <TenantDetailDrawer
      :open="drawerOpen"
      :tenant="selectedTenant"
      :loading="detailLoading"
      @close="drawerOpen = false"
      @update-status="updateStatus"
    />
  </div>
</template>

<style scoped>
.clinics-page {
  display: grid;
  gap: var(--space-5);
}

.page-heading,
.heading-actions {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-3);
}

.page-heading p,
.page-heading h2 {
  margin: 0;
}

.eyebrow {
  color: var(--color-brand-primary);
  font-size: 12px;
  font-weight: 800;
  text-transform: uppercase;
}

.page-heading h2 {
  max-width: 100%;
  overflow-wrap: anywhere;
  margin-top: var(--space-1);
  color: var(--color-text-primary);
  font-size: 24px;
  line-height: 32px;
}

.heading-actions a {
  text-decoration: none;
}

.metrics-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: var(--space-3);
}

.error-state {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-4);
  border-bottom: 1px solid color-mix(in srgb, var(--color-status-warning) 24%, var(--color-border-subtle));
  padding: var(--space-4);
  background: color-mix(in srgb, var(--color-status-warning) 8%, var(--color-surface-elevated));
  color: var(--color-status-warning);
}

.filter-empty {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-3);
  border-top: 1px solid var(--color-border-subtle);
  padding: var(--space-4);
  color: var(--color-text-secondary);
}

.footer-grid {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(0, 1fr);
  gap: var(--space-4);
}

.footer-card {
  display: grid;
  gap: 10px;
}

.footer-card p {
  margin: 0;
  color: var(--color-text-secondary);
}

.footer-card-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.footer-card-head strong {
  color: var(--color-text-primary);
}

.status-tag {
  display: inline-flex;
  align-items: center;
  border-radius: 999px;
  padding: var(--space-1) var(--space-3);
  background: color-mix(in srgb, var(--color-status-danger) 12%, var(--color-surface-elevated));
  color: var(--color-status-danger);
  font-size: 12px;
  font-weight: 800;
}

.status-tag.info {
  background: color-mix(in srgb, var(--color-brand-accent) 14%, var(--color-surface-elevated));
  color: var(--color-brand-accent);
}

.footer-cta {
  display: inline-block;
  text-decoration: none;
}

@media (max-width: 980px) {
  .metrics-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .footer-grid {
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

  .metrics-grid {
    grid-template-columns: 1fr;
  }

  .page-heading {
    gap: var(--space-3);
  }

  .heading-actions {
    display: grid;
    grid-template-columns: 1fr 1fr;
  }

  .heading-actions :deep(.app-button) {
    width: 100%;
  }
}
</style>
