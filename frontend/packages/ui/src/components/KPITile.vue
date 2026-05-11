<script setup lang="ts">
import { computed } from "vue";

type KPITone = "primary" | "success" | "warning" | "danger" | "info" | "neutral" | "specialty";

const props = withDefaults(
  defineProps<{
    label: string;
    value: string | number;
    helper?: string;
    meta?: string;
    tone?: KPITone;
    trendLabel?: string;
    trendTone?: KPITone;
    sparkline?: number[];
  }>(),
  {
    helper: undefined,
    meta: undefined,
    tone: "primary",
    trendLabel: undefined,
    trendTone: "success",
    sparkline: () => []
  }
);

const SPARKLINE_WIDTH = 64;
const SPARKLINE_HEIGHT = 24;
const SPARKLINE_PADDING = 2;

const sparklinePoints = computed(() => {
  if (props.sparkline.length < 2) {
    return "";
  }

  const min = Math.min(...props.sparkline);
  const max = Math.max(...props.sparkline);
  const range = max - min || 1;
  const step = (SPARKLINE_WIDTH - SPARKLINE_PADDING * 2) / (props.sparkline.length - 1);

  return props.sparkline
    .map((point, index) => {
      const x = SPARKLINE_PADDING + index * step;
      const normalized = (point - min) / range;
      const y = SPARKLINE_HEIGHT - SPARKLINE_PADDING - normalized * (SPARKLINE_HEIGHT - SPARKLINE_PADDING * 2);

      return `${x.toFixed(2)},${y.toFixed(2)}`;
    })
    .join(" ");
});
</script>

<template>
  <article class="kpi-tile" :data-tone="tone">
    <div class="kpi-tile__header">
      <p class="kpi-tile__label">{{ label }}</p>
      <slot name="extra">
        <svg
          v-if="sparklinePoints"
          class="kpi-tile__sparkline"
          :viewBox="`0 0 ${SPARKLINE_WIDTH} ${SPARKLINE_HEIGHT}`"
          role="img"
          :aria-label="helper ?? label"
        >
          <polyline :points="sparklinePoints" />
        </svg>
        <span v-else class="kpi-tile__mark" aria-hidden="true"></span>
      </slot>
    </div>

    <strong class="kpi-tile__value">{{ value }}</strong>

    <div v-if="meta || trendLabel" class="kpi-tile__footer">
      <span v-if="trendLabel" class="kpi-tile__trend" :data-tone="trendTone">{{ trendLabel }}</span>
      <span v-if="meta" class="kpi-tile__meta">{{ meta }}</span>
    </div>

    <p v-if="helper" class="kpi-tile__helper">{{ helper }}</p>
  </article>
</template>

<style scoped>
.kpi-tile {
  min-height: 112px;
  display: grid;
  align-content: start;
  gap: var(--space-2, 8px);
  border: 1px solid var(--color-border-subtle, #d8d2c5);
  border-radius: var(--radius-card, 16px);
  padding: var(--space-5, 18px);
  background: var(--color-surface-elevated, #fffdf8);
  box-shadow: var(--shadow-elevation-1, 0 10px 24px rgba(57, 50, 40, 0.07));
  color: var(--color-text-primary, #102a43);
  transition:
    border-color var(--motion-duration-xs, 120ms) var(--motion-ease-standard, ease),
    transform var(--motion-duration-xs, 120ms) var(--motion-ease-standard, ease),
    box-shadow var(--motion-duration-xs, 120ms) var(--motion-ease-standard, ease);
}

.kpi-tile:hover {
  border-color: var(--color-border-strong, #c9bfad);
  box-shadow: var(--shadow-elevation-2, 0 14px 30px rgba(57, 50, 40, 0.1));
  transform: translateY(-1px);
}

.kpi-tile__header,
.kpi-tile__footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-3, 12px);
  min-width: 0;
}

.kpi-tile__label,
.kpi-tile__meta,
.kpi-tile__helper {
  margin: 0;
  color: var(--color-text-muted, #627d98);
  font-size: var(--text-caption-size, 12px);
  line-height: var(--text-caption-line-height, 16px);
}

.kpi-tile__label {
  font-weight: 700;
}

.kpi-tile__value {
  display: block;
  color: var(--tone-color, var(--color-text-primary, #102a43));
  font-size: 26px;
  line-height: 34px;
}

.kpi-tile__helper {
  color: var(--color-text-secondary, #486581);
}

.kpi-tile__mark {
  width: 60px;
  height: 24px;
  border-radius: calc(var(--radius-input, 12px) / 2);
  background:
    linear-gradient(var(--tone-color, var(--color-brand-primary, #0e7c86)), var(--tone-color, var(--color-brand-primary, #0e7c86))) var(--space-1, 4px) 16px / 52px 2px no-repeat,
    color-mix(in srgb, var(--tone-color, var(--color-brand-primary, #0e7c86)) 10%, transparent);
}

.kpi-tile__sparkline {
  width: 64px;
  height: 24px;
  color: var(--tone-color, var(--color-brand-primary, #0e7c86));
}

.kpi-tile__sparkline polyline {
  fill: none;
  stroke: currentColor;
  stroke-linecap: round;
  stroke-linejoin: round;
  stroke-width: 2;
}

.kpi-tile__trend {
  color: var(--trend-color, var(--color-status-success, #047857));
  font-size: 11px;
  font-weight: 800;
  line-height: 14px;
}

.kpi-tile__trend[data-tone="primary"] {
  --trend-color: var(--color-brand-primary, #0e7c86);
}

.kpi-tile__trend[data-tone="success"] {
  --trend-color: var(--color-status-success, #047857);
}

.kpi-tile__trend[data-tone="warning"] {
  --trend-color: var(--color-status-warning, #b45309);
}

.kpi-tile__trend[data-tone="danger"] {
  --trend-color: var(--color-status-danger, #b42318);
}

.kpi-tile__trend[data-tone="info"] {
  --trend-color: var(--color-status-info, #1d4ed8);
}

.kpi-tile__trend[data-tone="neutral"] {
  --trend-color: var(--color-status-draft, #475569);
}

.kpi-tile__trend[data-tone="specialty"] {
  --trend-color: var(--color-status-specialty, #7c3aed);
}

.kpi-tile[data-tone="primary"] {
  --tone-color: var(--color-brand-primary, #0e7c86);
}

.kpi-tile[data-tone="success"] {
  --tone-color: var(--color-status-success, #047857);
}

.kpi-tile[data-tone="warning"] {
  --tone-color: var(--color-status-warning, #b45309);
}

.kpi-tile[data-tone="danger"] {
  --tone-color: var(--color-status-danger, #b42318);
}

.kpi-tile[data-tone="info"] {
  --tone-color: var(--color-status-info, #1d4ed8);
}

.kpi-tile[data-tone="neutral"] {
  --tone-color: var(--color-status-draft, #475569);
}

.kpi-tile[data-tone="specialty"] {
  --tone-color: var(--color-status-specialty, #7c3aed);
}

@media (prefers-reduced-motion: reduce) {
  .kpi-tile {
    transition: none;
  }

  .kpi-tile:hover {
    transform: none;
  }
}
</style>
