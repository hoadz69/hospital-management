<script setup lang="ts">
defineProps<{
  label: string;
  value: string | number;
  meta?: string;
  tone?: "primary" | "success" | "warning" | "danger" | "neutral";
}>();
</script>

<template>
  <article class="metric-card" :data-tone="tone ?? 'primary'">
    <div class="metric-mark" aria-hidden="true"></div>
    <div>
      <p class="metric-label">{{ label }}</p>
      <strong class="metric-value">{{ value }}</strong>
      <p v-if="meta" class="metric-meta">{{ meta }}</p>
    </div>
  </article>
</template>

<style scoped>
.metric-card {
  display: grid;
  gap: var(--space-2, 8px);
  position: relative;
  min-height: 120px;
  border: 1px solid var(--color-border-subtle, #d9e2ec);
  border-radius: var(--radius-card, 8px);
  padding: 18px 20px;
  background: var(--color-surface-elevated, #ffffff);
  box-shadow: var(--shadow-elevation-1, 0 12px 30px rgba(16, 42, 67, 0.08));
  transition:
    transform var(--motion-duration-xs, 120ms) var(--motion-ease-standard, ease),
    box-shadow var(--motion-duration-xs, 120ms) var(--motion-ease-standard, ease);
}

.metric-card:hover {
  box-shadow: var(--shadow-elevation-2, 0 12px 30px rgba(16, 42, 67, 0.08));
  transform: translateY(-1px) scale(1.01);
}

.metric-card > div {
  padding-right: 76px;
}

.metric-mark {
  position: absolute;
  top: 18px;
  right: 20px;
  width: 60px;
  height: 24px;
  border-radius: 4px;
  background:
    linear-gradient(var(--color-text-primary, #0e7c86), var(--color-text-primary, #0e7c86)) 4px 16px / 52px 2px
      no-repeat,
    color-mix(in srgb, var(--color-text-primary, #0e7c86) 10%, transparent);
}

.metric-card[data-tone="success"] .metric-mark {
  background:
    linear-gradient(var(--color-status-success, #047857), var(--color-status-success, #047857)) 4px 16px / 52px
      2px no-repeat,
    color-mix(in srgb, var(--color-status-success, #047857) 10%, transparent);
}

.metric-card[data-tone="warning"] .metric-mark {
  background:
    linear-gradient(var(--color-status-warning, #b45309), var(--color-status-warning, #b45309)) 4px 16px / 52px
      2px no-repeat,
    color-mix(in srgb, var(--color-status-warning, #b45309) 10%, transparent);
}

.metric-card[data-tone="danger"] .metric-mark {
  background:
    linear-gradient(var(--color-status-danger, #b42318), var(--color-status-danger, #b42318)) 4px 16px / 52px
      2px no-repeat,
    color-mix(in srgb, var(--color-status-danger, #b42318) 10%, transparent);
}

.metric-card[data-tone="neutral"] .metric-mark {
  background:
    linear-gradient(var(--color-status-draft, #64748b), var(--color-status-draft, #64748b)) 4px 16px / 52px 2px
      no-repeat,
    color-mix(in srgb, var(--color-status-draft, #64748b) 10%, transparent);
}

.metric-label,
.metric-meta {
  margin: 0;
  color: var(--color-text-muted, #627d98);
  font-size: 12px;
}

.metric-value {
  display: block;
  margin-top: 8px;
  color: var(--color-text-primary, #102a43);
  font-size: 28px;
  line-height: 36px;
}

.metric-meta {
  margin-top: 8px;
}
</style>
