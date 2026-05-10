<script setup lang="ts">
// Wizard 4 bước cho luồng tạo tenant của Owner Admin theo Figma `V2 - Owner Admin Tenant Operations`.
// Bước: 1) Thông tin phòng khám, 2) Plan và module, 3) Domain mặc định, 4) Preview xác nhận.
// Khi nhận `conflict` từ parent (HTTP 409), wizard giữ nguyên form data và auto-focus field lỗi đầu tiên.
import type {
  ApiConflictError,
  TenantConflictField,
  TenantCreateRequest,
  TenantModuleCode,
  TenantPlanCode
} from "@clinic-saas/shared-types";
import { AppButton, AppCard, StatusPill } from "@clinic-saas/ui";
import { computed, nextTick, reactive, ref, watch } from "vue";
import ConflictState from "./ConflictState.vue";
import { formatModuleCode } from "../services/labels";

const props = defineProps<{
  /** Lỗi conflict 409 do parent đẩy xuống để wizard hiển thị banner và highlight field. */
  conflict?: ApiConflictError;
  /** Cờ submitting để khóa nút và tránh submit double. */
  submitting?: boolean;
}>();

const emit = defineEmits<{
  submit: [payload: TenantCreateRequest];
}>();

const steps = ["Phòng khám", "Gói & Module", "Tên miền", "Xác nhận"];
const activeStep = ref(0);

// Module options: dùng formatModuleCode để label hiển thị tiếng Việt, value vẫn là enum backend.
const moduleOptions: { label: string; value: TenantModuleCode }[] = [
  { label: formatModuleCode("website"), value: "website" },
  { label: formatModuleCode("booking"), value: "booking" },
  { label: formatModuleCode("catalog"), value: "catalog" },
  { label: formatModuleCode("payments"), value: "payments" },
  { label: formatModuleCode("reports"), value: "reports" },
  { label: formatModuleCode("notifications"), value: "notifications" }
];

// Form data mặc định để Owner Admin có context demo nhanh; vẫn cho phép sửa toàn bộ.
const form = reactive<TenantCreateRequest>({
  slug: "aurora-dental",
  displayName: "Aurora Dental",
  clinicName: "Aurora Dental Center",
  planCode: "growth",
  specialty: "Dental",
  contactEmail: "ops@aurora-dental.example",
  phoneNumber: "+84 28 0000 1201",
  addressLine: "12 Nguyen Hue, District 1",
  defaultDomainName: "aurora-dental.clinicos.vn",
  moduleCodes: ["website", "booking", "catalog"]
});

// Refs cho input để auto focus khi conflict trỏ tới field cụ thể.
const slugInput = ref<HTMLInputElement | null>(null);
const domainInput = ref<HTMLInputElement | null>(null);
const emailInput = ref<HTMLInputElement | null>(null);

// Map field conflict sang step tương ứng để wizard tự nhảy đến bước có field lỗi.
const fieldToStep: Record<TenantConflictField, number> = {
  slug: 0,
  contactEmail: 0,
  defaultDomainName: 2
};

const conflictFields = computed(() => props.conflict?.fields ?? []);

function fieldHasConflict(field: keyof TenantCreateRequest) {
  return conflictFields.value.includes(field as TenantConflictField);
}

function focusFirstConflictField() {
  const firstField = props.conflict?.fields?.[0];
  if (!firstField) {
    return;
  }

  // Chuyển tới bước chứa field lỗi đầu tiên trước khi focus.
  activeStep.value = fieldToStep[firstField] ?? 0;

  void nextTick(() => {
    if (firstField === "slug") {
      slugInput.value?.focus();
    } else if (firstField === "defaultDomainName") {
      domainInput.value?.focus();
    } else if (firstField === "contactEmail") {
      emailInput.value?.focus();
    }
  });
}

watch(
  () => props.conflict,
  (next) => {
    if (next) {
      focusFirstConflictField();
    }
  }
);

function setPlan(planCode: TenantPlanCode) {
  form.planCode = planCode;

  if (planCode === "starter") {
    form.moduleCodes = ["website"];
  }

  if (planCode === "growth") {
    form.moduleCodes = ["website", "booking", "catalog"];
  }

  if (planCode === "premium") {
    form.moduleCodes = ["website", "booking", "catalog", "payments", "reports"];
  }
}

function toggleModule(moduleCode: TenantModuleCode) {
  if (form.moduleCodes.includes(moduleCode)) {
    form.moduleCodes = form.moduleCodes.filter((item) => item !== moduleCode);
    return;
  }

  form.moduleCodes = [...form.moduleCodes, moduleCode];
}

