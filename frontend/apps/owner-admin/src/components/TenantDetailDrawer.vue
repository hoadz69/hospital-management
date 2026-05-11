<script setup lang="ts">
// Drawer hiển thị tenant detail khi Owner Admin chọn row trong TenantTable.
// Component overlay-side, đóng được bằng click backdrop hoặc nút X, hỗ trợ Escape.
import type { TenantDetail, TenantDomainStatus, TenantStatus } from "@clinic-saas/shared-types";
import { AppButton, AppCard, DomainStateRow, ModuleChips, StatusPill } from "@clinic-saas/ui";
import { computed, onBeforeUnmount, watch } from "vue";
import { formatDomainStatus, formatModuleCode, formatTenantStatus } from "../services/labels";

const props = defineProps<{
  /** Hồ sơ tenant đang được xem; có thể undefined trong lúc loading. */
  tenant?: TenantDetail;
  /** Trạng thái mở/đóng drawer; parent giữ để cho phép undo state khi route đổi. */
  open: boolean;
  /** Loading flag cho phần body khi đang fetch detail. */
  loading?: boolean;
}>();

const emit = defineEmits<{
  close: [];
  updateStatus: [status: TenantStatus];
}>();

const nextStatus = computed<TenantStatus>(() => {
  if (!props.tenant || props.tenant.status !== "Active") {
    return "Active";
  }

  return "Suspended";
});

const domainCards = computed(() => {
  if (!props.tenant) {
    return [];
  }

  const domains =
    props.tenant.domains.length > 0
      ? props.tenant.domains
      : [
          {
            id: `${props.tenant.id}-default-domain`,
            domainName: props.tenant.defaultDomainName || `${props.tenant.slug}.clinicos.vn`,
            isPrimary: true,
            status: props.tenant.domainStatus
          }
        ];

  return domains.map((domain) => ({
    ...domain,
    helper: domain.isPrimary ? "Default subdomain" : domain.status === "verified" ? "Custom · CNAME OK" : domainHint(domain.status),
    action: domainAction(domain.status)
  }));
});

const domainCountLabel = computed(() => {
  const count = domainCards.value.length;
  const primaryCount = domainCards.value.filter((domain) => domain.isPrimary).length;
  const customCount = Math.max(count - primaryCount, 0);

  return `${count} domain · ${primaryCount} default + ${customCount} custom`;
});

const moduleTotal = 6;

function statusTone(status: TenantStatus) {
  if (status === "Active") {
    return "success";
  }

  if (status === "Suspended" || status === "Archived") {
    return "danger";
  }

  return "warning";
}

function domainTone(status: TenantDomainStatus) {
  if (status === "verified") {
    return "success";
  }

  if (status === "failed") {
    return "danger";
  }

  if (status === "pending") {
    return "warning";
  }

  return "info";
}

function domainHint(status: TenantDomainStatus) {
  if (status === "pending") {
    return "DNS đang propagate. Recheck khi bản ghi đã sẵn sàng.";
  }

  if (status === "failed") {
    return "CNAME/TXT chưa khớp. Cần rà soát bản ghi.";
  }

  return "Chưa có dữ liệu DNS/SSL từ backend.";
}

function domainAction(status: TenantDomainStatus) {
  if (status === "verified") {
    return "View cert";
  }

  if (status === "failed") {
    return "View error";
  }

  return "Recheck";
}

function moduleChipItems(moduleCodes: TenantDetail["moduleCodes"]) {
  return moduleCodes.map((moduleCode) => ({
    key: moduleCode,
    label: formatModuleCode(moduleCode),
    enabled: true,
    tone: "success" as const
  }));
}

// Đóng drawer bằng phím Escape để tăng accessibility cho keyboard user.
function handleEscape(event: KeyboardEvent) {
  if (event.key === "Escape" && props.open) {
    emit("close");
  }
}

watch(
  () => props.open,
  (open) => {
    if (typeof window === "undefined") {
      return;
    }

    if (open) {
      window.addEventListener("keydown", handleEscape);
    } else {
      window.removeEventListener("keydown", handleEscape);
    }
  },
  { immediate: true }
);

onBeforeUnmount(() => {
  if (typeof window !== "undefined") {
    window.removeEventListener("keydown", handleEscape);
  }
});
</script>

