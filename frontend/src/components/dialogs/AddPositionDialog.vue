<!-- components/positions/AddPositionDialog.vue -->
<template>
  <Teleport to="body">
    <div v-if="open" class="fixed inset-0 z-[100]" role="dialog" aria-modal="true">
      <!-- Overlay -->
      <div class="absolute inset-0 bg-black/60 backdrop-blur-sm" @click="$emit('close')"></div>

      <!-- Centered panel -->
      <div class="absolute inset-0 flex items-center justify-center p-4">
        <div
          class="w-full max-w-lg rounded-xl border border-slate-700 bg-slate-900/95 p-6 text-slate-100 shadow-2xl"
          @keydown.esc="$emit('close')"
        >
          <div class="mb-4 flex items-center justify-between">
            <h2 class="text-lg font-semibold">Add Position</h2>
          </div>

          <!-- Error banner -->
          <p
            v-if="error"
            class="mb-3 rounded-md border border-red-600/50 bg-red-900/40 px-3 py-2 text-sm text-red-200"
          >
            {{ error }}
          </p>

          <form @submit.prevent="submit" class="space-y-4">
            <div>
              <label class="mb-1 block text-sm font-medium text-slate-200">Name</label>
              <input
                v-model.trim="form.name"
                type="text"
                class="w-full rounded-md border border-slate-700 bg-slate-800 p-2 text-white"
                :class="{ 'border-red-600': touched && !form.name }"
                autocomplete="off"
                required
              />
            </div>

            <div>
              <label class="mb-1 block text-sm font-medium text-slate-200">Code</label>
              <input
                v-model.trim="form.code"
                type="text"
                class="w-full rounded-md border border-slate-700 bg-slate-800 p-2 text-white"
                :class="{ 'border-red-600': touched && !form.code }"
                autocomplete="off"
                required
              />
            </div>

            <div class="flex items-center gap-2">
              <input
                id="requiresLicense"
                v-model="form.requiresLicense"
                type="checkbox"
                class="h-4 w-4 accent-indigo-600"
              />
              <label for="requiresLicense" class="text-sm text-slate-200">Requires License?</label>
            </div>

            <div class="mt-6 flex justify-end gap-3">
              <button
                type="button"
                class="rounded-md bg-slate-700 px-3 py-1.5 text-sm text-white hover:bg-slate-600"
                @click="$emit('close')"
              >
                Cancel
              </button>
              <button
                type="submit"
                :disabled="submitting"
                class="rounded-md bg-indigo-600 px-3 py-1.5 text-sm text-white transition hover:bg-indigo-500 disabled:cursor-not-allowed disabled:opacity-60"
              >
                {{ submitting ? 'Saving…' : 'Save' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
  import { extractApiError } from '@/api/error';
  import { positionService } from '@/api/positions/services';
  import { onBeforeUnmount, onMounted, ref, watch } from 'vue';

  const props = defineProps<{ open: boolean }>();
  const emit = defineEmits<{ (e: 'close'): void; (e: 'saved'): void }>();

  const form = ref({ name: '', code: '', requiresLicense: false });
  const submitting = ref(false);
  const touched = ref(false);
  const error = ref<string | null>(null);

  /* Escape key */
  const handleKeydown = (e: KeyboardEvent): void => {
    if (e.key === 'Escape') emit('close');
  };

  /* Reset form each time dialog opens */
  watch(
    () => props.open,
    isOpen => {
      if (isOpen) {
        form.value = { name: '', code: '', requiresLicense: false };
        submitting.value = false;
        touched.value = false;
        error.value = null;
        window.addEventListener('keydown', handleKeydown);
      } else {
        window.removeEventListener('keydown', handleKeydown);
      }
    },
    { immediate: true },
  );

  onMounted(() => {
    if (props.open) window.addEventListener('keydown', handleKeydown);
  });
  onBeforeUnmount(() => window.removeEventListener('keydown', handleKeydown));

  /* Submit */
  const submit = async () => {
    touched.value = true;
    error.value = null;

    if (!form.value.name || !form.value.code) {
      error.value = 'Please fill out Name and Code.';
      return;
    }

    if (submitting.value) return;
    submitting.value = true;

    try {
      await positionService.add(form.value); // POST /positions
      emit('saved'); // parent should close + refetch
    } catch (e: unknown) {
      // Prefer field-level errors when available; fall back to ProblemDetails/detail/message
      // Try "name" first, then "code" (your validator uses Name/Code keys server-side -> JSON name/code)
      let msg = extractApiError(e, 'name');
      if (!msg || msg === 'An unexpected error occurred.') {
        msg = extractApiError(e, 'code');
      }
      error.value = msg;
    } finally {
      submitting.value = false;
    }
  };
</script>
