<script setup lang="ts">
import type { TenantPlanCode } from "@clinic-saas/shared-types";
import { AppButton, AppCard, KPITile, PlanBadge } from "@clinic-saas/ui";
import { computed, ref } from "vue";
import {
  getPlanPrice,
  moduleCatalog,
  planCatalog,
  tenantPlanAssignments,
  type ModuleCatalogRow,
  type TenantPlanAssignment
} from "../services/planCatalogMock";

const assignments = ref<TenantPlanAssignment[]>(tenantPlanAssignments.map((assignment) => ({ ...assignment })));
const lastBulkAction = ref<string | null>(null);

const selectedAssignments = computed(() => assignments.value.filter((assignment) => assignment.selected));
const totalTenants = computed(() => planCatalog.reduce((total, plan) => total + plan.tenantCount, 0));
const totalMrr = computed(() => assignments.value.reduce((total, assignment) => total + assignment.currentMrr, 0));
const selectedMrrDiff = computed(() =>
  selectedAssignments.value.reduce((total, assignment) => total + planDiff(assignment), 0)
);

function formatCurrency(value: number) {
  return `$${value.toLocaleString("en-US")}`;
}

function planTone(planCode: TenantPlanCode) {
  if (planCode === "premium") {
    return "warning";
  }

  if (planCode === "growth") {
    return "neutral";
  }

  return "info";
}

function moduleSummary(planCode: TenantPlanCode) {
  const includedCount = moduleCatalog.filter((module) => module[planCode] !== false).length;
  return `${includedCount}/${moduleCatalog.length} module`;
}

function formatCell(value: ModuleCatalogRow["starter"]) {
  if (value === true) {
    return "✓";
  }

  if (value === false) {
    return "—";
  }

  return value;
}

function cellClass(value: ModuleCatalogRow["starter"]) {
  if (value === true) {
    return "is-included";
  }

  if (value === false) {
    return "is-muted";
  }

  return "is-limit";
}

function planDiff(assignment: TenantPlanAssignment) {
  return getPlanPrice(assignment.targetPlan) - assignment.currentMrr;
}

function diffLabel(assignment: TenantPlanAssignment) {
  const diff = planDiff(assignment);

  if (diff === 0) {
    return "—";
  }

  return `${diff > 0 ? "+" : "-"}${formatCurrency(Math.abs(diff))}`;
}

function diffTone(assignment: TenantPlanAssignment) {
  const diff = planDiff(assignment);

  if (diff > 0) {
    return "positive";
  }

  if (diff < 0) {
    return "negative";
  }

  return "neutral";
}

function bulkChangePlan() {
  const selectedCount = selectedAssignments.value.length;

  if (selectedCount === 0) {
    lastBulkAction.value = "Chưa chọn tenant nào để đổi gói.";
    return;
  }

  lastBulkAction.value = `${selectedCount} tenant sẽ đổi gói ở chu kỳ kế tiếp. MRR dự kiến ${
    selectedMrrDiff.value >= 0 ? "tăng" : "giảm"
  } ${formatCurrency(Math.abs(selectedMrrDiff.value))}.`;
}
</script>

