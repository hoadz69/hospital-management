<script setup lang="ts">
import type { TenantStatus, TenantSummary } from "@clinic-saas/shared-types";
import { ModuleChips, PlanBadge, StatusPill } from "@clinic-saas/ui";
import { formatModuleCode, formatTenantStatus } from "../services/labels";

type DisplayTone = "success" | "warning" | "danger" | "info" | "neutral";

defineProps<{
  tenants: TenantSummary[];
  selectedTenantId?: string;
  loading?: boolean;
}>();

defineEmits<{
  select: [tenantId: string];
}>();

const moduleTotal = 6;

function statusTone(status: TenantStatus): DisplayTone {
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

function planTone(planCode: TenantSummary["planCode"]): DisplayTone {
  if (planCode === "premium") {
    return "warning";
  }

  if (planCode === "growth") {
    return "neutral";
  }

  return "info";
}

function moduleChipItems(moduleCodes: TenantSummary["moduleCodes"]) {
  return moduleCodes.map((moduleCode) => ({
    key: moduleCode,
    label: formatModuleCode(moduleCode),
    enabled: true,
    tone: "success" as const
  }));
}

function formatDate(value: string) {
  return new Date(value).toLocaleDateString("vi-VN");
}
</script>

<template>
  <div class="table-shell">
    <table aria-label="Danh sách tenant Owner Admin">
      <thead>
        <tr>
          <th>Slug</th>
          <th>Tên hiển thị</th>
          <th>Trạng thái</th>
          <th>Gói</th>
          <th>Modules</th>
          <th>Tên miền</th>
          <th>Ngày tạo</th>
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
          @keydown.space.prevent="$emit('select', tenant.id)"
        >
          <td class="slug-cell">{{ tenant.slug }}</td>
          <td>
            <strong>{{ tenant.displayName }}</strong>
          </td>
          <td>
            <StatusPill :label="formatTenantStatus(tenant.status)" :tone="statusTone(tenant.status)" />
          </td>
          <td>
            <PlanBadge :label="tenant.planDisplayName" :tone="planTone(tenant.planCode)" />
          </td>
          <td>
            <ModuleChips
              :items="moduleChipItems(tenant.moduleCodes)"
              :total="moduleTotal"
              compact
              :aria-label="`${tenant.moduleCodes.length}/${moduleTotal} module đang bật`"
            />
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
  border-radius: var(--radius-card);
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
  height: 60px;
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

tbody tr:focus-visible {
  outline: 2px solid color-mix(in srgb, var(--color-brand-primary) 34%, transparent);
  outline-offset: -2px;
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