/**
 * Kiểm tra một bước wizard đã đủ điều kiện để chuyển sang bước kế hay chưa.
 * Vì Vue conditional render unmount input của bước trước, HTML5 `required` trên
 * `<form @submit>` chỉ chặn được khi field thuộc bước cuối còn trong DOM. Nếu user
 * điền step 0, Tiếp tục, rồi quay lại clear field thì khi submit ở step 3 các input
 * step 0/2 đã unmount, required không kích hoạt được. Hàm này đóng vai gate-keeper:
 * - Step 0 (Phòng khám): các field nghiệp vụ bắt buộc phải truthy + trim non-empty.
 * - Step 1 (Gói & Module): phải chọn ít nhất 1 module.
 * - Step 2 (Tên miền): tên miền mặc định bắt buộc + trim non-empty.
 * - Step 3 (Xác nhận): bước review, luôn cho phép (submit có HTML5 required defense-in-depth).
 * @param step Index bước đang đứng (0..steps.length-1).
 * @returns true nếu bước đã đủ dữ liệu để chuyển bước hoặc submit.
 */
function isStepValid(step: number): boolean {
  if (step === 0) {
    return (
      form.displayName.trim() !== "" &&
      form.clinicName.trim() !== "" &&
      form.slug.trim() !== "" &&
      form.specialty.trim() !== "" &&
      form.contactEmail.trim() !== "" &&
      form.phoneNumber.trim() !== "" &&
      form.addressLine.trim() !== ""
    );
  }

  if (step === 1) {
    return form.moduleCodes.length >= 1;
  }

  if (step === 2) {
    return form.defaultDomainName.trim() !== "";
  }

  // Step 3 (xác nhận) là bước review; submit cuối có HTML5 required ở các field còn mounted.
  return true;
}

function next() {
  // Gate validation trước khi tăng step để chặn user nhảy step với field rỗng.
  if (!isStepValid(activeStep.value)) {
    return;
  }
  activeStep.value = Math.min(activeStep.value + 1, steps.length - 1);
}

function back() {
  activeStep.value = Math.max(activeStep.value - 1, 0);
}

function submit() {
  emit("submit", { ...form, moduleCodes: [...form.moduleCodes] });
}
</script>

<template>
  <div class="wizard-grid">
    <AppCard>
      <div class="stepper">
        <button
          v-for="(step, index) in steps"
          :key="step"
          type="button"
          :class="{ active: activeStep === index, done: activeStep > index }"
          @click="activeStep = index"
        >
          <span>{{ index + 1 }}</span>
          {{ step }}
        </button>
      </div>

      <ConflictState v-if="conflict" class="conflict" :conflict="conflict" />

      <form class="wizard-form" @submit.prevent="submit">
        <section v-if="activeStep === 0" class="form-section">
          <label :class="{ conflict: fieldHasConflict('displayName') }">
            <span>Tên hiển thị</span>
            <input v-model="form.displayName" required />
          </label>
          <label>
            <span>Tên pháp lý phòng khám</span>
            <input v-model="form.clinicName" required />
          </label>
          <label :class="{ conflict: fieldHasConflict('slug') }">
            <span>Slug phòng khám</span>
            <input ref="slugInput" v-model="form.slug" required />
          </label>
          <label>
            <span>Chuyên khoa</span>
            <input v-model="form.specialty" required />
          </label>
          <label :class="{ conflict: fieldHasConflict('contactEmail') }">
            <span>Email chủ sở hữu</span>
            <input ref="emailInput" v-model="form.contactEmail" type="email" required />
          </label>
          <label>
            <span>Số điện thoại</span>
            <input v-model="form.phoneNumber" required />
          </label>
          <label class="wide">
            <span>Địa chỉ</span>
            <input v-model="form.addressLine" required />
          </label>
        </section>

        <section v-else-if="activeStep === 1" class="form-section">
          <div class="plan-options">
            <button type="button" :class="{ selected: form.planCode === 'starter' }" @click="setPlan('starter')">
              <strong>Starter</strong>
              <span>Website giới thiệu phòng khám</span>
            </button>
            <button type="button" :class="{ selected: form.planCode === 'growth' }" @click="setPlan('growth')">
              <strong>Growth</strong>
              <span>Đặt lịch và danh mục dịch vụ</span>
            </button>
            <button type="button" :class="{ selected: form.planCode === 'premium' }" @click="setPlan('premium')">
              <strong>Premium</strong>
              <span>Thanh toán và báo cáo</span>
            </button>
          </div>

          <div class="module-options">
            <button
              v-for="moduleItem in moduleOptions"
              :key="moduleItem.value"
              type="button"
              :class="{ selected: form.moduleCodes.includes(moduleItem.value) }"
              @click="toggleModule(moduleItem.value)"
            >
              {{ moduleItem.label }}
            </button>
          </div>
        </section>

        <section v-else-if="activeStep === 2" class="form-section">
          <label :class="{ conflict: fieldHasConflict('defaultDomainName') }">
            <span>Tên miền mặc định</span>
            <input ref="domainInput" v-model="form.defaultDomainName" required />
          </label>
          <AppCard tone="muted" class="wide">
            <p class="note-title">DNS và SSL sẽ xử lý ở backend</p>
            <p class="note-copy">
              Việc xác minh tên miền, SSL và publish gateway do backend domain-service đảm nhiệm.
              Frontend Phase 3 chỉ gửi tên miền mặc định và hiển thị trạng thái do backend trả về.
            </p>
          </AppCard>
        </section>

        <section v-else class="preview-section">
          <div>
            <span>Phòng khám</span>
            <strong>{{ form.displayName }}</strong>
          </div>
          <div>
            <span>Gói</span>
            <strong>{{ form.planCode }}</strong>
          </div>
          <div>
            <span>Tên miền</span>
            <strong>{{ form.defaultDomainName }}</strong>
          </div>
          <div>
            <span>Module</span>
            <div class="preview-modules">
              <StatusPill
                v-for="moduleCode in form.moduleCodes"
                :key="moduleCode"
                :label="formatModuleCode(moduleCode)"
                tone="neutral"
              />
            </div>
          </div>
        </section>

        <div class="wizard-actions">
          <AppButton label="Quay lại" variant="secondary" :disabled="activeStep === 0 || submitting" @click="back" />
          <AppButton
            v-if="activeStep < steps.length - 1"
            label="Tiếp tục"
            variant="primary"
            :disabled="!isStepValid(activeStep) || submitting"
            @click="next"
          />
          <AppButton
            v-else
            label="Tạo phòng khám"
            type="submit"
            :loading="submitting"
            :disabled="submitting"
          />
        </div>
      </form>
    </AppCard>

    <AppCard class="side-preview">
      <p class="preview-eyebrow">Xem trước thiết lập</p>
      <h2>{{ form.displayName }}</h2>
      <p>Phòng khám {{ form.specialty }} đang đăng ký gói {{ form.planCode }}.</p>
      <div class="preview-domain">{{ form.defaultDomainName }}</div>
      <div class="preview-modules">
        <StatusPill
          v-for="moduleCode in form.moduleCodes"
          :key="moduleCode"
          :label="formatModuleCode(moduleCode)"
          tone="info"
        />
      </div>
    </AppCard>
  </div>
