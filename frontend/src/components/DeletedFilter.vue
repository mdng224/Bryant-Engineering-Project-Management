<template>
  <div class="relative inline-block text-left">
    <!-- Dropdown button -->
    <button
      @click="isOpen = !isOpen"
      type="button"
      class="flex w-52 items-center justify-between rounded-md border border-slate-600 bg-slate-800/60 px-3 py-2 text-sm text-slate-100 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/30"
    >
      <span class="flex items-center gap-2">
        <component :is="currentOption.icon" :class="['h-4 w-4', currentOption.color]" />
        {{ currentOption.label }}
      </span>
      <ChevronDown
        class="h-4 w-4 text-slate-400 transition-transform"
        :class="{ 'rotate-180': isOpen }"
      />
    </button>

    <!-- Dropdown options -->
    <div
      v-if="isOpen"
      class="absolute z-50 mt-1 w-52 rounded-md border border-slate-700 bg-slate-800 shadow-lg"
    >
      <button
        v-for="opt in options"
        :key="opt.label"
        @click="selectOption(opt.value)"
        class="flex w-full items-center gap-2 px-3 py-2 text-left text-sm text-slate-100 hover:bg-slate-700/50"
      >
        <component :is="opt.icon" class="h-4 w-4 text-slate-400" />
        <span>{{ opt.label }}</span>
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { CheckCircle2, ChevronDown, Trash2 } from 'lucide-vue-next';
  import { computed, ref, watch } from 'vue';

  const props = defineProps<{ modelValue: boolean | null }>();

  const emit = defineEmits<{
    (e: 'update:modelValue', value: boolean | null): void;
    (e: 'change', value: boolean | null): void;
  }>();

  const options = [
    { value: false, label: 'Active Only', icon: CheckCircle2, color: 'text-emerald-400' },
    { value: true, label: 'Deleted Only', icon: Trash2, color: 'text-rose-400' },
  ];

  const isOpen = ref(false);
  const localValue = ref<boolean | null>(props.modelValue ?? null);

  watch(
    () => props.modelValue,
    val => {
      if (val !== localValue.value) localValue.value = val;
    },
  );

  const currentOption = computed(
    () => options.find(o => o.value === localValue.value) ?? options[0],
  );

  const selectOption = (val: boolean | null) => {
    localValue.value = val;
    emit('update:modelValue', val);
    emit('change', val);
    isOpen.value = false;
  };

  // Optional: close dropdown when clicking outside
  const handleClickOutside = (e: MouseEvent) => {
    const target = e.target as HTMLElement;
    if (!target.closest('.relative.inline-block')) isOpen.value = false;
  };

  window.addEventListener('click', handleClickOutside);
</script>
