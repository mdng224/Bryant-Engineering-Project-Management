<!-- src/components/ui/AppDialog.vue -->
<template>
  <Teleport to="body">
    <div v-if="open" class="fixed inset-0 z-[100]" role="dialog" aria-modal="true">
      <!-- Overlay -->
      <div class="absolute inset-0 bg-black/60 backdrop-blur-sm" @click="handleClose" />

      <!-- Panel -->
      <div class="absolute inset-0 flex items-center justify-center p-4">
        <div
          class="w-full rounded-xl border border-slate-700 bg-slate-900/95 p-6 text-slate-100 shadow-2xl"
          :class="width"
        >
          <!-- Header -->
          <header class="relative mb-4 flex items-center justify-center">
            <!-- Centered title -->
            <div class="flex-1 text-center">
              <slot name="title">
                <h2 class="text-2xl font-semibold">{{ title }}</h2>
              </slot>
            </div>

            <!-- Close button pinned to the right -->
            <button
              class="absolute right-0 top-1/2 -translate-y-1/2 rounded-md p-1.5 text-slate-300 hover:bg-slate-700/70"
              type="button"
              aria-label="Close dialog"
              @click="handleClose"
            >
              <X class="block h-5 w-5" />
            </button>
          </header>

          <!-- Default content -->
          <div>
            <slot />
          </div>

          <!-- Footer -->
          <footer v-if="$slots.footer" class="mt-6 flex justify-end gap-3">
            <slot name="footer" :loading="loading" />
          </footer>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
  import { X } from 'lucide-vue-next';
  import { onBeforeUnmount, onMounted, watch } from 'vue';

  const props = defineProps<{
    open: boolean;
    title?: string;
    width?: string;
    loading?: boolean;
  }>();

  const emit = defineEmits<{
    (e: 'close'): void;
    (e: 'confirm'): void;
  }>();

  const handleClose = () => emit('close');

  const handleKeydown = (e: KeyboardEvent) => {
    if (e.key === 'Escape') handleClose();
  };

  watch(
    () => props.open,
    isOpen => {
      if (isOpen) window.addEventListener('keydown', handleKeydown);
      else window.removeEventListener('keydown', handleKeydown);
    },
    { immediate: true },
  );

  onMounted(() => {
    if (props.open) window.addEventListener('keydown', handleKeydown);
  });

  onBeforeUnmount(() => window.removeEventListener('keydown', handleKeydown));
</script>
