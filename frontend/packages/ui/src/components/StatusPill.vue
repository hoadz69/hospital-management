<script setup lang="ts">
withDefaults(
  defineProps<{
    label: string;
    tone?: "info" | "warning";
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
</style>
