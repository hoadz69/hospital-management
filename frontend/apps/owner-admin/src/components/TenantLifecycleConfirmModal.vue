<script setup lang="ts">
import type { TenantStatus } from "@clinic-saas/shared-types";
import { AppButton, StatePanel, StatusPill, useFocusTrap } from "@clinic-saas/ui";
import { computed, nextTick, onBeforeUnmount, ref, watch } from "vue";

type TenantLifecycleAction = "activate" | "suspend" | "archive" | "restore";
type LifecycleModalState = "idle" | "loading" | "success" | "error";

const props = withDefaults(
  defineProps<{
    open: boolean;
    action: TenantLifecycleAction;
    tenantName: string;
    tenantSlug: string;
    currentStatus: TenantStatus;
    targetStatus: TenantStatus;
    state?: LifecycleModalState;
    message?: string;
  }>(),
  {
    state: "idle",
    message: undefined
  }
);

const emit = defineEmits<{
  close: [];
  confirm: [action: TenantLifecycleAction, reason: string];
}>();

const modalRef = ref<HTMLElement | null>(null);
const reason = ref("");

const focusTrap = useFocusTrap(modalRef, {
  onEscape: () => emit("close")
});

const actionConfig = computed(() => {
  if (props.action === "activate") {
    return {
      tone: "success",
      icon: "AC",
      title: "Kích hoạt tenant?",
      description: "Tenant draft sẽ chuyển sang Active qua Tenant API. DNS/SSL vẫn giữ trạng thái hiện tại.",
      confirmLabel: "Xác nhận kích hoạt",
      reasonLabel: "",
      reasonPlaceholder: "",
      warning: "",
      preview: "Tenant chuyển Active và Owner Admin có thể tiếp tục vận hành cấu hình.",
      effects: ["Tenant status thành Active", "Clinic admin được phép đăng nhập", "Public site dùng domain state hiện có", "Audit reason chỉ hiển thị trong UI hiện tại"]
    };
  }

  if (props.action === "archive") {
    return {
      tone: "danger",
      icon: "AR",
      title: "Lưu trữ tenant?",
      description:
        "Hành động này gửi cập nhật status lưu trữ qua Tenant API. Dữ liệu vận hành sâu hơn phụ thuộc policy backend hiện có.",
      confirmLabel: "Xác nhận lưu trữ",
      reasonLabel: "Lý do lưu trữ *",
      reasonPlaceholder: "Vd: Tenant ngừng hợp đồng hoặc cần khóa dữ liệu...",
      warning: "Archive là hành động rủi ro cao: public site bị dừng, admin tenant bị khóa, và domain release chỉ nên làm khi đã có quyết định vận hành rõ ràng.",
      preview: "Tenant sẽ chuyển sang trạng thái lưu trữ theo Tenant API.",
      effects: [
        "Tenant status thành Archived",
        "Clinic admin dùng trạng thái mới theo policy backend",
        "Domain state giữ theo dữ liệu hiện có",
        "Audit reason chỉ hiển thị trong UI hiện tại"
      ]
    };
  }

  if (props.action === "restore") {
    return {
      tone: "success",
      icon: "RE",
      title: "Khôi phục tenant?",
      description:
        "Reactivate tenant từ trạng thái tạm ngừng hoặc lưu trữ. Public site sẽ resume sau khi DNS check hoàn tất.",
      confirmLabel: "Xác nhận khôi phục",
      reasonLabel: "",
      reasonPlaceholder: "",
      warning: "",
      preview: "Tenant chuyển Active trở lại theo Tenant API.",
      effects: [
        "Tenant status thành Active",
        "Clinic admin dùng trạng thái mới theo policy backend",
        "Domain state giữ theo dữ liệu hiện có",
        "Audit reason chỉ hiển thị trong UI hiện tại"
      ]
    };
  }

  return {
    tone: "warning",
    icon: "SP",
    title: "Tạm ngừng tenant?",
    description:
      "Public site sẽ chuyển sang fallback page. Clinic admin không truy cập được. Dữ liệu giữ nguyên.",
    confirmLabel: "Xác nhận tạm ngừng",
    reasonLabel: "Lý do tạm ngừng *",
    reasonPlaceholder: "Vd: Tenant không thanh toán plan...",
    warning: "Suspend sẽ chặn clinic admin và đưa public site về fallback page. Chỉ dùng khi có lý do vận hành rõ ràng.",
    preview: "Tenant chuyển Suspended theo Tenant API.",
    effects: [
      "Tenant status thành Suspended",
      "Clinic admin dùng trạng thái mới theo policy backend",
      "Public site dùng trạng thái tenant mới khi backend áp dụng",
      "Audit reason chỉ hiển thị trong UI hiện tại"
    ]
  };
});

