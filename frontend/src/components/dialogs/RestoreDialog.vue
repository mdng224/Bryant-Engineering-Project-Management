<template>
  <app-dialog :open="open" :title="title" width="max-w-md" :loading="loading" @close="handleClose">
    <p class="text-slate-300">
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
        class="inline-flex h-9 items-center gap-2 rounded-md bg-emerald-600 px-3 text-sm font-medium text-white shadow hover:bg-emerald-500 disabled:cursor-not-allowed disabled:opacity-60"
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
  import { nextTick, ref, watch } from 'vue';
  import AppDialog from '../ui/AppDialog.vue';

  type Props = {
    open: boolean;
    title?: string;
    message?: string;
    confirmLabel?: string;
    cancelLabel?: string;
    payload?: unknown;
    loading?: boolean;
  };

  const props = withDefaults(defineProps<Props>(), {
    title: 'Restore item',
    message: 'This will restore the previously deleted item and make it active again.',
    confirmLabel: 'Restore',
    cancelLabel: 'Cancel',
    loading: false,
  });

  const emit = defineEmits<{
    (e: 'confirm', payload?: unknown): void;
    (e: 'close'): void;
  }>();

  const confirmBtn = ref<HTMLButtonElement | null>(null);

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

  function handleClose() {
    if (props.loading) return;
    emit('close');
  }

  function onConfirm() {
    if (props.loading) return;
    emit('confirm', props.payload);
  }
</script>
