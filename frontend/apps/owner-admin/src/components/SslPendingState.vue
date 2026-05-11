<script setup lang="ts">
import { computed } from "vue";

export type SslPendingRow = {
  id: string;
  domainName: string;
  tenantSlug: string;
  orderId: string;
  status: string;
  eta: string;
  progress: number;
};

const props = withDefaults(
  defineProps<{
    rows?: SslPendingRow[];
  }>(),
  {
    rows: () => []
  }
);

const emit = defineEmits<{
  reissue: [rowId: string];
}>();

const fallbackRows: SslPendingRow[] = [
  {
    id: "ssl-tamduc",
    domainName: "tamduc-derm.com.vn",
    tenantSlug: "tamduc-derm",
    orderId: "le-order-2026-05-10-7421",
    status: "Verifying ACME challenge",
    eta: "ETA 30s",
    progress: 78
  },
  {
    id: "ssl-saigonkids",
    domainName: "saigon-kids.vn",
    tenantSlug: "saigon-kids",
    orderId: "le-order-2026-05-10-7422",
    status: "Submitting CSR",
    eta: "ETA 2m",
    progress: 36
  },
  {
    id: "ssl-ndtp",
    domainName: "ndtp-pediatric.com",
    tenantSlug: "ndtp-pediatric",
    orderId: "le-order-2026-05-10-7423",
    status: "Creating challenge",
    eta: "ETA 3m",
    progress: 18
  }
];

const displayRows = computed(() => (props.rows.length > 0 ? props.rows : fallbackRows));
const pipelineSteps = ["Domain verified", "Create CSR", "Submit challenge", "ACME validate", "Issue cert", "Active"];

function progressWidth(progress: number) {
  return `${Math.min(Math.max(progress, 8), 100)}%`;
}
</script>

<template>
  <section class="ssl-pending-surface" aria-labelledby="ssl-pending-title">
    <header class="surface-header">
      <div>
        <p class="surface-eyebrow">SSL pipeline</p>
        <h3 id="ssl-pending-title">SSL Issuing Pipeline</h3>
      </div>
      <span class="ssl-count">SSL issuing ({{ displayRows.length }})</span>
    </header>

    <p class="eta-copy">ETA Let's Encrypt: 1-3 phút mỗi cert · Auto-renew 30 ngày trước expiry</p>

    <div class="ssl-table" role="table" aria-label="Danh sách SSL pending">
      <article v-for="row in displayRows" :key="row.id" class="ssl-row" role="row">
        <div class="ssl-row-top">
          <div class="domain-cell">
            <strong>{{ row.domainName }}</strong>
            <small>Tenant: {{ row.tenantSlug }} · Order: {{ row.orderId }}</small>
          </div>
          <span class="status-pill">{{ row.status }}</span>
          <span class="eta">{{ row.eta }}</span>
          <button type="button" @click="$emit('reissue', row.id)">Re-issue</button>
        </div>
        <div class="progress-track" aria-hidden="true">
          <span :style="{ width: progressWidth(row.progress) }"></span>
        </div>
      </article>
    </div>

    <div class="state-machine" aria-label="ACME challenge state machine">
      <p>ACME challenge state machine</p>
      <div class="pipeline">
        <template v-for="(step, index) in pipelineSteps" :key="step">
          <span :data-step="index">{{ step }}</span>
          <i v-if="index < pipelineSteps.length - 1" aria-hidden="true"></i>
        </template>
      </div>
    </div>
  </section>
</template>

<style scoped>
.ssl-pending-surface {
  display: grid;
  gap: var(--space-4);
  min-width: 0;
}

.surface-header,
.ssl-row-top,
.pipeline {
  display: flex;
  align-items: center;
  gap: var(--space-3);
}

.surface-header {
  justify-content: space-between;
}

.surface-header h3,
.surface-header p,
.eta-copy,
.state-machine p {
  margin: 0;
}

.surface-eyebrow {
  color: var(--color-status-specialty);
  font-size: 11px;
  font-weight: 900;
  text-transform: uppercase;
}

.surface-header h3 {
  margin-top: var(--space-1);
  color: var(--color-text-primary);
  font-size: 18px;
}

.ssl-count {
  display: inline-flex;
  min-height: 28px;
  align-items: center;
  border-radius: var(--radius-pill);
  padding: 0 var(--space-3);
  background: var(--color-status-specialty);
  color: var(--color-surface-elevated);
  font-size: 11px;
  font-weight: 900;
  white-space: nowrap;
}

.eta-copy {
  color: var(--color-text-secondary);
  font-size: 12px;
  line-height: 18px;
}

.ssl-table,
.state-machine {
  overflow: hidden;
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-card);
  background: var(--color-surface-elevated);
}

.ssl-row {
  display: grid;
  gap: var(--space-2);
  padding: var(--space-4);
}

.ssl-row + .ssl-row {
  border-top: 1px solid var(--color-border-subtle);
}

.ssl-row-top {
  min-width: 0;
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
  font-size: 14px;
}

.domain-cell small {
  overflow-wrap: anywhere;
  color: var(--color-text-muted);
  font-size: 11px;
}

.status-pill {
  display: inline-flex;
  min-height: 24px;
  align-items: center;
  border-radius: 5px;
  padding: 0 var(--space-2);
  background: var(--color-status-specialty);
  color: var(--color-surface-elevated);
  font-size: 11px;
  font-weight: 900;
  white-space: nowrap;
}

.eta {
  color: var(--color-text-secondary);
  font-size: 11px;
  font-weight: 800;
  white-space: nowrap;
}

button {
  min-height: 32px;
  border: 1px solid var(--color-border-subtle);
  border-radius: 8px;
  padding: 0 var(--space-3);
  background: var(--color-surface-elevated);
  color: var(--color-text-secondary);
  cursor: pointer;
  font: inherit;
  font-size: 11px;
  font-weight: 800;
}

button:focus-visible {
  outline: 2px solid color-mix(in srgb, var(--color-status-specialty) 35%, transparent);
  outline-offset: 2px;
}

.progress-track {
  height: 6px;
  overflow: hidden;
  border-radius: var(--radius-pill);
  background: var(--color-surface-muted);
}

.progress-track span {
  display: block;
  height: 100%;
  border-radius: inherit;
  background: var(--color-status-specialty);
}

.state-machine {
  display: grid;
  gap: var(--space-4);
  padding: var(--space-4);
}

.state-machine p {
  color: var(--color-text-primary);
  font-size: 13px;
  font-weight: 900;
}

.pipeline span {
  min-height: 32px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: var(--radius-pill);
  padding: 0 var(--space-3);
  background: var(--color-status-specialty);
  color: var(--color-surface-elevated);
  font-size: 11px;
  font-weight: 900;
  white-space: nowrap;
}

.pipeline span[data-step="0"],
.pipeline span[data-step="5"] {
  background: var(--color-status-success);
}

.pipeline span[data-step="1"] {
  background: var(--color-status-info);
}

.pipeline span[data-step="4"] {
  background: var(--color-brand-accent);
}

.pipeline i {
  height: 2px;
  flex: 1 1 24px;
  background: var(--color-text-muted);
}

@media (max-width: 900px) {
  .surface-header,
  .ssl-row-top {
    align-items: stretch;
    flex-direction: column;
  }

  .pipeline {
    align-items: stretch;
    flex-direction: column;
  }

  .pipeline i {
    width: 2px;
    height: 16px;
    flex: 0 0 16px;
    margin-left: var(--space-4);
  }
}
</style>