const isBusy = computed(() => props.state === "loading");
const requiresReason = computed(() => props.action === "suspend" || props.action === "archive");
const reasonLength = computed(() => reason.value.trim().length);
const confirmDisabled = computed(() => isBusy.value || props.state === "success" || (requiresReason.value && reasonLength.value < 8));
const statePanelTone = computed(() => {
  if (props.state === "loading") {
    return "loading";
  }

  if (props.state === "success") {
    return "success";
  }

  if (props.state === "error") {
    return "danger";
  }

  return "info";
});
const stateTitle = computed(() => {
  if (props.state === "loading") {
    return "Đang xử lý lifecycle";
  }

  if (props.state === "success") {
    return "Đã áp dụng lifecycle action";
  }

  if (props.state === "error") {
    return "Không áp dụng được lifecycle action";
  }

  return "";
});
const statusTone = computed(() => {
  if (props.currentStatus === "Active") {
    return "success";
  }

  if (props.currentStatus === "Suspended" || props.currentStatus === "Archived") {
    return "danger";
  }

  return "warning";
});
const targetStatusTone = computed(() => {
  if (props.targetStatus === "Active") {
    return "success";
  }

  if (props.targetStatus === "Suspended" || props.targetStatus === "Archived") {
    return "danger";
  }

  return "warning";
});

watch(
  () => props.open,
  async (open) => {
    if (open) {
      reason.value = "";
      await nextTick();
      focusTrap.activate();
      if (typeof document !== "undefined") {
        document.body.style.overflow = "hidden";
      }
      return;
    }

    focusTrap.deactivate();
    if (typeof document !== "undefined") {
      document.body.style.overflow = "";
    }
  }
);

onBeforeUnmount(() => {
  focusTrap.deactivate();
  if (typeof document !== "undefined") {
    document.body.style.overflow = "";
  }
});

function confirmAction() {
  if (confirmDisabled.value) {
    return;
  }

  emit("confirm", props.action, reason.value.trim());
}
</script>

<template>
  <Teleport to="body">
    <div v-if="open" class="lifecycle-backdrop" role="presentation" @click.self="$emit('close')">
      <section
        ref="modalRef"
        class="lifecycle-modal"
        :data-tone="actionConfig.tone"
        role="dialog"
        aria-modal="true"
        :aria-labelledby="`tenant-lifecycle-${action}`"
        tabindex="-1"
      >
        <header class="lifecycle-header">
          <span class="lifecycle-icon" aria-hidden="true">{{ actionConfig.icon }}</span>
          <div>
            <h2 :id="`tenant-lifecycle-${action}`">{{ actionConfig.title }}</h2>
            <p>{{ tenantName }} · {{ tenantSlug }}</p>
          </div>
        </header>

        <div class="status-strip" aria-label="Lifecycle status change">
          <div>
            <span>Hiện tại</span>
            <StatusPill :label="currentStatus" :tone="statusTone" />
          </div>
          <strong aria-hidden="true">→</strong>
          <div>
            <span>Sau confirm</span>
            <StatusPill :label="targetStatus" :tone="targetStatusTone" />
          </div>
        </div>

        <p class="lifecycle-description">{{ actionConfig.description }}</p>

        <StatePanel
          v-if="state !== 'idle'"
          :title="stateTitle"
          :description="message"
          :tone="statePanelTone"
          :busy="state === 'loading'"
        />

        <div v-if="actionConfig.warning" class="warning-box" role="alert">
          {{ actionConfig.warning }}
        </div>

        <div class="impact-box">
          <p>Hệ quả</p>
          <ul>
            <li v-for="effect in actionConfig.effects" :key="effect">{{ effect }}</li>
          </ul>
        </div>

        <label v-if="requiresReason" class="reason-field">
          <span>{{ actionConfig.reasonLabel }}</span>
          <textarea
            v-model="reason"
            :placeholder="actionConfig.reasonPlaceholder"
            :disabled="isBusy || state === 'success'"
            rows="3"
          ></textarea>
          <small :data-invalid="requiresReason && reasonLength < 8 ? 'true' : 'false'">
            Tối thiểu 8 ký tự · hiện có {{ reasonLength }} ký tự
          </small>
        </label>

        <div class="preview-box">
          <span>Public site preview sau {{ action }}</span>
          <strong>{{ actionConfig.preview }}</strong>
        </div>

        <footer class="lifecycle-actions">
          <AppButton label="Hủy" variant="secondary" :disabled="isBusy" @click="$emit('close')" />
          <AppButton
            :label="actionConfig.confirmLabel"
            :variant="actionConfig.tone === 'danger' || actionConfig.tone === 'warning' ? 'danger' : 'primary'"
            :loading="isBusy"
            :disabled="confirmDisabled"
            @click="confirmAction"
          />
        </footer>
      </section>
    </div>
  </Teleport>
</template>

<style scoped>
.lifecycle-backdrop {
  position: fixed;
  inset: 0;
  z-index: 120;
  display: grid;
  place-items: center;
  padding: var(--space-6);
  background: color-mix(in srgb, var(--color-text-primary) 70%, transparent);
}

