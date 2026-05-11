<script setup lang="ts">
import { computed } from "vue";

export type DomainDnsRetryRow = {
  id: string;
  domainName: string;
  tenantSlug: string;
  issue: string;
  lastCheck: string;
  selected?: boolean;
  expanded?: boolean;
  expected?: string;
  actual?: string;
};

const props = withDefaults(
  defineProps<{
    rows?: DomainDnsRetryRow[];
  }>(),
  {
    rows: () => []
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
    issue: "TXT mismatch",
    lastCheck: "2 phút trước",
    selected: true,
    expanded: true,
    expected: "clinicos-tenant-verify=8a7f3e2c",
    actual: "clinicos-tenant-verify=8a7f3e2c-OLD-2025"
  },
  {
    id: "dns-hongduc",
    domainName: "hongduc-obgyn.com.vn",
    tenantSlug: "hongduc-obgyn",
    issue: "TXT not found",
    lastCheck: "2 phút trước",
    selected: true
  },
  {
    id: "dns-phuongmai",
    domainName: "phuongmaiclinic.com",
    tenantSlug: "phuong-mai-clinic",
    issue: "DNS timeout 30s",
    lastCheck: "2 phút trước",
    selected: true
  },
  {
    id: "dns-saigonkids",
    domainName: "saigonkidsclinic.vn",
    tenantSlug: "saigon-kids",
    issue: "ACME challenge fail",
    lastCheck: "2 phút trước"
  }
];

const displayRows = computed(() => (props.rows.length > 0 ? props.rows : fallbackRows));
const selectedRows = computed(() => displayRows.value.filter((row) => row.selected));

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
      <button type="button" class="bulk-button" :disabled="selectedRows.length === 0" @click="emitBulkRetry">
        Bulk re-verify selected ({{ selectedRows.length }})
      </button>
    </header>

    <div class="filter-row" aria-label="Bộ lọc trạng thái DNS">
      <span>Lọc:</span>
      <button type="button">All</button>
      <button type="button">Pending (4)</button>
      <button type="button">Propagating (2)</button>
      <button type="button">Active (242)</button>
      <button type="button" class="active">Error ({{ displayRows.length }})</button>
    </div>

    <div class="dns-table" role="table" aria-label="Danh sách domain lỗi DNS">
      <article v-for="row in displayRows" :key="row.id" class="dns-row" role="row">
        <div class="dns-row-main">
          <span class="checkmark" :data-selected="row.selected ? 'true' : 'false'" aria-hidden="true">
            {{ row.selected ? "✓" : "" }}
          </span>
          <div class="domain-cell">
            <strong>{{ row.domainName }}</strong>
            <small>Tenant: {{ row.tenantSlug }}</small>
          </div>
          <span class="issue-pill">× {{ row.issue }}</span>
          <small class="last-check">Last check: {{ row.lastCheck }}</small>
          <div class="row-actions">
            <button type="button" @click="$emit('diagnostic', row.id)">Diagnostic</button>
            <button type="button" class="primary" @click="$emit('retry', row.id)">Retry</button>
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
            <button type="button" @click="$emit('retry', row.id)">Retry verify</button>
            <button type="button" class="warning" disabled>Manual override</button>
            <button type="button" class="neutral" disabled>Contact tenant admin</button>
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
.filter-row,
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

.bulk-button,
.filter-row button,
.row-actions button,
.diagnostic-actions button {
  min-height: 32px;
  border: 1px solid var(--color-border-subtle);
  border-radius: 8px;
  padding: 0 var(--space-3);
  background: var(--color-surface-elevated);
  color: var(--color-text-primary);
  cursor: pointer;
  font: inherit;
  font-size: 11px;
  font-weight: 800;
}

.bulk-button,
.row-actions button.primary,
.diagnostic-actions button:first-child {
  border-color: var(--color-status-specialty);
  background: var(--color-status-specialty);
  color: var(--color-surface-elevated);
}

button:disabled {
  cursor: not-allowed;
  opacity: 0.62;
}

button:focus-visible {
  outline: 2px solid color-mix(in srgb, var(--color-status-specialty) 35%, transparent);
  outline-offset: 2px;
}

.filter-row {
  flex-wrap: wrap;
  color: var(--color-text-secondary);
  font-size: 12px;
  font-weight: 800;
}

.filter-row button {
  min-height: 28px;
  border-radius: var(--radius-pill);
  font-size: 11px;
}

.filter-row button.active {
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

.dns-row + .dns-row {
  border-top: 1px solid var(--color-border-subtle);
}

.dns-row-main {
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
  flex: 1 1 auto;
}

.domain-cell strong {
  overflow-wrap: anywhere;
  color: var(--color-text-primary);
  font-size: 13px;
}

.domain-cell small,
.last-check {
  color: var(--color-text-muted);
  font-size: 11px;
}

.issue-pill {
  display: inline-flex;
  min-height: 24px;
  align-items: center;
  border-radius: 5px;
  padding: 0 var(--space-2);
  background: var(--color-status-danger);
  color: var(--color-surface-elevated);
  font-size: 10px;
  font-weight: 900;
  white-space: nowrap;
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

.diagnostic-actions .warning {
  border-color: var(--color-status-warning);
  background: var(--color-status-warning);
  color: var(--color-surface-elevated);
}

.diagnostic-actions .neutral {
  border-color: var(--color-text-muted);
  background: var(--color-text-muted);
  color: var(--color-surface-elevated);
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
}
</style>
