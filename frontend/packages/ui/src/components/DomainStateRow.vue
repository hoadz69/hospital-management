<script setup lang="ts">
type DomainStateTone = "success" | "info" | "warning" | "danger" | "neutral" | "specialty";

type DomainStateAction = {
  key: string;
  label: string;
  disabled?: boolean;
  tone?: "primary" | "secondary" | "danger" | "ghost";
};

withDefaults(
  defineProps<{
    label: string;
    value?: string;
    helper?: string;
    tone?: DomainStateTone;
    actions?: DomainStateAction[];
  }>(),
  {
    value: undefined,
    helper: undefined,
    tone: "info",
    actions: () => []
  }
);

defineEmits<{
  action: [key: string];
}>();
</script>

<template>
  <article class="domain-state-row" :data-tone="tone">
    <div class="domain-state-row__main">
      <div class="domain-state-row__heading">
        <strong>{{ label }}</strong>
        <span v-if="value" class="domain-state-row__badge">
          <span aria-hidden="true"></span>
          {{ value }}
        </span>
      </div>
      <p v-if="helper">{{ helper }}</p>
      <slot></slot>
    </div>

    <div v-if="actions.length > 0 || $slots.actions" class="domain-state-row__actions">
      <slot name="actions">
        <button
          v-for="action in actions"
          :key="action.key"
          type="button"
          class="domain-state-row__action"
          :data-variant="action.tone ?? 'secondary'"
          :disabled="action.disabled"
          :aria-disabled="action.disabled"
          @click="$emit('action', action.key)"
        >
          {{ action.label }}
        </button>
      </slot>
    </div>

    <slot name="extra"></slot>
  </article>
</template>

<style scoped>
.domain-state-row {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: var(--space-4, 16px);
  border: 1px solid var(--color-border-subtle, #d8d2c5);
  border-radius: var(--radius-card, 16px);
  padding: var(--space-4, 16px);
  background: var(--color-surface-elevated, #fffdf8);
  color: var(--color-text-primary, #102a43);
}

.domain-state-row__main {
  display: grid;
  gap: var(--space-2, 8px);
  min-width: 0;
}

.domain-state-row__heading {
  display: flex;
  align-items: center;
  gap: var(--space-3, 12px);
  min-width: 0;
  flex-wrap: wrap;
}

.domain-state-row__heading strong {
  min-width: 0;
  overflow-wrap: anywhere;
  font-size: 13px;
  line-height: 18px;
}

.domain-state-row__main p,
.domain-state-row__main :deep(p) {
  margin: 0;
  color: var(--color-text-secondary, #486581);
  font-size: 11px;
  line-height: 16px;
}

.domain-state-row__badge {
  min-height: 24px;
  display: inline-flex;
  align-items: center;
  gap: var(--space-2, 8px);
  border: 1px solid color-mix(in srgb, var(--domain-color, var(--color-status-info, #1d4ed8)) 30%, transparent);
  border-radius: var(--radius-pill, 999px);
  padding: var(--space-1, 4px) var(--space-2, 8px);
  background: color-mix(in srgb, var(--domain-color, var(--color-status-info, #1d4ed8)) 12%, var(--color-surface-elevated, #fffdf8));
  color: var(--domain-color, var(--color-status-info, #1d4ed8));
  font-size: 10px;
  font-weight: 800;
  line-height: 12px;
  white-space: nowrap;
}

.domain-state-row__badge span {
  width: 6px;
  height: 6px;
  border-radius: var(--radius-pill, 999px);
  background: currentColor;
}

.domain-state-row__actions {
  display: flex;
  flex-wrap: wrap;
  justify-content: flex-end;
  gap: var(--space-2, 8px);
  flex: 0 0 auto;
}

.domain-state-row__action {
  min-height: 30px;
  border: 1px solid var(--color-border-subtle, #d8d2c5);
  border-radius: var(--radius-input, 12px);
  padding: 0 var(--space-3, 12px);
  background: var(--color-surface-muted, #f6f1e8);
  color: var(--color-text-primary, #102a43);
  cursor: pointer;
  font: inherit;
  font-size: 12px;
  font-weight: 800;
  transition:
    background var(--motion-duration-xs, 120ms) var(--motion-ease-standard, ease),
    border-color var(--motion-duration-xs, 120ms) var(--motion-ease-standard, ease),
    color var(--motion-duration-xs, 120ms) var(--motion-ease-standard, ease);
}

.domain-state-row__action[data-variant="primary"] {
  border-color: var(--color-brand-primary, #0e7c86);
  background: var(--color-brand-primary, #0e7c86);
  color: var(--color-on-brand, #ffffff);
}

.domain-state-row__action[data-variant="danger"] {
  border-color: var(--color-status-danger, #b42318);
  background: color-mix(in srgb, var(--color-status-danger, #b42318) 10%, var(--color-surface-elevated, #fffdf8));
  color: var(--color-status-danger, #b42318);
}

.domain-state-row__action[data-variant="ghost"] {
  border-color: transparent;
  background: transparent;
  color: var(--color-brand-primary, #0e7c86);
}

.domain-state-row__action:hover:not(:disabled) {
  border-color: var(--color-border-strong, #c9bfad);
  background: var(--color-surface-elevated, #fffdf8);
}

.domain-state-row__action:focus-visible {
  outline: 3px solid color-mix(in srgb, var(--color-brand-primary, #0e7c86) 28%, transparent);
  outline-offset: 2px;
}

.domain-state-row__action:disabled {
  cursor: not-allowed;
  opacity: 0.62;
}

.domain-state-row[data-tone="success"] {
  --domain-color: var(--color-status-success, #047857);
}

.domain-state-row[data-tone="info"] {
  --domain-color: var(--color-status-info, #1d4ed8);
}

.domain-state-row[data-tone="warning"] {
  --domain-color: var(--color-status-warning, #b45309);
}

.domain-state-row[data-tone="danger"] {
  --domain-color: var(--color-status-danger, #b42318);
}

.domain-state-row[data-tone="neutral"] {
  --domain-color: var(--color-status-draft, #475569);
}

.domain-state-row[data-tone="specialty"] {
  --domain-color: var(--color-status-specialty, #7c3aed);
}

@media (max-width: 640px) {
  .domain-state-row {
    flex-direction: column;
  }

  .domain-state-row__actions {
    justify-content: flex-start;
  }
}

@media (prefers-reduced-motion: reduce) {
  .domain-state-row__action {
    transition: none;
  }
}
</style>
