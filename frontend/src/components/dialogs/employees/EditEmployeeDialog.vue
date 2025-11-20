<!-- components/EditEmployeeDialog.vue -->
<template>
  <app-dialog :open title="Edit Employee" width="max-w-3xl" :loading="saving" @close="onCancel">
    <!-- IDs line -->
    <p class="mb-4 text-xs text-slate-400">
      EmployeeId •
      <span class="font-mono">{{ selectedEmployee?.id }}</span>
      <span class="mx-2">|</span>
      UserId •
      <span class="font-mono">{{ selectedEmployee?.userId }}</span>
    </p>

    <!-- FORM BODY -->
    <form
      id="edit-employee-form"
      class="grid grid-cols-1 gap-4 sm:grid-cols-2"
      @submit.prevent="onSubmit"
    >
      <!-- Read-only identity -->
      <div class="rounded-lg border border-slate-700/70 bg-slate-900/70 p-4 sm:col-span-2">
        <div class="grid grid-cols-1 gap-3 sm:grid-cols-3">
          <div>
            <label class="text-xs text-slate-400">First Name</label>
            <input
              class="mt-1 w-full rounded-md border border-slate-700 bg-slate-800/70 px-3 py-2 text-sm"
              :value="selectedEmployee?.firstName"
              disabled
            />
          </div>
          <div>
            <label class="text-xs text-slate-400">Last Name</label>
            <input
              class="mt-1 w-full rounded-md border border-slate-700 bg-slate-800/70 px-3 py-2 text-sm"
              :value="selectedEmployee?.lastName"
              disabled
            />
          </div>
          <div>
            <label class="text-xs text-slate-400">Preferred Name</label>
            <input
              v-model.trim="model.preferredName"
              class="mt-1 w-full rounded-md border border-slate-700 bg-slate-800/70 px-3 py-2 text-sm"
              placeholder="—"
            />
          </div>
        </div>
      </div>

      <!-- Company Email -->
      <div>
        <label class="text-xs text-slate-400">Company Email</label>
        <input
          v-model.trim="model.companyEmail"
          type="email"
          class="mt-1 w-full rounded-md border border-slate-700 bg-slate-800/70 px-3 py-2 text-sm"
          placeholder="name@company.com"
          autocomplete="email"
        />
        <p v-if="emailError" class="mt-1 text-xs text-rose-400">{{ emailError }}</p>
      </div>

      <!-- Department -->
      <div>
        <label class="text-xs text-slate-400">Department</label>
        <select
          v-model="model.department"
          class="mt-1 w-full rounded-md border border-slate-700 bg-slate-800/70 px-3 py-2 text-sm"
        >
          <option :value="null">—</option>
          <option v-for="d in departmentsComputed" :key="d" :value="d">
            {{ d }}
          </option>
        </select>
      </div>

      <!-- Employment Type -->
      <div>
        <label class="text-xs text-slate-400">Employment Type</label>
        <select
          v-model="model.employmentType"
          class="mt-1 w-full rounded-md border border-slate-700 bg-slate-800/70 px-3 py-2 text-sm"
        >
          <option :value="null">—</option>
          <option v-for="t in employmentTypesComputed" :key="t" :value="t">
            {{ t }}
          </option>
        </select>
      </div>

      <!-- Salary Type -->
      <div>
        <label class="text-xs text-slate-400">Salary Type</label>
        <select
          v-model="model.salaryType"
          class="mt-1 w-full rounded-md border border-slate-700 bg-slate-800/70 px-3 py-2 text-sm"
        >
          <option :value="null">—</option>
          <option v-for="t in salaryTypesComputed" :key="t" :value="t">
            {{ t }}
          </option>
        </select>
      </div>

      <!-- Work Location -->
      <div>
        <label class="text-xs text-slate-400">Work Location</label>
        <select
          v-model="model.workLocation"
          class="mt-1 w-full rounded-md border border-slate-700 bg-slate-800/70 px-3 py-2 text-sm"
        >
          <option :value="null">—</option>
          <option v-for="w in workLocationsComputed" :key="w" :value="w">
            {{ w }}
          </option>
        </select>
      </div>

      <!-- Recommended Role Id -->
      <div class="sm:col-span-2">
        <label class="text-xs text-slate-400">Recommended Role Id</label>
        <input
          v-model.trim="model.recommendedRoleId"
          class="mt-1 w-full rounded-md border border-slate-700 bg-slate-800/70 px-3 py-2 font-mono text-sm"
          placeholder="GUID (optional)"
        />
      </div>

      <!-- Dates -->
      <div>
        <label class="text-xs text-slate-400">Hire Date</label>
        <input
          v-model="model.hireDate"
          type="date"
          class="mt-1 w-full rounded-md border border-slate-700 bg-slate-800/70 px-3 py-2 text-sm"
        />
      </div>
      <div>
        <label class="text-xs text-slate-400">End Date</label>
        <input
          v-model="model.endDate"
          type="date"
          class="mt-1 w-full rounded-md border border-slate-700 bg-slate-800/70 px-3 py-2 text-sm"
        />
        <p v-if="dateError" class="mt-1 text-xs text-rose-400">{{ dateError }}</p>
      </div>

      <!-- Notes -->
      <div class="sm:col-span-2">
        <label class="text-xs text-slate-400">License Notes</label>
        <textarea
          v-model="model.licenseNotes"
          rows="3"
          class="mt-1 w-full resize-y rounded-md border border-slate-700 bg-slate-800/70 px-3 py-2 text-sm"
          placeholder="—"
        />
      </div>
      <div class="sm:col-span-2">
        <label class="text-xs text-slate-400">Notes</label>
        <textarea
          v-model="model.notes"
          rows="4"
          class="mt-1 w-full resize-y rounded-md border border-slate-700 bg-slate-800/70 px-3 py-2 text-sm"
          placeholder="—"
        />
      </div>

      <!-- Timestamps (read-only) -->
      <div class="grid grid-cols-1 gap-3 text-xs text-slate-400 sm:col-span-2 sm:grid-cols-3">
        <div>
          <div class="text-slate-500">Created</div>
          <div>{{ fmt(selectedEmployee?.createdAtUtc) }}</div>
        </div>
        <div>
          <div class="text-slate-500">Updated</div>
          <div>{{ fmt(selectedEmployee?.updatedAtUtc) }}</div>
        </div>
        <div>
          <div class="text-slate-500">Deleted</div>
          <div>{{ fmt(selectedEmployee?.deletedAtUtc) }}</div>
        </div>
      </div>
    </form>

    <!-- FOOTER -->
    <template #footer>
      <button
        type="button"
        class="rounded-md px-3 py-1.5 text-sm text-slate-300 hover:bg-slate-700/70 disabled:opacity-50"
        :disabled="saving"
        @click="onCancel"
      >
        Cancel
      </button>
      <button
        type="submit"
        form="edit-employee-form"
        class="rounded-md bg-indigo-600 px-4 py-1.5 text-sm font-medium text-white hover:bg-indigo-500 disabled:cursor-not-allowed disabled:opacity-50"
        :disabled="saving || !isValid"
      >
        <span v-if="!saving">Save</span>
        <span v-else>Saving…</span>
      </button>
    </template>
  </app-dialog>
