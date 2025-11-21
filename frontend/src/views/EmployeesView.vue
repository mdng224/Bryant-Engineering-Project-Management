<template>
  <h2 class="pb-4 text-xl font-semibold text-slate-100">Employees</h2>

  <div class="flex items-center justify-between pb-4">
    <div class="flex gap-4">
      <table-search v-model="nameFilter" placeholder="Search by name..." @commit="commitNameNow" />
      <boolean-filter v-model="deletedFilter" :options="deletedOptions" />
    </div>
    <button
      class="flex items-center gap-2 rounded-md bg-indigo-600 px-3 py-2 text-sm font-medium text-white transition hover:bg-indigo-500"
      @click="addDialogIsOpen = true"
    >
      <circle-plus class="block h-4 w-4 shrink-0 self-center" aria-hidden="true" />
      <span class="text-white">Add Employee</span>
    </button>
  </div>

  <app-alert v-if="errorMessage" :message="errorMessage" variant="error" :icon="AlertTriangle" />

  <data-table :table :loading :total-count empty-text="No employees found.">
    <!-- actions slot for this table only -->
    <template #cell="{ cell }">
      <template v-if="(cell.column.columnDef.meta as any)?.kind === 'actions'">
        <span class="flex gap-2">
          <!-- View button -->
          <button
            v-if="!cell.row.original.deletedAtUtc"
            :class="actionButtonClass"
            aria-label="view employee"
            @click="handleOpenEmployeeDetails(cell.row.original.id as string)"
          >
            <eye class="h-4 w-4" />
          </button>
        </span>
      </template>
      <cell-renderer :cell />
    </template>
  </data-table>

  <table-footer :table :totalCount :totalPages :pagination :setPageSize />

  <add-employee-dialog :open="addDialogIsOpen" @close="addDialogIsOpen = false" @saved="refetch" />

  <details-dialog
    :open="openDetailsDialog"
    title="Employee Details"
    name-key="fullName"
    id-key="id"
    :item="selectedEmployeeForDialog"
    :fields
    :format-utc
    @close="openDetailsDialog = false"
  />

  <edit-employee-dialog
    :open="editEmployeeDialogIsOpen"
    :selected-employee
    @close="editEmployeeDialogIsOpen = false"
  />
</template>

