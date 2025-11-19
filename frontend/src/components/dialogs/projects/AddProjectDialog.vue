<!-- src/components/dialogs/projects/AddProjectDialog.vue -->
<template>
  <app-dialog
    :open
    title="Add Project"
    width="max-w-3xl"
    :loading="submitting"
    @close="emit('close')"
  >
    <app-alert v-if="errorMessage" :message="errorMessage" variant="error" :icon="AlertTriangle" />

    <!-- FORM -->
    <form id="add-project-form" class="space-y-4" @submit.prevent="submit">
      <p class="text-xs text-slate-400">
        <span class="text-rose-400">*</span>
        Required field.
      </p>

      <!-- Basic info -->
      <div class="grid grid-cols-1 gap-3 sm:grid-cols-2">
        <div>
          <label :class="labelClass">
            Project Name
            <span class="text-rose-400">*</span>
          </label>
          <input
            v-model.trim="form.name"
            type="text"
            :class="[formClass, { 'border-red-600': touched && !form.name }]"
            autocomplete="off"
            placeholder="Water Main Replacement – Elm St."
            required
          />
        </div>

        <div>
          <label :class="labelClass">
            Project Code
            <span class="text-rose-400">*</span>
          </label>
          <input
            v-model.trim="form.code"
            type="text"
            :class="[formClass, { 'border-red-600': touched && !form.code }]"
            autocomplete="off"
            placeholder="2025-034"
            required
          />
        </div>
      </div>

      <!-- Year / Number (optional if code already encodes this) -->
      <div class="grid grid-cols-1 gap-3 sm:grid-cols-3">
        <div>
          <label :class="labelClass">Year</label>
          <input
            v-model.number="form.year"
            type="number"
            :class="formClass"
            min="1900"
            max="2100"
            placeholder="2025"
          />
        </div>

        <div>
          <label :class="labelClass">Number</label>
          <input
            v-model.number="form.number"
            type="number"
            :class="formClass"
            min="1"
            placeholder="34"
          />
        </div>

        <div>
          <label :class="labelClass">Location</label>
          <input
            v-model.trim="form.location"
            type="text"
            :class="formClass"
            placeholder="Denver, CO"
          />
        </div>
      </div>

      <!-- Client / Scope (IDs only for now; can wire up full lookups later) -->
      <div class="grid grid-cols-1 gap-3 sm:grid-cols-2">
        <div>
          <label :class="labelClass">Client ID</label>
          <input
            v-model.trim="form.clientId"
            type="text"
            :class="formClass"
            placeholder="Paste from client details…"
          />
          <p class="mt-1 text-xs text-slate-500">
            Link this project to an existing client (optional for now).
          </p>
        </div>

        <div>
          <label :class="labelClass">Scope ID</label>
          <input
            v-model.trim="form.scopeId"
            type="text"
            :class="formClass"
            placeholder="Scope GUID (optional)"
          />
        </div>
      </div>

      <!-- Manager / Type -->
      <div class="grid grid-cols-1 gap-3 sm:grid-cols-2">
        <div>
          <label :class="labelClass">
            Project Manager
            <span class="text-rose-400">*</span>
          </label>
          <select
            v-model="form.manager"
            :class="[formClass, 'pr-8', { 'border-red-600': touched && !form.manager }]"
            required
          >
            <option value="">Select manager...</option>
            <option v-for="m in managers" :key="m" :value="m">
              {{ m }}
            </option>
          </select>
        </div>

        <div>
          <label :class="labelClass">Project Type</label>
          <input
            v-model.trim="form.type"
            type="text"
            :class="formClass"
            placeholder="Water, Sewer, Roadway..."
          />
        </div>
      </div>
    </form>

    <!-- FOOTER BUTTONS -->
    <template #footer>
      <button
        type="submit"
        form="add-project-form"
        :disabled="submitting"
        class="flex gap-2 rounded-md bg-indigo-600 px-3 py-2 text-sm text-white transition hover:bg-indigo-500 disabled:cursor-not-allowed disabled:opacity-60"
      >
        <Plus class="mt-[2px] h-4 w-4" />
        {{ submitting ? 'Adding...' : 'Add Project' }}
      </button>
    </template>
  </app-dialog>
</template>

<script setup lang="ts">
  import { extractApiError } from '@/api/error';
  import type { AddProjectRequest } from '@/api/projects';
  import { projectService } from '@/api/projects';
  import AppAlert from '@/components/ui/AppAlert.vue';
  import AppDialog from '@/components/ui/AppDialog.vue';
  import { useProjectLookups } from '@/composables/useProjectLookups';
  import { AlertTriangle, Plus } from 'lucide-vue-next';
  import { onMounted, ref, watch } from 'vue';

  const labelClass = 'mb-1 block text-sm font-medium text-slate-200';
  const formClass = 'w-full rounded-md border border-slate-700 bg-slate-800 p-2 text-sm text-white';

  const props = defineProps<{ open: boolean }>();
  const emit = defineEmits<{ (e: 'close'): void; (e: 'saved'): void }>();

  const errorMessage = ref<string | null>(null);
  const submitting = ref(false);
  const touched = ref(false);

  // NOTE: Shape of AddProjectRequest should be defined in src/api/projects/contracts.ts
  // e.g.:
  // export type AddProjectRequest = {
  //   code: string;
  //   name: string;
  //   clientId: string | null;
  //   scopeId: string | null;
  //   manager: string;
  //   type: string | null;
  //   location: string | null;
  //   year: number | null;
  //   number: number | null;
  // };

  const form = ref<AddProjectRequest>({
    code: '',
    name: '',
    clientId: null,
    scopeId: null,
    manager: '',
    type: null,
    location: null,
    year: null,
    number: null,
  });

  // Lookups
  const { managers, loadLookups } = useProjectLookups();

  // Reset when dialog opens
  const resetForm = () => {
    form.value = {
      code: '',
      name: '',
      clientId: null,
      scopeId: null,
      manager: '',
      type: null,
      location: null,
      year: null,
      number: null,
    };
    submitting.value = false;
    touched.value = false;
    errorMessage.value = null;
  };

  watch(
    () => props.open,
    isOpen => {
      if (isOpen) {
        resetForm();
      }
    },
    { immediate: true },
  );

  onMounted(loadLookups);

  const errors = {
    required: 'Please fill in all required fields.',
    unexpected: 'An unexpected error occurred.',
  };

  const submit = async (): Promise<void> => {
    touched.value = true;
    errorMessage.value = null;

    if (!form.value.name || !form.value.code || !form.value.manager) {
      errorMessage.value = errors.required;
      return;
    }

    if (submitting.value) return;
    submitting.value = true;

    try {
      await projectService.add(form.value);
      emit('saved');
      emit('close');
    } catch (e) {
      const msg = extractApiError(e, 'name');
      errorMessage.value = msg || errors.unexpected;
    } finally {
      submitting.value = false;
    }
  };
</script>
