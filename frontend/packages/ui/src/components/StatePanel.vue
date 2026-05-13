<script setup lang="ts">
type StatePanelTone = "loading" | "info" | "success" | "warning" | "danger" | "neutral" | "specialty";

const props = withDefaults(
  defineProps<{
    title: string;
    description?: string;
    tone?: StatePanelTone;
    busy?: boolean;
  }>(),
  {
    description: undefined,
    tone: "info",
    busy: false
  }
);
</script>

<template>
  <section class="state-panel" :data-tone="props.tone" :aria-busy="props.busy || props.tone === 'loading'">
    <div class="state-panel__mark" aria-hidden="true">
      <span v-if="props.tone === 'loading'" class="state-panel__spinner"></span>
      <span v-else class="state-panel__dot"></span>
    </div>

    <div class="state-panel__copy">
      <h2>{{ props.title }}</h2>
      <p v-if="props.description">{{ props.description }}</p>
      <slot></slot>
    </div>

    <div v-if="$slots.action" class="state-panel__action">
      <slot name="action"></slot>
    </div>
  </section>
</template>

<style scoped>
.state-panel {
  position: relative;
  overflow: hidden;
  display: grid;
  grid-template-columns: auto minmax(0, 1fr) auto;
  align-items: center;
  gap: var(--space-4, 16px);
  border: 1px solid color-mix(in srgb, var(--state-panel-color, var(--color-status-info, #1d4ed8)) 24%, var(--color-border-subtle, #d8d2c5));
  border-radius: var(--radius-card, 16px);
  padding: var(--space-4, 16px);
  background: color-mix(in srgb, var(--state-panel-color, var(--color-status-info, #1d4ed8)) 8%, var(--color-surface-elevated, #fffdf8));
  color: var(--color-text-primary, #102a43);
  box-shadow: var(--shadow-elevation-1, 0 1px 2px rgba(15, 23, 42, 0.04));
}

.state-panel::before {
  content: "";
  position: absolute;
  inset: 0 auto 0 0;
  width: 4px;
  background: var(--state-panel-color, var(--color-status-info, #1d4ed8));
}

.state-panel__mark {
  width: 40px;
  height: 40px;
  display: grid;
  place-items: center;
  border-radius: 12px;
  background: color-mix(in srgb, var(--state-panel-color, var(--color-status-info, #1d4ed8)) 12%, var(--color-surface-elevated, #fffdf8));
  color: var(--state-panel-color, var(--color-status-info, #1d4ed8));
}

.state-panel__dot {
  width: 12px;
  height: 12px;
  border-radius: var(--radius-pill, 999px);
  background: currentColor;
  box-shadow: 0 0 0 6px color-mix(in srgb, currentColor 14%, transparent);
}

.state-panel__spinner {
  width: 18px;
  height: 18px;
  border: 2px solid currentColor;
  border-right-color: transparent;
  border-radius: var(--radius-pill, 999px);
  animation: state-panel-spin 760ms linear infinite;
}

.state-panel__copy {
  min-width: 0;
  display: grid;
  gap: var(--space-1, 4px);
}

.state-panel__copy h2,
.state-panel__copy p,
.state-panel__copy :deep(p) {
  margin: 0;
}

.state-panel__copy h2 {
  color: var(--color-text-primary, #102a43);
  font-size: 15px;
  font-weight: 900;
  line-height: 20px;
  overflow-wrap: anywhere;
}

.state-panel__copy p,
.state-panel__copy :deep(p) {
  color: var(--color-text-secondary, #486581);
  font-size: 12px;
  font-weight: 700;
  line-height: 18px;
}

.state-panel__action {
  display: flex;
  justify-content: flex-end;
  gap: var(--space-2, 8px);
}

.state-panel[data-tone="loading"],
.state-panel[data-tone="info"] {
  --state-panel-color: var(--color-status-info, #1d4ed8);
}

.state-panel[data-tone="success"] {
  --state-panel-color: var(--color-status-success, #047857);
}

.state-panel[data-tone="warning"] {
  --state-panel-color: var(--color-status-warning, #b45309);
}

.state-panel[data-tone="danger"] {
  --state-panel-color: var(--color-status-danger, #b42318);
}

.state-panel[data-tone="neutral"] {
  --state-panel-color: var(--color-status-draft, #475569);
}

.state-panel[data-tone="specialty"] {
  --state-panel-color: var(--color-status-specialty, #7c3aed);
}

@keyframes state-panel-spin {
  to {
    transform: rotate(360deg);
  }
}

@media (max-width: 640px) {
  .state-panel {
    grid-template-columns: 1fr;
    justify-items: start;
  }

  .state-panel__action,
  .state-panel__action :deep(.app-button) {
    width: 100%;
  }
}

@media (prefers-reduced-motion: reduce) {
  .state-panel__spinner {
    animation-duration: 1200ms;
  }
}
</style>
