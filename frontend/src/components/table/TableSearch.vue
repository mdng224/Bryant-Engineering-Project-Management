<!-- components/TableSearch.vue -->
<template>
  <div
    class="flex h-9 min-w-[260px] items-center gap-2 rounded-md border border-slate-700 bg-slate-900/70 px-3 focus-within:border-indigo-500 focus-within:ring-2 focus-within:ring-indigo-500/30 sm:w-1/2 lg:w-1/3"
  >
    <Search class="h-4 w-4 text-slate-400" aria-hidden="true" />
    <input
      v-model="localValue"
      :placeholder="placeholder ?? 'Search…'"
      type="search"
      class="flex-1 bg-transparent text-sm text-slate-100 placeholder-slate-500 focus:outline-none"
      @keydown.enter="emit('commit')"
    />
  </div>
</template>

<script setup lang="ts">
  import { Search } from 'lucide-vue-next';
  import { ref, watch } from 'vue';

  const props = defineProps<{
    modelValue: string | null;
    placeholder?: string;
  }>();

  const emit = defineEmits<{
    'update:modelValue': [string | null];
    commit: [];
  }>();

  const localValue = ref(props.modelValue ?? '');

  // Keep localValue in sync if parent changes it externally
  watch(
    () => props.modelValue,
    v => {
      if (v !== localValue.value) {
        localValue.value = v ?? '';
      }
    },
  );

  // Push changes up to parent
  watch(localValue, v => {
    emit('update:modelValue', v === '' ? null : v);
  });
</script>
