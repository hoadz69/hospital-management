<script setup lang="ts">
import { computed } from "vue";
import { AppButton, StatePanel } from "@clinic-saas/ui";

export type DomainDnsRetryRow = {
  id: string;
  domainName: string;
  tenantSlug: string;
  recordType: "CNAME" | "TXT";
  host: string;
  issue: string;
  lastCheck: string;
  status: "pending" | "propagating" | "failed" | "verified";
  selected?: boolean;
  expanded?: boolean;
  expected?: string;
  actual?: string;
};

type DomainDnsRetryState = "ready" | "loading" | "empty" | "error" | "success";

const props = withDefaults(
  defineProps<{
    rows?: DomainDnsRetryRow[];
    state?: DomainDnsRetryState;
    stateMessage?: string;
  }>(),
  {
    rows: () => [],
    state: "ready",
    stateMessage: undefined
  }
);

const emit = defineEmits<{
  retry: [rowId: string];
  bulkRetry: [rowIds: string[]];
  diagnostic: [rowId: string];
}>();

const fallbackRows: DomainDnsRetryRow[] = [
  {
    id: "dns-phongkham-q1",
    domainName: "phongkham-q1.net",
    tenantSlug: "clinic-saigon-q1",
    recordType: "TXT",
    host: "_clinicos.phongkham-q1.net",
    issue: "TXT mismatch",
    lastCheck: "2 phút trước",
    status: "failed",
    selected: true,
    expanded: true,
    expected: "clinicos-tenant-verify=8a7f3e2c",
    actual: "clinicos-tenant-verify=8a7f3e2c-OLD-2025"
  },
  {
    id: "dns-hongduc",
    domainName: "hongduc-obgyn.com.vn",
    tenantSlug: "hongduc-obgyn",
    recordType: "CNAME",
    host: "www.hongduc-obgyn.com.vn",
    issue: "TXT not found",
    lastCheck: "2 phút trước",
    status: "pending",
    selected: true
  },
  {
    id: "dns-phuongmai",
    domainName: "phuongmaiclinic.com",
    tenantSlug: "phuong-mai-clinic",
    recordType: "CNAME",
    host: "phuongmaiclinic.com",
    issue: "DNS timeout 30s",
    lastCheck: "2 phút trước",
    status: "propagating",
    selected: true
  },
  {
    id: "dns-saigonkids",
    domainName: "saigonkidsclinic.vn",
    tenantSlug: "saigon-kids",
    recordType: "TXT",
    host: "_acme-challenge.saigonkidsclinic.vn",
    issue: "ACME challenge fail",
    lastCheck: "2 phút trước",
    status: "failed"
  }
];

const displayRows = computed(() => (props.rows.length > 0 ? props.rows : fallbackRows));
const selectedRows = computed(() => displayRows.value.filter((row) => row.selected));
const statusCounts = computed(() => ({
  pending: displayRows.value.filter((row) => row.status === "pending").length,
  propagating: displayRows.value.filter((row) => row.status === "propagating").length,
  failed: displayRows.value.filter((row) => row.status === "failed").length,
  verified: displayRows.value.filter((row) => row.status === "verified").length
}));

const stateTitle = computed(() => {
  if (props.state === "loading") {
    return "Đang verify DNS";
  }

  if (props.state === "empty") {
    return "Không có DNS record cần retry";
  }

  if (props.state === "error") {
    return "DNS verify chưa hoàn tất";
  }

  if (props.state === "success") {
    return "DNS retry đã nhận lệnh";
  }

  return "";
});

const stateDescription = computed(() => {
  if (props.stateMessage) {
    return props.stateMessage;
  }

  if (props.state === "loading") {
    return "Owner Admin đang chạy mock retry verify cho các bản ghi được chọn.";
  }

  if (props.state === "empty") {
    return "Tên miền hiện không có CNAME/TXT nào cần retry trong mock queue.";
  }

  if (props.state === "error") {
    return "Mock diagnostic phát hiện bản ghi chưa khớp. Rà lại expected và actual value trước khi retry.";
  }

  if (props.state === "success") {
    return "Retry verify đã được ghi nhận ở local UI state. Chưa có request backend thật.";
  }

  return "";
});

function emitBulkRetry() {
  emit(
    "bulkRetry",
    selectedRows.value.map((row) => row.id)
  );
}
</script>

