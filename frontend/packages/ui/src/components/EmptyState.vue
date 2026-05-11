<script setup lang="ts">
type EmptyStateTone = "primary" | "success" | "warning" | "danger" | "info" | "neutral" | "specialty";

withDefaults(
  defineProps<{
    label: string;
    helper?: string;
    tone?: EmptyStateTone;
  }>(),
  {
    helper: undefined,
    tone: "primary"
  }
);
</script>

<template>
  <section class="empty-state" :data-tone="tone" :aria-label="label">
    <slot name="icon">
      <div class="empty-state__icon" aria-hidden="true">
        <span></span>
      </div>
    </slot>

    <div class="empty-state__copy">
      <h2>{{ label }}</h2>
      <p v-if="helper">{{ helper }}</p>
      <slot></slot>
    </div>

    <div v-if="$slots.action" class="empty-state__action">
      <slot name="action"></slot>
    </div>

    <slot name="extra"></slot>
  </section>
</template>

<style scoped>
.empty-state {
  display: grid;
  justify-items: center;
  gap: var(--space-4);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-card);
  padding: var(--space-14);
  background: var(--color-surface-elevated);
  color: var(--color-text-primary);
  text-align: center;
}

.empty-state__icon {
  width: 120px;
  height: 120px;
  display: grid;
  place-items: center;
  border-radius: var(--radius-pill);
  background: color-mix(in srgb, var(--empty-color, var(--color-brand-primary)) 8%, transparent);
}

.empty-state__icon span {
  width: 48px;
  height: 48px;
  display: block;
  border: 2px solid var(--empty-color, var(--color-brand-primary));
  border-radius: var(--radius-card);
  background:
    linear-gradient(var(--empty-color, var(--color-brand-primary)), var(--empty-color, var(--color-brand-primary))) 12px 15px / 24px 2px no-repeat,
    linear-gradient(var(--empty-color, var(--color-brand-primary)), var(--empty-color, var(--color-brand-primary))) 12px 24px / 18px 2px no-repeat,
    transparent;
}

.empty-state__copy {
  display: grid;
  gap: var(--space-2);
  max-width: 520px;
}

.empty-state__copy h2,
.empty-state__copy p {
  margin: 0;
}

.empty-state__copy h2 {
  color: var(--color-text-primary);
  font-size: 20px;
  line-height: 28px;
}

.empty-state__copy p,
.empty-state__copy :deep(p) {
  color: var(--color-text-secondary);
  font-size: 13px;
  line-height: 20px;
}

.empty-state__action {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: var(--space-3);
}

.empty-state[data-tone="primary"] {
  --empty-color: var(--color-brand-primary);
}

.empty-state[data-tone="success"] {
  --empty-color: var(--color-status-success);
}

.empty-state[data-tone="warning"] {
  --empty-color: var(--color-status-warning);
}

.empty-state[data-tone="danger"] {
  --empty-color: var(--color-status-danger);
}

.empty-state[data-tone="info"] {
  --empty-color: var(--color-status-info);
}

.empty-state[data-tone="neutral"] {
  --empty-color: var(--color-status-draft);
}

.empty-state[data-tone="specialty"] {
  --empty-color: var(--color-status-specialty);
}

@media (max-width: 640px) {
  .empty-state {
    padding: var(--space-8) var(--space-5);
  }
}
</style>
