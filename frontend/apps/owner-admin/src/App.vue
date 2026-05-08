<script setup lang="ts">
import { AppButton, StatusPill } from "@clinic-saas/ui";
</script>

<template>
  <main class="owner-shell">
    <aside>Owner Admin</aside>
    <section>
      <p class="eyebrow">Platform scoped</p>
      <h1>Clinic SaaS Operations</h1>
      <p>Placeholder dashboard for tenants, domains, templates, plans, and monitoring.</p>
      <div class="actions">
        <AppButton label="Create clinic" />
        <StatusPill label="cross-tenant role required" tone="info" />
      </div>
    </section>
  </main>
</template>

<style scoped>
.owner-shell {
  min-height: 100vh;
  display: grid;
  grid-template-columns: 240px 1fr;
  font-family: Inter, system-ui, sans-serif;
}

aside {
  padding: 32px;
  background: #111827;
  color: white;
}

section {
  padding: 48px;
}

.actions {
  display: flex;
  gap: 12px;
  align-items: center;
}

.eyebrow {
  color: #2563eb;
  font-weight: 700;
}
</style>
