<script setup lang="ts">
import { AppButton } from "@clinic-saas/ui";

withDefaults(
  defineProps<{
    title?: string;
    subtitle?: string;
    showSearch?: boolean;
    showCreateAction?: boolean;
    sidebarOpen?: boolean;
  }>(),
  {
    title: "Quản lý phòng khám",
    subtitle: "Điều phối toàn bộ phòng khám và trạng thái vận hành nền tảng.",
    showSearch: true,
    showCreateAction: true,
    sidebarOpen: false
  }
);

const emit = defineEmits<{
  (event: "toggle-sidebar"): void;
  (event: "open-command-palette"): void;
}>();
</script>

<template>
  <header class="topbar">
    <button
      type="button"
      class="hamburger"
      aria-label="Mở menu điều hướng"
      :aria-expanded="sidebarOpen"
      @click="$emit('toggle-sidebar')"
    >
      <span class="hamburger-bar" aria-hidden="true"></span>
      <span class="hamburger-bar" aria-hidden="true"></span>
      <span class="hamburger-bar" aria-hidden="true"></span>
    </button>

    <div class="topbar-heading" aria-label="Đường dẫn hiện tại">
      <span>Owner Admin</span>
      <span aria-hidden="true">/</span>
      <h1>{{ title }}</h1>
      <p v-if="subtitle">{{ subtitle }}</p>
    </div>

    <div class="topbar-actions">
      <button
        v-if="showSearch"
        type="button"
        class="search"
        aria-label="Mở command palette"
        @click="$emit('open-command-palette')"
      >
        <span aria-hidden="true">⌕</span>
        <span class="search-label">Tìm phòng khám, slug hoặc tên miền...</span>
        <kbd>⌘K</kbd>
      </button>
      <button type="button" class="icon-button" aria-label="Thông báo">•</button>
      <RouterLink v-if="showCreateAction" to="/clinics/create" class="create-link">
        <AppButton label="Thêm phòng khám" />
      </RouterLink>
    </div>
  </header>
</template>

<style scoped>
.topbar {
  box-sizing: border-box;
  min-height: 56px;
  min-width: 0;
  max-width: 100%;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-5);
  overflow-x: clip;
  border-bottom: 1px solid var(--color-border-subtle);
  padding: 0 var(--space-6);
  background: var(--color-surface-elevated);
}

.topbar *,
.topbar *::before,
.topbar *::after {
  box-sizing: border-box;
}

.hamburger {
  display: none;
  width: 40px;
  height: 40px;
  flex-shrink: 0;
  align-items: center;
  justify-content: center;
  flex-direction: column;
  gap: 4px;
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-input);
  padding: 0;
  background: var(--color-surface-elevated);
  color: var(--color-text-primary);
  cursor: pointer;
}

.hamburger:hover {
  background: var(--color-surface-muted);
}

.hamburger:focus-visible {
  outline: 2px solid var(--color-brand-primary);
  outline-offset: 2px;
}

.hamburger-bar {
  display: block;
  width: 18px;
  height: 2px;
  border-radius: 1px;
  background: currentColor;
}

.topbar-heading {
  flex: 1 1 auto;
  display: flex;
  align-items: center;
  gap: 6px;
  min-width: 0;
  color: var(--color-text-muted);
  font-size: 13px;
  white-space: nowrap;
}

.topbar-heading h1,
.topbar-heading p {
  margin: 0;
}

.topbar-heading h1 {
  min-width: 0;
  overflow: hidden;
  color: var(--color-text-primary);
  font-size: 13px;
  font-weight: 800;
  text-overflow: ellipsis;
}

.topbar-heading p {
  display: none;
}

.topbar-actions {
  min-width: 0;
  display: flex;
  flex: 0 1 auto;
  align-items: center;
  gap: var(--space-3);
}

.search {
  width: min(360px, 34vw);
  min-width: 0;
  min-height: 36px;
  display: grid;
  grid-template-columns: auto 1fr auto;
  gap: var(--space-2);
  align-items: center;
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-input);
  padding: 0 var(--space-3);
  background: var(--color-surface-muted);
  color: var(--color-text-muted);
  cursor: pointer;
  text-align: left;
}

.search-label {
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.search:hover,
.search:focus-visible {
  border-color: color-mix(in srgb, var(--color-brand-primary) 34%, var(--color-border-subtle));
  outline: 2px solid color-mix(in srgb, var(--color-brand-primary) 20%, transparent);
  outline-offset: 2px;
}

.search kbd {
  border: 1px solid var(--color-border-subtle);
  border-radius: 4px;
  padding: 2px 6px;
  background: var(--color-surface-elevated);
  color: var(--color-text-muted);
  font-size: 10px;
  font-weight: 800;
}

.icon-button {
  width: 36px;
  height: 36px;
  border: 0;
  border-radius: var(--radius-input);
  background: transparent;
  color: var(--color-status-warning);
  cursor: pointer;
  font-size: 20px;
  line-height: 1;
}

.icon-button:hover,
.icon-button:focus-visible {
  background: var(--color-surface-muted);
  outline: 2px solid color-mix(in srgb, var(--color-brand-primary) 20%, transparent);
  outline-offset: 2px;
}

.create-link {
  flex: 0 0 auto;
  text-decoration: none;
}

@media (max-width: 1023px) {
  .hamburger {
    display: inline-flex;
  }

  .topbar {
    padding: 0 var(--space-5);
    gap: var(--space-3);
  }

  .search {
    width: min(240px, 38vw);
  }
}

@media (max-width: 640px) {
  .topbar {
    min-height: 56px;
    align-items: center;
    flex-wrap: nowrap;
    gap: var(--space-2);
    padding: 0 var(--space-4);
  }

  .topbar-heading {
    flex: 1 1 auto;
    overflow: hidden;
  }

  .topbar-actions {
    flex: 0 0 auto;
    flex-basis: auto;
    flex-wrap: nowrap;
    justify-content: flex-end;
    gap: var(--space-2);
  }

  .search {
    width: 40px;
    height: 40px;
    min-height: 40px;
    grid-template-columns: 1fr;
    place-items: center;
    padding: 0;
  }

  .search-label,
  .search kbd,
  .icon-button,
  .create-link {
    display: none;
  }
}
</style>
