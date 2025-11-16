<template>
  <app-dialog
    :open
    title="Add Client"
    width="max-w-2xl"
    :loading="submitting"
    @close="emit('close')"
  >
    <app-alert v-if="errorMessage" :message="errorMessage" variant="error" :icon="AlertTriangle" />

    <!-- FORM -->
    <form id="add-client-form" class="space-y-4" @submit.prevent="submit">
      <!-- Client name (required) -->
      <div>
        <label :class="labelClass">Client Name</label>
        <input
          v-model.trim="form.name"
          type="text"
          class="w-full rounded-md border border-slate-700 bg-slate-800 p-2 text-white"
          :class="{ 'border-red-600': touched && !form.name }"
          autocomplete="off"
          required
        />
      </div>

      <!-- Person name pieces (optional) -->
      <div class="grid grid-cols-1 gap-3 sm:grid-cols-4">
        <div>
          <label :class="labelClass">Prefix</label>
          <input
            v-model.trim="form.namePrefix"
            type="text"
            :class="formClass"
            placeholder="Mr, Ms, Dr..."
          />
        </div>

        <div class="sm:col-span-2">
          <label :class="labelClass">First Name</label>
          <input v-model.trim="form.firstName" type="text" :class="formClass" />
        </div>

        <div>
          <label :class="labelClass">Last Name</label>
          <input v-model.trim="form.lastName" type="text" :class="formClass" />
        </div>

        <div>
          <label :class="labelClass">Suffix</label>
          <input
            v-model.trim="form.nameSuffix"
            type="text"
            :class="formClass"
            placeholder="Jr, Sr, III..."
          />
        </div>
      </div>

      <!-- Contact info -->
      <div class="grid grid-cols-1 gap-3 sm:grid-cols-2">
        <div>
          <label class="mb-1 block text-sm text-slate-200">Email</label>
          <input
            v-model.trim="form.email"
            type="email"
            :class="formClass"
            placeholder="name@example.com"
            autocomplete="email"
          />
        </div>

        <div>
          <label class="mb-1 block text-sm text-slate-200">Phone</label>
          <input
            v-model.trim="form.phone"
            type="tel"
            :class="formClass"
            placeholder="(555) 555-5555"
            autocomplete="tel"
          />
        </div>
      </div>

      <!-- Notes -->
      <div>
        <label class="mb-1 block text-sm text-slate-200">Notes</label>
        <textarea
          v-model.trim="form.note"
          rows="3"
          class="w-full resize-y rounded-md border border-slate-700 bg-slate-800 p-2 text-sm text-white"
          placeholder="Optional notes about this client..."
        />
      </div>
    </form>

    <!-- FOOTER BUTTONS -->
    <template #footer>
      <button
        type="submit"
        form="add-client-form"
        :disabled="submitting"
        class="flex gap-2 rounded-md bg-indigo-600 px-3 py-2 text-sm text-white transition hover:bg-indigo-500 disabled:cursor-not-allowed disabled:opacity-60"
      >
        <Plus class="mt-[2px] h-4 w-4" />
        {{ submitting ? 'Adding...' : 'Add' }}
      </button>
    </template>
  </app-dialog>
</template>

<script setup lang="ts">
  import AppAlert from '@/components/AppAlert.vue';
  import { AlertTriangle, Plus } from 'lucide-vue-next';
  import AppDialog from '../ui/AppDialog.vue';

  import type { AddClientRequest } from '@/api/clients';
  import { clientService } from '@/api/clients/services';
  import { extractApiError } from '@/api/error';
  import { ref, watch } from 'vue';

  const labelClass = 'mb-1 block text-sm font-medium text-slate-200';
  const formClass = 'w-full rounded-md border border-slate-700 bg-slate-800 p-2 text-sm text-white';
  const props = defineProps<{ open: boolean }>();
  const emit = defineEmits<{ (e: 'close'): void; (e: 'saved'): void }>();

  const errorMessage = ref<string | null>(null);
  const submitting = ref(false);
  const touched = ref(false);
  const form = ref<AddClientRequest>({
    name: '',
    namePrefix: '',
    firstName: '',
    lastName: '',
    nameSuffix: '',
    email: '',
    phone: '',
    address: null,
    note: '',
  });

  watch(
    () => props.open,
    isOpen => {
      if (isOpen) {
        form.value = {
          name: '',
          namePrefix: '',
          firstName: '',
          lastName: '',
          nameSuffix: '',
          email: '',
          phone: '',
          address: null,
          note: '',
        };
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

    if (!form.value.name) {
      errorMessage.value = 'Please fill out Client Name.';
      return;
    }

    if (submitting.value) return;
    submitting.value = true;

    try {
      await clientService.add(form.value);
      emit('saved');
      emit('close');
    } catch (e) {
      let msg = extractApiError(e, 'name');
      errorMessage.value =
        !msg || msg === 'An unexpected error occurred.' ? 'An unexpected error occurred.' : msg;
    } finally {
      submitting.value = false;
    }
  };
</script>
