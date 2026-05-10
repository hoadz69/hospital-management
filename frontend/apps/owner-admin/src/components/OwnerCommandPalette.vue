<script setup lang="ts">
import { onBeforeUnmount, watch } from "vue";

const props = defineProps<{
  open: boolean;
}>();

const emit = defineEmits<{
  close: [];
}>();

const recentTenants = [
  { slug: "mat-saigon", label: "Phòng khám Mắt Sài Gòn" },
  { slug: "rhm-hadong", label: "Răng Hàm Mặt Hà Đông" }
];

const actions = [
  { label: "Tạo phòng khám mới", hint: "Ctrl N", to: "/clinics/create", icon: "+" },
  { label: "Mở danh sách phòng khám", hint: "Ctrl T", to: "/clinics", icon: "▦" },
  { label: "Xuất báo cáo CSV", hint: "Ctrl E", to: "/clinics", icon: "⇩" }
];

function handleKeydown(event: KeyboardEvent) {
  if (event.key === "Escape" && props.open) {
    emit("close");
  }
}

watch(
  () => props.open,
  (open) => {
    if (typeof document === "undefined") {
      return;
    }

    document.body.style.overflow = open ? "hidden" : "";
  }
);

if (typeof window !== "undefined") {
  window.addEventListener("keydown", handleKeydown);
}

onBeforeUnmount(() => {
  if (typeof window !== "undefined") {
    window.removeEventListener("keydown", handleKeydown);
  }
  if (typeof document !== "undefined") {
    document.body.style.overflow = "";
  }
});
</script>

<template>
  <Teleport to="body">
    <div v-if="open" class="command-overlay" role="dialog" aria-modal="true" @click.self="$emit('close')">
      <section class="command-panel" aria-label="Bảng lệnh nhanh">
        <header class="command-search">
          <span aria-hidden="true">⌕</span>
          <input autofocus type="search" placeholder="Tìm phòng khám, tên miền, thao tác..." />
          <kbd>ESC</kbd>
        </header>

        <div class="command-section">
          <p>Gần đây</p>
          <RouterLink
            v-for="tenant in recentTenants"
            :key="tenant.slug"
            class="command-row is-active"
            to="/clinics"
            @click="$emit('close')"
          >
            <span aria-hidden="true">▣</span>
            <strong>{{ tenant.slug }}</strong>
            <small>{{ tenant.label }}</small>
          </RouterLink>
        </div>

        <div class="command-section">
          <p>Thao tác</p>
          <RouterLink
            v-for="action in actions"
            :key="action.label"
            class="command-row"
            :to="action.to"
            @click="$emit('close')"
          >
            <span aria-hidden="true">{{ action.icon }}</span>
            <strong>{{ action.label }}</strong>
            <small>{{ action.hint }}</small>
          </RouterLink>
        </div>
      </section>
    </div>
  </Teleport>
</template>

<style scoped>
.command-overlay {
  position: fixed;
  inset: 0;
  z-index: 100;
  display: flex;
  align-items: flex-start;
  justify-content: center;
  padding-top: min(14vh, 120px);
  background: color-mix(in srgb, var(--color-text-primary) 60%, transparent);
}

.command-panel {
  width: min(640px, calc(100vw - 32px));
  max-height: min(520px, calc(100vh - 80px));
  overflow: hidden auto;
  border-radius: var(--radius-card);
  background: var(--color-surface-elevated);
  box-shadow: var(--shadow-elevation-3);
}

.command-search {
  display: grid;
  grid-template-columns: auto 1fr auto;
  gap: var(--space-3);
  align-items: center;
  min-height: 60px;
  border-bottom: 1px solid var(--color-border-subtle);
  padding: 0 var(--space-5);
  color: var(--color-text-secondary);
}

.command-search input {
  width: 100%;
  border: 0;
  outline: 0;
  color: var(--color-text-primary);
}

kbd {
  border-radius: 6px;
  padding: 4px 8px;
  background: var(--color-surface-muted);
  color: var(--color-text-secondary);
  font-size: 11px;
  font-weight: 800;
}

.command-section {
  padding: var(--space-3) 0;
}

.command-section p {
  margin: 0;
  padding: 0 var(--space-5) var(--space-2);
  color: var(--color-text-muted);
  font-size: 10px;
  font-weight: 800;
  text-transform: uppercase;
}

.command-row {
  min-height: 40px;
  display: grid;
  grid-template-columns: 18px 1fr auto;
  gap: var(--space-3);
  align-items: center;
  padding: 0 var(--space-5);
  color: var(--color-text-primary);
  text-decoration: none;
}

.command-row:hover,
.command-row:focus-visible,
.command-row.is-active {
  background: color-mix(in srgb, var(--color-brand-primary) 8%, var(--color-surface-elevated));
  outline: none;
}

.command-row span,
.command-row small {
  color: var(--color-text-muted);
}

.command-row strong {
  font-size: 13px;
}

.command-row small {
  font-size: 11px;
}
</style>
