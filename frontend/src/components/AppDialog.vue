<!-- components/ui/AppDialog.vue -->
<template>
  <Teleport to="body">
    <div v-if="open" class="fixed inset-0 z-[100]" role="dialog" aria-modal="true">
      <!-- Overlay -->
      <div class="absolute inset-0 bg-black/60 backdrop-blur-sm" @click="$emit('close')"></div>

      <!-- Panel -->
      <div class="absolute inset-0 flex items-center justify-center p-4">
        <div
          class="w-full max-w-lg rounded-xl border border-slate-700 bg-slate-900/95 p-6 text-slate-100 shadow-2xl"
        >
          <div class="mb-4 flex items-center justify-between">
            <h2 class="text-lg font-semibold">
              <slot name="title" />
            </h2>
            <button
              class="rounded-md px-2 py-1 text-sm text-slate-300 hover:bg-slate-700/70"
              @click="$emit('close')"
            >
              <X class="block h-5 w-5" />
            </button>
          </div>

          <!-- optional error slot -->
          <slot name="error" />

          <!-- body -->
          <div>
            <slot />
          </div>

          <!-- footer -->
          <div class="mt-6 flex justify-end gap-3">
            <slot name="footer" />
          </div>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
  import { X } from 'lucide-vue-next';

  defineProps<{ open: boolean }>();
  defineEmits<{ (e: 'close'): void }>();
</script>