<template>
  <div class="plans-page">
    <section class="page-heading">
      <div>
        <p class="eyebrow">Owner Admin · Wave A8</p>
        <h2>Plan & Module Catalog</h2>
        <p class="heading-copy">
          Mock-first catalog cho gói dịch vụ, module entitlement và thao tác đổi gói tenant ở chu kỳ kế tiếp.
        </p>
      </div>
      <AppButton label="Bulk change plan" variant="secondary" @click="bulkChangePlan" />
    </section>

    <div class="kpi-grid">
      <KPITile label="Tổng tenant" :value="totalTenants" meta="Theo mock plan catalog" tone="info" />
      <KPITile label="Plan active" :value="planCatalog.length" meta="Starter, Growth, Premium" tone="neutral" />
      <KPITile label="MRR bảng mẫu" :value="formatCurrency(totalMrr)" meta="Từ assignment mock" tone="warning" />
      <KPITile label="Selected diff" :value="formatCurrency(selectedMrrDiff)" meta="Hiệu lực kỳ gia hạn kế tiếp" tone="specialty" />
    </div>

    <section class="plan-grid" aria-label="Plan cards">
      <AppCard
        v-for="plan in planCatalog"
        :key="plan.code"
        class="plan-card"
        :class="[`plan-card--${plan.code}`, { featured: plan.popular }]"
      >
        <span v-if="plan.popular" class="popular-pill">Most popular</span>
        <div class="plan-card__header">
          <h3>{{ plan.name }}</h3>
          <PlanBadge :label="plan.name" :tone="planTone(plan.code)" />
        </div>
        <div class="plan-price">
          <strong>{{ formatCurrency(plan.price) }}</strong>
          <span>/tháng</span>
        </div>
        <p>{{ plan.description }}</p>
        <div class="plan-card__footer">
          <span>
            <b>{{ plan.tenantCount }}</b>
            tenants
          </span>
          <span>
            <b>{{ moduleSummary(plan.code) }}</b>
            enabled
          </span>
        </div>
      </AppCard>
    </section>

    <AppCard class="matrix-card" :padded="false">
      <header class="section-heading">
        <div>
          <h3>Module toggle matrix</h3>
          <p>12 module x 3 plan, cell là check, giới hạn hoặc chưa bật.</p>
        </div>
      </header>

      <div class="matrix-table" role="table" aria-label="Module toggle matrix">
        <div class="matrix-row matrix-row--head" role="row">
          <span>Module</span>
          <span>Category</span>
          <span>Starter</span>
          <span>Growth</span>
          <span>Premium</span>
        </div>
        <div v-for="module in moduleCatalog" :key="module.id" class="matrix-row" role="row">
          <strong>{{ module.name }}</strong>
          <span>{{ module.category }}</span>
          <span :class="cellClass(module.starter)">{{ formatCell(module.starter) }}</span>
          <span :class="cellClass(module.growth)">{{ formatCell(module.growth) }}</span>
          <span :class="cellClass(module.premium)">{{ formatCell(module.premium) }}</span>
        </div>
      </div>
    </AppCard>

    <AppCard class="assignment-card" :padded="false">
      <header class="section-heading assignment-heading">
        <div>
          <h3>Tenant plan assignment</h3>
          <p>{{ selectedAssignments.length }} tenant selected · effective next renewal · audit reason pending BE.</p>
        </div>
        <AppButton :label="`Apply mock change (${selectedAssignments.length})`" @click="bulkChangePlan" />
      </header>

      <div class="assignment-table" role="table" aria-label="Tenant plan assignment">
        <div class="assignment-row assignment-row--head" role="row">
          <span>Tenant</span>
          <span>Current plan</span>
          <span>MRR</span>
          <span>Next renewal</span>
          <span>Change to</span>
          <span>Diff</span>
        </div>
        <div v-for="assignment in assignments" :key="assignment.id" class="assignment-row" role="row">
          <label class="tenant-check">
            <input v-model="assignment.selected" type="checkbox" />
            <span>{{ assignment.slug }}</span>
          </label>
          <PlanBadge :label="assignment.currentPlanName" :tone="planTone(assignment.currentPlan)" />
          <strong>{{ formatCurrency(assignment.currentMrr) }}</strong>
          <span>{{ assignment.nextRenewal }}</span>
          <select v-model="assignment.targetPlan" :aria-label="`Đổi gói cho ${assignment.slug}`">
            <option v-for="plan in planCatalog" :key="plan.code" :value="plan.code">{{ plan.name }}</option>
          </select>
          <b :data-tone="diffTone(assignment)">{{ diffLabel(assignment) }}</b>
        </div>
      </div>

      <p v-if="lastBulkAction" class="bulk-result" role="status">{{ lastBulkAction }}</p>
    </AppCard>

    <aside class="handoff-note">
      <strong>BE handoff request</strong>
      <span>
        Cần contract plan catalog, module entitlement, tenant assignment bulk-change và audit reason trước khi chuyển mock
        sang real API.
      </span>
    </aside>
  </div>
</template>

<style scoped>
.plans-page {
  display: grid;
  gap: var(--space-5);
  color: var(--color-text-primary);
}

.page-heading,
.section-heading,
.plan-card__header,
.plan-card__footer,
.assignment-heading {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-4);
}

.page-heading p,
.page-heading h2,
.section-heading h3,
.section-heading p,
.plan-card h3,
.plan-card p,
.bulk-result,
.handoff-note span {
  margin: 0;
}

.eyebrow {
  color: var(--color-status-specialty);
  font-size: 12px;
  font-weight: 900;
  letter-spacing: 0.04em;
  text-transform: uppercase;
}

.page-heading h2 {
  margin-top: var(--space-1);
  color: var(--color-text-primary);
  font-size: 30px;
  font-weight: 900;
  line-height: 38px;
}

.heading-copy,
.section-heading p {
  margin-top: var(--space-1);
  color: color-mix(in srgb, var(--color-text-secondary) 88%, var(--color-text-primary));
  font-size: 14px;
  font-weight: 650;
  line-height: 22px;
}

.kpi-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: var(--space-3);
}

.plan-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: var(--space-4);
}

