<script setup lang="ts">
// Hiển thị state 409 khi backend trả conflict slug/domain/email.
// Component giữ nguyên dữ liệu form ở wizard cha, chỉ chịu trách nhiệm trình bày lỗi và liệt kê field bị trùng.
import type { ApiConflictError, TenantConflictField } from "@clinic-saas/shared-types";
import { AppCard } from "@clinic-saas/ui";

defineProps<{
  /** Payload conflict do API client chuẩn hóa khi gặp HTTP 409. */
  conflict: ApiConflictError;
}>();

// Map sang tiếng Việt để Owner Admin nhận diện nhanh field nào trùng.
const fieldLabel: Record<TenantConflictField, string> = {
  slug: "Slug phòng khám",
  defaultDomainName: "Tên miền mặc định",
  contactEmail: "Email chủ sở hữu"
};
</script>

<template>
  <AppCard tone="danger">
    <div class="conflict-state" role="alert" aria-live="polite">
      <div class="conflict-head">
        <strong>Trùng slug hoặc tên miền</strong>
        <span class="conflict-tag">HTTP 409</span>
      </div>
      <p>{{ conflict.message }}</p>
      <ul v-if="conflict.fields.length > 0">
        <li v-for="field in conflict.fields" :key="field">
          {{ fieldLabel[field] ?? field }}
        </li>
      </ul>
      <!-- Khi backend trả ProblemDetails RFC 9457 không kèm `fields`, adapter sẽ để mảng rỗng;
           hiển thị copy generic để Owner Admin biết rà lại slug/domain/email. -->
      <p v-else class="conflict-hint">
        Dữ liệu bị trùng — vui lòng kiểm tra slug, tên miền hoặc email và thử lại.
      </p>
      <p class="conflict-hint">
        Wizard vẫn giữ nguyên dữ liệu đã nhập. Hãy đổi giá trị bị trùng và gửi lại.
      </p>
    </div>
  </AppCard>
</template>

<style scoped>
.conflict-state {
  display: grid;
  gap: 8px;
  color: #7f1d1d;
}

strong,
p,
ul {
  margin: 0;
}

.conflict-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.conflict-tag {
  display: inline-flex;
  align-items: center;
  border-radius: 999px;
  padding: 4px 10px;
  background: #fee2e2;
  color: #b42318;
  font-size: 12px;
  font-weight: 800;
}

ul {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  padding: 0;
  list-style: none;
}

li {
  border-radius: 999px;
  padding: 5px 10px;
  background: #fee2e2;
  font-size: 12px;
  font-weight: 800;
}

.conflict-hint {
  color: #9a3412;
  font-size: 13px;
}
</style>
