<script setup lang="ts">
type NavItem = {
  label: string;
  to: string;
  enabled: boolean;
  icon: string;
  count?: string;
};

const navItems: NavItem[] = [
  { label: "Tổng quan", to: "/dashboard", enabled: true, icon: "▥" },
  { label: "Phòng khám", to: "/clinics", enabled: true, icon: "▣", count: "248" },
  { label: "Gói & module", to: "/plans", enabled: true, icon: "▦", count: "3" },
  { label: "Tên miền", to: "", enabled: false, icon: "◎" },
  { label: "Mẫu giao diện", to: "", enabled: false, icon: "◫" },
  { label: "Thanh toán", to: "", enabled: false, icon: "▤" },
  { label: "Báo cáo", to: "", enabled: false, icon: "▧" },
  { label: "Nhật ký", to: "", enabled: false, icon: "☷" },
  { label: "Cài đặt", to: "", enabled: false, icon: "⚙" }
];

const emit = defineEmits<{
  (event: "navigate"): void;
}>();

function handleNavigate() {
  emit("navigate");
}
</script>

<template>
  <aside class="sidebar">
    <RouterLink class="brand" to="/dashboard" aria-label="ClinicOS Owner Admin" @click="handleNavigate">
      <span class="brand-mark">+</span>
      <strong>ClinicOS</strong>
    </RouterLink>

    <span class="role-badge">Owner Super Admin</span>

    <nav class="nav-list" aria-label="Điều hướng Owner Admin">
      <template v-for="item in navItems" :key="item.label">
        <RouterLink v-if="item.enabled" class="nav-item" :to="item.to" @click="handleNavigate">
          <span class="nav-icon" aria-hidden="true">{{ item.icon }}</span>
          <span class="nav-label">{{ item.label }}</span>
          <span v-if="item.count" class="count-badge" aria-hidden="true">{{ item.count }}</span>
        </RouterLink>

        <button
          v-else
          type="button"
          class="nav-item disabled"
          disabled
          tabindex="-1"
          aria-disabled="true"
          title="Sắp ra mắt"
        >
          <span class="nav-icon" aria-hidden="true">{{ item.icon }}</span>
          <span class="nav-label">{{ item.label }}</span>
          <span class="soon-badge">Sắp ra mắt</span>
        </button>
      </template>
    </nav>

    <div class="sidebar-footer">
      <RouterLink to="/clinics/create" class="sidebar-create" @click="handleNavigate">+ Tạo phòng khám</RouterLink>
    </div>
  </aside>
</template>

<style scoped>
.sidebar {
  position: sticky;
  top: 0;
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
  padding: var(--space-6) var(--space-4);
  background: var(--color-admin-sidebar);
  color: color-mix(in srgb, var(--color-surface-elevated) 72%, transparent);
}

.brand {
  display: flex;
  align-items: center;
  gap: var(--space-2);
  min-height: 64px;
  color: var(--color-surface-elevated);
  text-decoration: none;
}

.brand-mark {
  width: 28px;
  height: 28px;
  display: inline-grid;
  place-items: center;
  border-radius: 6px;
  background: var(--color-brand-primary);
  color: var(--color-surface-elevated);
  font-weight: 800;
}

.brand strong {
  font-size: 18px;
}

.role-badge {
  align-self: flex-start;
  border-radius: var(--radius-input);
  padding: var(--space-2) var(--space-3);
  background: color-mix(in srgb, var(--color-surface-elevated) 6%, transparent);
  color: color-mix(in srgb, var(--color-surface-elevated) 70%, transparent);
  font-size: 11px;
  font-weight: 800;
}

.nav-list {
  display: grid;
  gap: var(--space-2);
  margin-top: var(--space-4);
}

.nav-item {
  min-height: 44px;
  display: flex;
  align-items: center;
  gap: var(--space-3);
  width: 100%;
  border: 1px solid transparent;
  border-radius: 8px;
  padding: 0 var(--space-3);
  background: transparent;
  color: color-mix(in srgb, var(--color-surface-elevated) 72%, transparent);
  font: inherit;
  font-size: 13px;
  font-weight: 700;
  text-align: left;
  text-decoration: none;
  cursor: pointer;
}

.nav-item:hover:not(.disabled):not(:disabled) {
  background: color-mix(in srgb, var(--color-surface-elevated) 6%, transparent);
}

.nav-item:focus-visible,
.sidebar-create:focus-visible,
.brand:focus-visible {
  outline: 2px solid color-mix(in srgb, var(--color-brand-primary) 55%, transparent);
  outline-offset: 2px;
}

.nav-item.router-link-active {
  border-color: color-mix(in srgb, var(--color-brand-primary) 40%, transparent);
  background: color-mix(in srgb, var(--color-brand-primary) 18%, transparent);
  color: var(--color-surface-elevated);
}

.nav-item.disabled,
.nav-item:disabled {
  color: color-mix(in srgb, var(--color-surface-elevated) 48%, transparent);
  cursor: not-allowed;
}

.nav-icon {
  width: 16px;
  flex-shrink: 0;
  text-align: center;
}

.nav-label {
  flex: 1 1 auto;
  min-width: 0;
}

.count-badge {
  flex-shrink: 0;
  border-radius: var(--radius-pill);
  padding: 2px 8px;
  background: color-mix(in srgb, var(--color-surface-elevated) 12%, transparent);
  color: color-mix(in srgb, var(--color-surface-elevated) 80%, transparent);
  font-size: 10px;
  font-weight: 800;
}

.soon-badge {
  flex-shrink: 0;
  border-radius: var(--radius-pill);
  padding: 2px 6px;
  background: color-mix(in srgb, var(--color-surface-elevated) 10%, transparent);
  color: color-mix(in srgb, var(--color-surface-elevated) 64%, transparent);
  font-size: 9px;
  font-weight: 800;
  white-space: nowrap;
}

.sidebar-footer {
  margin-top: auto;
}

.sidebar-create {
  min-height: 44px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: var(--radius-input);
  background: var(--color-brand-primary);
  color: var(--color-surface-elevated);
  font-size: 13px;
  font-weight: 800;
  text-decoration: none;
}

@media (max-width: 1023px) {
  .sidebar {
    position: static;
    min-height: 100vh;
    box-shadow: 8px 0 24px color-mix(in srgb, var(--color-text-primary) 32%, transparent);
  }
}
</style>
