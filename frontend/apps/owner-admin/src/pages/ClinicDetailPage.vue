<script setup lang="ts">
// Trang chi tiết tenant tương ứng route `/clinics/:tenantId`.
// Page này có cùng dữ liệu nguồn với drawer ở `/clinics`, nhưng được trình bày dạng full-page
// để Owner Admin có thể bookmark/chia sẻ link riêng từng tenant.
import type { TenantDetail, TenantDomain, TenantDomainStatus, TenantStatus } from "@clinic-saas/shared-types";
import { AppButton, AppCard, DomainStateRow, ModuleChips, PlanBadge, StatePanel, StatusPill } from "@clinic-saas/ui";
import { computed, onMounted, onUnmounted, ref, watch } from "vue";
import DomainDnsRetryState from "../components/DomainDnsRetryState.vue";
import SslPendingState from "../components/SslPendingState.vue";
import TenantLifecycleConfirmModal from "../components/TenantLifecycleConfirmModal.vue";
import { formatDomainStatus, formatModuleCode, formatTenantStatus } from "../services/labels";
import { tenantClient } from "../services/tenantClient";

type LifecycleAction = "suspend" | "archive" | "restore";
type DomainSurface = "dns" | "ssl";
type DomainOperationState = "ready" | "loading" | "empty" | "error" | "success";

const props = defineProps<{
  /** Tenant ID lấy từ route params, dùng để fetch chi tiết. */
  tenantId: string;
}>();

const tenant = ref<TenantDetail | undefined>();
const loading = ref(false);
const error = ref<string | null>(null);
const lifecycleAction = ref<LifecycleAction>("suspend");
const lifecycleModalOpen = ref(false);
const activeDomainSurface = ref<DomainSurface>("dns");
const domainOperationState = ref<DomainOperationState>("ready");
const domainOperationMessage = ref<string | undefined>();
let retryTimer: ReturnType<typeof window.setTimeout> | undefined;

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

const domainCards = computed<TenantDomain[]>(() => {
  if (!tenant.value) {
    return [];
  }

  if (tenant.value.domains.length > 0) {
    return tenant.value.domains;
  }

  return [
    {
      id: `${tenant.value.id}-default-domain`,
      domainName: tenant.value.defaultDomainName || `${tenant.value.slug}.clinicos.vn`,
      isPrimary: true,
      status: tenant.value.domainStatus
    }
  ];
});

const dnsRetryRows = computed(() =>
  domainCards.value
    .filter((domain) => domain.status !== "verified")
    .map((domain) => ({
      id: `dns-${domain.id}`,
      domainName: domain.domainName,
      tenantSlug: tenant.value?.slug ?? "tenant",
      recordType: domain.isPrimary ? ("CNAME" as const) : ("TXT" as const),
      host: domain.isPrimary ? domain.domainName : `_clinicos.${domain.domainName}`,
      issue: domain.status === "failed" ? "Record mismatch" : domain.status === "pending" ? "Propagation pending" : "Resolver pending",
      lastCheck: domain.status === "failed" ? "5 phút trước" : "1 phút trước",
      status: domain.status === "failed" ? ("failed" as const) : domain.status === "pending" ? ("pending" as const) : ("propagating" as const),
      selected: domain.status !== "verified",
      expanded: domain.status === "failed",
      expected: domain.isPrimary ? "cname.owner-gateway.clinicos.vn" : `clinicos-tenant-verify=${tenant.value?.slug ?? "tenant"}`,
      actual: domain.status === "failed" ? "legacy-gateway.clinicos.vn" : "Đang chờ resolver"
    }))
);

const sslPendingRows = computed(() =>
  domainCards.value
    .filter((domain) => domain.status === "pending")
    .map((domain, index) => ({
      id: `ssl-${domain.id}`,
      domainName: domain.domainName,
      tenantSlug: tenant.value?.slug ?? "tenant",
      orderId: `mock-acme-${tenant.value?.slug ?? "tenant"}-${index + 1}`,
      status: index % 2 === 0 ? "Verifying ACME challenge" : "Submitting CSR",
      eta: index % 2 === 0 ? "ETA 45s" : "ETA 2m",
      progress: index % 2 === 0 ? 68 : 34
    }))
);

const dnsSurfaceState = computed<DomainOperationState>(() => {
  if (activeDomainSurface.value === "dns" && domainOperationState.value !== "ready") {
    return domainOperationState.value;
  }

  return dnsRetryRows.value.length === 0 ? "empty" : "ready";
});

const sslSurfaceState = computed<DomainOperationState>(() => {
  if (activeDomainSurface.value === "ssl" && domainOperationState.value !== "ready") {
    return domainOperationState.value;
  }

  return sslPendingRows.value.length === 0 ? "empty" : "ready";
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
  clearRetryTimer();
  loading.value = true;
  error.value = null;
  tenant.value = undefined;
  domainOperationState.value = "ready";
  domainOperationMessage.value = undefined;

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
    selectDomainSurface("dns");
    return;
  }

  if (status === "pending" || key === "Recheck") {
    selectDomainSurface("ssl");
  }
}

