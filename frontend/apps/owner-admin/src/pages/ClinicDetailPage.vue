<script setup lang="ts">
// Trang chi tiết tenant tương ứng route `/clinics/:tenantId`.
// Page này có cùng dữ liệu nguồn với drawer ở `/clinics`, nhưng được trình bày dạng full-page
// để Owner Admin có thể bookmark/chia sẻ link riêng từng tenant.
import type { TenantDetail, TenantDomainStatus, TenantStatus } from "@clinic-saas/shared-types";
import { AppButton, AppCard, DomainStateRow, ModuleChips, PlanBadge, StatePanel, StatusPill } from "@clinic-saas/ui";
import { computed, onMounted, ref, watch } from "vue";
import DomainDnsRetryState from "../components/DomainDnsRetryState.vue";
import SslPendingState from "../components/SslPendingState.vue";
import TenantLifecycleConfirmModal from "../components/TenantLifecycleConfirmModal.vue";
import { formatDomainStatus, formatModuleCode, formatTenantStatus } from "../services/labels";
import { tenantClient } from "../services/tenantClient";

type LifecycleAction = "suspend" | "archive" | "restore";
type DomainSurface = "dns" | "ssl" | null;

const props = defineProps<{
  /** Tenant ID lấy từ route params, dùng để fetch chi tiết. */
  tenantId: string;
}>();

const tenant = ref<TenantDetail | undefined>();
const loading = ref(false);
const error = ref<string | null>(null);
const lifecycleAction = ref<LifecycleAction>("suspend");
const lifecycleModalOpen = ref(false);
const activeDomainSurface = ref<DomainSurface>(null);

const detailHeading = computed(() => {
  if (tenant.value) {
    return tenant.value.displayName;
  }

  if (error.value) {
    return "Không tìm thấy phòng khám";
  }

  return "Đang tải phòng khám...";
});

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

function domainTone(status: TenantDomainStatus) {
  if (status === "verified") {
    return "success";
  }

  if (status === "failed") {
    return "danger";
  }

  if (status === "pending") {
    return "warning";
  }

  return "info";
}

function domainHelper(domain: TenantDetail["domains"][number]) {
  if (domain.isPrimary) {
    return "Default subdomain";
  }

  if (domain.status === "verified") {
    return "Custom · CNAME OK";
  }

  if (domain.status === "pending") {
    return "DNS đang propagate. Recheck khi bản ghi đã sẵn sàng.";
  }

  if (domain.status === "failed") {
    return "CNAME/TXT chưa khớp. Cần rà soát bản ghi.";
  }

  return "Chưa có dữ liệu DNS/SSL từ backend.";
}

function domainAction(domain: TenantDetail["domains"][number]) {
  if (domain.status === "verified") {
    return "View cert";
  }

  if (domain.status === "failed") {
    return "View error";
  }

  return "Recheck";
}

function domainActionTone(status: TenantDomainStatus) {
  if (status === "failed") {
    return "danger" as const;
  }

  if (status === "pending") {
    return "primary" as const;
  }

  return "secondary" as const;
}

function planTone(planCode: TenantDetail["planCode"]) {
  if (planCode === "premium") {
    return "warning";
  }

  if (planCode === "growth") {
    return "neutral";
  }

  return "info";
}

function moduleChipItems(moduleCodes: TenantDetail["moduleCodes"]) {
  return moduleCodes.map((moduleCode) => ({
    key: moduleCode,
    label: formatModuleCode(moduleCode),
    enabled: true,
    tone: "success" as const
  }));
}

async function loadTenant() {
  loading.value = true;
  error.value = null;
  tenant.value = undefined;

  try {
    tenant.value = await tenantClient.getTenant(props.tenantId);
  } catch (loadError) {
    if (loadError instanceof Error && loadError.message === "Tenant not found.") {
      error.value = "Không tìm thấy phòng khám.";
      return;
    }

    error.value = loadError instanceof Error ? loadError.message : "Không tải được phòng khám.";
  } finally {
    loading.value = false;
  }
}

function updateCurrentStatus() {
  openLifecycleModal(nextStatus.value === "Active" ? "restore" : "suspend");
}

function openLifecycleModal(action: LifecycleAction) {
  lifecycleAction.value = action;
  lifecycleModalOpen.value = true;
}

async function confirmLifecycle(action: LifecycleAction) {
  lifecycleModalOpen.value = false;

  const status: TenantStatus = action === "archive" ? "Archived" : action === "restore" ? "Active" : "Suspended";
  await updateStatus(status);
}

async function updateStatus(status: TenantStatus) {
  if (!tenant.value) {
    return;
  }

  loading.value = true;

  try {
    tenant.value = await tenantClient.updateTenantStatus(tenant.value.id, { status });
  } catch (updateError) {
    error.value = (updateError as { message?: string }).message ?? "Không cập nhật được trạng thái phòng khám.";
  } finally {
    loading.value = false;
  }
}

