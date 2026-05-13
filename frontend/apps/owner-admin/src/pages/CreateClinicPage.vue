<script setup lang="ts">
// Trang `/clinics/create` chứa wizard tạo tenant.
// Trang giữ trách nhiệm:
//  - Submit payload qua tenantClient.createTenant.
//  - Bắt 409 conflict, đẩy vào component ConflictState/CreateTenantWizard để giữ nguyên data form và focus field lỗi.
//  - Điều hướng sang trang detail khi tạo thành công.
import { isApiConflictError } from "@clinic-saas/api-client";
import type { ApiConflictError, TenantCreateRequest } from "@clinic-saas/shared-types";
import { AppButton, StatePanel } from "@clinic-saas/ui";
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

    <StatePanel v-if="error" title="Không tạo được phòng khám" :description="error" tone="danger">
      <template #action>
        <AppButton label="Bỏ qua" variant="secondary" @click="error = null" />
      </template>
    </StatePanel>

    <CreateTenantWizard :conflict="conflict" :submitting="submitting" @submit="createTenant" />
  </div>
</template>

<style scoped>
.create-page {
  display: grid;
  gap: var(--space-5);
  min-width: 0;
}

.page-heading {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  border: 1px solid color-mix(in srgb, var(--color-border-subtle) 78%, var(--color-brand-primary));
  border-radius: var(--radius-card);
  padding: var(--space-5);
  background:
    linear-gradient(135deg, color-mix(in srgb, var(--color-status-success) 8%, transparent), transparent 44%),
    var(--color-surface-elevated);
  box-shadow: var(--shadow-elevation-1);
}

.page-heading p,
.page-heading h2 {
  margin: 0;
}

.eyebrow {
  color: var(--color-brand-primary);
  font-size: 12px;
  font-weight: 800;
  text-transform: uppercase;
}

.page-heading h2 {
  max-width: 100%;
  overflow-wrap: anywhere;
  margin-top: var(--space-1);
  color: var(--color-text-primary);
  font-size: 24px;
  line-height: 32px;
}

a {
  text-decoration: none;
}

@media (max-width: 640px) {
  .page-heading {
    align-items: stretch;
    flex-direction: column;
  }

  .page-heading {
    gap: var(--space-3);
  }

  .page-heading h2 {
    font-size: 22px;
    line-height: 30px;
  }

  .page-heading a :deep(.app-button) {
    width: 100%;
  }
}
</style>