.lifecycle-modal {
  width: min(420px, 100%);
  max-height: calc(100vh - var(--space-12, 96px));
  display: grid;
  gap: var(--space-4);
  overflow-y: auto;
  border-radius: var(--radius-card);
  padding: var(--space-6);
  background: var(--color-surface-elevated);
  box-shadow: var(--shadow-elevation-3);
}

.lifecycle-modal:focus {
  outline: none;
}

.lifecycle-header {
  display: flex;
  align-items: center;
  gap: var(--space-3);
}

.lifecycle-icon {
  width: 40px;
  height: 40px;
  display: grid;
  place-items: center;
  flex: 0 0 auto;
  border-radius: var(--radius-pill);
  background: var(--lifecycle-color);
  color: var(--color-surface-elevated);
  font-size: 12px;
  font-weight: 900;
}

.lifecycle-header h2,
.lifecycle-header p,
.lifecycle-description,
.impact-box p,
.impact-box ul,
.warning-box,
.preview-box span,
.preview-box strong {
  margin: 0;
}

.lifecycle-header h2 {
  color: var(--color-text-primary);
  font-size: 18px;
  line-height: 24px;
}

.lifecycle-header p,
.lifecycle-description {
  color: var(--color-text-secondary);
  font-size: 13px;
  line-height: 20px;
}

.status-strip {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto minmax(0, 1fr);
  align-items: center;
  gap: var(--space-3);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-input);
  padding: var(--space-3);
  background: var(--color-surface-muted);
}

.status-strip div {
  display: grid;
  justify-items: start;
  gap: var(--space-2);
  min-width: 0;
}

.status-strip span {
  color: var(--color-text-secondary);
  font-size: 11px;
  font-weight: 900;
  text-transform: uppercase;
}

.status-strip strong {
  color: var(--color-text-muted);
}

.impact-box,
.warning-box,
.preview-box,
.reason-field textarea {
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-input);
  background: var(--color-surface-muted);
}

.warning-box {
  border-color: color-mix(in srgb, var(--lifecycle-color) 32%, var(--color-border-subtle));
  padding: var(--space-3);
  background: color-mix(in srgb, var(--lifecycle-color) 11%, var(--color-surface-elevated));
  color: var(--color-text-primary);
  font-size: 12px;
  font-weight: 800;
  line-height: 18px;
}

.impact-box {
  display: grid;
  gap: var(--space-2);
  padding: var(--space-3) var(--space-4);
}

.impact-box p,
.reason-field span,
.preview-box span {
  color: var(--color-text-secondary);
  font-size: 11px;
  font-weight: 800;
  text-transform: uppercase;
}

.impact-box ul {
  display: grid;
  gap: var(--space-1);
  padding-left: var(--space-4);
  color: var(--color-text-primary);
  font-size: 12px;
  line-height: 18px;
}

.reason-field {
  display: grid;
  gap: var(--space-2);
}

.reason-field small {
  color: var(--color-text-muted);
  font-size: 11px;
  font-weight: 750;
}

.reason-field small[data-invalid="true"] {
  color: var(--color-status-danger);
}

.reason-field textarea {
  width: 100%;
  box-sizing: border-box;
  resize: vertical;
  padding: var(--space-3);
  color: var(--color-text-primary);
  font: inherit;
  font-size: 13px;
}

.reason-field textarea:focus-visible {
  outline: 2px solid color-mix(in srgb, var(--lifecycle-color) 36%, transparent);
  outline-offset: 2px;
}

.preview-box {
  display: grid;
  gap: var(--space-2);
  padding: var(--space-3);
  background: color-mix(in srgb, var(--color-surface-muted) 70%, var(--color-surface-elevated));
}

.preview-box strong {
  color: var(--color-text-primary);
  font-size: 12px;
  line-height: 18px;
}

.lifecycle-actions {
  display: flex;
  justify-content: flex-end;
  gap: var(--space-2);
  padding-top: var(--space-2);
}

.lifecycle-modal[data-tone="warning"] {
  --lifecycle-color: var(--color-status-warning);
}

.lifecycle-modal[data-tone="danger"] {
  --lifecycle-color: var(--color-status-danger);
}

.lifecycle-modal[data-tone="success"] {
  --lifecycle-color: var(--color-status-success);
}

@media (max-width: 520px) {
  .lifecycle-backdrop {
    align-items: end;
    padding: var(--space-3);
  }

  .lifecycle-modal {
    width: 100%;
    max-height: calc(100vh - var(--space-6));
  }

  .lifecycle-actions {
    flex-direction: column-reverse;
  }

  .status-strip {
    grid-template-columns: 1fr;
  }

  .status-strip strong {
    display: none;
  }

  .lifecycle-actions :deep(.app-button) {
    width: 100%;
  }
}
</style>
