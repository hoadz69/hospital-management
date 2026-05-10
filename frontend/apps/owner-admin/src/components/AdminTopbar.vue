<script setup lang="ts">
// Topbar dùng chung cho mọi route Owner Admin. Title và subtitle nhận qua props
// để mỗi page tự cung cấp ngữ cảnh, tránh hardcode "Tenant Operations" cho cả dashboard.
//
// Responsive: ở viewport < 1024px (drawer mode) hiển thị nút hamburger bên trái title
// để mở/đóng sidebar. State `sidebarOpen` truyền từ layout để hamburger render đúng aria-expanded.
import { AppButton } from "@clinic-saas/ui";

withDefaults(
  defineProps<{
    /** Tiêu đề chính hiển thị ở header, ví dụ "Quản lý phòng khám". */
    title?: string;
    /** Mô tả ngắn dưới tiêu đề, giải thích mục đích trang hiện tại. */
    subtitle?: string;
    /** Cho phép ẩn nhanh search bar khi trang không cần (ví dụ wizard thêm phòng khám). */
    showSearch?: boolean;
    /** Cho phép ẩn nút "Thêm phòng khám" khi đang ở chính trang tạo. */
    showCreateAction?: boolean;
    /** Trạng thái drawer sidebar - dùng để toggle aria-expanded của hamburger. */
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

// Emit toggle để layout cha thay đổi state sidebar. Hamburger không tự quản lý state.
const emit = defineEmits<{
  (event: "toggle-sidebar"): void;
}>();

function handleToggle() {
  emit("toggle-sidebar");
}
</script>

<template>
  <header class="topbar">
    <!--
      Hamburger chỉ visible ở mobile/tablet (CSS display none ở desktop).
      aria-label tiếng Việt theo CLAUDE.md, aria-expanded báo trạng thái drawer cho screen reader.
    -->
    <button
      type="button"
      class="hamburger"
      aria-label="Mở menu điều hướng"
      :aria-expanded="sidebarOpen"
      @click="handleToggle"
    >
      <span class="hamburger-bar" aria-hidden="true"></span>
      <span class="hamburger-bar" aria-hidden="true"></span>
      <span class="hamburger-bar" aria-hidden="true"></span>
    </button>

    <div class="topbar-heading">
      <h1>{{ title }}</h1>
      <p v-if="subtitle">{{ subtitle }}</p>
    </div>

    <div class="topbar-actions">
      <label v-if="showSearch" class="search">
        <span class="visually-hidden">Tìm phòng khám</span>
        <input type="search" placeholder="Tìm phòng khám theo tên, slug hoặc tên miền" />
      </label>
      <RouterLink v-if="showCreateAction" to="/clinics/create" class="create-link">
        <AppButton label="Thêm phòng khám" />
      </RouterLink>
    </div>
  </header>
</template>

<style scoped>
.topbar {
  min-height: 84px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 20px;
  border-bottom: 1px solid #d9e2ec;
  padding: 16px 28px;
  background: #ffffff;
}

/* Hamburger ẩn mặc định ở desktop, chỉ hiện ở drawer mode. */
.hamburger {
  display: none;
  width: 40px;
  height: 40px;
  flex-shrink: 0;
  align-items: center;
  justify-content: center;
  flex-direction: column;
  gap: 4px;
  border: 1px solid #d9e2ec;
  border-radius: 8px;
  padding: 0;
  background: #ffffff;
  color: #102a43;
  cursor: pointer;
}

.hamburger:hover {
  background: #f6f8fb;
}

.hamburger:focus-visible {
  outline: 2px solid #0e7c86;
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
  min-width: 0;
}

.topbar-heading h1,
.topbar-heading p {
  margin: 0;
}

.topbar-heading h1 {
  color: #102a43;
  font-size: 22px;
}

.topbar-heading p {
  margin-top: 4px;
  color: #627d98;
  font-size: 14px;
}

.topbar-actions {
  display: flex;
  align-items: center;
  gap: 12px;
}

.search {
  width: min(320px, 34vw);
}

.search input {
  width: 100%;
  min-height: 42px;
  border: 1px solid #d9e2ec;
  border-radius: 8px;
  padding: 0 12px;
  color: #102a43;
}

.create-link {
  text-decoration: none;
}

.visually-hidden {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border: 0;
}

/* Drawer mode: hamburger hiện, search co lại, layout giữ ngang để hamburger sát title. */
@media (max-width: 1023px) {
  .hamburger {
    display: inline-flex;
  }

  .topbar {
    padding: 14px 18px;
    gap: 12px;
  }

  .topbar-heading h1 {
    font-size: 18px;
  }

  .topbar-heading p {
    /* Subtitle ẩn ở mobile để topbar không cao quá; title vẫn cho ngữ cảnh. */
    display: none;
  }

  .search {
    width: min(220px, 30vw);
  }
}

/* Mobile hẹp: stack actions xuống dưới để search và nút tạo tenant không tràn. */
@media (max-width: 640px) {
  .topbar {
    align-items: stretch;
    flex-wrap: wrap;
  }

  .topbar-actions {
    flex-basis: 100%;
    justify-content: flex-end;
  }

  .search {
    width: 100%;
  }
}
</style>
