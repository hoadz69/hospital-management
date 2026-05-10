<script setup lang="ts">
// Filter bar tenant theo Figma `V2 - Owner Admin Tenant Operations`.
// Component dùng v-model nhận object filters từ parent để giữ stateless và dễ test.
// Nút Export là placeholder: backend domain-service chưa cung cấp endpoint export trong Phase 3.
import type { TenantDomainStatus, TenantPlanCode, TenantStatus } from "@clinic-saas/shared-types";
import { AppButton } from "@clinic-saas/ui";
import { formatDomainStatus, formatModuleCode, formatTenantStatus } from "../services/labels";

export type TenantFilters = {
  status: "all" | TenantStatus;
  plan: "all" | TenantPlanCode;
  domainStatus: "all" | TenantDomainStatus;
  moduleCode: "all" | "website" | "booking" | "catalog" | "payments" | "reports" | "notifications";
};

const filters = defineModel<TenantFilters>({ required: true });

defineEmits<{
  reset: [];
}>();

// Option list cho từng select. Giá trị giữ nguyên enum backend (Draft/Active/...) còn label dùng formatter VN.
// Plan giữ tên thương mại Starter/Growth/Premium vì là tên gói, không cần dịch.
const statusOptions: { value: "all" | TenantStatus; label: string }[] = [
  { value: "all", label: "Tất cả trạng thái" },
  { value: "Draft", label: formatTenantStatus("Draft") },
  { value: "Active", label: formatTenantStatus("Active") },
  { value: "Suspended", label: formatTenantStatus("Suspended") },
  { value: "Archived", label: formatTenantStatus("Archived") }
];

const planOptions: { value: "all" | TenantPlanCode; label: string }[] = [
  { value: "all", label: "Tất cả gói" },
  { value: "starter", label: "Starter" },
  { value: "growth", label: "Growth" },
  { value: "premium", label: "Premium" }
];

const domainOptions: { value: "all" | TenantDomainStatus; label: string }[] = [
  { value: "all", label: "Tất cả tên miền" },
  { value: "verified", label: formatDomainStatus("verified") },
  { value: "pending", label: formatDomainStatus("pending") },
  { value: "failed", label: formatDomainStatus("failed") },
  { value: "unknown", label: formatDomainStatus("unknown") }
];

const moduleOptions: { value: TenantFilters["moduleCode"]; label: string }[] = [
  { value: "all", label: "Tất cả module" },
  { value: "website", label: formatModuleCode("website") },
  { value: "booking", label: formatModuleCode("booking") },
  { value: "catalog", label: formatModuleCode("catalog") },
  { value: "payments", label: formatModuleCode("payments") },
  { value: "reports", label: formatModuleCode("reports") },
  { value: "notifications", label: formatModuleCode("notifications") }
];
</script>

<template>
  <div class="filter-bar">
    <label>
      <span>Trạng thái</span>
      <select v-model="filters.status">
        <option v-for="opt in statusOptions" :key="opt.value" :value="opt.value">
          {{ opt.label }}
        </option>
      </select>
    </label>

    <label>
      <span>Gói</span>
      <select v-model="filters.plan">
        <option v-for="opt in planOptions" :key="opt.value" :value="opt.value">
          {{ opt.label }}
        </option>
      </select>
    </label>

    <label>
      <span>Tên miền</span>
      <select v-model="filters.domainStatus">
        <option v-for="opt in domainOptions" :key="opt.value" :value="opt.value">
          {{ opt.label }}
        </option>
      </select>
    </label>

    <label>
      <span>Module</span>
      <select v-model="filters.moduleCode">
        <option v-for="opt in moduleOptions" :key="opt.value" :value="opt.value">
          {{ opt.label }}
        </option>
      </select>
    </label>

    <div class="filter-actions">
      <AppButton label="Đặt lại" variant="secondary" @click="$emit('reset')" />
      <AppButton label="Xuất file" variant="ghost" disabled />
    </div>
  </div>
</template>

<style scoped>
.filter-bar {
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-2);
  align-items: center;
}

label {
  display: flex;
  align-items: center;
  gap: var(--space-2);
}

span {
  color: var(--color-text-muted);
  font-size: 11px;
  font-weight: 800;
}

select {
  min-height: 36px;
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-input);
  padding: 0 var(--space-3);
  background: var(--color-surface-elevated);
  color: var(--color-text-primary);
  font-size: 12px;
  font-weight: 700;
}

.filter-actions {
  display: flex;
  gap: var(--space-2);
  margin-left: auto;
}

@media (max-width: 1100px) {
  .filter-bar {
    align-items: stretch;
  }
}

@media (max-width: 640px) {
  .filter-bar {
    display: grid;
    grid-template-columns: 1fr;
  }

  .filter-actions {
    flex-direction: column;
  }
}
</style>
