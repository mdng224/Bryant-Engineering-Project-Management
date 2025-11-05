<template>
  <select
    v-model="localValue"
    @change="emitChange"
    class="rounded-md border border-slate-600 bg-slate-800/60 px-3 py-2 text-sm text-slate-100 focus:border-indigo-500 focus:outline-none"
  >
    <option :value="null">Active + Deleted</option>
    <option :value="false">Active Only</option>
    <option :value="true">Deleted Only</option>
  </select>
</template>

<script setup lang="ts">
  import { ref, watch } from 'vue';

  const props = defineProps<{
    modelValue: boolean | null;
  }>();

  const emit = defineEmits<{
    (e: 'update:modelValue', value: boolean | null): void;
    (e: 'change', value: boolean | null): void;
  }>();

  // local copy for v-model
  const localValue = ref<boolean | null>(props.modelValue ?? false);

  // sync with parent when prop changes externally
  watch(
    () => props.modelValue,
    val => {
      if (val !== localValue.value) localValue.value = val;
    },
  );

  const emitChange = () => {
    emit('update:modelValue', localValue.value);
    emit('change', localValue.value);
  };
</script>
