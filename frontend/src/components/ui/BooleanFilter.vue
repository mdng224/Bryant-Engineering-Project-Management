<template>
  <div ref="root" class="relative inline-block text-left">
    <!-- Dropdown button -->
    <button
      @click="toggleOpen"
      type="button"
      class="flex w-52 items-center justify-between rounded-md border border-slate-600 bg-slate-800/60 px-3 py-2 text-sm text-slate-100 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/30"
    >
      <span class="flex items-center gap-2">
        <component :is="currentOption.icon" :class="['h-4 w-4', currentOption.color]" />
        {{ currentOption.label }}
      </span>

      <ChevronDown
        class="h-4 w-4 text-slate-400 transition-transform duration-150"
        :class="{ 'rotate-180': isOpen }"
      />
    </button>

    <!-- Options -->
    <div
      v-if="isOpen"
      class="absolute z-50 mt-1 w-52 rounded-md border border-slate-700 bg-slate-800 shadow-lg"
    >
      <button
        v-for="option in props.options"
        :key="option.label"
        @click="selectOption(option.value)"
        class="flex w-full items-center gap-2 px-3 py-2 text-left text-sm text-slate-100 hover:bg-slate-700/50"
      >
        <component :is="option.icon" class="h-4 w-4 text-slate-400" />
        <span>{{ option.label }}</span>
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { ChevronDown } from 'lucide-vue-next';
  import type { Component } from 'vue';
  import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue';

  type BoolOption = {
    value: boolean | null;
    label: string;
    icon: Component;
    color: string;
  };

  const props = defineProps<{
    modelValue: boolean | null;
    options: BoolOption[];
  }>();

  const emit = defineEmits<{
    (e: 'update:modelValue', value: boolean | null): void;
    (e: 'change', value: boolean | null): void;
  }>();

  // --- State -----------------------------------------------------------------
  const root = ref<HTMLElement | null>(null);
  const isOpen = ref(false);
  const localValue = ref(props.modelValue);

  // Sync localValue when parent updates v-model
  watch(
    () => props.modelValue,
    val => {
      localValue.value = val;
    },
  );

  // --- Derived state ---------------------------------------------------------
  const currentOption = computed(() => {
    return props.options.find(o => o.value === localValue.value) ?? props.options[0];
  });

  // --- Methods ---------------------------------------------------------------
  const toggleOpen = () => (isOpen.value = !isOpen.value);

  const selectOption = (val: boolean | null) => {
    localValue.value = val;
    emit('update:modelValue', val);
    emit('change', val);
    isOpen.value = false;
  };

  // Click outside using a safer ref instead of a CSS selector
  const handleClickOutside = (e: Event) => {
    const el = root.value;
    if (el && !el.contains(e.target as Node)) {
      isOpen.value = false;
    }
  };

  // --- Lifecycle -------------------------------------------------------------
  onMounted(() => {
    window.addEventListener('pointerdown', handleClickOutside);
  });

  onBeforeUnmount(() => {
    window.removeEventListener('pointerdown', handleClickOutside);
  });
</script>
