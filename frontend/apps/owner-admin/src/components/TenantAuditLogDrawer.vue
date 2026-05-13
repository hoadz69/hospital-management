<script setup lang="ts">
import { AppButton, StatePanel, StatusPill, useFocusTrap } from "@clinic-saas/ui";
import { computed, nextTick, ref, watch } from "vue";

export type TenantAuditEventType = "lifecycle" | "domain" | "plan" | "mock";
export type TenantAuditEvent = {
  id: string;
  type: TenantAuditEventType;
  title: string;
  detail: string;
  actor: string;
  occurredAt: string;
  outcome: "success" | "warning" | "danger" | "neutral";
  metadata?: string[];
};

type EventTypeFilter = "all" | TenantAuditEventType;
type DateFilter = "all" | "today" | "7d" | "30d";

const props = withDefaults(
  defineProps<{
    open: boolean;
    tenantName: string;
    tenantSlug: string;
    events: TenantAuditEvent[];
    loading?: boolean;
    error?: string;
  }>(),
  {
    loading: false,
    error: undefined
  }
);

const emit = defineEmits<{
  close: [];
  refresh: [];
}>();

const drawerRef = ref<HTMLElement | null>(null);
const typeFilter = ref<EventTypeFilter>("all");
const actorFilter = ref("all");
const dateFilter = ref<DateFilter>("all");

const focusTrap = useFocusTrap(drawerRef, {
  onEscape: () => emit("close")
});

const actorOptions = computed(() => ["all", ...Array.from(new Set(props.events.map((event) => event.actor)))]);

const filteredEvents = computed(() =>
  props.events.filter((event) => {
    const typeMatch = typeFilter.value === "all" || event.type === typeFilter.value;
    const actorMatch = actorFilter.value === "all" || event.actor === actorFilter.value;
    const dateMatch = matchesDateFilter(event.occurredAt, dateFilter.value);

    return typeMatch && actorMatch && dateMatch;
  })
);

function matchesDateFilter(value: string, filter: DateFilter) {
  if (filter === "all") {
    return true;
  }

  const occurredAt = new Date(value).getTime();
  const now = Date.now();

  if (Number.isNaN(occurredAt)) {
    return false;
  }

  if (filter === "today") {
    return new Date(value).toDateString() === new Date().toDateString();
  }

  const days = filter === "7d" ? 7 : 30;
  return now - occurredAt <= days * 24 * 60 * 60 * 1000;
}

function eventTypeLabel(type: TenantAuditEventType) {
  if (type === "lifecycle") {
    return "Lifecycle";
  }

  if (type === "domain") {
    return "Domain";
  }

  if (type === "plan") {
    return "Plan";
  }

  return "Mock";
}

function eventTone(outcome: TenantAuditEvent["outcome"]) {
  if (outcome === "success") {
    return "success";
  }

  if (outcome === "warning") {
    return "warning";
  }

  if (outcome === "danger") {
    return "danger";
  }

  return "neutral";
}

function formatDateTime(value: string) {
  return new Intl.DateTimeFormat("vi-VN", {
    day: "2-digit",
    month: "2-digit",
    hour: "2-digit",
    minute: "2-digit"
  }).format(new Date(value));
}

function resetFilters() {
  typeFilter.value = "all";
  actorFilter.value = "all";
  dateFilter.value = "all";
}

watch(
  () => props.open,
  async (open) => {
    if (open) {
      await nextTick();
      focusTrap.activate();
      return;
    }

    focusTrap.deactivate();
  }
);
</script>