function handleDomainAction(status: TenantDomainStatus, key: string) {
  if (status === "failed" || key === "View error") {
    activeDomainSurface.value = "dns";
    return;
  }

  if (status === "pending" || key === "Recheck") {
    activeDomainSurface.value = "ssl";
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
        <h2>{{ detailHeading }}</h2>
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
          @click="updateCurrentStatus"
        />
        <AppButton
          v-if="tenant && tenant.status !== 'Archived'"
          label="Lưu trữ tenant"
          variant="danger"
          :loading="loading"
          @click="openLifecycleModal('archive')"
        />
      </div>
    </section>

    <StatePanel v-if="error" title="Không tải được hồ sơ phòng khám" :description="error" tone="danger">
      <template #action>
        <AppButton label="Thử lại" variant="secondary" @click="loadTenant" />
      </template>
    </StatePanel>

    <StatePanel
      v-if="loading && !tenant"
      title="Đang tải hồ sơ phòng khám"
      description="Owner Admin đang lấy tenant detail, domain và module entitlement."
      tone="loading"
      busy
    />

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
              <dd>
                <PlanBadge :label="tenant.planDisplayName" :tone="planTone(tenant.planCode)" />
              </dd>
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
          <div class="domain-list">
            <DomainStateRow
              v-for="domain in tenant.domains"
              :key="domain.id"
              :label="domain.domainName"
              :value="formatDomainStatus(domain.status)"
              :helper="domainHelper(domain)"
              :tone="domainTone(domain.status)"
              :actions="[{ key: domainAction(domain), label: domainAction(domain), disabled: domain.status === 'verified', tone: domainActionTone(domain.status) }]"
              @action="handleDomainAction(domain.status, $event)"
            />
          </div>
        </AppCard>
      </div>

      <AppCard>
        <h3>Module đang bật</h3>
        <ModuleChips :items="moduleChipItems(tenant.moduleCodes)" :total="tenant.moduleCodes.length" />
      </AppCard>

      <AppCard v-if="activeDomainSurface" class="state-surface-card">
        <div class="state-surface-heading">
          <h3>{{ activeDomainSurface === "dns" ? "DNS retry state" : "SSL pending state" }}</h3>
          <button type="button" @click="activeDomainSurface = null">Ẩn state surface</button>
        </div>
        <DomainDnsRetryState v-if="activeDomainSurface === 'dns'" />
        <SslPendingState v-else />
      </AppCard>
    </template>

    <TenantLifecycleConfirmModal
      v-if="tenant"
      :open="lifecycleModalOpen"
      :action="lifecycleAction"
      :tenant-name="tenant.displayName"
      :tenant-slug="tenant.slug"
      :loading="loading"
      @close="lifecycleModalOpen = false"
      @confirm="confirmLifecycle"
    />
  </div>
</template>

<style scoped>
.detail-page {
  display: grid;
  gap: var(--space-5);
  min-width: 0;
}

.page-heading,
.heading-actions,
.status-row {
  display: flex;
  align-items: center;
  gap: var(--space-3);
}

.page-heading {
  justify-content: space-between;
  border: 1px solid color-mix(in srgb, var(--color-border-subtle) 78%, var(--color-brand-primary));
  border-radius: var(--radius-card);
  padding: var(--space-5);
  background:
    linear-gradient(135deg, color-mix(in srgb, var(--color-status-info) 9%, transparent), transparent 44%),
    var(--color-surface-elevated);
  box-shadow: var(--shadow-elevation-1);
}

.page-heading p,
.page-heading h2,
h3,
dl {
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

a {
  text-decoration: none;
}

.detail-grid {
  display: grid;
  grid-template-columns: minmax(0, 1fr) 380px;
  gap: var(--space-5);
}

.detail-grid :deep(.app-card) {
  min-width: 0;
}

dl {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: var(--space-4);
  margin-top: var(--space-5);
}

dt {
  color: var(--color-text-secondary);
  font-size: 12px;
  font-weight: 800;
}

dd {
  margin: 4px 0 0;
  overflow-wrap: anywhere;
  color: var(--color-text-primary);
  font-weight: 750;
}

.wide {
  grid-column: 1 / -1;
}

.domain-list {
  display: grid;
  gap: var(--space-3);
  margin: var(--space-4) 0 0;
}

.status-row {
  flex-wrap: wrap;
  border-bottom: 1px solid var(--color-border-subtle);
  padding-bottom: var(--space-4);
}

.state-surface-card {
  min-width: 0;
}

.state-surface-heading {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-3);
  margin-bottom: var(--space-4);
}

.state-surface-heading button {
  min-height: 32px;
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-input);
  padding: 0 var(--space-3);
  background: var(--color-surface-elevated);
  color: var(--color-text-secondary);
  cursor: pointer;
  font: inherit;
  font-size: 12px;
  font-weight: 800;
}

.state-surface-heading button:focus-visible {
  outline: 2px solid color-mix(in srgb, var(--color-brand-primary) 28%, transparent);
  outline-offset: 2px;
}

@media (max-width: 980px) {
  .detail-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 640px) {
  .page-heading,
  .heading-actions {
    align-items: stretch;
    flex-direction: column;
  }

  dl {
    grid-template-columns: 1fr;
  }
}
</style>
