<script setup lang="ts">
import { computed } from "vue";

type ModuleChipTone = "success" | "info" | "warning" | "danger" | "neutral" | "specialty";

type ModuleChipItem = {
  key: string;
  label: string;
  enabled?: boolean;
  tone?: ModuleChipTone;
};

const props = withDefaults(
  defineProps<{
    items: ModuleChipItem[];
    label?: string;
    helper?: string;
    total?: number;
    showCount?: boolean;
    compact?: boolean;
  }>(),
  {
    label: undefined,
    helper: undefined,
    total: undefined,
    showCount: true,
    compact: false
  }
);

const enabledCount = computed(() => props.items.filter((item) => item.enabled !== false).length);
const totalCount = computed(() => props.total ?? props.items.length);
</script>

<template>
  <div class="module-chips" :class="{ 'module-chips--compact': compact }">
    <div v-if="label || showCount || $slots.extra" class="module-chips__header">
      <div>
        <p v-if="label" class="module-chips__label">{{ label }}</p>
        <p v-if="helper" class="module-chips__helper">{{ helper }}</p>
      </div>
      <slot name="extra">
        <span v-if="showCount" class="module-chips__count">{{ enabledCount }}/{{ totalCount }}</span>
      </slot>
    </div>

    <div class="module-chips__list" :aria-label="label">
      <span
        v-for="item in items"
        :key="item.key"
        class="module-chips__item"
        :data-tone="item.tone ?? 'success'"
        :data-enabled="item.enabled !== false"
        :title="item.label"
      >
        <span class="module-chips__dot" aria-hidden="true"></span>
        <span v-if="!compact" class="module-chips__text">{{ item.label }}</span>
      </span>
    </div>
  </div>
</template>

<style scoped>
.module-chips {
  display: grid;
  gap: var(--space-2);
  min-width: 0;
}

.module-chips__header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: var(--space-3);
}

.module-chips__label,
.module-chips__helper {
  margin: 0;
}

.module-chips__label {
  color: var(--color-text-primary);
  font-size: var(--text-caption-size);
  font-weight: 800;
  line-height: var(--text-caption-line-height);
}

.module-chips__helper {
  margin-top: var(--space-1);
  color: var(--color-text-secondary);
  font-size: 11px;
  line-height: 14px;
}

.module-chips__count {
  color: var(--color-text-secondary);
  font-size: 11px;
  font-weight: 800;
  line-height: 16px;
  white-space: nowrap;
}

.module-chips__list {
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-2);
  min-width: 0;
}

.module-chips__item {
  min-height: 26px;
  display: inline-flex;
  align-items: center;
  gap: var(--space-2);
  border: 1px solid color-mix(in srgb, var(--chip-color, var(--color-status-success)) 28%, transparent);
  border-radius: var(--radius-pill);
  padding: var(--space-1) var(--space-3);
  background: color-mix(in srgb, var(--chip-color, var(--color-status-success)) 12%, transparent);
  color: var(--chip-color, var(--color-status-success));
  font-size: 11px;
  font-weight: 800;
  line-height: 14px;
}

.module-chips--compact .module-chips__item {
  width: 8px;
  min-height: 8px;
  border: 0;
  border-radius: 2px;
  padding: 0;
  background: var(--chip-color, var(--color-status-success));
}

.module-chips__item[data-enabled="false"] {
  border-color: transparent;
  background: var(--color-surface-muted);
  color: var(--color-text-muted);
}

.module-chips__dot {
  width: 6px;
  height: 6px;
  flex: 0 0 auto;
  border-radius: var(--radius-pill);
  background: currentColor;
}

.module-chips--compact .module-chips__dot {
  display: none;
}

.module-chips__text {
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.module-chips__item[data-tone="success"] {
  --chip-color: var(--color-status-success);
}

.module-chips__item[data-tone="info"] {
  --chip-color: var(--color-status-info);
}

.module-chips__item[data-tone="warning"] {
  --chip-color: var(--color-status-warning);
}

.module-chips__item[data-tone="danger"] {
  --chip-color: var(--color-status-danger);
}

.module-chips__item[data-tone="neutral"] {
  --chip-color: var(--color-status-draft);
}

.module-chips__item[data-tone="specialty"] {
  --chip-color: var(--color-status-specialty);
}
</style>
