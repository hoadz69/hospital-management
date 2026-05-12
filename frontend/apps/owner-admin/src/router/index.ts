import { createRouter, createWebHistory } from "vue-router";
import OwnerAdminLayout from "../layouts/OwnerAdminLayout.vue";
import ClinicDetailPage from "../pages/ClinicDetailPage.vue";
import ClinicsPage from "../pages/ClinicsPage.vue";
import CreateClinicPage from "../pages/CreateClinicPage.vue";
import DashboardPage from "../pages/DashboardPage.vue";
import PlanModuleCatalogPage from "../pages/PlanModuleCatalogPage.vue";

// Khai báo meta chứa title/subtitle hiển thị ở topbar và cờ điều khiển hiển thị nút tạo tenant.
// Việc tập trung tại router giúp Layout không phải hardcode mapping route -> tiêu đề.
declare module "vue-router" {
  interface RouteMeta {
    title?: string;
    subtitle?: string;
    showCreateAction?: boolean;
    showSearch?: boolean;
  }
}

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: "/",
      redirect: "/dashboard"
    },
    {
      path: "/",
      component: OwnerAdminLayout,
      children: [
        {
          path: "dashboard",
          component: DashboardPage,
          meta: {
            title: "Tổng quan",
            subtitle: "Theo dõi sức khoẻ vận hành nền tảng và toàn bộ phòng khám."
          }
        },
        {
          path: "clinics",
          component: ClinicsPage,
          meta: {
            title: "Quản lý phòng khám",
            subtitle: "Danh sách phòng khám đang vận hành, trạng thái và tên miền mặc định."
          }
        },
        {
          path: "clinics/create",
          component: CreateClinicPage,
          meta: {
            title: "Thêm phòng khám",
            subtitle: "Wizard 4 bước: thông tin phòng khám, gói & module, tên miền và xác nhận.",
            showCreateAction: false
          }
        },
        {
          path: "plans",
          component: PlanModuleCatalogPage,
          meta: {
            title: "Gói & module",
            subtitle: "Plan catalog, module entitlement và đổi gói tenant bằng mock data A8.",
            showCreateAction: false
          }
        },
        {
          path: "clinics/:tenantId",
          component: ClinicDetailPage,
          props: true,
          meta: {
            title: "Chi tiết phòng khám",
            subtitle: "Hồ sơ chi tiết của phòng khám đang được quản trị viên theo dõi."
          }
        }
      ]
    }
  ]
});
