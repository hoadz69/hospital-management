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
  grid-template-columns: 12px 1fr;
  gap: 14px;
  min-height: 118px;
  border: 1px solid #d9e2ec;
  border-radius: 8px;
  padding: 18px;
  background: #ffffff;
  box-shadow: 0 12px 30px rgba(16, 42, 67, 0.08);
}

.metric-mark {
  width: 12px;
  height: 52px;
  border-radius: 999px;
  background: #0e7c86;
}

.metric-card[data-tone="success"] .metric-mark {
  background: #047857;
}

.metric-card[data-tone="warning"] .metric-mark {
  background: #b45309;
}

.metric-card[data-tone="danger"] .metric-mark {
  background: #b42318;
}

.metric-card[data-tone="neutral"] .metric-mark {
  background: #64748b;
}

.metric-label,
.metric-meta {
  margin: 0;
  color: #627d98;
  font-size: 13px;
}

.metric-value {
  display: block;
  margin-top: 8px;
  color: #102a43;
  font-size: 30px;
  line-height: 1;
}

.metric-meta {
  margin-top: 8px;
}
</style>
