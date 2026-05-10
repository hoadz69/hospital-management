<script setup lang="ts">
withDefaults(
  defineProps<{
    padded?: boolean;
    tone?: "default" | "muted" | "danger";
  }>(),
  {
    padded: true,
    tone: "default"
  }
);
</script>

<template>
  <section class="app-card" :class="{ 'is-padded': padded }" :data-tone="tone">
    <slot />
  </section>
</template>

<style scoped>
.app-card {
  border: 1px solid #d9e2ec;
  border-radius: 8px;
  background: #ffffff;
  box-shadow: 0 12px 30px rgba(16, 42, 67, 0.08);
}

.app-card.is-padded {
  padding: 20px;
}

.app-card[data-tone="muted"] {
  background: #f8fafc;
  box-shadow: none;
}

.app-card[data-tone="danger"] {
  border-color: #fca5a5;
  background: #fff7f7;
}
</style>
