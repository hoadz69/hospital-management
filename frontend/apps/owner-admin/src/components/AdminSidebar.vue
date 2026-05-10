<script setup lang="ts">
// Danh sách menu trái cho Owner Super Admin theo Figma frame V2 - Owner Admin Tenant Operations.
// Phase 3 chỉ Dashboard và Phòng khám có route thật (RouterLink); 6 mục còn lại render bằng
// <button disabled> để click không navigate, không nhận router-link-active style và không gây
// nhầm lẫn semantic. Khi mở rộng module ở phase sau chỉ cần đổi enabled=true + cập nhật `to`.
//
// Khi sidebar render trong drawer mode (mobile/tablet), nhận event close để layout đóng drawer
// sau khi user click vào nav item enabled - tránh che content sau navigation.
type NavItem = {
  label: string;
  to: string;
  enabled: boolean;
};

const navItems: NavItem[] = [
  { label: "Tổng quan", to: "/dashboard", enabled: true },
  { label: "Phòng khám", to: "/clinics", enabled: true },
  { label: "Tên miền", to: "", enabled: false },
  { label: "Mẫu giao diện", to: "", enabled: false },
  { label: "Thanh toán", to: "", enabled: false },
  { label: "Giám sát", to: "", enabled: false },
  { label: "Hỗ trợ", to: "", enabled: false },
  { label: "Cài đặt", to: "", enabled: false }
];

// Emit `navigate` để layout cha đóng drawer khi user chọn route mới. Disabled item không emit.
const emit = defineEmits<{
  (event: "navigate"): void;
}>();

function handleNavigate() {
  emit("navigate");
}
</script>

<template>
  <aside class="sidebar">
    <RouterLink class="brand" to="/dashboard" aria-label="ClinicOS Owner Admin">
      <span class="brand-mark">CO</span>
      <span>
        <strong>ClinicOS Owner</strong>
        <small>Owner Super Admin</small>
      </span>
    </RouterLink>

    <span class="role-badge">Owner Super Admin</span>

    <nav class="nav-list" aria-label="Điều hướng Owner Admin">
      <template v-for="item in navItems" :key="item.label">
        <!-- Item enabled: dùng RouterLink để Vue Router xử lý active state.
             Click sẽ emit navigate để layout cha tự đóng drawer trên mobile. -->
        <RouterLink v-if="item.enabled" class="nav-item" :to="item.to" @click="handleNavigate">
          <span class="nav-icon" aria-hidden="true"></span>
          <span class="nav-label">{{ item.label }}</span>
        </RouterLink>

        <!-- Item disabled: dùng button không navigate, tabindex=-1 tránh focus, title hint placeholder.
             Hiển thị badge "Sắp ra mắt" để owner thấy ngay không cần hover. -->
        <button
          v-else
          type="button"
          class="nav-item disabled"
          disabled
          tabindex="-1"
          aria-disabled="true"
          title="Sắp ra mắt - Phase 4+"
        >
          <span class="nav-icon" aria-hidden="true"></span>
          <span class="nav-label">{{ item.label }}</span>
          <span class="soon-badge" aria-hidden="true">Sắp ra mắt</span>
        </button>
      </template>
    </nav>

    <div class="sidebar-footer">
      <span>Phiên bản</span>
      <strong>Phase 3 - Tenant Slice</strong>
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
  gap: 18px;
  padding: 22px;
  background: #102a43;
  color: #d9e6f2;
}

.brand {
  display: flex;
  align-items: center;
  gap: 12px;
  color: #ffffff;
  text-decoration: none;
}

.brand-mark {
  width: 40px;
  height: 40px;
  display: inline-grid;
  place-items: center;
  border-radius: 8px;
  background: #d8f3f1;
  color: #075e66;
  font-weight: 800;
}

.brand strong,
.brand small {
  display: block;
}

.brand small {
  margin-top: 2px;
  color: #9fb3c8;
  font-size: 12px;
}

.role-badge {
  align-self: flex-start;
  border-radius: 999px;
  padding: 6px 12px;
  background: rgba(216, 243, 241, 0.16);
  color: #d8f3f1;
  font-size: 12px;
  font-weight: 800;
  letter-spacing: 0.04em;
  text-transform: uppercase;
}

.nav-list {
  display: grid;
  gap: 6px;
}

/* Style chung cho cả RouterLink (a) và button disabled để đồng nhất hierarchy. */
.nav-item {
  display: flex;
  align-items: center;
  gap: 10px;
  width: 100%;
  min-height: 40px;
  border: none;
  border-radius: 8px;
  padding: 0 12px;
  background: transparent;
  color: #d9e6f2;
  font: inherit;
  font-weight: 700;
  text-align: left;
  text-decoration: none;
  cursor: pointer;
}

.nav-item:hover:not(.disabled):not(:disabled) {
  background: rgba(216, 243, 241, 0.08);
}

.nav-item.router-link-active {
  background: #d8f3f1;
  color: #075e66;
}

/* Item disabled không bao giờ nhận active style vì không phải RouterLink. */
.nav-item.disabled,
.nav-item:disabled {
  color: #9fb3c8;
  cursor: not-allowed;
  opacity: 0.7;
  background: transparent;
}

.nav-icon {
  width: 8px;
  height: 8px;
  border-radius: 999px;
  background: currentColor;
  flex-shrink: 0;
}

/* Label chiếm toàn bộ không gian giữa icon và badge để badge dính sát mép phải. */
.nav-label {
  flex: 1 1 auto;
  min-width: 0;
}

/* Badge "Sắp ra mắt" hiển thị ngay trên nav item disabled, không cần user hover xem title. */
.soon-badge {
  flex-shrink: 0;
  border-radius: 999px;
  padding: 2px 8px;
  background: rgba(255, 255, 255, 0.1);
  color: #9fb3c8;
  font-size: 10px;
  font-weight: 700;
  letter-spacing: 0.02em;
  text-transform: uppercase;
}

.sidebar-footer {
  margin-top: auto;
  border: 1px solid rgba(217, 230, 242, 0.18);
  border-radius: 8px;
  padding: 14px;
  background: rgba(255, 255, 255, 0.06);
}

.sidebar-footer span,
.sidebar-footer strong {
  display: block;
}

.sidebar-footer span {
  color: #9fb3c8;
  font-size: 12px;
}

.sidebar-footer strong {
  margin-top: 4px;
  color: #ffffff;
  font-size: 14px;
}

/* Trong drawer mode, sidebar không sticky vì đã được layout cha định vị fixed.
   Layout cha (OwnerAdminLayout) thêm class `sidebar--drawer` qua wrapper khi viewport < 1024px. */
@media (max-width: 1023px) {
  .sidebar {
    position: static;
    min-height: 100vh;
    box-shadow: 8px 0 24px rgba(16, 42, 67, 0.32);
  }
}
</style>
