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
    <span>{{ loading ? "Working" : label }}</span>
  </button>
</template>

<style scoped>
.app-button {
  min-height: 44px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  border: 1px solid transparent;
  border-radius: 8px;
  padding: 0 18px;
  background: #0e7c86;
  color: white;
  font: inherit;
  font-weight: 700;
  cursor: pointer;
  white-space: nowrap;
  transition:
    background 160ms ease,
    border-color 160ms ease,
    color 160ms ease,
    opacity 160ms ease;
}

.app-button:hover:not(:disabled) {
  background: #075e66;
}

.app-button[data-variant="secondary"] {
  background: #ffffff;
  border-color: #d9e2ec;
  color: #102a43;
}

.app-button[data-variant="secondary"]:hover:not(:disabled) {
  background: #f8fafc;
}

.app-button[data-variant="danger"] {
  background: #b42318;
  color: #ffffff;
}

.app-button[data-variant="danger"]:hover:not(:disabled) {
  background: #8f1d16;
}

.app-button[data-variant="ghost"] {
  background: transparent;
  color: #0e7c86;
}

.app-button[data-variant="ghost"]:hover:not(:disabled) {
  background: #d8f3f1;
}

.app-button:disabled {
  cursor: not-allowed;
  opacity: 0.62;
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
</style>
