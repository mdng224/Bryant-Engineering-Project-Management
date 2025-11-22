<template>
  <app-dialog
    :open
    title="Add Client"
    width="max-w-5xl"
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

      <!-- Client category & type (required)-->
      <div class="grid grid-cols-1 gap-3 sm:grid-cols-2">
        <div>
          <label class="mb-1 block text-sm text-slate-200">
            Client Category
            <span class="text-rose-400">*</span>
          </label>
          <select
            v-model="form.clientCategoryId"
            class="w-full rounded-md border border-slate-700 bg-slate-800 p-2 text-sm text-slate-100"
            required
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
            required
          >
            <option value="">Select type...</option>
            <option v-for="t in filteredTypes" :key="t.id" :value="t.id">
              {{ t.name }}
            </option>
          </select>
        </div>
      </div>

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
          placeholder="Acme Construction"
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
          <input
            v-model.trim="form.firstName"
            type="text"
            :class="formClass"
            required
            placeholder="John"
          />
        </div>

        <div>
          <label :class="labelClass">Middle Name</label>
          <input
            v-model.trim="form.middleName"
            type="text"
            :class="formClass"
            placeholder="A. (optional)"
          />
        </div>

        <div>
          <label :class="labelClass">
            Last Name
            <span class="text-rose-400">*</span>
          </label>
          <input
            v-model.trim="form.lastName"
            type="text"
            :class="formClass"
            required
            placeholder="Smith"
          />
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

      <!-- Notes -->
      <div>
        <label class="mb-1 block pt-2 text-sm text-slate-200">Notes</label>
        <textarea
          v-model.trim="form.note"
          rows="3"
          class="w-full resize-y rounded-md border border-slate-700 bg-slate-800 p-2 text-sm text-white"
          placeholder="Optional notes about this client..."
        />
      </div>

      <!-- Address (optional to add, all-or-nothing once enabled) -->
      <div class="border-b border-t border-slate-700 py-6">
        <div class="mb-2 flex items-center justify-between">
          <h3 class="text-sm font-medium text-slate-200">Address</h3>

          <button
            v-if="!showAddress"
            type="button"
            class="text-xs font-medium text-indigo-400 hover:text-indigo-300"
            @click="enableAddress"
          >
            + Add address
          </button>

          <button
            v-else
            type="button"
            class="text-xs font-medium text-slate-400 hover:text-slate-100"
            @click="clearAddress"
          >
            Remove address
          </button>
        </div>

        <div v-if="showAddress" class="space-y-3">
          <p class="text-xs text-slate-400">
            Address is optional, but if added, all fields except Line 2 are required.
          </p>

          <div>
            <label :class="labelClass">
              Address Line 1
              <span class="text-rose-400">*</span>
            </label>
            <input
              v-model.trim="form.address!.line1"
              type="text"
              autocomplete="address-line1"
              placeholder="123 Main St"
              required
              :class="[formClass, { 'border-red-600': touched && !form.address?.line1 }]"
            />
          </div>

          <div>
            <label :class="labelClass">Address Line 2</label>
            <input
              v-model.trim="form.address!.line2"
              type="text"
              autocomplete="address-line2"
              placeholder="Apt 4B (optional)"
              :class="formClass"
            />
          </div>

          <div class="grid grid-cols-1 gap-3 sm:grid-cols-3">
            <div>
              <label :class="labelClass">
                City
                <span class="text-rose-400">*</span>
              </label>
              <input
                v-model.trim="form.address!.city"
                type="text"
                autocomplete="address-level2"
                placeholder="Owensboro"
                required
                :class="[formClass, { 'border-red-600': touched && !form.address?.city }]"
              />
            </div>

            <div>
              <label :class="labelClass">
                State
                <span class="text-rose-400">*</span>
              </label>
              <input
                v-model.trim="form.address!.state"
                type="text"
                autocomplete="address-level1"
                placeholder="KY"
                required
                :class="[formClass, { 'border-red-600': touched && !form.address?.state }]"
              />
            </div>

            <div>
              <label :class="labelClass">
                Postal Code
                <span class="text-rose-400">*</span>
              </label>
              <input
                v-model.trim="form.address!.postalCode"
                type="text"
                autocomplete="postal-code"
                required
                placeholder="42303"
                :class="[formClass, { 'border-red-600': touched && !form.address?.postalCode }]"
              />
            </div>
          </div>
        </div>
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
  import type { AddClientRequest } from '@/api/clients';
  import { clientService } from '@/api/clients/services';
  import { extractApiError } from '@/api/error';
  import { AppDialog } from '@/components/ui';
  import AppAlert from '@/components/ui/AppAlert.vue';
  import { useClientLookups } from '@/composables/useClientLookups';
  import { useDialogForm } from '@/composables/useDialogForm';
  import { AlertTriangle, Plus } from 'lucide-vue-next';
  import { computed, onMounted, ref, watch } from 'vue';

  const labelClass = 'mb-1 block text-sm font-medium text-slate-200';
  const formClass = 'w-full rounded-md border border-slate-700 bg-slate-800 p-2 text-sm text-white';

  const props = defineProps<{ open: boolean }>();
  const emit = defineEmits<{ (e: 'close'): void; (e: 'saved'): void }>();

  const showAddress = ref(false);

  const createInitialForm = (): AddClientRequest => ({
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

  const { form, submitting, errorMessage, touched, reset } =
    useDialogForm<AddClientRequest>(createInitialForm);

  const enableAddress = () => {
    if (!form.value.address) {
      form.value.address = {
        line1: '',
        line2: '',
        city: '',
        state: '',
        postalCode: '',
      };
    }
    showAddress.value = true;
  };

  const clearAddress = () => {
    showAddress.value = false;
    form.value.address = null;
  };

  // Lookups (shared composable)
  const { categories, loadLookups, getTypesForCategory } = useClientLookups();
  const filteredTypes = computed(() => getTypesForCategory(form.value.clientCategoryId || null));

  // Reset when dialog opens
  watch(
    () => props.open,
    isOpen => {
      if (isOpen) {
        reset();
        showAddress.value = false;
      }
    },
    { immediate: true },
  );

  onMounted(loadLookups);

  const errors = {
    contact: 'Please enter either an email or a phone number.',
    unexpected: 'An unexpected error occurred.',
  };

  const submit = async (): Promise<void> => {
    touched.value = true;
    errorMessage.value = null;

    if (!form.value.email && !form.value.phone) {
      errorMessage.value = errors.contact;
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
