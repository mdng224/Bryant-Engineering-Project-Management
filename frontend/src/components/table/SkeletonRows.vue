<!-- components/SkeletonRows.vue -->
<template>
  <tr v-for="row in rows" :key="row">
    <td v-for="col in cols" :key="col" class="px-4 py-3">
      <div class="h-3 w-40 animate-pulse rounded bg-slate-700/50" />
    </td>
  </tr>
</template>
<script setup lang="ts">
  defineProps<{ rows?: number; cols: number }>();
</script>
