<script setup lang="ts">
// Filter bar tenant theo Figma `V2 - Owner Admin Tenant Operations`.
// Component dÃ¹ng v-model nháº­n object filters tá»« parent Ä‘á»ƒ giá»¯ stateless vÃ  dá»… test.
// NÃºt Export lÃ  placeholder: backend domain-service chÆ°a cung cáº¥p endpoint export trong Phase 3.
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

// Option list cho tá»«ng select. GiÃ¡ trá»‹ giá»¯ nguyÃªn enum backend (Draft/Active/...) cÃ²n label dÃ¹ng formatter VN.
// Plan giá»¯ tÃªn thÆ°Æ¡ng máº¡i Starter/Growth/Premium vÃ¬ lÃ  tÃªn gÃ³i, khÃ´ng cáº§n dá»‹ch.
const statusOptions: { value: "all" | TenantStatus; label: string }[] = [
  { value: "all", label: "Táº¥t cáº£ tráº¡ng thÃ¡i" },
  { value: "Draft", label: formatTenantStatus("Draft") },
  { value: "Active", label: formatTenantStatus("Active") },
  { value: "Suspended", label: formatTenantStatus("Suspended") },
  { value: "Archived", label: formatTenantStatus("Archived") }
];

const planOptions: { value: "all" | TenantPlanCode; label: string }[] = [
  { value: "all", label: "Táº¥t cáº£ gÃ³i" },
  { value: "starter", label: "Starter" },
  { value: "growth", label: "Growth" },
  { value: "premium", label: "Premium" }
];

const domainOptions: { value: "all" | TenantDomainStatus; label: string }[] = [
  { value: "all", label: "Táº¥t cáº£ tÃªn miá»n" },
  { value: "verified", label: formatDomainStatus("verified") },
  { value: "pending", label: formatDomainStatus("pending") },
  { value: "failed", label: formatDomainStatus("failed") },
  { value: "unknown", label: formatDomainStatus("unknown") }
];

const moduleOptions: { value: TenantFilters["moduleCode"]; label: string }[] = [
  { value: "all", label: "Táº¥t cáº£ module" },
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

function setPlanFromEvent(event: Event) {
  setPlan((event.target as HTMLSelectElement).value as TenantFilters["plan"]);
}

function setDomainStatusFromEvent(event: Event) {
  setDomainStatus((event.target as HTMLSelectElement).value as TenantFilters["domainStatus"]);
}

function setModuleCodeFromEvent(event: Event) {
  setModuleCode((event.target as HTMLSelectElement).value as TenantFilters["moduleCode"]);
}
</script>

<template>
  <div class="filter-bar">
    <div class="filter-group filter-group-status">
      <span class="group-label">Tráº¡ng thÃ¡i</span>
      <div class="status-segment" role="group" aria-label="Lá»c theo tráº¡ng thÃ¡i">
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
    </div>

    <label class="filter-group">
      <span class="group-label">GÃ³i</span>
      <select class="filter-select" :value="filters.plan" @change="setPlanFromEvent">
        <option v-for="opt in planOptions" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
      </select>
    </label>

    <label class="filter-group">
      <span class="group-label">TÃªn miá»n</span>
      <select class="filter-select" :value="filters.domainStatus" @change="setDomainStatusFromEvent">
        <option v-for="opt in domainOptions" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
      </select>
    </label>

    <label class="filter-group">
      <span class="group-label">Module</span>
      <select class="filter-select" :value="filters.moduleCode" @change="setModuleCodeFromEvent">
        <option v-for="opt in moduleOptions" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
      </select>
    </label>

    <div class="filter-actions">
      <AppButton label="Äáº·t láº¡i" variant="secondary" @click="$emit('reset')" />
      <AppButton label="Xuáº¥t file" variant="ghost" disabled />
    </div>
  </div>
</template>

<style scoped>
.filter-bar {
  display: grid;
  grid-template-columns: minmax(280px, 1.45fr) repeat(3, minmax(150px, 0.7fr)) auto;
  gap: var(--space-3);
  align-items: end;
}

.filter-group {
  display: grid;
  gap: var(--space-2);
  min-width: 0;
}

.group-label {
  color: var(--color-text-muted);
  font-size: 11px;
  font-weight: 800;
  line-height: 14px;
  text-transform: uppercase;
}

.status-segment {
  min-width: 0;
  display: inline-flex;
  align-items: center;
  gap: 2px;
  overflow-x: auto;
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-input);
  padding: 3px;
  background: var(--color-surface-muted);
}

.filter-chip {
  min-height: 30px;
  flex: 0 0 auto;
  border: 0;
  border-radius: 8px;
  padding: 0 var(--space-2);
  background: transparent;
  color: var(--color-text-secondary);
  cursor: pointer;
  font-size: 12px;
  font-weight: 700;
}

.filter-chip:hover,
.filter-chip:focus-visible {
  outline: 2px solid color-mix(in srgb, var(--color-brand-primary) 22%, transparent);
  outline-offset: 0;
}

.filter-chip.active {
  background: var(--color-text-primary);
  color: var(--color-surface-elevated);
}

.filter-select {
  width: 100%;
  min-height: 38px;
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-input);
  padding: 0 var(--space-3);
  background: var(--color-surface-elevated);
  color: var(--color-text-primary);
  font: inherit;
  font-size: 13px;
}

.filter-select:focus-visible {
  border-color: color-mix(in srgb, var(--color-brand-primary) 36%, var(--color-border-subtle));
  outline: 2px solid color-mix(in srgb, var(--color-brand-primary) 22%, transparent);
  outline-offset: 2px;
}

.filter-actions {
  display: flex;
  gap: var(--space-2);
  justify-content: end;
  white-space: nowrap;
}

@media (max-width: 1180px) {
  .filter-bar {
    grid-template-columns: minmax(0, 1fr) minmax(150px, 0.5fr) minmax(150px, 0.5fr);
  }

  .filter-group-status,
  .filter-actions {
    grid-column: 1 / -1;
  }

  .filter-actions {
    justify-content: flex-start;
  }
}

@media (max-width: 640px) {
  .filter-bar {
    grid-template-columns: 1fr;
  }

  .status-segment {
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    overflow-x: visible;
  }

  .filter-actions {
    flex-direction: column;
  }
}
</style>
