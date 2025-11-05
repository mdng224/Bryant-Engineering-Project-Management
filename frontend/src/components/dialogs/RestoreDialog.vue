<template>
  <Teleport to="body">
    <div
      v-if="open"
      class="fixed inset-0 z-[100]"
      role="dialog"
      aria-modal="true"
      :aria-label="title"
      @click="onBackdropClick"
    >
      <!-- Backdrop -->
      <div class="absolute inset-0 bg-black/60 backdrop-blur-sm"></div>

      <!-- Panel -->
      <div class="absolute inset-0 flex items-center justify-center p-4">
        <div
          ref="panelEl"
          class="w-full max-w-md rounded-xl border border-slate-700 bg-slate-900/95 p-6 text-slate-100 shadow-2xl"
        >
          <h2 class="text-base font-semibold">{{ title }}</h2>
          <p class="mt-2 text-sm text-slate-300">
            {{ message }}
          </p>

          <div class="mt-6 flex items-center justify-end gap-2">
            <button
              type="button"
              class="h-9 rounded-md border border-slate-700 bg-slate-800/70 px-3 text-sm hover:bg-slate-800 disabled:opacity-60"
              :disabled="loading"
              @click="emit('close')"
            >
              {{ cancelLabel }}
            </button>

            <button
              ref="confirmBtn"
              type="button"
              class="inline-flex h-9 items-center gap-2 rounded-md bg-emerald-600 px-3 text-sm font-medium text-white shadow hover:bg-emerald-500 disabled:cursor-not-allowed disabled:opacity-60"
              :disabled="loading"
              @click="emit('confirm', payload)"
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
                <path
                  class="opacity-75"
                  fill="currentColor"
                  d="M4 12a8 8 0 018-8v4a4 4 0 00-4 4H4z"
                />
              </svg>
              <span>{{ confirmLabel }}</span>
            </button>
          </div>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
  import { nextTick, onMounted, onUnmounted, ref, watch } from 'vue';

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

  const panelEl = ref<HTMLDivElement | null>(null);
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

  function onBackdropClick(e: MouseEvent) {
    if (props.loading) return;
    if (e.target === e.currentTarget) emit('close');
  }

  function onEsc(e: KeyboardEvent) {
    if (!props.open || props.loading) return;
    if (e.key === 'Escape') emit('close');
  }

  onMounted(() => window.addEventListener('keydown', onEsc));
  onUnmounted(() => window.removeEventListener('keydown', onEsc));
</script>