<template>
  <section class="dns-retry-surface" aria-labelledby="dns-retry-title">
    <header class="surface-header">
      <div>
        <p class="surface-eyebrow">Domain operations</p>
        <h3 id="dns-retry-title">DNS retry queue</h3>
      </div>
      <AppButton
        :label="`Retry verify selected (${selectedRows.length})`"
        :disabled="selectedRows.length === 0"
        :loading="state === 'loading'"
        @click="emitBulkRetry"
      />
    </header>

    <div class="filter-row" aria-label="Bộ lọc trạng thái DNS">
      <span>Lọc:</span>
      <span class="filter-chip">All ({{ displayRows.length }})</span>
      <span class="filter-chip">Pending ({{ statusCounts.pending }})</span>
      <span class="filter-chip">Propagating ({{ statusCounts.propagating }})</span>
      <span class="filter-chip">Active ({{ statusCounts.verified }})</span>
      <span class="filter-chip active">Error ({{ statusCounts.failed }})</span>
    </div>

    <StatePanel
      v-if="state !== 'ready'"
      :title="stateTitle"
      :description="stateDescription"
      :tone="state === 'loading' ? 'loading' : state === 'success' ? 'success' : state === 'error' ? 'danger' : 'neutral'"
      :busy="state === 'loading'"
    >
      <template v-if="state === 'error'" #action>
        <AppButton label="Retry verify" variant="secondary" @click="emitBulkRetry" />
      </template>
    </StatePanel>

    <div v-if="state !== 'loading' && state !== 'empty'" class="dns-table" role="table" aria-label="Danh sách DNS record cần xác minh lại">
      <div class="dns-table-head" role="row">
        <span></span>
        <span>Domain</span>
        <span>Record</span>
        <span>Expected</span>
        <span>Actual</span>
        <span>State</span>
        <span>Action</span>
      </div>
      <article v-for="row in displayRows" :key="row.id" class="dns-row" role="row">
        <div class="dns-row-main">
          <span class="checkmark" :data-selected="row.selected ? 'true' : 'false'" aria-hidden="true">
            {{ row.selected ? "✓" : "" }}
          </span>
          <div class="domain-cell">
            <strong>{{ row.domainName }}</strong>
            <small>{{ row.tenantSlug }} · {{ row.lastCheck }}</small>
          </div>
          <div class="record-cell">
            <strong>{{ row.recordType }}</strong>
            <small>{{ row.host }}</small>
          </div>
          <code>{{ row.expected || "clinicos-tenant-verify=mock" }}</code>
          <code :class="{ danger: row.status === 'failed' }">{{ row.actual || "Đang chờ resolver" }}</code>
          <div class="row-state">
            <span class="issue-pill" :data-status="row.status">{{ row.issue }}</span>
            <small>Last check: {{ row.lastCheck }}</small>
          </div>
          <div class="row-actions">
            <AppButton label="Diagnostic" variant="secondary" @click="$emit('diagnostic', row.id)" />
            <AppButton label="Retry" @click="$emit('retry', row.id)" />
          </div>
        </div>

        <div v-if="row.expanded" class="diagnostic-panel">
          <p>DIAGNOSTIC - DNS query result</p>
          <div class="diagnostic-grid">
            <div>
              <span>Expected</span>
              <code>{{ row.expected }}</code>
            </div>
            <div>
              <span>Actual</span>
              <code class="danger">{{ row.actual }}</code>
            </div>
          </div>
          <div class="diagnostic-actions">
            <AppButton label="Retry verify" @click="$emit('retry', row.id)" />
            <AppButton label="Manual override" variant="secondary" disabled />
            <AppButton label="Contact tenant admin" variant="secondary" disabled />
          </div>
        </div>
      </article>
    </div>
  </section>
</template>

<style scoped>
.dns-retry-surface {
  display: grid;
  gap: var(--space-4);
  min-width: 0;
}

.surface-header,
.dns-row-main,
.row-actions,
.diagnostic-actions {
  display: flex;
  align-items: center;
  gap: var(--space-3);
}

.surface-header {
  justify-content: space-between;
}

.surface-header h3,
.surface-header p,
.diagnostic-panel p {
  margin: 0;
}

.surface-eyebrow {
  color: var(--color-brand-primary);
  font-size: 11px;
  font-weight: 900;
  letter-spacing: 0;
  text-transform: uppercase;
}

.surface-header h3 {
  margin-top: var(--space-1);
  color: var(--color-text-primary);
  font-size: 18px;
}

