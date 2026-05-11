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
  gap: var(--space-4);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-card);
  padding: var(--space-4);
  background: var(--color-surface-elevated);
  color: var(--color-text-primary);
}

.domain-state-row__main {
  display: grid;
  gap: var(--space-2);
  min-width: 0;
}

.domain-state-row__heading {
  display: flex;
  align-items: center;
  gap: var(--space-3);
  min-width: 0;
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
  color: var(--color-text-secondary);
  font-size: 11px;
  line-height: 16px;
}

.domain-state-row__badge {
  min-height: 24px;
  display: inline-flex;
  align-items: center;
  gap: var(--space-2);
  border: 1px solid color-mix(in srgb, var(--domain-color, var(--color-status-info)) 30%, transparent);
  border-radius: var(--radius-pill);
  padding: var(--space-1) var(--space-2);
  background: color-mix(in srgb, var(--domain-color, var(--color-status-info)) 12%, transparent);
  color: var(--domain-color, var(--color-status-info));
  font-size: 10px;
  font-weight: 800;
  line-height: 12px;
  white-space: nowrap;
}

.domain-state-row__badge span {
  width: 6px;
  height: 6px;
  border-radius: var(--radius-pill);
  background: currentColor;
}

.domain-state-row__actions {
  display: flex;
  flex-wrap: wrap;
  justify-content: flex-end;
  gap: var(--space-2);
  flex: 0 0 auto;
}

.domain-state-row__action {
  min-height: 30px;
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-input);
  padding: 0 var(--space-3);
  background: var(--color-surface-muted);
  color: var(--color-text-primary);
  cursor: pointer;
  font: inherit;
  font-size: 12px;
  font-weight: 800;
}

.domain-state-row__action[data-variant="primary"] {
  border-color: var(--color-brand-primary);
  background: var(--color-brand-primary);
  color: var(--color-surface-elevated);
}

.domain-state-row__action[data-variant="danger"] {
  border-color: var(--color-status-danger);
  background: color-mix(in srgb, var(--color-status-danger) 10%, var(--color-surface-elevated));
  color: var(--color-status-danger);
}

.domain-state-row__action[data-variant="ghost"] {
  border-color: transparent;
  background: transparent;
  color: var(--color-brand-primary);
}

.domain-state-row__action:disabled {
  cursor: not-allowed;
  opacity: 0.62;
}

.domain-state-row[data-tone="success"] {
  --domain-color: var(--color-status-success);
}

.domain-state-row[data-tone="info"] {
  --domain-color: var(--color-status-info);
}

.domain-state-row[data-tone="warning"] {
  --domain-color: var(--color-status-warning);
}

.domain-state-row[data-tone="danger"] {
  --domain-color: var(--color-status-danger);
}

.domain-state-row[data-tone="neutral"] {
  --domain-color: var(--color-status-draft);
}

.domain-state-row[data-tone="specialty"] {
  --domain-color: var(--color-status-specialty);
}

@media (max-width: 640px) {
  .domain-state-row {
    flex-direction: column;
  }

  .domain-state-row__actions {
    justify-content: flex-start;
  }
}
</style>
