<template>
  <app-dialog :open="open" :title="title" width="max-w-md" :loading="loading" @close="handleClose">
    <!-- Body -->
    <p v-if="message" class="text-slate-300">
      {{ message }}
    </p>

    <!-- Footer -->
    <template #footer>
      <button
        type="button"
        class="h-9 rounded-md border border-slate-700 bg-slate-800/70 px-3 text-sm hover:bg-slate-800 disabled:opacity-60"
        :disabled="loading"
        @click="handleClose"
      >
        {{ cancelLabel }}
      </button>

      <button
        ref="confirmBtn"
        type="button"
        class="inline-flex h-9 items-center gap-2 rounded-md px-3 text-sm font-medium text-white shadow transition disabled:cursor-not-allowed disabled:opacity-60"
        :class="confirmClass"
        :disabled="loading"
        :aria-busy="loading"
        @click="onConfirm"
      >
        <svg v-if="loading" class="h-4 w-4 animate-spin" viewBox="0 0 24 24" fill="none">
          <circle
            class="opacity-25"
            cx="12"
            cy="12"
            r="10"
            stroke="currentColor"
            stroke-width="4"
          />
          <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v4a4 4 0 00-4 4H4z" />
        </svg>
        <span>{{ confirmLabel }}</span>
      </button>
    </template>
  </app-dialog>
</template>

<script setup lang="ts">
  import { AppDialog } from '@/components/ui';
  import { computed, defineOptions, nextTick, ref, watch } from 'vue';

  defineOptions({ name: 'RestoreDialog' });

  type Props = {
    open: boolean;
    title?: string;
    message?: string;
    confirmLabel?: string;
    cancelLabel?: string;
    payload?: unknown;
    loading?: boolean;
    confirmColor?: 'emerald' | 'indigo' | 'red' | 'yellow';
  };

  const props = withDefaults(defineProps<Props>(), {
    title: 'Restore item',
    message: 'This will restore the previously deleted item and make it active again.',
    confirmLabel: 'Restore',
    cancelLabel: 'Cancel',
    confirmColor: 'emerald',
    loading: false,
  });

  const emit = defineEmits<{
    (e: 'confirm', payload?: unknown): void;
    (e: 'close'): void;
  }>();

  const confirmBtn = ref<HTMLButtonElement | null>(null);

  const focusDefault = async () => {
    await nextTick();
    confirmBtn.value?.focus();
  };

  watch(
    () => props.open,
    open => {
      if (open) focusDefault();
    },
  );

  const handleClose = () => {
    if (props.loading) return;
    emit('close');
  };

  const onConfirm = () => {
    if (props.loading) return;
    emit('confirm', props.payload);
  };

  /** Compute Tailwind color classes based on confirmColor */
  const confirmClass = computed(() => {
    const base = {
      emerald: 'bg-emerald-600 hover:bg-emerald-500',
      indigo: 'bg-indigo-600 hover:bg-indigo-500',
      red: 'bg-red-600 hover:bg-red-500',
      yellow: 'bg-yellow-600 hover:bg-yellow-500',
    };

    return base[props.confirmColor];
  });
</script>
