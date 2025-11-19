<template>
  <app-dialog
    :open
    title="Add Position"
    width="max-w-lg"
    :loading="submitting"
    @close="emit('close')"
  >
    <app-alert v-if="errorMessage" :message="errorMessage" variant="error" :icon="AlertTriangle" />

    <!-- FORM -->
    <form id="add-position-form" class="space-y-4" @submit.prevent="submit">
      <p class="text-xs text-slate-400">
        <span class="text-rose-400">*</span>
        Required field.
      </p>

      <!-- Name -->
      <div>
        <label :class="labelClass">
          Name
          <span class="text-rose-400">*</span>
        </label>
        <input
          v-model.trim="form.name"
          type="text"
          :class="[formClass, { 'border-red-600': touched && !form.name }]"
          autocomplete="off"
          required
        />
      </div>

      <!-- Code -->
      <div>
        <label :class="labelClass">
          Code
          <span class="text-rose-400">*</span>
        </label>
        <input
          v-model.trim="form.code"
          type="text"
          :class="[formClass, { 'border-red-600': touched && !form.code }]"
          autocomplete="off"
          required
        />
      </div>

      <!-- Requires license -->
      <div class="flex items-center gap-2">
        <input
          id="requiresLicense"
          v-model="form.requiresLicense"
          type="checkbox"
          class="h-4 w-4 accent-indigo-600"
        />
        <label for="requiresLicense" class="text-sm text-slate-200">Requires License?</label>
      </div>
    </form>

    <!-- FOOTER BUTTONS -->
    <template #footer>
      <button
        type="submit"
        form="add-position-form"
        :disabled="submitting"
        class="flex gap-2 rounded-md bg-indigo-600 px-3 py-2 text-sm text-white transition hover:bg-indigo-500 disabled:cursor-not-allowed disabled:opacity-60"
      >
        <Plus class="h-4 w-4" />
        {{ submitting ? 'Creating...' : 'Create' }}
      </button>
    </template>
  </app-dialog>
</template>

<script setup lang="ts">
  // Vue
  import { ref, watch } from 'vue';

  // Icons & UI
  import { AppDialog } from '@/components/ui';
  import AppAlert from '@/components/ui/AppAlert.vue';
  import { AlertTriangle, Plus } from 'lucide-vue-next';

  // API & types
  import { extractApiError } from '@/api/error';
  import type { AddPositionRequest } from '@/api/positions';
  import { positionService } from '@/api/positions/services';

  const labelClass = 'mb-1 block text-sm font-medium text-slate-200';
  const formClass = 'w-full rounded-md border border-slate-700 bg-slate-800 p-2 text-sm text-white';

  const props = defineProps<{ open: boolean }>();
  const emit = defineEmits<{ (e: 'close'): void; (e: 'saved'): void }>();

  const errorMessage = ref<string | null>(null);
  const submitting = ref(false);
  const touched = ref(false);

  const form = ref<AddPositionRequest>({
    name: '',
    code: '',
    requiresLicense: false,
  });

  // Reset when dialog opens
  watch(
    () => props.open,
    isOpen => {
      if (isOpen) {
        form.value = { name: '', code: '', requiresLicense: false };
        submitting.value = false;
        touched.value = false;
        errorMessage.value = null;
      }
    },
    { immediate: true },
  );

  const submit = async (): Promise<void> => {
    touched.value = true;
    errorMessage.value = null;

    if (submitting.value) return;
    submitting.value = true;

    try {
      await positionService.add(form.value);
      emit('saved');
      emit('close');
    } catch (e) {
      let msg = extractApiError(e, 'name');
      if (!msg) {
        msg = extractApiError(e, 'code');
      }
      errorMessage.value = msg;
    } finally {
      submitting.value = false;
    }
  };
</script>
