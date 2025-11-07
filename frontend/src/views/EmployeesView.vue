<template>
  <h2 class="pb-4 text-xl font-semibold text-slate-100">Employees</h2>

  <div class="flex items-center justify-between pb-4">
    <div class="flex gap-4">
      <TableSearch v-model="nameFilter" placeholder="Search by name..." @commit="commitNameNow" />
      <DeletedFilter v-model="deletedFilter" @change="handleDeletedFilterChange" />
    </div>
    <button
      class="flex items-center gap-2 rounded-md bg-indigo-600 px-3 py-2 text-sm font-medium text-white transition hover:bg-indigo-500"
      @click="addDialogIsOpen = true"
    >
      <CirclePlus class="block h-4 w-4 shrink-0 self-center" aria-hidden="true" />
      <span class="text-white">Add Employee</span>
    </button>
  </div>

  <DataTable :table :loading :total-count empty-text="No employees found.">
    <!-- actions slot for this table only -->
    <template #cell="{ cell }">
      <template v-if="(cell.column.columnDef.meta as any)?.kind === 'actions'">
        <span class="flex gap-2">
          <!-- View button -->
          <button
            v-if="!cell.row.original.deletedAtUtc"
            :class="actionButtonClass"
            aria-label="view employee"
            @click="handleView(cell.row.original as EmployeeSummaryResponse)"
          >
            <Eye class="h-4 w-4" />
          </button>

          <!-- Delete button -->
          <button
            v-if="!cell.row.original.deletedAtUtc"
            class="rounded-md bg-indigo-600 p-1.5 transition hover:bg-rose-200"
            aria-label="delete position"
            @click="handleOpenDeleteDialog(cell.row.original as EmployeeSummaryResponse)"
          >
            {{ cell.row.original.deletedAtUtc }}
            <Trash2 class="h-4 w-4 text-rose-500 hover:text-rose-400" />
          </button>

          <!-- Reactivate button -->
          <button
            v-else
            class="rounded-md bg-indigo-600 p-1.5 text-emerald-200 transition hover:bg-green-200"
            aria-label="reactivate position"
            @click="handleOpenReactivateDialog(cell.row.original as EmployeeSummaryResponse)"
          >
            <RotateCcw class="h-4 w-4 hover:text-green-400" />
          </button>
        </span>
      </template>
      <CellRenderer :cell="cell" />
    </template>
  </DataTable>

  <TableFooter :table :totalCount :totalPages :pagination :setPageSize />

  <ViewEmployeeDialog
    :open="viewEmployeeDialogIsOpen"
    :selected-employee
    @close="viewEmployeeDialogIsOpen = false"
  />

  <EditEmployeeDialog
    :open="editEmployeeDialogIsOpen"
    :selected-employee
    @close="editEmployeeDialogIsOpen = false"
  />
</template>