.plan-card {
  position: relative;
  min-height: 220px;
  display: grid;
  align-content: start;
  gap: var(--space-4);
  border-width: 1.5px;
  background: linear-gradient(180deg, color-mix(in srgb, var(--color-surface-elevated) 94%, white), var(--color-surface-elevated));
  box-shadow: 0 16px 36px rgba(16, 42, 67, 0.08);
}

.plan-card.featured {
  border-color: color-mix(in srgb, var(--color-status-specialty) 58%, var(--color-admin-sidebar));
  background: linear-gradient(145deg, color-mix(in srgb, var(--color-admin-sidebar) 92%, var(--color-status-specialty)), var(--color-admin-sidebar));
  color: var(--color-surface-elevated);
  box-shadow: 0 22px 48px rgba(16, 42, 67, 0.22);
}

.plan-card--starter {
  border-color: color-mix(in srgb, var(--color-status-info) 42%, var(--color-border-subtle));
}

.plan-card--premium {
  border-color: color-mix(in srgb, var(--color-status-warning) 54%, var(--color-border-subtle));
  background: linear-gradient(180deg, color-mix(in srgb, var(--color-status-warning) 8%, var(--color-surface-elevated)), var(--color-surface-elevated));
}

.popular-pill {
  justify-self: start;
  border-radius: var(--radius-pill);
  padding: 4px 10px;
  background: var(--color-brand-accent);
  color: var(--color-text-primary);
  font-size: 10px;
  font-weight: 900;
  text-transform: uppercase;
}

.plan-card h3 {
  color: inherit;
  font-size: 24px;
  font-weight: 900;
}

.plan-price {
  display: flex;
  align-items: baseline;
  gap: 6px;
}

.plan-price strong {
  color: var(--color-brand-accent);
  font-size: 34px;
  font-weight: 950;
  line-height: 40px;
}

.plan-price span {
  color: color-mix(in srgb, currentColor 86%, transparent);
  font-size: 14px;
  font-weight: 850;
}

.plan-card p,
.plan-card__footer span {
  color: color-mix(in srgb, currentColor 86%, transparent);
  font-size: 13px;
  font-weight: 700;
  line-height: 20px;
}

.plan-card__footer {
  align-items: stretch;
  border-top: 1px solid color-mix(in srgb, currentColor 18%, transparent);
  padding-top: var(--space-3);
}

.plan-card__footer span {
  display: grid;
  gap: 2px;
}

.plan-card__footer b {
  color: inherit;
  font-size: 18px;
  font-weight: 950;
}

.matrix-card,
.assignment-card {
  overflow: hidden;
  border: 1.5px solid color-mix(in srgb, var(--color-border-subtle) 80%, var(--color-text-secondary));
  box-shadow: 0 18px 44px rgba(16, 42, 67, 0.08);
}

.section-heading {
  padding: var(--space-5) var(--space-6);
  background: linear-gradient(180deg, var(--color-surface-elevated), color-mix(in srgb, var(--color-surface-muted) 70%, var(--color-surface-elevated)));
}

.section-heading h3 {
  color: var(--color-text-primary);
  font-size: 20px;
  font-weight: 900;
}

.matrix-table,
.assignment-table {
  display: grid;
  overflow-x: auto;
}

.matrix-row,
.assignment-row {
  min-width: 840px;
  display: grid;
  align-items: center;
  gap: var(--space-3);
  border-top: 1px solid color-mix(in srgb, var(--color-border-subtle) 84%, var(--color-text-secondary));
  padding: 14px var(--space-6);
  color: color-mix(in srgb, var(--color-text-secondary) 92%, var(--color-text-primary));
  font-size: 13px;
  font-weight: 700;
}

.matrix-row:nth-child(even),
.assignment-row:nth-child(even) {
  background: color-mix(in srgb, var(--color-surface-muted) 45%, transparent);
}

.matrix-row {
  grid-template-columns: minmax(180px, 1.35fr) minmax(120px, 0.8fr) repeat(3, minmax(110px, 1fr));
}

.assignment-row {
  grid-template-columns: minmax(180px, 1.35fr) minmax(120px, 0.85fr) minmax(80px, 0.6fr) minmax(110px, 0.8fr) minmax(120px, 0.8fr) minmax(80px, 0.6fr);
}

.matrix-row--head,
.assignment-row--head {
  border-top: 0;
  background: color-mix(in srgb, var(--color-admin-sidebar) 9%, var(--color-surface-muted));
  color: var(--color-text-primary);
  font-size: 12px;
  font-weight: 900;
  text-transform: uppercase;
}

.matrix-row strong,
.assignment-row strong,
.tenant-check span {
  color: var(--color-text-primary);
}

