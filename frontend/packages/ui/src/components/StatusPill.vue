<script setup lang="ts">
withDefaults(
  defineProps<{
    label: string;
    tone?: "info" | "success" | "warning" | "danger" | "neutral";
  }>(),
  {
    tone: "info"
  }
);
</script>

<template>
  <span class="status-pill" :data-tone="tone">{{ label }}</span>
</template>

<style scoped>
.status-pill {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-height: 24px;
  border-radius: var(--radius-pill, 999px);
  padding: 3px 9px;
  border: 1px solid color-mix(in srgb, var(--color-status-info, #1d4ed8) 24%, transparent);
  background: color-mix(in srgb, var(--color-status-info, #1d4ed8) 12%, var(--color-surface-elevated, #fffdf8));
  color: var(--color-status-info, #1d4ed8);
  font-size: 11px;
  font-weight: 800;
  line-height: 14px;
  white-space: nowrap;
}

.status-pill[data-tone="warning"] {
  border-color: color-mix(in srgb, var(--color-status-warning, #92400e) 24%, transparent);
  background: color-mix(in srgb, var(--color-status-warning, #92400e) 12%, var(--color-surface-elevated, #fffdf8));
  color: var(--color-status-warning, #92400e);
}

.status-pill[data-tone="success"] {
  border-color: color-mix(in srgb, var(--color-status-success, #047857) 24%, transparent);
  background: color-mix(in srgb, var(--color-status-success, #047857) 12%, var(--color-surface-elevated, #fffdf8));
  color: var(--color-status-success, #047857);
}

.status-pill[data-tone="danger"] {
  border-color: color-mix(in srgb, var(--color-status-danger, #b42318) 24%, transparent);
  background: color-mix(in srgb, var(--color-status-danger, #b42318) 12%, var(--color-surface-elevated, #fffdf8));
  color: var(--color-status-danger, #b42318);
}

.status-pill[data-tone="neutral"] {
  border-color: color-mix(in srgb, var(--color-status-draft, #475569) 24%, transparent);
  background: color-mix(in srgb, var(--color-status-draft, #475569) 12%, var(--color-surface-elevated, #fffdf8));
  color: var(--color-status-draft, #475569);
}
</style>
