<!-- src/components/SelectFilter.vue -->
<template>
  <div ref="root" class="relative inline-block text-left">
    <button
      type="button"
      @click="toggleOpen"
      class="flex w-72 items-center justify-between gap-2 rounded-md border border-slate-600 bg-slate-800/60 px-3 py-2 text-sm text-slate-100 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/30"
    >
      <span class="flex items-center gap-2">
        <component
          v-if="currentOption?.icon"
          :is="currentOption.icon"
          class="h-4 w-4 text-slate-300"
        />
        <span>{{ currentOption?.label ?? placeholder }}</span>
      </span>

      <chevron-down
        class="h-4 w-4 shrink-0 text-slate-400 transition-transform duration-150"
        :class="{ 'rotate-180': isOpen }"
      />
    </button>

    <div
      v-if="isOpen"
      class="absolute z-50 mt-1 w-full rounded-md border border-slate-700 bg-slate-800 shadow-lg"
    >
      <button
        v-for="option in options"
        :key="optionKey(option)"
        type="button"
        @click="selectOption(option.value)"
        class="flex w-full items-center gap-2 px-3 py-2 text-left text-sm text-slate-100 hover:bg-slate-700/50"
      >
        <component v-if="option.icon" :is="option.icon" class="h-4 w-4 text-slate-400" />
        <span>{{ option.label }}</span>
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { ChevronDown } from 'lucide-vue-next';
  import type { Component } from 'vue';
  import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue';

  type SelectOption = {
    value: string | null;
    label: string;
    icon?: Component;
  };

  const props = defineProps<{
    modelValue: string | null;
    options: SelectOption[];
    placeholder?: string;
  }>();

  const emit = defineEmits<{
    (e: 'update:modelValue', value: string | null): void;
    (e: 'change', value: string | null): void;
  }>();

  const root = ref<HTMLElement | null>(null);
  const isOpen = ref(false);
  const localValue = ref<string | null>(props.modelValue ?? null);

  watch(
    () => props.modelValue,
    val => {
      localValue.value = val ?? null;
    },
  );

  const currentOption = computed(() => {
    return props.options.find(o => o.value === localValue.value) ?? null;
  });

  const placeholder = computed(() => props.placeholder ?? 'Select…');

  const toggleOpen = () => {
    isOpen.value = !isOpen.value;
  };

  const selectOption = (val: string | null) => {
    localValue.value = val;
    emit('update:modelValue', val);
    emit('change', val);
    isOpen.value = false;
  };

  const handleClickOutside = (e: Event) => {
    const el = root.value;
    if (el && !el.contains(e.target as Node)) {
      isOpen.value = false;
    }
  };

  const optionKey = (o: SelectOption) => o.value ?? '__all__';

  onMounted(() => {
    window.addEventListener('pointerdown', handleClickOutside);
  });

  onBeforeUnmount(() => {
    window.removeEventListener('pointerdown', handleClickOutside);
  });
</script>
