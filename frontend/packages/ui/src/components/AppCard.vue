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
  border: 1px solid var(--color-border-subtle, #d9e2ec);
  border-radius: var(--radius-card, 8px);
  background: var(--color-surface-elevated, #ffffff);
  box-shadow: var(--shadow-elevation-1, 0 12px 30px rgba(16, 42, 67, 0.08));
}

.app-card.is-padded {
  padding: var(--space-5, 20px);
}

.app-card[data-tone="muted"] {
  background: var(--color-surface-muted, #f8fafc);
  box-shadow: none;
}

.app-card[data-tone="danger"] {
  border-color: var(--color-status-danger, #fca5a5);
  background: color-mix(in srgb, var(--color-status-danger, #b42318) 8%, var(--color-surface-elevated, #ffffff));
}
</style>
