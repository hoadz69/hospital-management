<script setup lang="ts">
// Trang dashboard Owner Admin bám V3 cross-tenant cockpit, chỉ derive dữ liệu từ tenantClient hiện có.
import type { TenantSummary } from "@clinic-saas/shared-types";
import { AppButton, AppCard, EmptyState, KPITile, StatusPill } from "@clinic-saas/ui";
import { computed, onMounted, ref } from "vue";
import DomainDnsRetryState from "../components/DomainDnsRetryState.vue";
import SslPendingState from "../components/SslPendingState.vue";
import { formatTenantStatus } from "../services/labels";
import { tenantClient } from "../services/tenantClient";

const tenants = ref<TenantSummary[]>([]);
const loading = ref(false);
const error = ref<string | null>(null);

const totalCount = computed(() => tenants.value.length);
const activeCount = computed(() => tenants.value.filter((tenant) => tenant.status === "Active").length);
const verifiedDomains = computed(() => tenants.value.filter((tenant) => tenant.domainStatus === "verified").length);
const suspendedCount = computed(() => tenants.value.filter((tenant) => tenant.status === "Suspended").length);
const supportCount = computed(() =>
  tenants.value.filter((tenant) => tenant.domainStatus === "failed" || tenant.status === "Draft").length
);
const domainPendingCount = computed(() => tenants.value.filter((tenant) => tenant.domainStatus === "pending").length);
const newTenantCount = computed(() => {
  const now = new Date();
  return tenants.value.filter((tenant) => {
    const createdAt = new Date(tenant.createdAt);
    return createdAt.getFullYear() === now.getFullYear() && createdAt.getMonth() === now.getMonth();
  }).length;
});
const estimatedMrr = computed(() =>
  tenants.value.reduce((total, tenant) => {
    if (tenant.status === "Archived") {
      return total;
    }

    const planPrice = tenant.planCode === "premium" ? 149 : tenant.planCode === "growth" ? 79 : 29;
    return total + planPrice;
  }, 0)
);
const activeRate = computed(() => {
  if (totalCount.value === 0) {
    return "0% active rate";
  }

  return `${((activeCount.value / totalCount.value) * 100).toFixed(1)}% active rate`;
});
const recentTenants = computed(() =>
  [...tenants.value]
    .sort((left, right) => new Date(right.createdAt).getTime() - new Date(left.createdAt).getTime())
    .slice(0, 8)
);
const attentionItems = computed(() => [
  {
    icon: "DNS",
    label: `${tenants.value.filter((tenant) => tenant.domainStatus === "failed").length} tên miền lỗi xác minh`,
    helper: "Cần rà soát bản ghi DNS",
    tone: "danger"
  },
  {
    icon: "SSL",
    label: `${domainPendingCount.value} tên miền đang chờ`,
    helper: "Theo dõi DNS/SSL pending",
    tone: "warning"
  },
  {
    icon: "TEN",
    label: `${suspendedCount.value} tenant đang tạm ngưng`,
    helper: "Kiểm tra vòng đời tenant",
    tone: "neutral"
  }
]);
const mrrBars = computed(() => {
  const base = Math.max(estimatedMrr.value, 1);
  return [
    { label: "12/25", value: Math.round(base * 0.55) },
    { label: "01/26", value: Math.round(base * 0.64) },
    { label: "02/26", value: Math.round(base * 0.72) },
    { label: "03/26", value: Math.round(base * 0.82) },
    { label: "04/26", value: Math.round(base * 0.91) },
    { label: "05/26", value: base }
  ];
});

function statusTone(status: TenantSummary["status"]) {
  if (status === "Active") {
    return "success";
  }

  if (status === "Suspended") {
    return "danger";
  }

  return "warning";
}

