<script setup lang="ts">
import { AppButton } from "@clinic-saas/ui";
import { mockTenant } from "@clinic-saas/shared-types";
</script>

<template>
  <main class="app-shell">
    <p class="eyebrow">Public Website</p>
    <h1>{{ mockTenant.name }}</h1>
    <p>Tenant-aware public shell. Resolve tenant by domain/subdomain before loading content.</p>
    <AppButton label="Dat lich kham" />
  </main>
</template>

<style scoped>
.app-shell {
  min-height: 100vh;
  padding: 48px;
  font-family: Inter, system-ui, sans-serif;
}

.eyebrow {
  color: #0e7c86;
  font-weight: 700;
}
</style>
