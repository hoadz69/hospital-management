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
  min-height: 28px;
  border-radius: 999px;
  padding: 0 12px;
  background: #dbeafe;
  color: #1d4ed8;
  font-size: 13px;
  font-weight: 700;
}

.status-pill[data-tone="warning"] {
  background: #fef3c7;
  color: #92400e;
}

.status-pill[data-tone="success"] {
  background: #dcfce7;
  color: #047857;
}

.status-pill[data-tone="danger"] {
  background: #fee2e2;
  color: #b42318;
}

.status-pill[data-tone="neutral"] {
  background: #e2e8f0;
  color: #475569;
}
</style>