</template>

<script setup lang="ts">
  import type { GetEmployeeDetailsResponse } from '@/api/employees/contracts';
  import { AppDialog } from '@/components/ui';
  import { useDateFormat } from '@/composables/UseDateFormat';
  import { computed, reactive, ref, watch } from 'vue';

  /******************** Props & Emits ********************/
  const props = defineProps<{
    open: boolean;
    selectedEmployee: GetEmployeeDetailsResponse | null;
    departments?: string[];
    employmentTypes?: string[];
    salaryTypes?: string[];
    workLocations?: string[];
  }>();

  const emit = defineEmits<{
    (e: 'close'): void;
    (e: 'save', payload: UpdateEmployeeRequest): void;
  }>();

  /******************** Types ********************/
  export type UpdateEmployeeRequest = {
    id: string;
    preferredName?: string | null;
    companyEmail?: string | null;
    department?: string | null;
    employmentType?: string | null;
    salaryType?: string | null;
    workLocation?: string | null;
    recommendedRoleId?: string | null;
    hireDate?: string | null;
    endDate?: string | null;
    licenseNotes?: string | null;
    notes?: string | null;
  };

  /******************** State ********************/
  const { formatUtc } = useDateFormat();
  const fmt = (s?: string | null) => (s ? formatUtc(s) : '—');

  const saving = ref(false);

  const model = reactive({
    preferredName: '' as string | null,
    companyEmail: '' as string | null,
    department: null as string | null,
    employmentType: null as string | null,
    salaryType: null as string | null,
    workLocation: null as string | null,
    recommendedRoleId: '' as string | null,
    hireDate: '' as string | null,
    endDate: '' as string | null,
    licenseNotes: '' as string | null,
    notes: '' as string | null,
  });

  const departmentsComputed = computed(() => props.departments ?? []);
  const employmentTypesComputed = computed(
    () => props.employmentTypes ?? ['Full-Time', 'Part-Time', 'Contract'],
  );
  const salaryTypesComputed = computed(() => props.salaryTypes ?? ['Salary', 'Hourly']);
  const workLocationsComputed = computed(
    () => props.workLocations ?? ['On-site', 'Hybrid', 'Remote'],
  );

  /******************** Validation ********************/
  const emailError = computed(() => {
    if (!model.companyEmail) return '';
    const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return regex.test(model.companyEmail) ? '' : 'Invalid email format.';
  });

  const dateError = computed(() => {
    if (!model.hireDate || !model.endDate) return '';
    return model.endDate >= model.hireDate ? '' : 'End Date must be on/after Hire Date.';
  });

  const isValid = computed(() => !emailError.value && !dateError.value);

  /******************** Helpers ********************/
  function toDateInputValue(iso?: string | null): string | null {
    if (!iso) return null;
    const d = new Date(iso);
    if (isNaN(d.getTime())) return null;
    const yyyy = d.getUTCFullYear();
    const mm = String(d.getUTCMonth() + 1).padStart(2, '0');
    const dd = String(d.getUTCDate()).padStart(2, '0');
    return `${yyyy}-${mm}-${dd}`;
  }

  function fromDateInputValue(val?: string | null): string | null {
    return val || null;
  }

  /******************** Effects ********************/
  watch(
    () => [props.open, props.selectedEmployee],
    () => {
      const e = props.selectedEmployee;
      if (!props.open || !e) return;
      model.preferredName = e.preferredName ?? '';
      model.companyEmail = e.companyEmail ?? '';
      model.department = e.department ?? null;
      model.employmentType = e.employmentType ?? null;
      model.salaryType = e.salaryType ?? null;
      model.workLocation = e.workLocation ?? null;
      model.recommendedRoleId = e.recommendedRoleId ?? '';
      model.hireDate = toDateInputValue(e.hireDate);
      model.endDate = toDateInputValue(e.endDate);
      model.licenseNotes = e.licenseNotes ?? '';
      model.notes = e.notes ?? '';
    },
    { immediate: true, deep: false },
  );

  /******************** Handlers ********************/
  function onCancel() {
    if (saving.value) return;
    emit('close');
  }

  async function onSubmit() {
    if (!props.selectedEmployee || !isValid.value || saving.value) return;
    saving.value = true;
    try {
      const payload: UpdateEmployeeRequest = {
        id: props.selectedEmployee.id,
        preferredName: model.preferredName?.trim() || null,
        companyEmail: model.companyEmail?.trim() || null,
        department: model.department || null,
        employmentType: model.employmentType || null,
        salaryType: model.salaryType || null,
        workLocation: model.workLocation || null,
        recommendedRoleId: model.recommendedRoleId?.trim() || null,
        hireDate: fromDateInputValue(model.hireDate),
        endDate: fromDateInputValue(model.endDate),
        licenseNotes: model.licenseNotes?.trim() || null,
        notes: model.notes?.trim() || null,
      };
      emit('save', payload);
    } finally {
      saving.value = false;
    }
  }
</script>