<template>
  <Teleport to="body">
    <div v-if="open" class="audit-backdrop" role="presentation" @click.self="$emit('close')">
      <aside ref="drawerRef" class="audit-drawer" role="dialog" aria-modal="true" aria-labelledby="tenant-audit-title" tabindex="-1">
        <header class="audit-header">
          <div>
            <p class="eyebrow">Audit log</p>
            <h2 id="tenant-audit-title">{{ tenantName }}</h2>
            <span>{{ tenantSlug }} · {{ events.length }} sự kiện mock</span>
          </div>
          <button type="button" class="close-button" aria-label="Đóng audit log" @click="$emit('close')">×</button>
        </header>

        <div class="audit-toolbar">
          <label>
            <span>Loại</span>
            <select v-model="typeFilter">
              <option value="all">Tất cả</option>
              <option value="lifecycle">Lifecycle</option>
              <option value="domain">Domain</option>
              <option value="plan">Plan</option>
              <option value="mock">Mock</option>
            </select>
          </label>

          <label>
            <span>Actor</span>
            <select v-model="actorFilter">
              <option v-for="actor in actorOptions" :key="actor" :value="actor">
                {{ actor === "all" ? "Tất cả" : actor }}
              </option>
            </select>
          </label>

          <label>
            <span>Thời gian</span>
            <select v-model="dateFilter">
              <option value="all">Tất cả</option>
              <option value="today">Hôm nay</option>
              <option value="7d">7 ngày</option>
              <option value="30d">30 ngày</option>
            </select>
          </label>
        </div>

        <div class="audit-actions">
          <AppButton label="Refresh" variant="secondary" :loading="loading" @click="$emit('refresh')" />
          <AppButton label="Xóa filter" variant="ghost" @click="resetFilters" />
        </div>

        <StatePanel
          v-if="loading"
          title="Đang tải audit log"
          description="Owner Admin đang lấy mock tenant audit events từ local state."
          tone="loading"
          busy
        />

        <StatePanel v-else-if="error" title="Không tải được audit log" :description="error" tone="danger">
          <template #action>
            <AppButton label="Thử lại" variant="secondary" @click="$emit('refresh')" />
          </template>
        </StatePanel>

        <StatePanel
          v-else-if="filteredEvents.length === 0"
          title="Không có audit event phù hợp"
          description="Thay đổi filter để xem lifecycle, domain hoặc plan events khác."
          tone="neutral"
        />

        <ol v-else class="audit-list" aria-label="Tenant audit timeline">
          <li v-for="event in filteredEvents" :key="event.id" class="audit-event">
            <div class="event-marker" :data-tone="event.outcome" aria-hidden="true"></div>
            <article>
              <div class="event-topline">
                <StatusPill :label="eventTypeLabel(event.type)" :tone="eventTone(event.outcome)" />
                <time :datetime="event.occurredAt">{{ formatDateTime(event.occurredAt) }}</time>
              </div>
              <h3>{{ event.title }}</h3>
              <p>{{ event.detail }}</p>
              <div class="event-meta">
                <span>{{ event.actor }}</span>
                <span v-for="item in event.metadata" :key="item">{{ item }}</span>
              </div>
            </article>
          </li>
        </ol>
      </aside>
    </div>
  </Teleport>
</template>

<style scoped>
.audit-backdrop {
  position: fixed;
  inset: 0;
  z-index: 110;
  display: flex;
  justify-content: flex-end;
  background: color-mix(in srgb, var(--color-text-primary) 34%, transparent);
}

.audit-drawer {
  width: min(680px, 100vw);
  min-height: 100vh;
  display: grid;
  align-content: start;
  gap: var(--space-4);
  overflow-y: auto;
  padding: var(--space-6);
  background: var(--color-surface-elevated);
  box-shadow: var(--shadow-elevation-3);
}

.audit-drawer:focus {
  outline: none;
}

.audit-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: var(--space-4);
  border-bottom: 1px solid var(--color-border-subtle);
  margin: calc(var(--space-6) * -1) calc(var(--space-6) * -1) 0;
  padding: var(--space-6);
  background: var(--color-surface-page);
}

.audit-header h2,
.audit-header p,
.audit-header span,
.audit-event h3,
.audit-event p {
  margin: 0;
}

