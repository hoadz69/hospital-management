<script setup lang="ts">
import { CommandPalette } from "@clinic-saas/ui";
import { computed, ref, watch } from "vue";
import { useRouter } from "vue-router";

const props = defineProps<{
  open: boolean;
}>();

const emit = defineEmits<{
  close: [];
}>();

type OwnerCommandItem = {
  id: string;
  label: string;
  meta?: string;
  hint?: string;
  icon?: string;
  to: string;
};

const router = useRouter();
const query = ref("");

const recentTenants = [
  { id: "tenant-mat-saigon", label: "mat-saigon", meta: "Tenant", icon: "▣", to: "/clinics" },
  { id: "tenant-rhm-hadong", label: "rhm-hadong", meta: "Tenant", icon: "▣", to: "/clinics" }
];

const actions = [
  { id: "create-clinic", label: "Tạo phòng khám mới", hint: "Ctrl N", to: "/clinics/create", icon: "+" },
  { id: "plan-catalog", label: "Mở gói & module", hint: "Ctrl P", to: "/plans", icon: "▦" },
  { id: "add-domain", label: "Thêm domain", hint: "Ctrl D", to: "/clinics", icon: "◎" },
  { id: "audit-log", label: "Xem audit log", hint: "Ctrl L", to: "/clinics", icon: "▤" },
  { id: "export-report", label: "Export report CSV", hint: "Ctrl E", to: "/clinics", icon: "⇩" }
];

const routesById = new Map<string, string>(
  [...recentTenants, ...actions].map((item) => [item.id, item.to])
);

const sections = computed(() => {
  const normalizedQuery = query.value.trim().toLowerCase();
  const matchesQuery = (item: OwnerCommandItem) => {
    if (!normalizedQuery) {
      return true;
    }

    return [item.label, item.meta, item.hint].some((value) => value?.toLowerCase().includes(normalizedQuery));
  };

  return [
    {
      id: "recent",
      label: "Recent",
      items: recentTenants.filter(matchesQuery)
    },
    {
      id: "actions",
      label: "Actions",
      items: actions.filter(matchesQuery)
    }
  ];
});

async function selectCommand(item: { id: string; disabled?: boolean }) {
  if (item.disabled) {
    return;
  }

  const target = routesById.get(item.id);
  if (target) {
    await router.push(target);
  }

  emit("close");
}

watch(
  () => props.open,
  (open) => {
    if (open) {
      query.value = "";
    }
  }
);
</script>

<template>
  <CommandPalette
    :open="open"
    v-model:query="query"
    :sections="sections"
    placeholder="Tìm phòng khám, tên miền, thao tác..."
    empty-label="Không tìm thấy lệnh phù hợp."
    empty-helper="Thử tìm theo slug phòng khám hoặc thao tác cần làm."
    @close="$emit('close')"
    @select="selectCommand"
  />
</template>
