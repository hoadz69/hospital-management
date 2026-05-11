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

function setStatus(value: TenantFilters["status"]) {
  filters.value.status = value;
}

function setPlan(value: TenantFilters["plan"]) {
  filters.value.plan = value;
}

function setDomainStatus(value: TenantFilters["domainStatus"]) {
  filters.value.domainStatus = value;
}

function setModuleCode(value: TenantFilters["moduleCode"]) {
  filters.value.moduleCode = value;
}
</script>

<template>
  <div class="filter-bar">
    <div class="chip-group chip-group-wide">
      <span class="group-label">Trạng thái</span>
      <button
        v-for="opt in statusOptions"
        :key="opt.value"
        type="button"
        class="filter-chip"
        :class="{ active: filters.status === opt.value }"
        :aria-pressed="filters.status === opt.value"
        @click="setStatus(opt.value)"
      >
        {{ opt.label }}
      </button>
    </div>

    <div class="chip-group">
      <span class="group-label">Gói</span>
      <button
        v-for="opt in planOptions"
        :key="opt.value"
        type="button"
        class="filter-chip"
        :class="{ active: filters.plan === opt.value }"
        :aria-pressed="filters.plan === opt.value"
        @click="setPlan(opt.value)"
      >
        {{ opt.label }}
      </button>
    </div>

    <div class="chip-group">
      <span class="group-label">Tên miền</span>
      <button
        v-for="opt in domainOptions"
        :key="opt.value"
        type="button"
        class="filter-chip"
        :class="{ active: filters.domainStatus === opt.value }"
        :aria-pressed="filters.domainStatus === opt.value"
        @click="setDomainStatus(opt.value)"
      >
        {{ opt.label }}
      </button>
    </div>

    <div class="chip-group chip-group-wide">
      <span class="group-label">Module</span>
      <button
        v-for="opt in moduleOptions"
        :key="opt.value"
        type="button"
        class="filter-chip"
        :class="{ active: filters.moduleCode === opt.value }"
        :aria-pressed="filters.moduleCode === opt.value"
        @click="setModuleCode(opt.value)"
      >
        {{ opt.label }}
      </button>
    </div>

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
  gap: var(--space-3);
  align-items: center;
  padding: var(--space-1);
}

.chip-group {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: var(--space-2);
  min-width: 0;
}

.chip-group-wide {
  flex: 1 1 340px;
}

.group-label {
  margin-right: var(--space-1);
  color: var(--color-text-muted);
  font-size: 11px;
  font-weight: 800;
  text-transform: uppercase;
}

.filter-chip {
  min-height: 32px;
  border: 1px solid var(--color-border-subtle);
  border-radius: 8px;
  padding: 0 var(--space-3);
  background: var(--color-surface-elevated);
  color: var(--color-text-secondary);
  cursor: pointer;
  font-size: 12px;
  font-weight: 700;
}

.filter-chip:hover,
.filter-chip:focus-visible {
  border-color: color-mix(in srgb, var(--color-brand-primary) 36%, var(--color-border-subtle));
  outline: 2px solid color-mix(in srgb, var(--color-brand-primary) 22%, transparent);
  outline-offset: 2px;
}

.filter-chip.active {
  border-color: var(--color-text-primary);
  background: var(--color-text-primary);
  color: var(--color-surface-elevated);
}

.filter-actions {
  display: flex;
  gap: var(--space-2);
  margin-left: auto;
  white-space: nowrap;
}

@media (max-width: 1100px) {
  .filter-bar {
    align-items: start;
  }

  .filter-actions {
    margin-left: 0;
  }
}

@media (max-width: 640px) {
  .filter-bar {
    display: grid;
    grid-template-columns: 1fr;
  }

  .chip-group {
    align-items: flex-start;
  }

  .group-label {
    width: 100%;
  }

  .filter-actions {
    flex-direction: column;
  }
}
</style>
