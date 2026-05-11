<script setup lang="ts">
// Layout dùng chung cho Owner Admin shell: sidebar, topbar và slot route content.
// Topbar lấy title/subtitle từ `route.meta` để mỗi route tự khai báo trong router/index.ts.
// Bọc RouterView bằng <Transition fade + slide-up nhẹ> để chuyển trang không bị giật.
//
// Responsive:
//  - Desktop (>= 1024px): sidebar fixed 248px bám trái, content fill phần còn lại.
//  - Mobile/Tablet (< 1024px): sidebar collapse thành drawer overlay.
//    - Hamburger ở topbar mở drawer (emit `toggle-sidebar`).
//    - Drawer đóng khi: click backdrop, nhấn Escape, click nav item enabled (sidebar emit `navigate`),
//      hoặc khi route đổi (watch route.fullPath để cover trường hợp navigate ngoài sidebar).
//    - Khi drawer mở, body scroll bị khoá để không cuộn nền sau lưng.
import { computed, onBeforeUnmount, ref, watch } from "vue";
import { useRoute } from "vue-router";
import AdminSidebar from "../components/AdminSidebar.vue";
import AdminTopbar from "../components/AdminTopbar.vue";
import OwnerCommandPalette from "../components/OwnerCommandPalette.vue";

const route = useRoute();

const topbarTitle = computed(() => route.meta?.title ?? "Quản lý phòng khám");
const topbarSubtitle = computed(
  () => route.meta?.subtitle ?? "Điều phối toàn bộ phòng khám và trạng thái vận hành nền tảng."
);
const showCreateAction = computed(() => route.meta?.showCreateAction !== false);
const showSearch = computed(() => route.meta?.showSearch !== false);

// State drawer chỉ có nghĩa ở mobile/tablet. Trên desktop, sidebar luôn render qua grid 248px,
// drawer state không ảnh hưởng. Khi resize từ mobile lên desktop, đóng drawer để khỏi cấn.
const sidebarOpen = ref(false);
const commandPaletteOpen = ref(false);

function openSidebar() {
  sidebarOpen.value = true;
}

function closeSidebar() {
  sidebarOpen.value = false;
}

function toggleSidebar() {
  sidebarOpen.value = !sidebarOpen.value;
}

function openCommandPalette() {
  commandPaletteOpen.value = true;
}

function closeCommandPalette() {
  commandPaletteOpen.value = false;
}

// Đóng drawer khi route đổi để mobile không bị che content sau navigation.
watch(
  () => route.fullPath,
  () => {
    if (sidebarOpen.value) {
      closeSidebar();
    }
  }
);

// Khoá body scroll khi drawer mở để tránh scroll nền dưới backdrop.
watch(sidebarOpen, (open) => {
  if (typeof document === "undefined") {
    return;
  }
  document.body.style.overflow = open ? "hidden" : "";
});

// Bắt phím Escape global để đóng drawer kể cả khi focus không ở sidebar.
function handleKeydown(event: KeyboardEvent) {
  if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === "k") {
    event.preventDefault();
    openCommandPalette();
    return;
  }

  if (event.key === "Escape" && sidebarOpen.value) {
    closeSidebar();
  }
}

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
  <div class="owner-layout" :class="{ 'owner-layout--drawer-open': sidebarOpen }">
    <!--
      Sidebar wrapper:
       - Desktop: hiển thị inline qua grid column trái.
       - Mobile/tablet: position fixed, slide-in từ trái khi sidebarOpen=true.
       Sidebar emit `navigate` khi user click nav item enabled -> đóng drawer.
    -->
    <div class="sidebar-slot" :class="{ 'sidebar-slot--open': sidebarOpen }">
      <AdminSidebar @navigate="closeSidebar" />
    </div>

    <!--
      Backdrop chỉ hiện ở mobile khi drawer mở. Click vào backdrop sẽ đóng drawer.
      role="presentation" để screen reader bỏ qua, focus stay trên drawer content.
    -->
    <div
      v-if="sidebarOpen"
      class="sidebar-backdrop"
      role="presentation"
      aria-hidden="true"
      @click="closeSidebar"
    ></div>

    <div class="layout-main">
      <AdminTopbar
        :title="topbarTitle"
        :subtitle="topbarSubtitle"
        :show-create-action="showCreateAction"
        :show-search="showSearch"
        :sidebar-open="sidebarOpen"
        @toggle-sidebar="toggleSidebar"
        @open-command-palette="openCommandPalette"
      />
      <main class="layout-content">
        <!--
          Transition out-in tránh 2 page tồn tại đồng thời, mode appear để mount đầu cũng có hiệu ứng.
          key theo route.fullPath để đảm bảo nested route /clinics -> /clinics/:tenantId vẫn re-trigger transition.
        -->
        <RouterView v-slot="{ Component, route: matchedRoute }">
          <Transition name="page" mode="out-in" appear>
            <component :is="Component" :key="matchedRoute.fullPath" />
          </Transition>
        </RouterView>
      </main>
    </div>

    <OwnerCommandPalette :open="commandPaletteOpen" @close="closeCommandPalette" />
  </div>
