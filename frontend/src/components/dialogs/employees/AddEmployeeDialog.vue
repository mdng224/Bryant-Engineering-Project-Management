<!-- src/components/dialogs/AddEmployeeDialog.vue -->
<template>
  <app-dialog
    :open
    title="Add Employee"
    width="max-w-2xl"
    :loading="submitting"
    @close="emit('close')"
  >
    <app-alert v-if="errorMessage" :message="errorMessage" variant="error" :icon="AlertTriangle" />

    <!-- FORM -->
    <form id="add-employee-form" class="space-y-4" @submit.prevent="submit">
      <p class="text-xs text-slate-400">
        <span class="text-rose-400">*</span>
        Required field.
      </p>

      <!-- Name block -->
      <div class="grid grid-cols-1 gap-3 sm:grid-cols-3">
        <div>
          <label :class="labelClass">
            First Name
            <span class="text-rose-400">*</span>
          </label>
          <input
            v-model.trim="form.firstName"
            type="text"
            :class="[formClass, { 'border-red-600': touched && !form.firstName }]"
            placeholder="Jane"
            autocomplete="given-name"
            required
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
            :class="[formClass, { 'border-red-600': touched && !form.lastName }]"
            placeholder="Doe"
            autocomplete="family-name"
            required
          />
        </div>

        <div>
          <label :class="labelClass">Preferred Name</label>
          <input
            v-model.trim="form.preferredName"
            type="text"
            :class="formClass"
            placeholder="Optional nickname"
            autocomplete="off"
          />
        </div>
      </div>

      <!-- Employment info -->
      <div class="grid grid-cols-1 gap-3 sm:grid-cols-3">
        <div>
          <label :class="labelClass">Employment Type</label>
          <select v-model="form.employmentType" :class="formClass">
            <option value="">Select type...</option>
            <option v-for="opt in employmentTypeOptions" :key="opt.value" :value="opt.value">
              {{ opt.label }}
            </option>
          </select>
        </div>

        <div>
          <label :class="labelClass">Salary Type</label>
          <select v-model="form.salaryType" :class="formClass">
            <option value="">Select pay...</option>
            <option v-for="opt in salaryTypeOptions" :key="opt.value" :value="opt.value">
              {{ opt.label }}
            </option>
          </select>
        </div>

        <div>
          <label :class="labelClass">Department</label>
          <select v-model="form.department" :class="formClass">
            <option value="">Select department...</option>
            <option v-for="opt in departmentOptions" :key="opt.value" :value="opt.value">
              {{ opt.label }}
            </option>
          </select>
        </div>
      </div>

      <!-- Dates -->
      <div class="grid grid-cols-1 gap-3 sm:grid-cols-2">
        <div>
          <label :class="labelClass">Hire Date</label>
          <input v-model="form.hireDate" type="date" :class="formClass" />
        </div>
      </div>

      <!-- Contact / work info -->
      <div class="grid grid-cols-1 gap-3 sm:grid-cols-2">
        <div>
          <label :class="labelClass">Company Email</label>
          <input
            v-model.trim="form.companyEmail"
            type="email"
            :class="formClass"
            placeholder="name@company.com"
            autocomplete="email"
            required
          />
        </div>

        <div>
          <label :class="labelClass">Work Location</label>
          <input
            v-model.trim="form.workLocation"
            type="text"
            :class="formClass"
            placeholder="Office, city, etc."
            autocomplete="off"
          />
        </div>
      </div>

      <!-- Notes -->
      <div class="grid grid-cols-1 gap-3 sm:grid-cols-2">
        <div>
          <label :class="labelClass">License Notes</label>
          <textarea
            v-model.trim="form.licenseNotes"
            rows="3"
            class="w-full resize-y rounded-md border border-slate-700 bg-slate-800 p-2 text-sm text-white"
            placeholder="Any license / certification notes..."
          />
        </div>

        <div>
          <label :class="labelClass">Notes</label>
          <textarea
            v-model.trim="form.notes"
            rows="3"
            class="w-full resize-y rounded-md border border-slate-700 bg-slate-800 p-2 text-sm text-white"
            placeholder="Internal notes about this employee..."
          />
        </div>
      </div>
    </form>

    <!-- FOOTER BUTTONS -->
    <template #footer>
      <button
        type="submit"
        form="add-employee-form"
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
  import { employeeService, type AddEmployeeRequest } from '@/api/employees';
  import { extractApiError } from '@/api/error';
  import { AppAlert, AppDialog } from '@/components/ui';
  import { useDialogForm } from '@/composables/useDialogForm';
  import { AlertTriangle, Plus } from 'lucide-vue-next';
  import { computed, watch } from 'vue';

  const props = defineProps<{ open: boolean }>();
  const emit = defineEmits<{ (e: 'close'): void; (e: 'saved'): void }>();

  const labelClass = 'mb-1 block text-sm font-medium text-slate-200';
  const formClass = 'w-full rounded-md border border-slate-700 bg-slate-800 p-2 text-sm text-white';

  const createInitialForm = (): AddEmployeeRequest => ({
    firstName: '',
    lastName: '',
    preferredName: null,
    employmentType: null,
    salaryType: null,
    hireDate: null,
    department: null,
    companyEmail: '',
    workLocation: null,
    licenseNotes: null,
    notes: null,
  });

  const { form, submitting, errorMessage, touched, reset } =
    useDialogForm<AddEmployeeRequest>(createInitialForm);

  // Simple local option lists – keep in sync with backend enums
  const employmentTypeOptions = [
    { value: 'FullTime', label: 'Full-time' },
    { value: 'PartTime', label: 'Part-time' },
  ];

  const salaryTypeOptions = [
    { value: 'Salary', label: 'Salary' },
    { value: 'Hourly', label: 'Hourly' },
  ];

  const departmentOptions = [
    { value: 'Unknown', label: 'Unknown' },
    { value: 'Engineering', label: 'Engineering' },
    { value: 'Drafting', label: 'Drafting' },
    { value: 'Surveying', label: 'Surveying' },
    { value: 'OfficeAdmin', label: 'Office Admin' },
  ];

  watch(
    () => props.open,
    isOpen => {
      if (isOpen) {
        reset();
      }
    },
    { immediate: true },
  );

  const errors = {
    requiredName: 'First and last name are required.',
    unexpected: 'An unexpected error occurred.',
  };

  const hasName = computed(() => !!form.value.firstName && !!form.value.lastName);

  const submit = async (): Promise<void> => {
    touched.value = true;
    errorMessage.value = null;

    if (!hasName.value) {
      errorMessage.value = errors.requiredName;
      return;
    }

    if (submitting.value) return;
    submitting.value = true;

    try {
      await employeeService.add(form.value);
      emit('saved');
      emit('close');
    } catch (e) {
      const msg = extractApiError(e, 'firstName');
      errorMessage.value = msg || errors.unexpected;
    } finally {
      submitting.value = false;
    }
  };
</script>
