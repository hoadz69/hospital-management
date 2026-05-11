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
  box-sizing: border-box;
  border: 1px solid var(--color-border-subtle, #d8d2c5);
  border-radius: var(--radius-card, 16px);
  background: var(--color-surface-elevated, #fffdf8);
  box-shadow: var(--shadow-elevation-1, 0 10px 24px rgba(57, 50, 40, 0.07));
}

.app-card.is-padded {
  padding: var(--space-5, 18px);
}

.app-card[data-tone="muted"] {
  background: var(--color-surface-muted, #f6f1e8);
  box-shadow: none;
}

.app-card[data-tone="danger"] {
  border-color: var(--color-status-danger, #fca5a5);
  background: color-mix(in srgb, var(--color-status-danger, #b42318) 8%, var(--color-surface-elevated, #fffdf8));
}
</style>
