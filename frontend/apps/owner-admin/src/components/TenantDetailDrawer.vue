<script setup lang="ts">
// Drawer hiển thị tenant detail khi Owner Admin chọn row trong TenantTable.
// Component overlay-side, đóng được bằng click backdrop hoặc nút X, hỗ trợ Escape.
import type { TenantDetail, TenantStatus } from "@clinic-saas/shared-types";
import { AppButton, AppCard, StatusPill } from "@clinic-saas/ui";
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

function statusTone(status: TenantStatus) {
  if (status === "Active") {
    return "success";
  }

  if (status === "Suspended" || status === "Archived") {
    return "danger";
  }

  return "warning";
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
          <div>
            <p>Chi tiết phòng khám</p>
            <h2>{{ tenant?.displayName ?? "Đang tải phòng khám..." }}</h2>
          </div>
          <button type="button" class="close-button" aria-label="Đóng drawer" @click="$emit('close')">
            X
          </button>
        </header>

        <div v-if="loading" class="drawer-state">Đang tải hồ sơ phòng khám...</div>

        <template v-else-if="tenant">
          <div class="drawer-status">
            <StatusPill :label="formatTenantStatus(tenant.status)" :tone="statusTone(tenant.status)" />
            <StatusPill :label="formatDomainStatus(tenant.domainStatus)" tone="info" />
          </div>

          <AppCard>
            <dl class="detail-grid">
              <div>
                <dt>Mã phòng khám</dt>
                <dd>{{ tenant.id }}</dd>
              </div>
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

          <AppCard>
            <h3>Tên miền</h3>
            <ul class="domain-list">
              <li v-for="domain in tenant.domains" :key="domain.id">
                <span>{{ domain.domainName }}</span>
                <StatusPill :label="formatDomainStatus(domain.status)" tone="info" />
              </li>
            </ul>
          </AppCard>

          <AppCard>
            <h3>Module</h3>
            <div class="module-list">
              <StatusPill
                v-for="moduleCode in tenant.moduleCodes"
                :key="moduleCode"
                :label="formatModuleCode(moduleCode)"
                tone="neutral"
              />
            </div>
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
  background: rgba(16, 42, 67, 0.28);
}

.drawer {
  width: min(520px, 100vw);
  min-height: 100vh;
  display: grid;
  align-content: start;
  gap: 16px;
  overflow-y: auto;
  padding: 22px;
  background: #ffffff;
  box-shadow: 0 24px 60px rgba(16, 42, 67, 0.18);
}

.drawer-header {
  display: flex;
  align-items: start;
  justify-content: space-between;
  gap: 16px;
}

.drawer-header p,
.drawer-header h2,
h3,
dl {
  margin: 0;
}

.drawer-header p {
  color: #0e7c86;
  font-size: 12px;
  font-weight: 800;
  text-transform: uppercase;
}

.drawer-header h2 {
  margin-top: 4px;
  font-size: 24px;
}

.close-button {
  width: 36px;
  height: 36px;
  border: 1px solid #d9e2ec;
  border-radius: 8px;
  background: #ffffff;
  cursor: pointer;
  font-weight: 800;
}

.drawer-status,
.module-list,
.drawer-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
}

.detail-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 14px;
}

dt {
  color: #627d98;
  font-size: 12px;
  font-weight: 800;
}

dd {
  margin: 4px 0 0;
  color: #102a43;
  overflow-wrap: anywhere;
}

.domain-list {
  display: grid;
  gap: 10px;
  margin: 12px 0 0;
  padding: 0;
  list-style: none;
}

.domain-list li {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.drawer-state {
  border: 1px dashed #d9e2ec;
  border-radius: 8px;
  padding: 32px;
  color: #627d98;
  text-align: center;
}

.drawer-actions a {
  text-decoration: none;
}
</style>
