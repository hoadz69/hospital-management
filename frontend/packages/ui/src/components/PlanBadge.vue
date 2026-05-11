<script setup lang="ts">
type PlanBadgeTone = "primary" | "success" | "warning" | "danger" | "info" | "neutral" | "specialty";

withDefaults(
  defineProps<{
    label: string;
    value?: string | number;
    helper?: string;
    tone?: PlanBadgeTone;
  }>(),
  {
    value: undefined,
    helper: undefined,
    tone: "primary"
  }
);
</script>

<template>
  <span class="plan-badge" :data-tone="tone" :aria-label="helper ? `${label}: ${helper}` : label">
    <span class="plan-badge__label">{{ label }}</span>
    <span v-if="value !== undefined" class="plan-badge__value">{{ value }}</span>
    <slot name="extra"></slot>
  </span>
</template>

<style scoped>
.plan-badge {
  min-height: 26px;
  display: inline-flex;
  align-items: center;
  gap: var(--space-2, 8px);
  max-width: 100%;
  border: 1px solid color-mix(in srgb, var(--plan-color, var(--color-brand-primary, #0e7c86)) 28%, transparent);
  border-radius: var(--radius-input, 12px);
  padding: var(--space-1, 4px) var(--space-3, 12px);
  background: color-mix(in srgb, var(--plan-color, var(--color-brand-primary, #0e7c86)) 10%, var(--color-surface-elevated, #fffdf8));
  color: var(--plan-color, var(--color-brand-primary, #0e7c86));
  font-size: 11px;
  font-weight: 800;
  line-height: 14px;
  white-space: nowrap;
}

.plan-badge__label,
.plan-badge__value {
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
}

.plan-badge__value {
  color: var(--color-text-secondary, #486581);
  font-weight: 700;
}

.plan-badge[data-tone="primary"] {
  --plan-color: var(--color-brand-primary, #0e7c86);
}

.plan-badge[data-tone="success"] {
  --plan-color: var(--color-status-success, #047857);
}

.plan-badge[data-tone="warning"] {
  --plan-color: var(--color-status-warning, #b45309);
}

.plan-badge[data-tone="danger"] {
  --plan-color: var(--color-status-danger, #b42318);
}

.plan-badge[data-tone="info"] {
  --plan-color: var(--color-status-info, #1d4ed8);
}

.plan-badge[data-tone="neutral"] {
  --plan-color: var(--color-status-draft, #475569);
}

.plan-badge[data-tone="specialty"] {
  --plan-color: var(--color-status-specialty, #7c3aed);
}
</style>
