<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, ref, watch } from "vue";
import { useFocusTrap, useReducedMotion } from "../composables";

type CommandPaletteTone = "default" | "primary" | "success" | "warning" | "danger" | "info" | "neutral";

type CommandPaletteItem = {
  id: string;
  label: string;
  meta?: string;
  hint?: string;
  icon?: string;
  disabled?: boolean;
  tone?: CommandPaletteTone;
};

type CommandPaletteSection = {
  id: string;
  label: string;
  items: CommandPaletteItem[];
};

const props = withDefaults(
  defineProps<{
    open: boolean;
    query: string;
    sections: CommandPaletteSection[];
    placeholder?: string;
    loading?: boolean;
    emptyLabel?: string;
    emptyHelper?: string;
    closeLabel?: string;
    autofocus?: boolean;
  }>(),
  {
    placeholder: "Tìm kiếm thao tác...",
    loading: false,
    emptyLabel: "Không có kết quả phù hợp.",
    emptyHelper: undefined,
    closeLabel: "esc",
    autofocus: true
  }
);

const emit = defineEmits<{
  close: [];
  select: [item: CommandPaletteItem];
  "update:query": [value: string];
}>();

const panelRef = ref<HTMLElement | null>(null);
const inputRef = ref<HTMLInputElement | null>(null);
const activeIndex = ref(0);
const previousBodyOverflow = ref<string | undefined>();
const { prefersReducedMotion } = useReducedMotion();

const enabledItems = computed(() =>
  props.sections.flatMap((section) => section.items).filter((item) => !item.disabled)
);

const hasItems = computed(() => props.sections.some((section) => section.items.length > 0));

const focusTrap = useFocusTrap(panelRef, {
  initialFocus: inputRef,
  restoreFocus: true,
  onEscape: () => emit("close")
});

function clampActiveIndex(): void {
  const maxIndex = Math.max(enabledItems.value.length - 1, 0);
  activeIndex.value = Math.min(Math.max(activeIndex.value, 0), maxIndex);
}

function itemActiveIndex(item: CommandPaletteItem): number {
  return enabledItems.value.findIndex((enabledItem) => enabledItem.id === item.id);
}

function moveActive(delta: number): void {
  if (enabledItems.value.length === 0) {
    return;
  }

  const nextIndex = (activeIndex.value + delta + enabledItems.value.length) % enabledItems.value.length;
  activeIndex.value = nextIndex;
}

function selectItem(item: CommandPaletteItem): void {
  if (item.disabled) {
    return;
  }

  emit("select", item);
}

function selectActiveItem(): void {
  const item = enabledItems.value[activeIndex.value];
  if (item) {
    selectItem(item);
  }
}

function handleKeydown(event: KeyboardEvent): void {
  if (event.key === "ArrowDown") {
    event.preventDefault();
    moveActive(1);
    return;
  }

  if (event.key === "ArrowUp") {
    event.preventDefault();
    moveActive(-1);
    return;
  }

  if (event.key === "Enter") {
    event.preventDefault();
    selectActiveItem();
  }
}

watch(
  () => [props.query, props.sections] as const,
  () => {
    activeIndex.value = 0;
    clampActiveIndex();
  },
  { deep: true }
);

watch(
  () => props.open,
  async (open) => {
    if (typeof document !== "undefined") {
      if (open) {
        previousBodyOverflow.value = document.body.style.overflow;
        document.body.style.overflow = "hidden";
      } else if (previousBodyOverflow.value !== undefined) {
        document.body.style.overflow = previousBodyOverflow.value;
        previousBodyOverflow.value = undefined;
      }
    }

    if (open) {
      await nextTick();
      activeIndex.value = 0;
      clampActiveIndex();
      focusTrap.activate();
    } else {
      focusTrap.deactivate();
    }
  },
  { immediate: true }
);

onBeforeUnmount(() => {
  if (typeof document !== "undefined" && previousBodyOverflow.value !== undefined) {
    document.body.style.overflow = previousBodyOverflow.value;
  }
});
</script>

