<script setup lang="ts">
// Trang chi tiết tenant tương ứng route `/clinics/:tenantId`.
// Page này có cùng dữ liệu nguồn với drawer ở `/clinics`, nhưng được trình bày dạng full-page
// để Owner Admin có thể bookmark/chia sẻ link riêng từng tenant.
import type { TenantDomainDnsSslState } from "@clinic-saas/api-client";
import type { TenantDetail, TenantDomain, TenantDomainStatus, TenantStatus } from "@clinic-saas/shared-types";
import { AppButton, AppCard, DomainStateRow, ModuleChips, PlanBadge, StatePanel, StatusPill } from "@clinic-saas/ui";
import { computed, onMounted, onUnmounted, ref, watch } from "vue";
import DomainDnsRetryState from "../components/DomainDnsRetryState.vue";
import SslPendingState from "../components/SslPendingState.vue";
import TenantAuditLogDrawer, { type TenantAuditEvent } from "../components/TenantAuditLogDrawer.vue";
import TenantLifecycleConfirmModal from "../components/TenantLifecycleConfirmModal.vue";
import { formatDomainStatus, formatModuleCode, formatTenantStatus } from "../services/labels";
import { tenantClient } from "../services/tenantClient";

type LifecycleAction = "activate" | "suspend" | "archive" | "restore";
type LifecycleModalState = "idle" | "loading" | "success" | "error";
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
const lifecycleState = ref<LifecycleModalState>("idle");
const lifecycleMessage = ref<string | undefined>();
const auditDrawerOpen = ref(false);
const auditLoading = ref(false);
const auditError = ref<string | undefined>();
const localAuditEvents = ref<TenantAuditEvent[]>([]);
const activeDomainSurface = ref<DomainSurface>("dns");
const domainOperationState = ref<DomainOperationState>("ready");
const domainOperationMessage = ref<string | undefined>();
const domainOperationStates = ref<TenantDomainDnsSslState[]>([]);
let retryTimer: ReturnType<typeof window.setTimeout> | undefined;
let lifecycleTimer: ReturnType<typeof window.setTimeout> | undefined;
let auditTimer: ReturnType<typeof window.setTimeout> | undefined;

const detailHeading = computed(() => {
  if (tenant.value) {
    return tenant.value.displayName;
  }

  if (error.value) {
    return "Không tìm thấy phòng khám";
  }

  return "Đang tải phòng khám...";
});

const primaryLifecycleAction = computed<LifecycleAction>(() => {
  if (!tenant.value) {
    return "activate";
  }

  if (tenant.value.status === "Draft") {
    return "activate";
  }

  if (tenant.value.status === "Active") {
    return "suspend";
  }

  return "restore";
});

const primaryLifecycleLabel = computed(() => lifecycleActionLabel(primaryLifecycleAction.value));
const isLifecycleBusy = computed(() => lifecycleState.value === "loading");
const lifecycleTargetStatus = computed(() => targetStatusForAction(lifecycleAction.value));

