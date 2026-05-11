<script setup lang="ts">
withDefaults(
  defineProps<{
    label: string;
    variant?: "primary" | "secondary" | "danger" | "ghost";
    loading?: boolean;
    disabled?: boolean;
    type?: "button" | "submit";
  }>(),
  {
    variant: "primary",
    loading: false,
    disabled: false,
    type: "button"
  }
);
</script>

<template>
  <button class="app-button" :data-variant="variant" :disabled="disabled || loading" :type="type">
    <span v-if="loading" class="spinner" aria-hidden="true"></span>
    <span>{{ loading ? "Đang xử lý" : label }}</span>
  </button>
</template>

<style scoped>
.app-button {
  box-sizing: border-box;
  min-height: 40px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  border: 1px solid transparent;
  border-radius: var(--radius-button, 14px);
  padding: 0 18px;
  background: var(--color-brand-primary, #0e7c86);
  color: var(--color-on-brand, #ffffff);
  font: inherit;
  font-size: 13px;
  font-weight: 800;
  line-height: 18px;
  cursor: pointer;
  white-space: nowrap;
  box-shadow: var(--shadow-button, 0 1px 2px rgba(16, 42, 67, 0.08));
  transition:
    background var(--motion-duration-sm, 160ms) var(--motion-ease-standard, ease),
    border-color var(--motion-duration-sm, 160ms) var(--motion-ease-standard, ease),
    box-shadow var(--motion-duration-sm, 160ms) var(--motion-ease-standard, ease),
    color var(--motion-duration-sm, 160ms) var(--motion-ease-standard, ease),
    opacity var(--motion-duration-sm, 160ms) var(--motion-ease-standard, ease),
    transform var(--motion-duration-xs, 120ms) var(--motion-ease-standard, ease);
}

.app-button:hover:not(:disabled) {
  background: color-mix(in srgb, var(--color-brand-primary, #0e7c86) 88%, black);
  transform: translateY(-1px);
}

.app-button[data-variant="secondary"] {
  background: var(--color-surface-elevated, #fffdf8);
  border-color: var(--color-border-subtle, #d8d2c5);
  color: var(--color-text-primary, #102a43);
}

.app-button[data-variant="secondary"]:hover:not(:disabled) {
  background: var(--color-surface-muted, #f6f1e8);
  border-color: var(--color-border-strong, #c9bfad);
}

.app-button[data-variant="danger"] {
  background: var(--color-status-danger, #b42318);
  color: var(--color-surface-elevated, #ffffff);
}

.app-button[data-variant="danger"]:hover:not(:disabled) {
  background: color-mix(in srgb, var(--color-status-danger, #b42318) 88%, black);
}

.app-button[data-variant="ghost"] {
  background: transparent;
  color: var(--color-brand-primary, #0e7c86);
  box-shadow: none;
}

.app-button[data-variant="ghost"]:hover:not(:disabled) {
  background: color-mix(in srgb, var(--color-brand-primary, #0e7c86) 10%, var(--color-surface-muted, #f6f1e8));
}

.app-button:focus-visible {
  outline: 3px solid color-mix(in srgb, var(--color-brand-primary, #0e7c86) 28%, transparent);
  outline-offset: 2px;
}

.app-button:active:not(:disabled) {
  transform: translateY(0);
}

.app-button:disabled {
  cursor: not-allowed;
  opacity: 0.62;
  box-shadow: none;
}

.spinner {
  width: 14px;
  height: 14px;
  border: 2px solid currentColor;
  border-right-color: transparent;
  border-radius: 999px;
  animation: spin 700ms linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

@media (prefers-reduced-motion: reduce) {
  .app-button {
    transition: none;
  }

  .app-button:hover:not(:disabled),
  .app-button:active:not(:disabled) {
    transform: none;
  }

  .spinner {
    animation-duration: 1200ms;
  }
}
</style>