<template>
  <Teleport to="body">
    <div v-if="open" class="drawer-backdrop" role="dialog" aria-modal="true" @click.self="$emit('close')">
      <aside class="drawer" aria-label="Drawer chi tiết phòng khám">
        <header class="drawer-header">
          <div class="drawer-title">
            <div class="tenant-avatar" aria-hidden="true">✚</div>
            <div>
              <h2>{{ tenant?.displayName ?? "Đang tải phòng khám..." }}</h2>
              <p v-if="tenant">tenants/{{ tenant.slug }} · {{ tenant.defaultDomainName || "chưa có domain" }}</p>
              <p v-else>Đang đồng bộ hồ sơ tenant</p>
            </div>
          </div>
          <button type="button" class="close-button" aria-label="Đóng drawer" @click="$emit('close')">
            ×
          </button>
        </header>

        <div v-if="loading" class="drawer-state">Đang tải hồ sơ phòng khám...</div>

        <template v-else-if="tenant">
          <div class="drawer-status">
            <StatusPill :label="formatTenantStatus(tenant.status)" :tone="statusTone(tenant.status)" />
            <StatusPill :label="formatDomainStatus(tenant.domainStatus)" tone="info" />
          </div>

          <AppCard class="summary-card">
            <dl class="detail-grid">
              <div>
                <dt>Slug</dt>
                <dd>{{ tenant.slug }}</dd>
              </div>
              <div>
                <dt>Gói</dt>
                <dd>{{ tenant.planDisplayName }}</dd>
              </div>
              <div>
                <dt>Chuyên khoa</dt>
                <dd>{{ tenant.specialty }}</dd>
              </div>
              <!-- ownerName từ backend hiện chưa có; chỉ render khi adapter đã set giá trị. -->
              <div v-if="tenant.ownerName">
                <dt>Chủ sở hữu</dt>
                <dd>{{ tenant.ownerName }}</dd>
              </div>
              <div>
                <dt>Ngày tạo</dt>
                <dd>{{ new Date(tenant.createdAt).toLocaleDateString("vi-VN") }}</dd>
              </div>
            </dl>
          </AppCard>

          <AppCard class="domain-section">
            <div class="section-heading">
              <div>
                <h3>Domains</h3>
                <p>{{ domainCountLabel }}</p>
              </div>
              <AppButton label="+ Thêm domain" variant="primary" disabled />
            </div>

            <div class="domain-list">
              <DomainStateRow
                v-for="domain in domainCards"
                :key="domain.id"
                :label="domain.domainName"
                :value="formatDomainStatus(domain.status)"
                :helper="domain.helper"
                :tone="domainTone(domain.status)"
                :actions="[{ key: domain.action, label: domain.action, disabled: true }]"
              />
            </div>
          </AppCard>

          <AppCard class="module-card">
            <div class="section-heading compact">
              <div>
                <h3>Module</h3>
                <p>{{ tenant.moduleCodes.length }} module đang bật</p>
              </div>
            </div>
            <ModuleChips :items="moduleChipItems(tenant.moduleCodes)" :total="moduleTotal" />
          </AppCard>

          <div class="drawer-actions">
            <RouterLink :to="`/clinics/${tenant.id}`">
              <AppButton label="Mở trang chi tiết" variant="secondary" />
            </RouterLink>
            <AppButton
              :label="nextStatus === 'Active' ? 'Kích hoạt phòng khám' : 'Tạm ngưng phòng khám'"
              :variant="nextStatus === 'Active' ? 'primary' : 'danger'"
              @click="$emit('updateStatus', nextStatus)"
            />
          </div>
        </template>
      </aside>
    </div>
  </Teleport>
</template>

<style scoped>
.drawer-backdrop {
  position: fixed;
  inset: 0;
  z-index: 20;
  display: flex;
  justify-content: flex-end;
  background: color-mix(in srgb, var(--color-text-primary) 32%, transparent);
}

.drawer {
  width: min(560px, 100vw);
  min-height: 100vh;
  display: grid;
  align-content: start;
  gap: var(--space-4);
  overflow-y: auto;
  padding: var(--space-6);
  background: var(--color-surface-elevated);
  box-shadow: var(--shadow-elevation-3);
}

.drawer-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: var(--space-4);
  border-bottom: 1px solid var(--color-border-subtle);
  margin: calc(var(--space-6) * -1) calc(var(--space-6) * -1) 0;
  padding: var(--space-6);
}

.drawer-title {
  display: flex;
  align-items: center;
  gap: var(--space-3);
  min-width: 0;
}

.tenant-avatar {
  width: 48px;
  height: 48px;
  display: grid;
  place-items: center;
  flex: 0 0 auto;
  border-radius: 14px;
  background: color-mix(in srgb, var(--color-brand-primary) 10%, var(--color-surface-elevated));
  color: var(--color-brand-primary);
  font-weight: 900;
}

.drawer-header p,
.drawer-header h2,
h3,
dl {
  margin: 0;
}

.drawer-header p {
  margin-top: var(--space-1);
  color: var(--color-text-secondary);
  font-size: 12px;
  font-weight: 600;
  overflow-wrap: anywhere;
}

.drawer-header h2 {
  color: var(--color-text-primary);
  font-size: 18px;
  line-height: 1.25;
  overflow-wrap: anywhere;
}

.close-button {
  width: 36px;
  height: 36px;
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-input);
  background: var(--color-surface-elevated);
  color: var(--color-text-secondary);
  cursor: pointer;
  font-weight: 800;
}

.drawer-status,
.module-list,
.drawer-actions {
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-2);
}

.summary-card {
  box-shadow: none;
}

.detail-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: var(--space-4);
}

dt {
  color: var(--color-text-muted);
  font-size: 12px;
  font-weight: 800;
}

dd {
  margin: var(--space-1) 0 0;
  color: var(--color-text-primary);
  overflow-wrap: anywhere;
  font-weight: 700;
}

.domain-section,
.module-card {
  box-shadow: none;
}

.section-heading {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: var(--space-3);
}

.section-heading.compact {
  margin-bottom: var(--space-3);
}

.section-heading h3 {
  color: var(--color-text-primary);
  font-size: 18px;
}

.section-heading p {
  margin: var(--space-1) 0 0;
  color: var(--color-text-secondary);
  font-size: 13px;
}

.domain-list {
  display: grid;
  gap: var(--space-3);
  margin: var(--space-4) 0 0;
}

.drawer-state {
  border: 1px dashed var(--color-border-subtle);
  border-radius: var(--radius-card);
  padding: 32px;
  color: var(--color-text-secondary);
  text-align: center;
}

.drawer-actions a {
  text-decoration: none;
}

@media (max-width: 560px) {
  .drawer {
    padding: var(--space-5);
  }

  .drawer-header {
    margin: calc(var(--space-5) * -1) calc(var(--space-5) * -1) 0;
    padding: var(--space-5);
  }

  .detail-grid,
  .section-heading,
  .drawer-actions {
    grid-template-columns: 1fr;
    flex-direction: column;
  }
}
</style>
