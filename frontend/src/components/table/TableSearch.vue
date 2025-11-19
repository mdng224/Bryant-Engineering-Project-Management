<!-- components/TableSearch.vue -->
<template>
  <div
    class="flex h-9 min-w-[260px] items-center gap-2 rounded-md border border-slate-700 bg-slate-900/70 px-3 focus-within:border-indigo-500 focus-within:ring-2 focus-within:ring-indigo-500/30 sm:w-1/2 lg:w-1/3"
  >
    <Search class="h-4 w-4 text-slate-400" aria-hidden="true" />
    <input
      v-model="model"
      :placeholder="placeholder ?? 'Search…'"
      class="flex-1 bg-transparent text-sm text-slate-100 placeholder-slate-500 focus:outline-none"
      @keydown.enter="commitNow()"
    />
  </div>
</template>

<script setup lang="ts">
  import { useDebouncedRef } from '@/composables/useDebouncedRef';
  import { Search } from 'lucide-vue-next';
  import { watch } from 'vue';

  const props = defineProps<{
    delay?: number;
    modelValue: string | null;
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
