<script setup lang="ts">
defineProps<{
  label: string;
}>();
</script>

<template>
  <button class="app-button" type="button">{{ label }}</button>
</template>

<style scoped>
.app-button {
  min-height: 44px;
  border: 0;
  border-radius: 8px;
  padding: 0 18px;
  background: #0e7c86;
  color: white;
  font-weight: 700;
  cursor: pointer;
}
</style>