<script setup lang="ts">
  import type {
    EmployeeRowResponse,
    GetEmployeeDetailsResponse,
    ListEmployeesRequest,
    ListEmployeesResponse,
  } from '@/api/employees';
  import { employeeService } from '@/api/employees';
  import { extractApiError } from '@/api/error';
  import { AddEmployeeDialog, EditEmployeeDialog } from '@/components/dialogs/employees';
  import DetailsDialog, { type FieldDef } from '@/components/dialogs/shared/DetailsDialog.vue';
  import { CellRenderer, DataTable, TableFooter, TableSearch } from '@/components/table';
  import { AppAlert } from '@/components/ui';
  import BooleanFilter from '@/components/ui/BooleanFilter.vue';
  import { useDataTable, type FetchParams } from '@/composables/useDataTable';
  import { useDateFormat } from '@/composables/UseDateFormat';
  import { useDebouncedRef } from '@/composables/useDebouncedRef';
  import { createColumnHelper, type ColumnDef, type ColumnHelper } from '@tanstack/vue-table';
  import { AlertTriangle, CheckCircle2, CirclePlus, Eye, Trash2 } from 'lucide-vue-next';
  import { computed, onBeforeUnmount, ref } from 'vue';

  const errorMessage = ref<string | null>(null);

  /* ------------------------------- Constants ------------------------------ */
  // Buttons
  const actionButtonClass =
    'rounded-md bg-indigo-600 p-1.5 text-white transition hover:bg-indigo-500';

  // Department badge colors
  type DepartmentType = 'Engineering' | 'Drafting' | 'Surveying' | 'OfficeAdmin' | 'Unknown';
  const departmentClasses: Record<DepartmentType, string> = {
    Engineering: 'bg-sky-700 text-sky-100',
    Drafting: 'bg-fuchsia-700 text-fuchsia-100',
    Surveying: 'bg-amber-700 text-amber-100',
    OfficeAdmin: 'bg-slate-700 text-slate-200',
    Unknown: 'bg-yellow-700 text-yellow-200',
  };
  const getDepartmentClass = (dept: string): string =>
    departmentClasses[dept as DepartmentType] ?? 'bg-slate-800/60 text-slate-300';

  /* ------------------------------- Details ------------------------------ */
  const selectedEmployeeForDialog = computed(() =>
    selectedEmployee.value
      ? {
          ...selectedEmployee.value,
          fullName: `${selectedEmployee.value.preferredName} ${selectedEmployee.value.lastName}`,
        }
      : null,
  );

  const fieldDef = <K extends keyof GetEmployeeDetailsResponse>(
    key: K,
    label: string,
    type: 'text' | 'date' | 'mono' | 'multiline' = 'text',
    span: 1 | 2 = 1,
  ) => ({ key: key as string, label, type, span }) satisfies FieldDef;

  const { formatUtc } = useDateFormat();

  const fields: FieldDef[] = [
    // Identity
    fieldDef('firstName', 'First Name'),
    fieldDef('lastName', 'Last Name'),
    fieldDef('preferredName', 'Preferred Name'),

    // Employment / org
    fieldDef('companyEmail', 'Email'),
    fieldDef('department', 'Department'),
    fieldDef('employmentType', 'Employment Type'),
    fieldDef('salaryType', 'Salary Type'),
    fieldDef('workLocation', 'Work Location'),
    fieldDef('recommendedRole', 'Recommended Role', 'text'),
    fieldDef('isPreapproved', 'Pre-Approved'),

    // Address
    fieldDef('line1', 'Address Line 1'),
    fieldDef('line2', 'Address Line 2'),
    fieldDef('city', 'City'),
    fieldDef('state', 'State'),
    fieldDef('postalCode', 'Postal Code'),

    // Dates
    fieldDef('hireDate', 'Hire Date', 'date'),
    fieldDef('endDate', 'End Date', 'date'),
    fieldDef('createdAtUtc', 'Created', 'date'),
    fieldDef('createdById', 'Created By', 'mono'),
    fieldDef('updatedAtUtc', 'Updated', 'date'),
    fieldDef('updatedById', 'Updated By', 'mono'),
    fieldDef('deletedAtUtc', 'Deleted', 'date'),
    fieldDef('deletedById', 'Deleted By', 'mono'),

    // Notes
    fieldDef('notes', 'Notes', 'multiline', 2),
  ] as any;

  /* -------------------------------- Columns ------------------------------- */
  const col: ColumnHelper<EmployeeRowResponse> = createColumnHelper<EmployeeRowResponse>();
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const columns: ColumnDef<EmployeeRowResponse, any>[] = [
    col.accessor('lastName', { header: 'Last Name', meta: { kind: 'text' as const } }),
    col.accessor('firstName', { header: 'First Name', meta: { kind: 'text' as const } }),
    col.accessor('preferredName', { header: 'Preferred Name', meta: { kind: 'text' as const } }),
    // TODO: Format position names
    col.accessor('positionNames', { header: 'Position', meta: { kind: 'text' as const } }),
    col.accessor('department', {
      header: 'Department',
      meta: { kind: 'badge' as const, classFor: (val: string) => getDepartmentClass(val) },
    }),
    col.accessor('employmentType', { header: 'Employment Type', meta: { kind: 'text' as const } }),
    col.accessor('hireDate', { header: 'Hire Date', meta: { kind: 'datetime' as const } }),
    { id: 'actions', header: 'Actions', meta: { kind: 'actions' as const }, enableSorting: false },
  ];

  /* ------------------------------- Filtering ------------------------------ */
  const deletedFilter = ref(false); // default: show only active (not deleted)

  const deletedOptions = [
    { value: false, label: 'Active', icon: CheckCircle2, color: 'text-emerald-400' },
    { value: true, label: 'Inactive', icon: Trash2, color: 'text-rose-400' },
  ];

  const {
    input: nameFilter, // bound to v-model
    debounced: name, // used in fetch
    setNow: commitNameNow, // call on Enter
    cancel: cancelNameDebounce, // cleanup on unmount
  } = useDebouncedRef('', 500);

  onBeforeUnmount(() => {
    cancelNameDebounce();
    destroy();
  });

  /* ------------------------------- Fetching ------------------------------- */
  type EmpQuery = { name?: string; isDeleted?: boolean | null };

  const employeeDetails = ref<GetEmployeeDetailsResponse[]>([]);
  const employeeDetailsById = computed(() => new Map(employeeDetails.value.map(e => [e.id, e])));

  const fetchEmployees = async ({ page, pageSize, query }: FetchParams<EmpQuery>) => {
    const params: ListEmployeesRequest = {
      page,
      pageSize,
      nameFilter: query?.name || null,
      isDeleted: query?.isDeleted ?? null,
    };
    try {
      const response: ListEmployeesResponse = await employeeService.list(params);

      return {
        items: response.employees,
        totalCount: response.totalCount,
        totalPages: response.totalPages,
        page: response.page,
        pageSize: response.pageSize,
      };
    } catch (err: unknown) {
      const msg: string = extractApiError(err, 'employee');
      errorMessage.value = msg;

      return {
        items: [],
        totalCount: 0,
        totalPages: 0,
        page,
        pageSize,
      };
    }
  };
  const query = computed(() => ({
    name: name.value ?? null,
    isDeleted: deletedFilter.value,
  }));

  const {
    table,
    loading,
    totalCount,
    totalPages,
    pagination,
    setPageSize,
    fetchNow: refetch,
    destroy,
  } = useDataTable<EmployeeRowResponse, typeof query.value>(columns, fetchEmployees, query);

  /* ------------------------------- Dialogs/UX ----------------------------- */
  const addDialogIsOpen = ref(false);
  const deleteDialogIsOpen = ref(false);
  const editEmployeeDialogIsOpen = ref(false);
  const openDetailsDialog = ref(false);
  const reactivateDialogIsOpen = ref(false);
  const selectedEmployee = ref<GetEmployeeDetailsResponse | null>(null);

  /* -------------------------------- Handlers ------------------------------ */
  const handleOpenEmployeeDetails = async (id: string): Promise<void> => {
    errorMessage.value = null;
    if (selectedEmployee.value?.id === id) {
      openDetailsDialog.value = true;
      return;
    }

    try {
      const response: GetEmployeeDetailsResponse = await employeeService.getDetails(id);
      selectedEmployee.value = response;
      openDetailsDialog.value = Boolean(response);
    } catch (err: unknown) {
      const msg: string = extractApiError(err, 'project');
      errorMessage.value = msg;
    }
  };

  const handleEditEmployee = (summary: EmployeeRowResponse): void => {
    const detail = employeeDetailsById.value.get(summary.id) ?? null;
    selectedEmployee.value = detail;
    editEmployeeDialogIsOpen.value = !!detail;
  };

  const handleEmployeeSaved = () => {
    // however you currently refresh; if useDataTable returns a `reload`, call that
    pagination.pageIndex = 0; // or call your own reload function
  };

  const handleOpenDeleteDialog = (summary: EmployeeRowResponse): void => {
    //selectedEmployee.value = summary;
    //deleteDialogIsOpen.value = true;
  };

  const handleOpenReactivateDialog = (position: EmployeeRowResponse): void => {
    // TODO: Call reactivate service
  };
</script>