.filter-row {
  display: flex;
  align-items: center;
  gap: var(--space-2);
  flex-wrap: wrap;
  color: var(--color-text-secondary);
  font-size: 12px;
  font-weight: 800;
}

.filter-chip {
  min-height: 28px;
  display: inline-flex;
  align-items: center;
  border-radius: var(--radius-pill);
  padding: 0 var(--space-3);
  background: var(--color-surface-muted);
  color: var(--color-text-secondary);
  font-size: 11px;
}

.filter-chip.active {
  border-color: var(--color-status-danger);
  background: var(--color-status-danger);
  color: var(--color-surface-elevated);
}

.dns-table {
  overflow: hidden;
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-card);
  background: var(--color-surface-elevated);
}

.dns-table-head {
  display: grid;
  grid-template-columns: 18px minmax(180px, 1.2fr) minmax(160px, 1fr) minmax(160px, 1fr) minmax(160px, 1fr) minmax(130px, 0.8fr) 132px;
  gap: var(--space-3);
  border-bottom: 1px solid var(--color-border-subtle);
  padding: var(--space-3) var(--space-4);
  background: var(--color-surface-muted);
  color: var(--color-text-secondary);
  font-size: 11px;
  font-weight: 900;
  text-transform: uppercase;
}

.dns-row + .dns-row {
  border-top: 1px solid var(--color-border-subtle);
}

.dns-row-main {
  display: grid;
  grid-template-columns: auto minmax(180px, 1.2fr) minmax(160px, 1fr) minmax(160px, 1fr) minmax(160px, 1fr) minmax(130px, 0.8fr) 132px;
  min-width: 0;
  padding: var(--space-4);
}

.checkmark {
  width: 18px;
  height: 18px;
  display: grid;
  place-items: center;
  flex: 0 0 auto;
  border: 1px solid var(--color-border-subtle);
  border-radius: 5px;
  color: var(--color-surface-elevated);
  font-size: 10px;
  font-weight: 900;
}

.checkmark[data-selected="true"] {
  border-color: var(--color-status-danger);
  background: var(--color-status-danger);
}

.domain-cell {
  display: grid;
  gap: 2px;
  min-width: 0;
}

.domain-cell strong,
.record-cell strong {
  overflow-wrap: anywhere;
  color: var(--color-text-primary);
  font-size: 13px;
}

.domain-cell small,
.record-cell small,
.row-state small {
  overflow-wrap: anywhere;
  color: var(--color-text-muted);
  font-size: 11px;
}

.record-cell,
.row-state {
  display: grid;
  align-content: start;
  gap: 4px;
  min-width: 0;
}

.issue-pill {
  display: inline-flex;
  min-height: 24px;
  align-items: center;
  justify-self: start;
  border-radius: 5px;
  padding: 0 var(--space-2);
  background: var(--color-status-warning);
  color: var(--color-surface-elevated);
  font-size: 10px;
  font-weight: 900;
  white-space: nowrap;
}

.issue-pill[data-status="failed"] {
  background: var(--color-status-danger);
}

.issue-pill[data-status="verified"] {
  background: var(--color-status-success);
}

.diagnostic-panel {
  display: grid;
  gap: var(--space-3);
  padding: var(--space-4) var(--space-5);
  background: #0d121f;
  color: #b2bdd1;
}

.diagnostic-panel p,
.diagnostic-grid span {
  color: #b2bdd1;
  font-size: 10px;
  font-weight: 900;
  text-transform: uppercase;
}

.diagnostic-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: var(--space-4);
}

.diagnostic-grid div {
  display: grid;
  gap: var(--space-1);
  border-radius: 8px;
  padding: var(--space-3);
  background: #121a26;
}

code {
  overflow-wrap: anywhere;
  color: var(--color-status-success);
  font-family: ui-monospace, SFMono-Regular, Consolas, monospace;
  font-size: 12px;
}

code.danger {
  color: var(--color-status-danger);
}

@media (max-width: 900px) {
  .surface-header,
  .dns-row-main {
    align-items: stretch;
    flex-direction: column;
  }

  .row-actions,
  .diagnostic-actions {
    flex-wrap: wrap;
  }

  .diagnostic-grid {
    grid-template-columns: 1fr;
  }

  .dns-table-head {
    display: none;
  }

  .dns-row-main {
    grid-template-columns: 1fr;
  }
}
</style>
