<script setup lang="ts">
import { StatusPill } from "@clinic-saas/ui";
import { mockTenant } from "@clinic-saas/shared-types";
</script>

<template>
  <main class="admin-shell">
    <aside>Clinic Admin</aside>
    <section>
      <p class="eyebrow">Tenant scoped</p>
      <h1>{{ mockTenant.displayName }}</h1>
      <p>Placeholder dashboard. Clinic Admin must only access current tenant data.</p>
      <StatusPill label="tenant context required" tone="warning" />
    </section>
  </main>
</template>

<style scoped>
.admin-shell {
  min-height: 100vh;
  display: grid;
  grid-template-columns: 240px 1fr;
  font-family: Inter, system-ui, sans-serif;
}

aside {
  padding: 32px;
  background: #0f172a;
  color: white;
}

section {
  padding: 48px;
}

.eyebrow {
  color: #0e7c86;
  font-weight: 700;
}
</style>
