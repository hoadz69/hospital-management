// Helper formatter chuyển enum giá trị từ backend sang label hiển thị tiếng Việt cho Owner Admin.
// Chú ý: các hàm dưới đây CHỈ tạo lớp trình bày phía UI, KHÔNG đổi giá trị enum gửi/nhận với backend.
// Tên/giá trị backend (Draft, Active, Suspended, Archived, verified, pending, failed, unknown,
// website, booking, catalog, payments, reports, notifications) phải giữ nguyên trong payload API,
// store và filter state. Khi cần render cho người dùng, gọi formatter tương ứng để có chuỗi VN.
import type {
  TenantDomainStatus,
  TenantModuleCode,
  TenantStatus
} from "@clinic-saas/shared-types";

/**
 * Trả về nhãn tiếng Việt cho trạng thái tenant để hiển thị trong bảng, drawer, filter dropdown.
 * @param status Giá trị enum TenantStatus đến từ backend hoặc filter state hiện tại.
 * @returns Chuỗi tiếng Việt tương ứng để Owner Admin đọc dễ; fallback chính status khi unknown.
 */
export function formatTenantStatus(status: TenantStatus): string {
  switch (status) {
    case "Draft":
      return "Bản nháp";
    case "Active":
      return "Đang hoạt động";
    case "Suspended":
      return "Đã tạm ngưng";
    case "Archived":
      return "Đã lưu trữ";
    default:
      return status;
  }
}

/**
 * Trả về nhãn tiếng Việt cho trạng thái domain mặc định gắn với tenant.
 * @param status Giá trị enum TenantDomainStatus do domain-service trả về.
 * @returns Chuỗi tiếng Việt tương ứng cho pill/option; fallback nguyên trị khi không khớp.
 */
export function formatDomainStatus(status: TenantDomainStatus): string {
  switch (status) {
    case "verified":
      return "Đã xác minh";
    case "pending":
      return "Đang chờ xác minh";
    case "failed":
      return "Xác minh thất bại";
    case "unknown":
      return "Chưa rõ";
    default:
      return status;
  }
}

/**
 * Trả về nhãn tiếng Việt cho từng module mã hoá thương mại trong wizard và detail.
 * @param code Giá trị enum TenantModuleCode đại diện cho module bật/tắt cho tenant.
 * @returns Chuỗi tiếng Việt thương mại; fallback chính code khi gặp giá trị mới chưa map.
 */
export function formatModuleCode(code: TenantModuleCode): string {
  switch (code) {
    case "website":
      return "Website";
    case "booking":
      return "Đặt lịch";
    case "catalog":
      return "Danh mục dịch vụ";
    case "payments":
      return "Thanh toán";
    case "reports":
      return "Báo cáo";
    case "notifications":
      return "Thông báo";
    default:
      return code;
  }
}