</template>

<style scoped>
.owner-layout {
  box-sizing: border-box;
  min-height: 100vh;
  max-width: 100vw;
  display: grid;
  grid-template-columns: 256px minmax(0, 1fr);
  overflow-x: hidden;
  background: var(--color-surface-muted);
  color: var(--color-text-primary);
}

.owner-layout *,
.owner-layout *::before,
.owner-layout *::after {
  box-sizing: border-box;
}

.sidebar-slot {
  /* Desktop default: chiếm cột grid 248px, không cần fixed. */
  min-width: 0;
}

.layout-main {
  min-width: 0;
  max-width: 100%;
  overflow-x: hidden;
}

.layout-content {
  min-width: 0;
  padding: var(--space-6) var(--space-6) var(--space-10);
  overflow-x: hidden;
}

/* Hiệu ứng chuyển trang: fade + slide-up nhẹ, dùng cubic-bezier chuẩn Material để cảm giác mượt. */
.page-enter-active,
.page-leave-active {
  transition:
    opacity var(--motion-duration-md) var(--motion-ease-standard),
    transform var(--motion-duration-md) var(--motion-ease-standard);
}

.page-enter-from {
  opacity: 0;
  transform: translateY(6px);
}

.page-leave-to {
  opacity: 0;
  transform: translateY(-4px);
}

/* Backdrop overlay khi drawer mở trên mobile. z-index thấp hơn drawer (60 vs 70). */
.sidebar-backdrop {
  position: fixed;
  inset: 0;
  z-index: 60;
  background: color-mix(in srgb, var(--color-text-primary) 55%, transparent);
  animation: backdrop-fade-in var(--motion-duration-sm) var(--motion-ease-standard);
}

@keyframes backdrop-fade-in {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}

/* Tablet/Mobile: layout 1 cột, sidebar chuyển sang drawer overlay. */
@media (max-width: 1023px) {
  .owner-layout {
    grid-template-columns: minmax(0, 1fr);
  }

  .layout-content {
    padding: var(--space-5);
  }

  /* Sidebar wrapper fixed trái, slide-in bằng transform. width 280px để vẫn có chỗ cho badge "Sắp ra mắt". */
  .sidebar-slot {
    position: fixed;
    top: 0;
    left: 0;
    bottom: 0;
    z-index: 70;
    width: min(280px, 82vw);
    transform: translateX(-100%);
    transition: transform var(--motion-duration-md) var(--motion-ease-standard);
    overflow-y: auto;
  }

  .sidebar-slot--open {
    transform: translateX(0);
  }
}

@media (max-width: 640px) {
  .layout-content {
    padding: var(--space-4);
  }
}

/* Tôn trọng prefers-reduced-motion: tắt slide animation cho user nhạy cảm motion. */
@media (prefers-reduced-motion: reduce) {
  .sidebar-slot {
    transition: none;
  }

  .sidebar-backdrop {
    animation: none;
  }

  .page-enter-active,
  .page-leave-active {
    transition: opacity 120ms ease;
  }

  .page-enter-from,
  .page-leave-to {
    transform: none;
  }
}
</style>