const domainCards = computed<TenantDomain[]>(() => {
  if (!tenant.value) {
    return [];
  }

  if (tenant.value.domains.length === 0 && domainOperationStates.value.length > 0) {
    return domainOperationStates.value.map((domainState, index) => ({
      id: domainState.domainId,
      domainName: domainState.domainName,
      isPrimary: index === 0,
      status: domainStatusFromDns(domainState.dnsStatus)
    }));
  }

  if (tenant.value.domains.length > 0) {
    return tenant.value.domains.map((domain) => {
      const domainState = domainOperationStates.value.find((item) => item.domainId === domain.id);
      return domainState
        ? {
            ...domain,
            domainName: domainState.domainName || domain.domainName,
            status: domainStatusFromDns(domainState.dnsStatus)
          }
        : domain;
    });
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

const dnsRetryRows = computed(() => {
  if (domainOperationStates.value.length > 0) {
    return domainOperationStates.value
      .filter((domainState) => domainState.dnsStatus !== "verified")
      .map((domainState) => {
        const record = domainState.dnsRecords[0];
        return {
          id: domainState.domainId,
          domainName: domainState.domainName,
          tenantSlug: tenant.value?.slug ?? "tenant",
          recordType: dnsRecordType(record?.recordType),
          host: record?.host || domainState.domainName,
          issue: record?.message || domainState.message || dnsIssue(domainState.dnsStatus),
          lastCheck: formatDomainTimestamp(domainState.lastCheckedAt),
          status: domainState.dnsStatus,
          selected: domainState.dnsStatus !== "verified",
          expanded: domainState.dnsStatus === "failed",
          expected: record?.expectedValue || "cname.owner-gateway.clinicos.vn",
          actual: record?.actualValue || (domainState.dnsStatus === "failed" ? "Chưa đọc được record hợp lệ" : "Đang chờ resolver")
        };
      });
  }

  return domainCards.value
    .filter((domain) => domain.status !== "verified")
    .map((domain) => ({
      id: domain.id,
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
    }));
});

const sslPendingRows = computed(() => {
  if (domainOperationStates.value.length > 0) {
    return domainOperationStates.value
      .filter((domainState) => domainState.sslStatus === "pending" || domainState.sslStatus === "failed")
      .map((domainState, index) => ({
        id: domainState.domainId,
        domainName: domainState.domainName,
        tenantSlug: tenant.value?.slug ?? "tenant",
        orderId: `be-domain-${domainState.domainId}`,
        status: domainState.sslStatus === "failed" ? domainState.message || "SSL issuing failed" : domainState.message || "Waiting for certificate issuance",
        eta: domainState.nextRetryAt ? `Poll ${formatDomainTimestamp(domainState.nextRetryAt)}` : "ETA pending",
        progress: domainState.sslStatus === "failed" ? 18 : Math.min(88, 40 + index * 14)
      }));
  }

  return domainCards.value
    .filter((domain) => domain.status === "pending")
    .map((domain, index) => ({
      id: domain.id,
      domainName: domain.domainName,
      tenantSlug: tenant.value?.slug ?? "tenant",
      orderId: `mock-acme-${tenant.value?.slug ?? "tenant"}-${index + 1}`,
      status: index % 2 === 0 ? "Verifying ACME challenge" : "Submitting CSR",
      eta: index % 2 === 0 ? "ETA 45s" : "ETA 2m",
      progress: index % 2 === 0 ? 68 : 34
    }));
});

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

const auditEvents = computed<TenantAuditEvent[]>(() => {
  if (!tenant.value) {
    return [];
  }

  return [...localAuditEvents.value, ...baseAuditEvents(tenant.value)].sort(
    (left, right) => new Date(right.occurredAt).getTime() - new Date(left.occurredAt).getTime()
  );
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

function domainStatusFromDns(status: TenantDomainDnsSslState["dnsStatus"]): TenantDomainStatus {
  if (status === "verified") {
    return "verified";
  }

  if (status === "failed") {
    return "failed";
  }

  return "pending";
}

function dnsRecordType(recordType: string | undefined): "CNAME" | "TXT" {
  return recordType?.toUpperCase() === "TXT" ? "TXT" : "CNAME";
}

function dnsIssue(status: TenantDomainDnsSslState["dnsStatus"]) {
  if (status === "failed") {
    return "Record mismatch";
  }

  if (status === "propagating") {
    return "Propagation pending";
  }

  return "Resolver pending";
}

function formatDomainTimestamp(value: string | undefined) {
  if (!value) {
    return "Chưa có lần kiểm tra";
  }

  const timestamp = new Date(value).getTime();
  if (Number.isNaN(timestamp)) {
    return value;
  }

  const diffMs = Date.now() - timestamp;
  const absMs = Math.abs(diffMs);
  const minute = 60 * 1000;
  const hour = 60 * minute;

  if (absMs < minute) {
    return diffMs >= 0 ? "vừa xong" : "sắp tới";
  }

  if (absMs < hour) {
    const minutes = Math.round(absMs / minute);
    return diffMs >= 0 ? `${minutes} phút trước` : `sau ${minutes} phút`;
  }

  const hours = Math.round(absMs / hour);
  return diffMs >= 0 ? `${hours} giờ trước` : `sau ${hours} giờ`;
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

function lifecycleActionLabel(action: LifecycleAction) {
  if (action === "activate") {
    return "Kích hoạt phòng khám";
  }

  if (action === "restore") {
    return "Khôi phục phòng khám";
  }

  if (action === "archive") {
    return "Lưu trữ tenant";
  }

  return "Tạm ngưng phòng khám";
}

function targetStatusForAction(action: LifecycleAction): TenantStatus {
  if (action === "archive") {
    return "Archived";
  }

  if (action === "suspend") {
    return "Suspended";
  }

  return "Active";
}

function minutesAgo(minutes: number) {
  return new Date(Date.now() - minutes * 60 * 1000).toISOString();
}

function daysAgo(days: number) {
  return new Date(Date.now() - days * 24 * 60 * 60 * 1000).toISOString();
}

function baseAuditEvents(currentTenant: TenantDetail): TenantAuditEvent[] {
  const domainStatus = formatDomainStatus(currentTenant.domainStatus);

  return [
    {
      id: `${currentTenant.id}-audit-domain`,
      type: "domain",
      title: "Domain verification snapshot",
      detail: `${currentTenant.defaultDomainName || `${currentTenant.slug}.clinicos.vn`} đang ở trạng thái ${domainStatus}.`,
      actor: "domain-service mock",
      occurredAt: minutesAgo(18),
      outcome: currentTenant.domainStatus === "failed" ? "danger" : currentTenant.domainStatus === "pending" ? "warning" : "success",
      metadata: ["DNS", "SSL"]
    },
    {
      id: `${currentTenant.id}-audit-plan`,
      type: "plan",
      title: "Plan entitlement checked",
      detail: `${currentTenant.planDisplayName} đang bật ${currentTenant.moduleCodes.length} module cho tenant.`,
      actor: "Owner Admin",
      occurredAt: daysAgo(1),
      outcome: "neutral",
      metadata: [currentTenant.planCode, `${currentTenant.moduleCodes.length} modules`]
    },
    {
      id: `${currentTenant.id}-audit-lifecycle`,
      type: "lifecycle",
      title: "Tenant status loaded",
      detail: `Hồ sơ tenant được mở ở trạng thái ${currentTenant.status}.`,
      actor: "Owner Admin",
      occurredAt: daysAgo(2),
      outcome: currentTenant.status === "Active" ? "success" : currentTenant.status === "Draft" ? "warning" : "danger",
      metadata: [currentTenant.status]
    },
    {
      id: `${currentTenant.id}-audit-mock`,
      type: "mock",
      title: "Mock-first audit source",
      detail: "Audit drawer dùng dữ liệu local UI state trong khi backend audit API chưa được chốt contract.",
      actor: "frontend mock",
      occurredAt: daysAgo(5),
      outcome: "neutral",
      metadata: ["contract-first", "no-api-call"]
    }
  ];
}

async function loadTenant() {
  clearRetryTimer();
  clearLifecycleTimer();
  clearAuditTimer();
  loading.value = true;
  error.value = null;
  tenant.value = undefined;
  domainOperationState.value = "ready";
  domainOperationMessage.value = undefined;
  lifecycleState.value = "idle";
  lifecycleMessage.value = undefined;
  auditDrawerOpen.value = false;
  auditLoading.value = false;
  auditError.value = undefined;
  localAuditEvents.value = [];
  domainOperationStates.value = [];

  try {
    const loadedTenant = await tenantClient.getTenant(props.tenantId);
    tenant.value = loadedTenant;
    await refreshDomainOperations(loadedTenant.id);
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
  openLifecycleModal(primaryLifecycleAction.value);
}

function openLifecycleModal(action: LifecycleAction) {
  if (isLifecycleBusy.value) {
    return;
  }

  clearLifecycleTimer();
  lifecycleAction.value = action;
  lifecycleState.value = "idle";
  lifecycleMessage.value = undefined;
  lifecycleModalOpen.value = true;
}

function closeLifecycleModal() {
  if (isLifecycleBusy.value) {
    return;
  }

  clearLifecycleTimer();
  lifecycleModalOpen.value = false;
  lifecycleState.value = "idle";
  lifecycleMessage.value = undefined;
}

function openAuditDrawer() {
  auditDrawerOpen.value = true;
  refreshAuditLog();
}

function refreshAuditLog() {
  clearAuditTimer();
  auditLoading.value = true;
  auditError.value = undefined;

  auditTimer = window.setTimeout(() => {
    auditLoading.value = false;
    auditTimer = undefined;
  }, 420);
}

async function confirmLifecycle(action: LifecycleAction, reason: string) {
  if (!tenant.value) {
    lifecycleState.value = "error";
    lifecycleMessage.value = "Không tìm thấy tenant hiện tại để cập nhật status.";
    return;
  }

  clearLifecycleTimer();
  const status = targetStatusForAction(action);
  lifecycleState.value = "loading";
  lifecycleMessage.value = reason
    ? `Đang gửi yêu cầu cập nhật status sang ${status}. Lý do được giữ trong audit UI local: ${reason}`
    : `Đang gửi yêu cầu cập nhật status sang ${status}.`;

  try {
    const updatedTenant = await tenantClient.updateTenantStatus(tenant.value.id, { status });
    const updatedStatus = updatedTenant.status;
    tenant.value = updatedTenant;
    localAuditEvents.value.unshift({
      id: `audit-${action}-${Date.now()}`,
      type: "lifecycle",
      title: `${lifecycleActionLabel(action)} đã xác nhận`,
      detail: reason
        ? `Owner Super Admin xác nhận với lý do local: ${reason}. Status đã cập nhật qua Tenant API.`
        : "Owner Super Admin xác nhận cập nhật status qua Tenant API.",
      actor: "Owner Super Admin",
      occurredAt: new Date().toISOString(),
      outcome: updatedStatus === "Active" ? "success" : "warning",
      metadata: reason ? [updatedStatus, "api-status-update", "reason-local-only"] : [updatedStatus, "api-status-update"]
    });
    lifecycleState.value = "success";
    lifecycleMessage.value = `${updatedTenant.displayName} đã chuyển sang ${updatedStatus} từ Tenant API.`;
    lifecycleTimer = window.setTimeout(() => {
      lifecycleModalOpen.value = false;
      lifecycleState.value = "idle";
      lifecycleMessage.value = undefined;
      lifecycleTimer = undefined;
    }, 760);
  } catch (lifecycleError) {
    lifecycleState.value = "error";
    lifecycleMessage.value =
      lifecycleError instanceof Error ? lifecycleError.message : "Không cập nhật được tenant status qua Tenant API.";
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

async function refreshDomainOperations(tenantId: string) {
  domainOperationState.value = "loading";
  domainOperationMessage.value = "Đang tải DNS/SSL state từ Domain API.";

  try {
    domainOperationStates.value = await tenantClient.listTenantDomainDnsSslStates(tenantId);
    domainOperationState.value = "ready";
    domainOperationMessage.value = undefined;
  } catch (operationError) {
    domainOperationStates.value = [];
    domainOperationState.value = "error";
    domainOperationMessage.value =
      operationError instanceof Error ? operationError.message : "Không tải được DNS/SSL state từ Domain API.";
  }
}

function clearRetryTimer() {
  if (retryTimer !== undefined) {
    window.clearTimeout(retryTimer);
    retryTimer = undefined;
  }
}

function clearLifecycleTimer() {
  if (lifecycleTimer !== undefined) {
    window.clearTimeout(lifecycleTimer);
    lifecycleTimer = undefined;
  }
}

function clearAuditTimer() {
  if (auditTimer !== undefined) {
    window.clearTimeout(auditTimer);
    auditTimer = undefined;
  }
}

function selectDomainSurface(surface: DomainSurface) {
  activeDomainSurface.value = surface;
  domainOperationState.value = "ready";
  domainOperationMessage.value = undefined;
}

function upsertDomainOperationState(nextState: TenantDomainDnsSslState) {
  const index = domainOperationStates.value.findIndex((item) => item.domainId === nextState.domainId);
  if (index === -1) {
    domainOperationStates.value = [...domainOperationStates.value, nextState];
    return;
  }

  domainOperationStates.value = domainOperationStates.value.map((item, itemIndex) =>
    itemIndex === index ? nextState : item
  );
}

function runMockDiagnostic(rowId: string) {
  domainOperationState.value = "error";
  domainOperationMessage.value = `Diagnostic cho ${rowId} phát hiện record chưa khớp expected value.`;
}

async function retryDnsRow(domainId: string) {
  if (!tenant.value) {
    return;
  }

  clearRetryTimer();
  domainOperationState.value = "loading";
  domainOperationMessage.value = `Đang gửi DNS retry cho domain ${domainId}.`;

  try {
    const nextState = await tenantClient.retryTenantDomainDns(tenant.value.id, domainId);
    upsertDomainOperationState(nextState);
    domainOperationState.value = "success";
    domainOperationMessage.value = nextState.message || `Domain ${nextState.domainName} đã nhận lệnh DNS retry.`;
  } catch (operationError) {
    domainOperationState.value = "error";
    domainOperationMessage.value =
      operationError instanceof Error ? operationError.message : "Không gửi được DNS retry.";
  }
}

async function retryDnsRows(domainIds: string[]) {
  for (const domainId of domainIds) {
    await retryDnsRow(domainId);
  }
}

async function retrySslRow(domainId: string) {
  if (!tenant.value) {
    return;
  }

  clearRetryTimer();
  domainOperationState.value = "loading";
  domainOperationMessage.value = `Đang poll SSL status cho domain ${domainId}.`;

  try {
    const nextState = await tenantClient.getTenantDomainSslStatus(tenant.value.id, domainId);
    upsertDomainOperationState(nextState);
    domainOperationState.value = "success";
    domainOperationMessage.value = nextState.message || `SSL status của ${nextState.domainName} đã được cập nhật.`;
  } catch (operationError) {
    domainOperationState.value = "error";
    domainOperationMessage.value =
      operationError instanceof Error ? operationError.message : "Không tải được SSL status.";
  }
}

onMounted(loadTenant);
onUnmounted(() => {
  clearRetryTimer();
  clearLifecycleTimer();
  clearAuditTimer();
});
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
        <AppButton v-if="tenant" label="Audit log" variant="secondary" @click="openAuditDrawer" />
        <AppButton
          v-if="tenant"
          :label="primaryLifecycleLabel"
          :variant="primaryLifecycleAction === 'suspend' ? 'danger' : 'primary'"
          :disabled="isLifecycleBusy"
          :loading="isLifecycleBusy"
          @click="updateCurrentStatus"
        />
        <AppButton
          v-if="tenant && tenant.status !== 'Archived'"
          label="Lưu trữ tenant"
          variant="danger"
          :disabled="isLifecycleBusy"
          :loading="isLifecycleBusy && lifecycleAction === 'archive'"
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
          <span class="mock-badge">API-first</span>
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
      :current-status="tenant.status"
      :target-status="lifecycleTargetStatus"
      :state="lifecycleState"
      :message="lifecycleMessage"
      @close="closeLifecycleModal"
      @confirm="confirmLifecycle"
    />

    <TenantAuditLogDrawer
      v-if="tenant"
      :open="auditDrawerOpen"
      :tenant-name="tenant.displayName"
      :tenant-slug="tenant.slug"
      :events="auditEvents"
      :loading="auditLoading"
      :error="auditError"
      @close="auditDrawerOpen = false"
      @refresh="refreshAuditLog"
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
