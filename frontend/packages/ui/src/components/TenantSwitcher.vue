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
}

.tenant-switcher__trigger,
.tenant-switcher__option {
  width: 100%;
  display: flex;
  align-items: center;
  gap: var(--space-3);
  border: 1px solid var(--color-border-subtle);
  background: var(--color-surface-elevated);
  color: var(--color-text-primary);
  cursor: pointer;
  font: inherit;
  text-align: left;
}

.tenant-switcher__trigger {
  min-height: 54px;
  border-radius: var(--radius-card);
  padding: var(--space-2) var(--space-3);
  box-shadow: var(--shadow-elevation-1);
}

.tenant-switcher__avatar {
  width: 34px;
  height: 34px;
  display: grid;
  place-items: center;
  flex: 0 0 auto;
  border-radius: var(--radius-input);
  background: color-mix(in srgb, var(--color-brand-primary) 10%, var(--color-surface-elevated));
  color: var(--color-brand-primary);
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
  color: var(--color-text-secondary);
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
  color: var(--color-text-muted);
}

.tenant-switcher__menu {
  position: absolute;
  right: 0;
  left: 0;
  z-index: 30;
  display: grid;
  gap: var(--space-1);
  margin-top: var(--space-2);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-card);
  padding: var(--space-2);
  background: var(--color-surface-elevated);
  box-shadow: var(--shadow-elevation-3);
}

.tenant-switcher__option {
  min-height: 48px;
  border-color: transparent;
  border-radius: var(--radius-input);
  padding: var(--space-2);
}

.tenant-switcher__option:hover,
.tenant-switcher__option:focus-visible,
.tenant-switcher__option[aria-selected="true"] {
  background: color-mix(in srgb, var(--color-brand-primary) 8%, var(--color-surface-elevated));
  outline: none;
}

.tenant-switcher__option:disabled {
  cursor: not-allowed;
  opacity: 0.62;
}

.tenant-switcher__state {
  padding: var(--space-4);
  color: var(--color-text-secondary);
  font-size: 12px;
  text-align: center;
}
</style>
