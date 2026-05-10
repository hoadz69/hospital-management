<script setup lang="ts">
// Bảng resource index hiển thị tenant cho Owner Admin.
// Bám frame Figma `V2 - Owner Admin Tenant Operations`: cols Tenant, Slug, Plan, Domain, Status, Action.
// Component thuần stateless, mọi data và selected state đều nhận từ parent qua props.
import type { TenantStatus, TenantSummary } from "@clinic-saas/shared-types";
import { StatusPill } from "@clinic-saas/ui";
import { formatDomainStatus, formatTenantStatus } from "../services/labels";

defineProps<{
  /** Danh sách tenant đã filter từ parent. */
  tenants: TenantSummary[];
  /** Tenant đang được chọn để highlight row tương ứng. */
  selectedTenantId?: string;
  /** Cờ loading cho row trạng thái khi đang fetch. */
  loading?: boolean;
}>();

defineEmits<{
  select: [tenantId: string];
}>();

function statusTone(status: TenantStatus) {
  if (status === "Active") {
    return "success";
  }

  if (status === "Suspended" || status === "Archived") {
    return "danger";
  }

  return "warning";
}

function domainTone(domainStatus: TenantSummary["domainStatus"]) {
  if (domainStatus === "verified") {
    return "success";
  }

  if (domainStatus === "failed") {
    return "danger";
  }

  if (domainStatus === "pending") {
    return "warning";
  }

  return "neutral";
}
</script>

<template>
  <div class="table-shell">
    <table>
      <thead>
        <tr>
          <th>Phòng khám</th>
          <th>Slug</th>
          <th>Gói</th>
          <th>Tên miền</th>
          <th>Trạng thái</th>
          <th class="action-col">Thao tác</th>
        </tr>
      </thead>
      <tbody>
        <tr v-if="loading">
          <td colspan="6" class="state-cell">Đang tải danh sách phòng khám...</td>
        </tr>
        <tr v-else-if="tenants.length === 0">
          <td colspan="6" class="state-cell">Không có phòng khám phù hợp bộ lọc hiện tại.</td>
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
          <td>
            <strong>{{ tenant.displayName }}</strong>
            <span>{{ tenant.id }}</span>
          </td>
          <td>{{ tenant.slug }}</td>
          <td>
            <StatusPill :label="tenant.planDisplayName" tone="info" />
          </td>
          <td>
            <StatusPill :label="formatDomainStatus(tenant.domainStatus)" :tone="domainTone(tenant.domainStatus)" />
            <span class="domain-name">{{ tenant.defaultDomainName }}</span>
          </td>
          <td>
            <StatusPill :label="formatTenantStatus(tenant.status)" :tone="statusTone(tenant.status)" />
          </td>
          <td class="action-col">
            <button class="action-button" type="button" @click.stop="$emit('select', tenant.id)">
              Xem
            </button>
          </td>
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
  min-width: 920px;
  border-collapse: collapse;
}

th {
  padding: 14px 16px;
  border-bottom: 1px solid #d9e2ec;
  color: #627d98;
  font-size: 12px;
  text-align: left;
  text-transform: uppercase;
}

td {
  padding: 16px;
  border-bottom: 1px solid #edf2f7;
  color: #102a43;
  vertical-align: middle;
}

tbody tr {
  cursor: pointer;
}

tbody tr:hover,
tbody tr:focus-visible,
tbody tr.selected {
  background: #f0fdfa;
  outline: none;
}

td strong,
td span {
  display: block;
}

td span {
  margin-top: 4px;
  color: #627d98;
  font-size: 13px;
}

.domain-name {
  margin-top: 6px;
  font-family: ui-monospace, "Menlo", "Consolas", monospace;
  font-size: 12px;
}

.state-cell {
  height: 140px;
  color: #627d98;
  text-align: center;
}

.action-col {
  text-align: right;
}

.action-button {
  min-height: 34px;
  border: 1px solid #d9e2ec;
  border-radius: 8px;
  padding: 0 12px;
  background: #ffffff;
  color: #0e7c86;
  cursor: pointer;
  font-weight: 800;
}

.action-button:hover {
  background: #d8f3f1;
}
</style>
