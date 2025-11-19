<template>
  <app-dialog :open="open" :title="title" width="max-w-md" :loading="loading" @close="handleClose">
    <p class="text-sm text-slate-300">
      {{ message }}
    </p>

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
        class="inline-flex h-9 items-center gap-2 rounded-md bg-red-600 px-3 text-sm font-medium text-white shadow hover:bg-red-500 disabled:cursor-not-allowed disabled:opacity-60"
        :disabled="loading"
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
  import { nextTick, ref, watch } from 'vue';

  type Props = {
    open: boolean;
    title?: string;
    message?: string;
    confirmLabel?: string;
    cancelLabel?: string;
    /** Anything you want back on confirm (id, full row, etc.) */
    payload?: unknown;
    /** Disable controls & show spinner while deleting */
    loading?: boolean;
  };

  const props = withDefaults(defineProps<Props>(), {
    title: 'Delete item',
    message: 'This action cannot be undone. This will permanently delete the selected item.',
    confirmLabel: 'Delete',
    cancelLabel: 'Cancel',
    loading: false,
  });

  const emit = defineEmits<{
    (e: 'confirm', payload?: unknown): void;
    (e: 'close'): void;
  }>();

  const confirmBtn = ref<HTMLButtonElement | null>(null);

  /** Focus the confirm button when the dialog opens */
  async function focusDefault() {
    await nextTick();
    confirmBtn.value?.focus();
  }

  watch(
    () => props.open,
    v => {
      if (v) focusDefault();
    },
  );

  /** prevent closing via overlay/esc while loading */
  function handleClose() {
    if (props.loading) return;
    emit('close');
  }

  function onConfirm() {
    if (props.loading) return;
    emit('confirm', props.payload);
  }
</script>