.matrix-row .is-included {
  width: 26px;
  height: 26px;
  display: inline-grid;
  place-items: center;
  border-radius: var(--radius-pill);
  background: var(--color-status-success);
  color: var(--color-surface-elevated);
  font-weight: 900;
}

.matrix-row .is-muted {
  width: 26px;
  height: 26px;
  display: inline-grid;
  place-items: center;
  border-radius: var(--radius-pill);
  background: color-mix(in srgb, var(--color-text-muted) 14%, transparent);
  color: color-mix(in srgb, var(--color-text-muted) 82%, var(--color-text-primary));
  font-size: 18px;
  font-weight: 900;
}

.matrix-row .is-limit {
  justify-self: start;
  border-radius: var(--radius-pill);
  padding: 5px 10px;
  background: color-mix(in srgb, var(--color-status-info) 13%, var(--color-surface-elevated));
  color: color-mix(in srgb, var(--color-status-info) 74%, var(--color-text-primary));
  font-weight: 900;
}

.tenant-check {
  display: inline-flex;
  align-items: center;
  gap: var(--space-2);
  font-weight: 800;
}

.tenant-check input {
  width: 16px;
  height: 16px;
  accent-color: var(--color-status-specialty);
}

select {
  min-height: 32px;
  border: 1px solid color-mix(in srgb, var(--color-border-subtle) 76%, var(--color-text-secondary));
  border-radius: 8px;
  padding: 0 var(--space-2);
  background: var(--color-surface-elevated);
  color: var(--color-text-primary);
  font: inherit;
  font-size: 12px;
  font-weight: 800;
}

b[data-tone="positive"] {
  color: color-mix(in srgb, var(--color-status-success) 76%, var(--color-text-primary));
}

b[data-tone="negative"] {
  color: var(--color-status-danger);
}

b[data-tone="neutral"] {
  color: color-mix(in srgb, var(--color-text-muted) 78%, var(--color-text-primary));
}

.bulk-result {
  border-top: 1px solid color-mix(in srgb, var(--color-status-specialty) 24%, var(--color-border-subtle));
  padding: var(--space-4) var(--space-5);
  background: color-mix(in srgb, var(--color-status-specialty) 13%, var(--color-surface-elevated));
  color: color-mix(in srgb, var(--color-status-specialty) 76%, var(--color-text-primary));
  font-size: 14px;
  font-weight: 900;
}

.handoff-note {
  display: grid;
  gap: var(--space-1);
  border: 1px solid color-mix(in srgb, var(--color-status-warning) 42%, var(--color-border-subtle));
  border-radius: var(--radius-input);
  padding: var(--space-4);
  background: color-mix(in srgb, var(--color-status-warning) 10%, var(--color-surface-elevated));
}

.handoff-note strong {
  color: var(--color-text-primary);
  font-size: 13px;
}

.handoff-note span {
  color: var(--color-text-secondary);
  font-size: 12px;
  line-height: 18px;
}

@media (max-width: 1180px) {
  .kpi-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .plan-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 640px) {
  .page-heading,
  .section-heading,
  .assignment-heading {
    align-items: stretch;
    flex-direction: column;
  }

  .kpi-grid {
    grid-template-columns: 1fr;
  }

  .page-heading h2 {
    font-size: 26px;
    line-height: 32px;
  }

  .page-heading :deep(.app-button),
  .assignment-heading :deep(.app-button) {
    width: 100%;
  }

  .plan-card {
    min-height: 0;
  }

  .section-heading {
    padding: var(--space-4);
  }

  .matrix-table,
  .assignment-table {
    overflow-x: visible;
  }

  .matrix-row--head,
  .assignment-row--head {
    display: none;
  }

  .matrix-row,
  .assignment-row {
    min-width: 0;
    grid-template-columns: 1fr;
    gap: var(--space-2);
    padding: var(--space-4);
    font-size: 13px;
  }

  .matrix-row {
    grid-template-columns: minmax(0, 1fr) repeat(3, minmax(0, max-content));
  }

  .matrix-row strong,
  .matrix-row > span:nth-child(2) {
    grid-column: 1 / -1;
  }

  .matrix-row > span:nth-child(2) {
    color: var(--color-text-secondary);
    font-size: 12px;
    font-weight: 800;
  }

  .assignment-row {
    border: 1px solid color-mix(in srgb, var(--color-border-subtle) 84%, var(--color-text-secondary));
    border-radius: var(--radius-card);
    margin: var(--space-3) var(--space-4);
    background: var(--color-surface-elevated);
  }

  .assignment-row > span,
  .assignment-row > strong,
  .assignment-row > b,
  .assignment-row > select {
    width: 100%;
  }

  .tenant-check {
    justify-content: space-between;
  }
}
</style>
