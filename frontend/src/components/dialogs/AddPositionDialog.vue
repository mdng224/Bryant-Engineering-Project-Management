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
      <div>
        <label class="mb-1 block text-sm font-medium text-slate-200">Name</label>
        <input
          v-model.trim="form.name"
          type="text"
          class="w-full rounded-md border border-slate-700 bg-slate-800 p-2 text-white"
          :class="{ 'border-red-600': touched && !form.name }"
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
    </form>

    <!-- FOOTER BUTTONS (now sibling of the form) -->
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
  import AppAlert from '@/components/AppAlert.vue';
  import { AlertTriangle, Plus } from 'lucide-vue-next';
  import AppDialog from '../ui/AppDialog.vue';

  import { extractApiError } from '@/api/error';
  import type { AddPositionRequest } from '@/api/positions';
  import { positionService } from '@/api/positions/services';
  import { ref, watch } from 'vue';

  const props = defineProps<{ open: boolean }>();
  const emit = defineEmits<{ (e: 'close'): void; (e: 'saved'): void }>();

  const errorMessage = ref<string | null>(null);
  const submitting = ref(false);
  const touched = ref(false);
  const form = ref<AddPositionRequest>({ name: '', code: '', requiresLicense: false });

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

  const submit = async () => {
    touched.value = true;
    errorMessage.value = null;

    if (!form.value.name || !form.value.code) {
      errorMessage.value = 'Please fill out Name and Code.';
      return;
    }

    if (submitting.value) return;
    submitting.value = true;

    try {
      await positionService.add(form.value);
      emit('saved');
      emit('close');
    } catch (e) {
      let msg = extractApiError(e, 'name');
      if (!msg || msg === 'An unexpected error occurred.') {
        msg = extractApiError(e, 'code');
      }
      errorMessage.value = msg;
    } finally {
      submitting.value = false;
    }
  };
</script>
