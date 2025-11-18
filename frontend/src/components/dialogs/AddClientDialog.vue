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
      <p class="text-xs text-slate-400">
        <span class="text-rose-400">*</span>
        Required field. Email or phone is also required.
      </p>

      <!-- Client name (required) -->
      <div>
        <label :class="labelClass">
          Client Name
          <span class="text-rose-400">*</span>
        </label>
        <input
          v-model.trim="form.name"
          type="text"
          class="w-full rounded-md border border-slate-700 bg-slate-800 p-2 text-white"
          :class="{ 'border-red-600': touched && !form.name }"
          autocomplete="off"
          required
        />
      </div>

      <!-- Person name pieces -->
      <div class="grid grid-cols-1 gap-3 sm:grid-cols-5">
        <div>
          <label :class="labelClass">Prefix</label>
          <input
            v-model.trim="form.namePrefix"
            type="text"
            :class="formClass"
            placeholder="Mr, Ms, Dr..."
          />
        </div>

        <div>
          <label :class="labelClass">
            First Name
            <span class="text-rose-400">*</span>
          </label>
          <input v-model.trim="form.firstName" type="text" :class="formClass" />
        </div>

        <div>
          <label :class="labelClass">Middle Name</label>
          <input v-model.trim="form.middleName" type="text" :class="formClass" />
        </div>

        <div>
          <label :class="labelClass">
            Last Name
            <span class="text-rose-400">*</span>
          </label>
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
          <label class="mb-1 block text-sm text-slate-200">
            Email
            <span class="text-rose-400" title="Email or phone required">*</span>
          </label>
          <input
            v-model.trim="form.email"
            type="email"
            :class="[formClass, { 'border-red-600': touched && !form.email && !form.phone }]"
            placeholder="name@example.com"
            autocomplete="email"
          />
        </div>

        <div>
          <label class="mb-1 block text-sm text-slate-200">
            Phone
            <span class="text-rose-400" title="Email or phone required">*</span>
          </label>
          <input
            v-model.trim="form.phone"
            type="tel"
            :class="[formClass, { 'border-red-600': touched && !form.email && !form.phone }]"
            placeholder="(555) 555-5555"
            autocomplete="tel"
          />
        </div>
      </div>

      <div>
        <p class="mb-2 text-xs italic text-slate-400">
          <span class="text-rose-400">*</span>
          At least one contact method is required: email or phone.
        </p>
      </div>

      <!-- Client category & type -->
      <div class="grid grid-cols-1 gap-3 sm:grid-cols-2">
        <div>
          <label class="mb-1 block text-sm text-slate-200">
            Client Category
            <span class="text-rose-400">*</span>
          </label>
          <select
            v-model="form.clientCategoryId"
            class="w-full rounded-md border border-slate-700 bg-slate-800 p-2 text-sm text-slate-100"
          >
            <option value="">Select category...</option>
            <option v-for="cat in categories" :key="cat.id" :value="cat.id">
              {{ cat.name }}
            </option>
          </select>
        </div>

        <div>
          <label class="mb-1 block text-sm text-slate-200">
            Client Type
            <span class="text-rose-400">*</span>
          </label>
          <select
            v-model="form.clientTypeId"
            class="w-full rounded-md border border-slate-700 bg-slate-800 p-2 text-sm text-slate-100"
          >
            <option value="">Select type...</option>
            <option v-for="t in filteredTypes" :key="t.id" :value="t.id">
              {{ t.name }}
            </option>
          </select>
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
  // Vue
  import { computed, onMounted, ref, watch } from 'vue';

  // Icons & UI
  import AppAlert from '@/components/AppAlert.vue';
  import { AlertTriangle, Plus } from 'lucide-vue-next';
  import AppDialog from '../ui/AppDialog.vue';

  // API & types
  import type { AddClientRequest } from '@/api/clients';
  import { clientService } from '@/api/clients/services';
  import { extractApiError } from '@/api/error';

  // Composables
  import { useClientLookups } from '@/composables/useClientLookups';

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
    middleName: '',
    lastName: '',
    nameSuffix: '',
    email: '',
    phone: '',
    address: null,
    note: '',
    clientCategoryId: '',
    clientTypeId: '',
  });

  // Lookups (shared composable)
  const { categories, loadLookups, getTypesForCategory } = useClientLookups();
  const filteredTypes = computed(() => getTypesForCategory(form.value.clientCategoryId || null));

  // Reset when dialog opens
  watch(
    () => props.open,
    isOpen => {
      if (isOpen) {
        form.value = {
          name: '',
          namePrefix: '',
          firstName: '',
          middleName: '',
          lastName: '',
          nameSuffix: '',
          email: '',
          phone: '',
          address: null,
          note: '',
          clientCategoryId: '',
          clientTypeId: '',
        };
        submitting.value = false;
        touched.value = false;
        errorMessage.value = null;
      }
    },
    { immediate: true },
  );

  onMounted(loadLookups);

  const errors = {
    name: 'Please fill out Client Name.',
    firstName: 'Please fill out First Name.',
    lastName: 'Please fill out Last Name.',
    contact: 'Please enter either an email or a phone number.',
    category: 'Please select a client category.',
    type: 'Please select a client type.',
    unexpected: 'An unexpected error occurred.',
  };

  const submit = async (): Promise<void> => {
    touched.value = true;
    errorMessage.value = null;

    if (!form.value.name) {
      errorMessage.value = errors.name;
      return;
    }

    if (!form.value.firstName) {
      errorMessage.value = errors.firstName;
      return;
    }

    if (!form.value.lastName) {
      errorMessage.value = errors.lastName;
      return;
    }

    if (!form.value.email && !form.value.phone) {
      errorMessage.value = errors.contact;
      return;
    }

    if (!form.value.clientCategoryId) {
      errorMessage.value = errors.category;
      return;
    }

    if (!form.value.clientTypeId) {
      errorMessage.value = errors.type;
      return;
    }

    if (submitting.value) return;
    submitting.value = true;

    try {
      await clientService.add(form.value);
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
