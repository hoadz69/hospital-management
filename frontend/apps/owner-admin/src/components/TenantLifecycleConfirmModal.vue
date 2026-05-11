<script setup lang="ts">
import { AppButton, useFocusTrap } from "@clinic-saas/ui";
import { computed, nextTick, onBeforeUnmount, ref, watch } from "vue";

type TenantLifecycleAction = "suspend" | "archive" | "restore";

const props = withDefaults(
  defineProps<{
    open: boolean;
    action: TenantLifecycleAction;
    tenantName: string;
    tenantSlug: string;
    loading?: boolean;
  }>(),
  {
    loading: false
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
  if (props.action === "archive") {
    return {
      tone: "danger",
      icon: "AR",
      title: "Lưu trữ tenant?",
      description:
        "Hành động này khóa tenant khỏi vận hành và đưa vào hàng đợi lưu trữ. Dữ liệu chỉ được xử lý tiếp khi backend lifecycle contract sẵn sàng.",
      confirmLabel: "Xác nhận Archive",
      reasonLabel: "Lý do lưu trữ *",
      reasonPlaceholder: "Vd: Tenant ngừng hợp đồng hoặc cần khóa dữ liệu...",
      preview: "Tenant sẽ chuyển sang trạng thái lưu trữ và subdomain bị đóng băng.",
      effects: [
        "Tenant chuyển archived state",
        "Subdomain freeze",
        "Custom domain release sau 30 ngày",
        "Backup retain 90 ngày theo chính sách platform"
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
      confirmLabel: "Xác nhận Restore",
      reasonLabel: "",
      reasonPlaceholder: "",
      preview: "Tenant active trở lại, DNS recheck được đưa vào hàng đợi.",
      effects: [
        "Tenant active trở lại",
        "DNS recheck triggered",
        "Clinic admin email notify",
        "Plan trở lại trạng thái cũ"
      ]
    };
  }

  return {
    tone: "warning",
    icon: "SP",
    title: "Tạm ngừng tenant?",
    description:
      "Public site sẽ chuyển sang fallback page. Clinic admin không truy cập được. Dữ liệu giữ nguyên.",
    confirmLabel: "Xác nhận Suspend",
    reasonLabel: "Lý do tạm ngừng *",
    reasonPlaceholder: "Vd: Tenant không thanh toán plan...",
    preview: "Tạm ngừng vận hành - vui lòng liên hệ chủ phòng khám.",
    effects: [
      "Public site chuyển sang maintenance page",
      "Clinic admin login bị block",
      "Các webhook tenant bị disable",
      "Booking flow auto-decline lịch mới"
    ]
  };
});

const requiresReason = computed(() => props.action !== "restore");
const confirmDisabled = computed(() => requiresReason.value && reason.value.trim().length < 4);

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
            <p>Tenant: {{ tenantSlug }}</p>
          </div>
        </header>

        <p class="lifecycle-description">{{ actionConfig.description }}</p>

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
            :disabled="loading"
            rows="3"
          ></textarea>
        </label>

        <div class="preview-box">
          <span>Public site preview sau {{ action }}</span>
          <strong>{{ actionConfig.preview }}</strong>
        </div>

        <footer class="lifecycle-actions">
          <AppButton label="Hủy" variant="secondary" :disabled="loading" @click="$emit('close')" />
          <AppButton
            :label="actionConfig.confirmLabel"
            :variant="actionConfig.tone === 'danger' || actionConfig.tone === 'warning' ? 'danger' : 'primary'"
            :loading="loading"
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

.impact-box,
.preview-box,
.reason-field textarea {
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-input);
  background: var(--color-surface-muted);
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

  .lifecycle-actions :deep(.app-button) {
    width: 100%;
  }
}
</style>