.eyebrow {
  color: var(--color-brand-primary);
  font-size: 11px;
  font-weight: 900;
  text-transform: uppercase;
}

.audit-header h2 {
  margin-top: var(--space-1);
  color: var(--color-text-primary);
  font-size: 22px;
  line-height: 28px;
}

.audit-header span {
  display: block;
  margin-top: var(--space-1);
  color: var(--color-text-secondary);
  font-size: 12px;
  font-weight: 700;
}

.close-button {
  width: 36px;
  height: 36px;
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-input);
  background: var(--color-surface-elevated);
  color: var(--color-text-secondary);
  cursor: pointer;
  font-weight: 900;
}

.audit-toolbar {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: var(--space-3);
}

.audit-toolbar label {
  display: grid;
  gap: var(--space-2);
  min-width: 0;
}

.audit-toolbar span {
  color: var(--color-text-secondary);
  font-size: 11px;
  font-weight: 900;
  text-transform: uppercase;
}

.audit-toolbar select {
  width: 100%;
  min-height: 38px;
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-input);
  padding: 0 var(--space-3);
  background: var(--color-surface-elevated);
  color: var(--color-text-primary);
  font: inherit;
  font-size: 13px;
  font-weight: 750;
}

.audit-toolbar select:focus-visible,
.close-button:focus-visible {
  outline: 2px solid color-mix(in srgb, var(--color-brand-primary) 28%, transparent);
  outline-offset: 2px;
}

.audit-actions {
  display: flex;
  justify-content: flex-end;
  gap: var(--space-2);
}

.audit-list {
  display: grid;
  gap: var(--space-3);
  margin: 0;
  padding: 0;
  list-style: none;
}

.audit-event {
  position: relative;
  display: grid;
  grid-template-columns: auto minmax(0, 1fr);
  gap: var(--space-3);
}

.event-marker {
  width: 12px;
  height: 12px;
  margin-top: var(--space-4);
  border-radius: var(--radius-pill);
  background: var(--color-text-muted);
  box-shadow: 0 0 0 5px color-mix(in srgb, var(--event-color, var(--color-text-muted)) 12%, transparent);
}

.event-marker[data-tone="success"] {
  --event-color: var(--color-status-success);
  background: var(--event-color);
}

.event-marker[data-tone="warning"] {
  --event-color: var(--color-status-warning);
  background: var(--event-color);
}

.event-marker[data-tone="danger"] {
  --event-color: var(--color-status-danger);
  background: var(--event-color);
}

.audit-event article {
  display: grid;
  gap: var(--space-2);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-card);
  padding: var(--space-4);
  background: var(--color-surface-elevated);
}

.event-topline,
.event-meta {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: var(--space-2);
}

.event-topline {
  justify-content: space-between;
}

.event-topline time {
  color: var(--color-text-muted);
  font-size: 12px;
  font-weight: 800;
}

.audit-event h3 {
  color: var(--color-text-primary);
  font-size: 15px;
  line-height: 20px;
}

.audit-event p {
  color: var(--color-text-secondary);
  font-size: 13px;
  line-height: 19px;
}

.event-meta span {
  min-height: 24px;
  display: inline-flex;
  align-items: center;
  border-radius: var(--radius-pill);
  padding: 0 var(--space-2);
  background: var(--color-surface-muted);
  color: var(--color-text-secondary);
  font-size: 11px;
  font-weight: 800;
}

@media (max-width: 720px) {
  .audit-drawer {
    padding: var(--space-5);
  }

  .audit-header {
    margin: calc(var(--space-5) * -1) calc(var(--space-5) * -1) 0;
    padding: var(--space-5);
  }

  .audit-toolbar {
    grid-template-columns: 1fr;
  }

  .audit-actions,
  .event-topline {
    align-items: stretch;
    flex-direction: column;
  }

  .audit-actions :deep(.app-button) {
    width: 100%;
  }
}
</style>