function formatMrr(value: number) {
  return `$${value.toLocaleString("en-US")}`;
}

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
        <h2>Owner Dashboard - Cross-Tenant Operations</h2>
      </div>
      <RouterLink to="/clinics/create">
        <AppButton label="Thêm phòng khám" />
      </RouterLink>
    </section>

    <div class="metrics-grid">
      <KPITile label="Tổng tenant" :value="totalCount" meta="Tất cả tenant đang quản lý" tone="info" />
      <KPITile label="Active" :value="activeCount" :meta="activeRate" tone="success" />
      <KPITile label="MRR ước" :value="formatMrr(estimatedMrr)" meta="+18% MoM theo mock hiện tại" tone="warning" />
      <KPITile label="Churn risk" value="1.2%" meta="Mục tiêu dưới 2%" tone="danger" />
      <KPITile label="New tenant" :value="newTenantCount" meta="Tháng hiện tại" tone="success" />
      <KPITile label="Domain pending" :value="domainPendingCount" meta="Cần admin verify" tone="warning" />
    </div>

    <div v-if="error" class="state">
      <p>{{ error }}</p>
      <AppButton label="Thử lại" variant="secondary" @click="loadDashboard" />
    </div>

    <EmptyState
      v-else-if="!loading && tenants.length === 0"
      label="Chưa có tenant nào trong hệ thống."
      helper="Thêm phòng khám đầu tiên để bắt đầu vận hành Owner Admin."
    >
      <template #action>
        <RouterLink to="/clinics/create">
          <AppButton label="Thêm phòng khám đầu tiên" />
        </RouterLink>
      </template>
    </EmptyState>

    <div v-else class="dashboard-grid">
      <AppCard class="chart-card">
        <div class="operations-header">
          <div>
            <h3>MRR Growth - 6 tháng gần nhất</h3>
            <p>{{ loading ? "Đang tải dữ liệu..." : "Ước tính từ plan tenant hiện tại." }}</p>
          </div>
          <div class="range-toggle" aria-label="Khoảng thời gian">
            <button type="button">3m</button>
            <button type="button" class="active">6m</button>
            <button type="button">12m</button>
          </div>
        </div>

        <div class="mrr-chart" role="img" aria-label="Biểu đồ MRR 6 tháng gần nhất">
          <div v-for="bar in mrrBars" :key="bar.label" class="mrr-bar">
            <div
              class="mrr-bar-fill"
              :style="{ height: `${Math.max((bar.value / Math.max(estimatedMrr, 1)) * 82, 22)}%` }"
            >
              <span>{{ formatMrr(bar.value) }}</span>
            </div>
            <small>{{ bar.label }}</small>
          </div>
        </div>
      </AppCard>

      <AppCard class="attention-card">
        <div class="operations-header">
          <div>
            <h3>Cảnh báo cần xử lý</h3>
            <p>Domain, lifecycle và support queue.</p>
          </div>
          <span class="alert-count">{{ supportCount + domainPendingCount }}</span>
        </div>

        <div class="attention-list">
          <article v-for="item in attentionItems" :key="item.label" :class="['attention-item', item.tone]">
            <span>{{ item.icon }}</span>
            <div>
              <strong>{{ item.label }}</strong>
              <small>{{ item.helper }}</small>
            </div>
          </article>
        </div>
      </AppCard>

      <AppCard class="tenant-card">
        <div class="operations-header">
          <div>
            <h3>Tenants gần đây ({{ recentTenants.length }}/{{ totalCount }})</h3>
            <p>{{ loading ? "Đang tải phòng khám..." : "Theo thứ tự tạo mới nhất." }}</p>
          </div>
          <RouterLink to="/clinics">
            <AppButton label="Xem tất cả" variant="secondary" />
          </RouterLink>
        </div>

        <div class="tenant-table" role="table" aria-label="Tenants gần đây">
          <div class="tenant-row tenant-row--head" role="row">
            <span>Tenant</span>
            <span>Slug</span>
            <span>Plan</span>
            <span>Domain</span>
            <span>Status</span>
          </div>
          <RouterLink
            v-for="tenant in recentTenants"
            :key="tenant.id"
            class="tenant-row"
            :to="`/clinics/${tenant.id}`"
            role="row"
          >
            <span>
              <strong>{{ tenant.displayName }}</strong>
              <small>{{ tenant.clinicName }}</small>
            </span>
            <span>{{ tenant.slug }}</span>
            <span>{{ tenant.planDisplayName }}</span>
            <span>{{ tenant.domainStatus }}</span>
            <StatusPill :label="formatTenantStatus(tenant.status)" :tone="statusTone(tenant.status)" />
          </RouterLink>
        </div>
      </AppCard>

      <AppCard class="state-card">
        <DomainDnsRetryState />
      </AppCard>

      <AppCard class="state-card">
        <SslPendingState />
      </AppCard>
    </div>
  </div>
</template>

<style scoped>
.dashboard-page {
  display: grid;
  gap: var(--space-5);
}

.page-heading,
.operations-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-4);
}

.page-heading p,
.page-heading h2,
.operations-header h3,
.operations-header p,
.state p {
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

.metrics-grid {
  display: grid;
  grid-template-columns: repeat(6, minmax(0, 1fr));
  gap: var(--space-3);
}

.operations-header p {
  margin-top: var(--space-1);
  color: var(--color-text-secondary);
  font-size: 12px;
}

.dashboard-grid {
  display: grid;
  grid-template-columns: minmax(0, 1.25fr) minmax(320px, 0.75fr);
  gap: var(--space-4);
}

.tenant-card {
  grid-column: 1 / -1;
}

.state-card {
  min-width: 0;
}

.range-toggle {
  display: inline-flex;
  border-radius: 8px;
  padding: 3px;
  background: var(--color-surface-muted);
}

.range-toggle button {
  min-height: 26px;
  border: 0;
  border-radius: 6px;
  padding: 0 var(--space-3);
  background: transparent;
  color: var(--color-text-secondary);
  cursor: pointer;
  font-size: 11px;
  font-weight: 700;
}

.range-toggle button.active {
  background: var(--color-surface-elevated);
  color: var(--color-text-primary);
  box-shadow: var(--shadow-elevation-1);
}

.mrr-chart {
  position: relative;
  min-height: 240px;
  display: grid;
  grid-template-columns: repeat(6, minmax(48px, 1fr));
  align-items: end;
  gap: var(--space-4);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-input);
  margin-top: var(--space-4);
  padding: var(--space-4);
  background:
    linear-gradient(to top, color-mix(in srgb, var(--color-border-subtle) 45%, transparent) 1px, transparent 1px) 0 0 / 100% 25%,
    color-mix(in srgb, var(--color-surface-page) 70%, var(--color-surface-elevated));
}

