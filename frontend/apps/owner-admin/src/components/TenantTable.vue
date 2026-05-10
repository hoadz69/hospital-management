<script setup lang="ts">
import type { TenantStatus, TenantSummary } from "@clinic-saas/shared-types";
import { StatusPill } from "@clinic-saas/ui";
import { formatTenantStatus } from "../services/labels";

defineProps<{
  tenants: TenantSummary[];
  selectedTenantId?: string;
  loading?: boolean;
}>();

defineEmits<{
  select: [tenantId: string];
}>();

const moduleTotal = 6;

function statusTone(status: TenantStatus) {
  if (status === "Active") {
    return "success";
  }

  if (status === "Suspended") {
    return "danger";
  }

  if (status === "Archived") {
    return "neutral";
  }

  return "warning";
}

function planTone(planCode: TenantSummary["planCode"]) {
  if (planCode === "premium") {
    return "warning";
  }

  if (planCode === "growth") {
    return "neutral";
  }

  return "info";
}

function formatDate(value: string) {
  return new Date(value).toLocaleDateString("vi-VN");
}
</script>

<template>
  <div class="table-shell">
    <table>
      <thead>
        <tr>
          <th>Slug</th>
          <th>Display name</th>
          <th>Status</th>
          <th>Plan</th>
          <th>Modules</th>
          <th>Default domain</th>
          <th>Created</th>
          <th class="action-col"></th>
        </tr>
      </thead>
      <tbody>
        <tr v-if="loading">
          <td colspan="8" class="state-cell">Đang tải danh sách phòng khám...</td>
        </tr>
        <tr v-else-if="tenants.length === 0">
          <td colspan="8" class="state-cell">Không có phòng khám phù hợp bộ lọc hiện tại.</td>
        </tr>
        <tr
          v-for="tenant in tenants"
          v-else
          :key="tenant.id"
          :class="{ selected: tenant.id === selectedTenantId }"
          tabindex="0"
          @click="$emit('select', tenant.id)"
          @keydown.enter.prevent="$emit('select', tenant.id)"
        >
          <td class="slug-cell">{{ tenant.slug }}</td>
          <td>
            <strong>{{ tenant.displayName }}</strong>
          </td>
          <td>
            <StatusPill :label="formatTenantStatus(tenant.status)" :tone="statusTone(tenant.status)" />
          </td>
          <td>
            <StatusPill :label="tenant.planDisplayName" :tone="planTone(tenant.planCode)" />
          </td>
          <td>
            <div class="module-meter" :aria-label="`${tenant.moduleCodes.length}/${moduleTotal} module đang bật`">
              <span
                v-for="index in moduleTotal"
                :key="index"
                :class="{ enabled: index <= tenant.moduleCodes.length }"
              ></span>
              <small>{{ tenant.moduleCodes.length }}/{{ moduleTotal }}</small>
            </div>
          </td>
          <td class="domain-cell">{{ tenant.defaultDomainName || "—" }}</td>
          <td>{{ formatDate(tenant.createdAt) }}</td>
          <td class="action-col" aria-hidden="true">›</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style scoped>
.table-shell {
  width: 100%;
  overflow-x: auto;
}

table {
  width: 100%;
  min-width: 1040px;
  border-collapse: collapse;
}

thead {
  position: sticky;
  top: 0;
  z-index: 1;
}

th {
  height: 52px;
  border-bottom: 1px solid var(--color-border-subtle);
  padding: 0 var(--space-4);
  background: var(--color-surface-ivory);
  color: var(--color-text-muted);
  font-size: 11px;
  font-weight: 800;
  text-align: left;
  text-transform: uppercase;
}

td {
  height: 64px;
  border-bottom: 1px solid var(--color-border-subtle);
  padding: 0 var(--space-4);
  color: var(--color-text-secondary);
  font-size: 12px;
  vertical-align: middle;
}

tbody tr {
  cursor: pointer;
  transition: background var(--motion-duration-xs) var(--motion-ease-standard);
}

tbody tr:nth-child(even) {
  background: color-mix(in srgb, var(--color-surface-ivory) 40%, transparent);
}

tbody tr:hover,
tbody tr:focus-visible,
tbody tr.selected {
  background: var(--color-surface-muted);
  outline: none;
}

td strong {
  display: block;
  max-width: 220px;
  color: var(--color-text-primary);
  font-size: 13px;
  line-height: 18px;
}

.slug-cell {
  font-weight: 700;
}

.domain-cell {
  color: var(--color-text-primary);
  font-weight: 700;
}

.module-meter {
  display: flex;
  align-items: center;
  gap: 4px;
}

.module-meter span {
  width: 8px;
  height: 8px;
  border-radius: 2px;
  background: var(--color-surface-muted);
}

.module-meter span.enabled {
  background: var(--color-status-success);
}

.module-meter small {
  margin-left: 4px;
  color: var(--color-text-secondary);
  font-size: 11px;
  font-weight: 700;
}

.state-cell {
  height: 140px;
  color: var(--color-text-secondary);
  text-align: center;
}

.action-col {
  width: 56px;
  color: var(--color-text-muted);
  font-size: 18px;
  font-weight: 800;
  text-align: center;
}
</style>
