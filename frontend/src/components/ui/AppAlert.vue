<!-- components/ui/AppAlert.vue -->
<template>
  <p
    v-if="message || $slots.default"
    class="mb-4 flex items-center gap-2 rounded-lg border px-3.5 py-2 text-sm leading-tight"
    :class="variantClasses"
    role="alert"
    aria-live="assertive"
    tabindex="-1"
  >
    <component
      :is="icon"
      v-if="icon"
      class="block h-4 w-4 shrink-0 self-center"
      aria-hidden="true"
    />
    <span>
      <slot>{{ message }}</slot>
    </span>
  </p>
</template>

<script setup lang="ts">
  import { computed } from 'vue';

  const props = defineProps<{
    message?: string | null;
    variant?: 'error' | 'success' | 'info' | 'warning';
    icon?: any;
  }>();

  const variantClasses = computed(() => {
    switch (props.variant) {
      case 'success':
        return 'border-emerald-700 bg-emerald-900/40 text-emerald-100';
      case 'info':
        return 'border-sky-700 bg-sky-900/40 text-sky-100';
      case 'warning':
        return 'border-amber-700 bg-amber-900/40 text-amber-100';
      case 'error':
      default:
        return 'border-rose-800 bg-rose-900/30 text-rose-200';
    }
  });
</script>