.mrr-bar {
  height: 200px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: end;
  gap: var(--space-2);
}

.mrr-bar-fill {
  width: min(44px, 70%);
  display: grid;
  place-items: center;
  border-radius: 6px;
  background: linear-gradient(180deg, var(--color-status-specialty), color-mix(in srgb, var(--color-status-specialty) 82%, var(--color-brand-primary)));
  color: var(--color-surface-elevated);
  font-size: 10px;
  font-weight: 800;
  box-shadow: 0 10px 22px color-mix(in srgb, var(--color-status-specialty) 24%, transparent);
  transition: height var(--motion-duration-md) var(--motion-ease-standard);
}

.mrr-bar-fill span {
  transform: rotate(-90deg);
  white-space: nowrap;
}

.mrr-bar small {
  color: var(--color-text-secondary);
  font-size: 10px;
}

.attention-list {
  display: grid;
  gap: var(--space-3);
  margin-top: var(--space-4);
}

.attention-item {
  display: flex;
  align-items: center;
  gap: var(--space-3);
  border-radius: var(--radius-input);
  padding: var(--space-3);
  background: var(--color-surface-muted);
}

.attention-item > span {
  width: 32px;
  height: 32px;
  display: grid;
  place-items: center;
  border-radius: 8px;
  background: var(--color-surface-elevated);
  color: var(--color-text-secondary);
  font-size: 10px;
  font-weight: 900;
}

.attention-item.danger > span {
  color: var(--color-status-danger);
}

.attention-item.warning > span {
  color: var(--color-status-warning);
}

.attention-item strong,
.attention-item small,
.tenant-row strong,
.tenant-row small {
  display: block;
}

.attention-item strong {
  color: var(--color-text-primary);
  font-size: 12px;
}

.attention-item small {
  margin-top: 2px;
  color: var(--color-text-muted);
  font-size: 10px;
}

.alert-count {
  display: inline-flex;
  min-width: 24px;
  min-height: 24px;
  align-items: center;
  justify-content: center;
  border-radius: var(--radius-pill);
  background: var(--color-status-danger);
  color: var(--color-surface-elevated);
  font-size: 12px;
  font-weight: 900;
}

.tenant-table {
  display: grid;
  gap: var(--space-2);
  margin-top: var(--space-4);
}

.tenant-row {
  display: grid;
  grid-template-columns: minmax(180px, 1.3fr) minmax(110px, 0.8fr) minmax(72px, 0.55fr) minmax(90px, 0.7fr) minmax(88px, 0.6fr);
  align-items: center;
  gap: var(--space-3);
  border: 1px solid var(--color-border-subtle);
  border-radius: 8px;
  padding: var(--space-3);
  color: var(--color-text-secondary);
  font-size: 12px;
  text-decoration: none;
}

.tenant-row:hover,
.tenant-row:focus-visible {
  background: var(--color-surface-muted);
  outline: 2px solid color-mix(in srgb, var(--color-brand-primary) 35%, transparent);
  outline-offset: 2px;
}

.tenant-row--head {
  border: 0;
  background: var(--color-surface-muted);
  color: var(--color-text-muted);
  font-size: 10px;
  font-weight: 800;
  text-transform: uppercase;
}

.tenant-row strong {
  color: var(--color-text-primary);
  font-size: 12px;
}

.tenant-row small {
  margin-top: 2px;
  color: var(--color-text-muted);
  font-size: 10px;
}

.state {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-4);
  border: 1px solid color-mix(in srgb, var(--color-status-warning) 22%, var(--color-border-subtle));
  border-radius: var(--radius-card);
  padding: var(--space-4);
  background: color-mix(in srgb, var(--color-status-warning) 8%, var(--color-surface-elevated));
  color: var(--color-status-warning);
}

a {
  text-decoration: none;
}

@media (max-width: 1200px) {
  .metrics-grid {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }
}

@media (max-width: 980px) {
  .metrics-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .dashboard-grid {
    grid-template-columns: 1fr;
  }

  .tenant-row {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 640px) {
  .page-heading,
  .operations-header,
  .state {
    align-items: stretch;
    flex-direction: column;
  }

  .metrics-grid {
    grid-template-columns: 1fr;
  }

  .mrr-chart {
    overflow-x: auto;
  }

  .page-heading a :deep(.app-button) {
    width: 100%;
  }
}
</style>