</template>

<style scoped>
.wizard-grid {
  display: grid;
  grid-template-columns: minmax(0, 1fr) 340px;
  gap: 20px;
}

.stepper {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 8px;
}

.stepper button {
  min-height: 42px;
  border: 1px solid #d9e2ec;
  border-radius: 8px;
  background: #ffffff;
  color: #627d98;
  cursor: pointer;
  font-weight: 800;
}

.stepper button.active,
.stepper button.done {
  border-color: #0e7c86;
  background: #d8f3f1;
  color: #075e66;
}

.stepper span {
  margin-right: 6px;
}

.conflict {
  margin-top: 16px;
}

.wizard-form {
  display: grid;
  gap: 18px;
  margin-top: 20px;
}

.form-section {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 14px;
}

label {
  display: grid;
  gap: 6px;
}

label span {
  color: #627d98;
  font-size: 12px;
  font-weight: 800;
}

input {
  width: 100%;
  min-height: 42px;
  border: 1px solid #d9e2ec;
  border-radius: 8px;
  padding: 0 12px;
  color: #102a43;
}

input:focus {
  outline: 2px solid #0e7c86;
  outline-offset: 2px;
}

label.conflict input {
  border-color: #b42318;
  background: #fff7f7;
}

.wide {
  grid-column: 1 / -1;
}

.plan-options,
.module-options {
  grid-column: 1 / -1;
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 12px;
}

.plan-options button,
.module-options button {
  min-height: 74px;
  border: 1px solid #d9e2ec;
  border-radius: 8px;
  padding: 12px;
  background: #ffffff;
  color: #102a43;
  cursor: pointer;
  text-align: left;
}

.module-options button {
  min-height: 44px;
  text-align: center;
  font-weight: 800;
}

.plan-options button.selected,
.module-options button.selected {
  border-color: #0e7c86;
  background: #d8f3f1;
}

.plan-options strong,
.plan-options span {
  display: block;
}

.plan-options span {
  margin-top: 6px;
  color: #627d98;
  font-size: 13px;
}

.note-title,
.note-copy,
.preview-section span,
.preview-eyebrow {
  margin: 0;
}

.note-title {
  color: #102a43;
  font-weight: 800;
}

.note-copy {
  margin-top: 6px;
  color: #627d98;
}

.preview-section {
  display: grid;
  gap: 14px;
}

.preview-section span,
.preview-eyebrow {
  color: #627d98;
  font-size: 12px;
  font-weight: 800;
  text-transform: uppercase;
}

.preview-section strong {
  display: block;
  margin-top: 4px;
  overflow-wrap: anywhere;
  color: #102a43;
}

.preview-modules {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-top: 10px;
}

.wizard-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
}

.side-preview {
  align-self: start;
}

.side-preview h2,
.side-preview p {
  margin: 0;
}

.side-preview h2 {
  margin-top: 8px;
}

.side-preview p {
  margin-top: 10px;
  color: #627d98;
}

.preview-domain {
  margin-top: 18px;
  border-radius: 8px;
  padding: 12px;
  background: #f0fdfa;
  color: #075e66;
  overflow-wrap: anywhere;
  font-weight: 800;
}

@media (max-width: 1080px) {
  .wizard-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 720px) {
  .stepper,
  .form-section,
  .plan-options,
  .module-options {
    grid-template-columns: 1fr;
  }

  .wizard-actions {
    flex-direction: column;
  }
}
</style>
