<script setup lang="ts">
import { computed } from "vue";

type TenantSwitcherItem = {
  id: string;
  label: string;
  slug?: string;
  meta?: string;
  avatarLabel?: string;
  disabled?: boolean;
};

const props = withDefaults(
  defineProps<{
    tenants: TenantSwitcherItem[];
    currentTenantId?: string;
    open?: boolean;
    label?: string;
    helper?: string;
    loading?: boolean;
    placeholder?: string;
    emptyLabel?: string;
  }>(),
  {
    currentTenantId: undefined,
    open: false,
    label: "Tenant",
    helper: undefined,
    loading: false,
    placeholder: "Chọn tenant",
    emptyLabel: "Chưa có tenant để chọn."
  }
);

const emit = defineEmits<{
  select: [tenantId: string];
  open: [];
  close: [];
  "update:open": [open: boolean];
}>();

const currentTenant = computed(() => props.tenants.find((tenant) => tenant.id === props.currentTenantId));

function setOpen(open: boolean): void {
  emit("update:open", open);
  if (open) {
    emit("open");
  } else {
    emit("close");
  }
}

function selectTenant(tenant: TenantSwitcherItem): void {
  if (tenant.disabled) {
    return;
  }

  emit("select", tenant.id);
  setOpen(false);
}
</script>

<template>
  <div class="tenant-switcher" :data-open="open">
    <button
      type="button"
      class="tenant-switcher__trigger"
      :aria-expanded="open"
      aria-haspopup="listbox"
      :aria-label="currentTenant ? `${label}: ${currentTenant.label}` : label"
      @click="setOpen(!open)"
    >
      <span class="tenant-switcher__avatar" aria-hidden="true">
        {{ currentTenant?.avatarLabel ?? currentTenant?.label?.slice(0, 2) ?? "TN" }}
      </span>
      <span class="tenant-switcher__copy">
        <span class="tenant-switcher__label">{{ label }}</span>
        <strong>{{ currentTenant?.label ?? placeholder }}</strong>
        <small v-if="helper || currentTenant?.meta">{{ currentTenant?.meta ?? helper }}</small>
      </span>
      <span class="tenant-switcher__chevron" aria-hidden="true">⌄</span>
    </button>

    <div v-if="open" class="tenant-switcher__menu" role="listbox" :aria-label="label">
      <div v-if="loading" class="tenant-switcher__state">Đang tải tenant...</div>
      <div v-else-if="tenants.length === 0" class="tenant-switcher__state">{{ emptyLabel }}</div>
      <button
        v-for="tenant in tenants"
        v-else
        :key="tenant.id"
        type="button"
        class="tenant-switcher__option"
        role="option"
        :aria-selected="tenant.id === currentTenantId"
        :disabled="tenant.disabled"
        :aria-disabled="tenant.disabled"
        @click="selectTenant(tenant)"
      >
        <span class="tenant-switcher__avatar" aria-hidden="true">
          {{ tenant.avatarLabel ?? tenant.label.slice(0, 2) }}
        </span>
        <span>
          <strong>{{ tenant.label }}</strong>
          <small>{{ tenant.slug ?? tenant.meta }}</small>
        </span>
      </button>
    </div>
  </div>
</template>

<style scoped>
.tenant-switcher {
  position: relative;
  min-width: 220px;
  color: var(--color-text-primary, #102a43);
}

.tenant-switcher__trigger,
.tenant-switcher__option {
  width: 100%;
  display: flex;
  align-items: center;
  gap: var(--space-3, 12px);
  border: 1px solid var(--color-border-subtle, #d8d2c5);
  background: var(--color-surface-elevated, #fffdf8);
  color: var(--color-text-primary, #102a43);
  cursor: pointer;
  font: inherit;
  text-align: left;
  transition:
    background var(--motion-duration-xs, 120ms) var(--motion-ease-standard, ease),
    border-color var(--motion-duration-xs, 120ms) var(--motion-ease-standard, ease),
    box-shadow var(--motion-duration-xs, 120ms) var(--motion-ease-standard, ease);
}

.tenant-switcher__trigger {
  min-height: 52px;
  border-radius: var(--radius-card, 16px);
  padding: var(--space-2, 8px) var(--space-3, 12px);
  box-shadow: var(--shadow-elevation-1, 0 10px 24px rgba(57, 50, 40, 0.07));
}

.tenant-switcher__trigger:hover,
.tenant-switcher__trigger:focus-visible,
.tenant-switcher[data-open="true"] .tenant-switcher__trigger {
  border-color: var(--color-border-strong, #c9bfad);
  box-shadow: var(--shadow-elevation-2, 0 14px 30px rgba(57, 50, 40, 0.1));
  outline: none;
}

.tenant-switcher__trigger:focus-visible {
  outline: 3px solid color-mix(in srgb, var(--color-brand-primary, #0e7c86) 28%, transparent);
  outline-offset: 2px;
}

.tenant-switcher__avatar {
  width: 34px;
  height: 34px;
  display: grid;
  place-items: center;
  flex: 0 0 auto;
  border-radius: var(--radius-input, 12px);
  background: color-mix(in srgb, var(--color-brand-primary, #0e7c86) 10%, var(--color-surface-elevated, #fffdf8));
  color: var(--color-brand-primary, #0e7c86);
  font-size: 11px;
  font-weight: 900;
  text-transform: uppercase;
}

.tenant-switcher__copy,
.tenant-switcher__option span:last-child {
  min-width: 0;
  display: grid;
  gap: 2px;
}

.tenant-switcher__label,
.tenant-switcher small {
  color: var(--color-text-secondary, #486581);
  font-size: 11px;
  font-weight: 700;
  line-height: 14px;
}

.tenant-switcher strong {
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  font-size: 13px;
  line-height: 18px;
  white-space: nowrap;
}

.tenant-switcher__chevron {
  margin-left: auto;
  color: var(--color-text-muted, #627d98);
}

.tenant-switcher__menu {
  position: absolute;
  right: 0;
  left: 0;
  z-index: 30;
  display: grid;
  gap: var(--space-1, 4px);
  margin-top: var(--space-2, 8px);
  border: 1px solid var(--color-border-subtle, #d8d2c5);
  border-radius: var(--radius-card, 16px);
  padding: var(--space-2, 8px);
  background: var(--color-surface-elevated, #fffdf8);
  box-shadow: var(--shadow-elevation-3, 0 28px 70px rgba(57, 50, 40, 0.18));
}

.tenant-switcher__option {
  min-height: 48px;
  border-color: transparent;
  border-radius: var(--radius-input, 12px);
  padding: var(--space-2, 8px);
}

.tenant-switcher__option:hover,
.tenant-switcher__option:focus-visible,
.tenant-switcher__option[aria-selected="true"] {
  background: color-mix(in srgb, var(--color-brand-primary, #0e7c86) 8%, var(--color-surface-elevated, #fffdf8));
  outline: none;
}

.tenant-switcher__option:disabled {
  cursor: not-allowed;
  opacity: 0.62;
}

.tenant-switcher__state {
  padding: var(--space-4, 16px);
  color: var(--color-text-secondary, #486581);
  font-size: 12px;
  text-align: center;
}

@media (prefers-reduced-motion: reduce) {
  .tenant-switcher__trigger,
  .tenant-switcher__option {
    transition: none;
  }
}
</style>
