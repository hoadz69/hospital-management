<script setup lang="ts">
// Trang `/clinics/create` chứa wizard tạo tenant.
// Trang giữ trách nhiệm:
//  - Submit payload qua tenantClient.createTenant.
//  - Bắt 409 conflict, đẩy vào component ConflictState/CreateTenantWizard để giữ nguyên data form và focus field lỗi.
//  - Điều hướng sang trang detail khi tạo thành công.
import { isApiConflictError } from "@clinic-saas/api-client";
import type { ApiConflictError, TenantCreateRequest } from "@clinic-saas/shared-types";
import { AppButton } from "@clinic-saas/ui";
import { ref } from "vue";
import { useRouter } from "vue-router";
import CreateTenantWizard from "../components/CreateTenantWizard.vue";
import { tenantClient } from "../services/tenantClient";

const router = useRouter();
const submitting = ref(false);
const conflict = ref<ApiConflictError | undefined>();
const error = ref<string | null>(null);

async function createTenant(payload: TenantCreateRequest) {
  submitting.value = true;
  conflict.value = undefined;
  error.value = null;

  try {
    const tenant = await tenantClient.createTenant(payload);
    await router.push(`/clinics/${tenant.id}`);
  } catch (createError) {
    if (isApiConflictError(createError)) {
      conflict.value = createError as ApiConflictError;
      return;
    }

    error.value = createError instanceof Error ? createError.message : "Không tạo được phòng khám.";
  } finally {
    submitting.value = false;
  }
}
</script>

<template>
  <div class="create-page">
    <section class="page-heading">
      <div>
        <p class="eyebrow">Luồng thêm phòng khám</p>
        <h2>Wizard thêm phòng khám mới</h2>
      </div>
      <RouterLink to="/clinics">
        <AppButton label="Quay lại danh sách" variant="secondary" />
      </RouterLink>
    </section>

    <div v-if="error" class="error-state">
      <span>{{ error }}</span>
      <AppButton label="Bỏ qua" variant="secondary" @click="error = null" />
    </div>

    <CreateTenantWizard :conflict="conflict" :submitting="submitting" @submit="createTenant" />
  </div>
</template>

<style scoped>
.create-page {
  display: grid;
  gap: 20px;
}

.page-heading {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
}

.page-heading p,
.page-heading h2 {
  margin: 0;
}

.eyebrow {
  color: #0e7c86;
  font-size: 12px;
  font-weight: 800;
  text-transform: uppercase;
}

.page-heading h2 {
  margin-top: 6px;
  font-size: 28px;
}

a {
  text-decoration: none;
}

.error-state {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  border-radius: 8px;
  padding: 14px 16px;
  background: #fff7ed;
  color: #9a3412;
}

@media (max-width: 640px) {
  .page-heading,
  .error-state {
    align-items: stretch;
    flex-direction: column;
  }
}
</style>