<script setup lang="ts">
  import type {
    EmployeeResponse,
    EmployeeSummaryResponse,
    GetEmployeesRequest,
    GetEmployeesResponse,
  } from '@/api/employees';
  import { employeeService } from '@/api/employees';
  import DeletedFilter from '@/components/DeletedFilter.vue';
  import EditEmployeeDialog from '@/components/dialogs/EditEmployeeDialog.vue';
  import ViewEmployeeDialog from '@/components/dialogs/ViewEmployeeDialog.vue';
  import CellRenderer from '@/components/table/CellRenderer.vue';
  import DataTable from '@/components/table/DataTable.vue';
  import TableFooter from '@/components/table/TableFooter.vue';
  import TableSearch from '@/components/TableSearch.vue';
  import { useDataTable, type FetchParams } from '@/composables/useDataTable';
  import { useDebouncedRef } from '@/composables/useDebouncedRef';
  import { createColumnHelper, type ColumnDef, type ColumnHelper } from '@tanstack/vue-table';
  import { CirclePlus, Eye, RotateCcw, Trash2 } from 'lucide-vue-next';
  import { computed, onBeforeUnmount, ref, watch } from 'vue';

  /* ------------------------------- Constants ------------------------------ */
  // Buttons
  const actionButtonClass =
    'rounded-md bg-indigo-600 p-1.5 text-white transition hover:bg-indigo-500';

  // Department badge colors
  type DepartmentType = 'Engineering' | 'Drafting' | 'Surveying' | 'OfficeAdmin';
  const departmentClasses: Record<DepartmentType, string> = {
    Engineering: 'bg-sky-700 text-sky-100',
    Drafting: 'bg-fuchsia-700 text-fuchsia-100',
    Surveying: 'bg-amber-700 text-amber-100',
    OfficeAdmin: 'bg-slate-700 text-slate-200',
  };
  const getDepartmentClass = (dept: string): string =>
    departmentClasses[dept as DepartmentType] ?? 'bg-slate-800/60 text-slate-300';

  /* -------------------------------- Columns ------------------------------- */
  const col: ColumnHelper<EmployeeSummaryResponse> = createColumnHelper<EmployeeSummaryResponse>();
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const columns: ColumnDef<EmployeeSummaryResponse, any>[] = [
    col.accessor('lastName', { header: 'Last Name', meta: { kind: 'text' as const } }),
    col.accessor('firstName', { header: 'First Name', meta: { kind: 'text' as const } }),
    col.accessor('preferredName', { header: 'Preferred Name', meta: { kind: 'text' as const } }),
    col.accessor('department', {
      header: 'Department',
      meta: { kind: 'badge' as const, classFor: (val: string) => getDepartmentClass(val) },
    }),
    col.accessor('employmentType', { header: 'Employment Type', meta: { kind: 'text' as const } }),
    col.accessor('hireDate', { header: 'Hire Date', meta: { kind: 'datetime' as const } }),
    col.accessor('isActive', { header: 'Active Employee?', meta: { kind: 'boolean' as const } }),
    { id: 'actions', header: 'Actions', meta: { kind: 'actions' as const }, enableSorting: false },
  ];

  /* ---------------------------- Deleted Filter State ---------------------------- */
  const deletedFilter = ref<boolean | null>(false); // default: Active Only

  const handleDeletedFilterChange = () => {
    setQuery({
      name: name.value || undefined,
      isDeleted: deletedFilter.value, // keep null when "Active + Deleted"
    });
  };

  /* ------------------------------- Searching ------------------------------ */
  const {
    input: nameFilter, // bound to v-model
    debounced: name, // used in fetch
    setNow: commitNameNow, // call on Enter
    cancel: cancelNameDebounce, // cleanup on unmount
  } = useDebouncedRef('', 500);
  onBeforeUnmount(cancelNameDebounce);

  /* ------------------------------- Fetching ------------------------------- */
  type EmpQuery = { name?: string; isDeleted?: boolean | null };

  const employeeDetails = ref<EmployeeResponse[]>([]);
  const employeeDetailsById = computed(() => new Map(employeeDetails.value.map(e => [e.id, e])));

  const fetchEmployees = async ({ page, pageSize, query }: FetchParams<EmpQuery>) => {
    const params: GetEmployeesRequest = {
      page,
      pageSize,
      name: query?.name || undefined,
      isDeleted: query?.isDeleted ?? null,
    };
    const response: GetEmployeesResponse = await employeeService.get(params);

    // Cache details for action dialogs
    employeeDetails.value = response.employees.map(e => e.details);

    return {
      items: response.employees.map(e => e.summary), // summaries are the table rows
      totalCount: response.totalCount,
      totalPages: response.totalPages,
      page: response.page,
      pageSize: response.pageSize,
    };
  };

  const { table, loading, totalCount, totalPages, pagination, setQuery, setPageSize } =
    useDataTable<EmployeeSummaryResponse, EmpQuery>(columns, fetchEmployees, { name: undefined });

  // Update query when search changes
  watch(name, () =>
    setQuery({
      name: name.value || undefined,
      isDeleted: deletedFilter.value,
    }),
  );

  /* ------------------------------- Dialogs/UX ----------------------------- */
  const addDialogIsOpen = ref(false);
  const deleteDialogIsOpen = ref(false);
  const editEmployeeDialogIsOpen = ref(false);
  const reactivateDialogIsOpen = ref(false);
  const selectedEmployee = ref<EmployeeResponse | null>(null);
  const viewEmployeeDialogIsOpen = ref(false);

  /* -------------------------------- Handlers ------------------------------ */
  const handleView = (summary: EmployeeSummaryResponse): void => {
    const detail = employeeDetailsById.value.get(summary.id) ?? null;
    selectedEmployee.value = detail;
    viewEmployeeDialogIsOpen.value = !!detail;
  };

  const handleEditEmployee = (summary: EmployeeSummaryResponse): void => {
    const detail = employeeDetailsById.value.get(summary.id) ?? null;
    selectedEmployee.value = detail;
    editEmployeeDialogIsOpen.value = !!detail;
  };

  const handleOpenDeleteDialog = (summary: EmployeeSummaryResponse): void => {
    //selectedEmployee.value = summary;
    //deleteDialogIsOpen.value = true;
  };

  const handleOpenReactivateDialog = (position: EmployeeSummaryResponse): void => {
    // TODO: Call reactivate service
  };
</script>