<template>
  <Teleport to="body">
    <div
      v-if="open"
      class="command-palette"
      :class="{ 'command-palette--reduced-motion': prefersReducedMotion }"
      role="dialog"
      aria-modal="true"
      @click.self="$emit('close')"
    >
      <section ref="panelRef" class="command-palette__panel" aria-label="Bảng lệnh nhanh" tabindex="-1">
        <header class="command-palette__search">
          <span aria-hidden="true">⌕</span>
          <input
            ref="inputRef"
            :autofocus="autofocus"
            type="search"
            :value="query"
            :placeholder="placeholder"
            @input="$emit('update:query', ($event.target as HTMLInputElement).value)"
            @keydown="handleKeydown"
          />
          <kbd>{{ closeLabel }}</kbd>
        </header>

        <div v-if="loading" class="command-palette__state">Đang tải...</div>

        <div v-else-if="!hasItems" class="command-palette__state">
          <strong>{{ emptyLabel }}</strong>
          <p v-if="emptyHelper">{{ emptyHelper }}</p>
        </div>

        <div v-else class="command-palette__body">
          <section v-for="section in sections" :key="section.id" class="command-palette__section">
            <p v-if="section.items.length > 0">{{ section.label }}</p>
            <button
              v-for="item in section.items"
              :key="item.id"
              type="button"
              class="command-palette__row"
              :class="{ 'is-active': itemActiveIndex(item) === activeIndex }"
              :data-tone="item.tone ?? 'default'"
              :disabled="item.disabled"
              @click="selectItem(item)"
              @mouseenter="activeIndex = Math.max(itemActiveIndex(item), 0)"
            >
              <span class="command-palette__icon" aria-hidden="true">{{ item.icon ?? "▣" }}</span>
              <strong>{{ item.label }}</strong>
              <small>{{ item.hint ?? item.meta }}</small>
            </button>
          </section>
        </div>
      </section>
    </div>
  </Teleport>
</template>

<style scoped>
.command-palette {
  position: fixed;
  inset: 0;
  z-index: 100;
  display: flex;
  align-items: flex-start;
  justify-content: center;
  padding-top: min(28vh, 250px);
  background: color-mix(in srgb, var(--color-text-primary) 58%, transparent);
}

.command-palette__panel {
  width: min(640px, calc(100vw - 32px));
  max-height: min(520px, calc(100vh - 80px));
  overflow: hidden auto;
  border-radius: var(--radius-card);
  background: var(--color-surface-elevated);
  box-shadow: var(--shadow-elevation-3);
  outline: none;
}

.command-palette__search {
  display: grid;
  grid-template-columns: auto 1fr auto;
  gap: var(--space-3);
  align-items: center;
  min-height: 62px;
  border-bottom: 1px solid var(--color-border-subtle);
  padding: 0 var(--space-5);
  color: var(--color-text-secondary);
}

.command-palette__search input {
  width: 100%;
  border: 0;
  outline: 0;
  background: transparent;
  color: var(--color-text-primary);
  font-size: 15px;
}

kbd {
  border-radius: 7px;
  padding: 4px 8px;
  background: var(--color-surface-muted);
  color: var(--color-text-secondary);
  font-size: 11px;
  font-weight: 800;
}

.command-palette__section {
  padding: var(--space-3) 0 var(--space-2);
}

.command-palette__section p {
  margin: 0;
  padding: 0 var(--space-5) var(--space-2);
  color: var(--color-text-muted);
  font-size: 10px;
  font-weight: 800;
  text-transform: uppercase;
}

.command-palette__row {
  width: 100%;
  min-height: 40px;
  display: grid;
  grid-template-columns: 18px 1fr auto;
  gap: var(--space-3);
  align-items: center;
  border: 0;
  padding: 0 var(--space-5);
  background: transparent;
  color: var(--color-text-primary);
  cursor: pointer;
  font: inherit;
  text-align: left;
}

.command-palette__row:hover,
.command-palette__row:focus-visible,
.command-palette__row.is-active {
  background: color-mix(in srgb, var(--row-color, var(--color-brand-primary)) 8%, var(--color-surface-elevated));
  outline: none;
}

.command-palette__row:disabled {
  cursor: not-allowed;
  opacity: 0.62;
}

.command-palette__row strong {
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  font-size: 13px;
  white-space: nowrap;
}

.command-palette__icon,
.command-palette__row small {
  color: var(--color-text-muted);
}

.command-palette__icon {
  width: 18px;
  text-align: center;
}

.command-palette__row small {
  font-size: 11px;
  white-space: nowrap;
}

.command-palette__state {
  display: grid;
  gap: var(--space-2);
  padding: var(--space-8) var(--space-5);
  color: var(--color-text-secondary);
  text-align: center;
}

.command-palette__state strong,
.command-palette__state p {
  margin: 0;
}

.command-palette__row[data-tone="primary"] {
  --row-color: var(--color-brand-primary);
}

.command-palette__row[data-tone="success"] {
  --row-color: var(--color-status-success);
}

.command-palette__row[data-tone="warning"] {
  --row-color: var(--color-status-warning);
}

.command-palette__row[data-tone="danger"] {
  --row-color: var(--color-status-danger);
}

.command-palette__row[data-tone="info"] {
  --row-color: var(--color-status-info);
}

.command-palette__row[data-tone="neutral"] {
  --row-color: var(--color-status-draft);
}

@media (max-width: 720px) {
  .command-palette {
    padding-top: var(--space-6);
  }
}

@media (prefers-reduced-motion: reduce) {
  .command-palette,
  .command-palette__panel {
    scroll-behavior: auto;
  }
}
</style>
