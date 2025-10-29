<!-- components/TableSearch.vue -->
<template>
  <input
    v-model="model"
    :placeholder="placeholder ?? 'Search…'"
    class="h-9 min-w-[260px] rounded-md border border-slate-700 bg-slate-900/70 px-3 text-sm focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/30 sm:w-1/2 lg:w-1/3"
    @keydown.enter="commitNow()"
  />
</template>

<script setup lang="ts">
  import { useDebouncedRef } from '@/composables/useDebouncedRef';
  import { watch } from 'vue';

  const props = defineProps<{
    delay?: number;
    modelValue?: string;
    placeholder?: string;
  }>();

  const emit = defineEmits<{ 'update:modelValue': [string]; commit: [] }>();

  const {
    input: model,
    debounced,
    setNow: commitNow,
  } = useDebouncedRef(props.modelValue ?? '', props.delay ?? 500);

  watch(model, v => emit('update:modelValue', v));
  watch(debounced, v => emit('update:modelValue', v)); // parent decides how to use debounced/live
  watch(commitNow, () => emit('commit'));
</script>
