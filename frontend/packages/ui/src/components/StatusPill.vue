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
  padding: 3px 8px;
  background: color-mix(in srgb, var(--color-status-info, #1d4ed8) 14%, transparent);
  color: var(--color-status-info, #1d4ed8);
  font-size: 11px;
  font-weight: 800;
  line-height: 14px;
}

.status-pill[data-tone="warning"] {
  background: color-mix(in srgb, var(--color-status-warning, #92400e) 14%, transparent);
  color: var(--color-status-warning, #92400e);
}

.status-pill[data-tone="success"] {
  background: color-mix(in srgb, var(--color-status-success, #047857) 14%, transparent);
  color: var(--color-status-success, #047857);
}

.status-pill[data-tone="danger"] {
  background: color-mix(in srgb, var(--color-status-danger, #b42318) 14%, transparent);
  color: var(--color-status-danger, #b42318);
}

.status-pill[data-tone="neutral"] {
  background: color-mix(in srgb, var(--color-status-draft, #475569) 14%, transparent);
  color: var(--color-status-draft, #475569);
}
</style>