function clearRetryTimer() {
  if (retryTimer !== undefined) {
    window.clearTimeout(retryTimer);
    retryTimer = undefined;
  }
}

function selectDomainSurface(surface: DomainSurface) {
  activeDomainSurface.value = surface;
  domainOperationState.value = "ready";
  domainOperationMessage.value = undefined;
}

function runMockRetry(label: string) {
  clearRetryTimer();
  domainOperationState.value = "loading";
  domainOperationMessage.value = `${label} đang chạy mock verify trong Owner Admin.`;

  retryTimer = window.setTimeout(() => {
    domainOperationState.value = "success";
    domainOperationMessage.value = `${label} đã nhận lệnh retry verify ở UI local.`;
    retryTimer = undefined;
  }, 700);
}

function runMockDiagnostic(rowId: string) {
  domainOperationState.value = "error";
  domainOperationMessage.value = `Diagnostic mock cho ${rowId} phát hiện record chưa khớp expected value.`;
}

function retryDnsRow(rowId: string) {
  runMockRetry(`DNS record ${rowId}`);
}

function retryDnsRows(rowIds: string[]) {
  runMockRetry(`${rowIds.length} DNS record`);
}

function retrySslRow(rowId: string) {
  runMockRetry(`SSL order ${rowId}`);
}

onMounted(loadTenant);
onUnmounted(clearRetryTimer);
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
              v-for="domain in domainCards"
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

      <AppCard class="state-surface-card">
        <div class="state-surface-heading">
          <div>
            <p class="eyebrow">Domain operations</p>
            <h3>{{ activeDomainSurface === "dns" ? "DNS retry state" : "SSL pending state" }}</h3>
          </div>
          <span class="mock-badge">Mock-first</span>
        </div>

        <div class="operation-switch" aria-label="Chọn domain operation">
          <button type="button" class="operation-card" :data-active="activeDomainSurface === 'dns'" @click="selectDomainSurface('dns')">
            <span>DNS Retry</span>
            <strong>{{ dnsRetryRows.length }}</strong>
            <small>{{ dnsRetryRows.length === 0 ? "Không có record cần retry" : "CNAME/TXT cần verify lại" }}</small>
          </button>
          <button type="button" class="operation-card" :data-active="activeDomainSurface === 'ssl'" @click="selectDomainSurface('ssl')">
            <span>SSL Pending</span>
            <strong>{{ sslPendingRows.length }}</strong>
            <small>{{ sslPendingRows.length === 0 ? "Không có cert pending" : "ACME order đang chờ" }}</small>
          </button>
        </div>

        <DomainDnsRetryState
          v-if="activeDomainSurface === 'dns'"
          :rows="dnsRetryRows"
          :state="dnsSurfaceState"
          :state-message="domainOperationMessage"
          @retry="retryDnsRow"
          @bulk-retry="retryDnsRows"
          @diagnostic="runMockDiagnostic"
        />
        <SslPendingState
          v-else
          :rows="sslPendingRows"
          :state="sslSurfaceState"
          :state-message="domainOperationMessage"
          @reissue="retrySslRow"
        />
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
.state-surface-heading p,
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

.state-surface-heading h3 {
  margin-top: var(--space-1);
}

.mock-badge {
  display: inline-flex;
  min-height: 28px;
  align-items: center;
  border-radius: var(--radius-pill);
  padding: 0 var(--space-3);
  background: color-mix(in srgb, var(--color-status-specialty) 13%, var(--color-surface-muted));
  color: var(--color-status-specialty);
  font-size: 11px;
  font-weight: 900;
  white-space: nowrap;
}

.operation-switch {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: var(--space-3);
  margin-bottom: var(--space-4);
}

.operation-card {
  display: grid;
  gap: var(--space-1);
  min-width: 0;
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-card);
  padding: var(--space-4);
  background: var(--color-surface-elevated);
  color: var(--color-text-primary);
  cursor: pointer;
  text-align: left;
  font: inherit;
  box-shadow: var(--shadow-elevation-1);
  transition:
    border-color var(--motion-duration-sm) var(--motion-ease-standard),
    background var(--motion-duration-sm) var(--motion-ease-standard),
    transform var(--motion-duration-xs) var(--motion-ease-standard);
}

.operation-card:hover {
  transform: translateY(-1px);
}

.operation-card[data-active="true"] {
  border-color: color-mix(in srgb, var(--color-brand-primary) 42%, var(--color-border-subtle));
  background: color-mix(in srgb, var(--color-brand-primary) 8%, var(--color-surface-elevated));
}

.operation-card span {
  color: var(--color-text-secondary);
  font-size: 12px;
  font-weight: 900;
  text-transform: uppercase;
}

.operation-card strong {
  color: var(--color-text-primary);
  font-size: 26px;
  line-height: 32px;
}

.operation-card small {
  overflow-wrap: anywhere;
  color: var(--color-text-muted);
  font-size: 12px;
  font-weight: 750;
}

.operation-card:focus-visible {
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
  .heading-actions,
  .state-surface-heading {
    align-items: stretch;
    flex-direction: column;
  }

  dl {
    grid-template-columns: 1fr;
  }

  .operation-switch {
    grid-template-columns: 1fr;
  }
}
</style>
