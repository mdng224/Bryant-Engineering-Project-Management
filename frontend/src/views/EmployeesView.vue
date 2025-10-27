<template>
  <div class="pb-4">
    <TableSearch v-model="nameSearch" placeholder="Search by name..." @commit="commitNameNow" />
  </div>

  <DataTable :table :loading :total-count empty-text="No employees found.">
    <!-- actions slot for this table only -->
    <template #cell="{ cell }">
      <template v-if="(cell.column.columnDef.meta as any)?.kind === 'actions'">
        <span class="flex gap-2">
          <!-- View button -->
          <button
            :class="actionButtonClass"
            aria-label="Edit employee"
            @click="handleViewEmployee(cell.row.original as EmployeeSummaryResponse)"
          >
            <Eye class="h-4 w-4" />
          </button>

          <!-- TODO: PUT MORE THOUGHT INTO THIS
          <button
            :class="actionButtonClass"
            aria-label="Edit employee"
            @click="handleEditEmployee(cell.row.original as EmployeeSummaryResponse)"
          >
            <Pencil class="h-4 w-4" />
          </button>
           -->
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
  import { employeeService } from '@/api/employees';
  import type {
    EmployeeResponse,
    EmployeeSummaryResponse,
    GetEmployeesRequest,
    GetEmployeesResponse,
  } from '@/api/employees/contracts';
  import EditEmployeeDialog from '@/components/dialogs/EditEmployeeDialog.vue';
  import ViewEmployeeDialog from '@/components/dialogs/ViewEmployeeDialog.vue';
  import CellRenderer from '@/components/table/CellRenderer.vue';
  import DataTable from '@/components/table/DataTable.vue';
  import TableFooter from '@/components/table/TableFooter.vue';
  import TableSearch from '@/components/TableSearch.vue';
  import { useDataTable } from '@/composables/useDataTable';
  import { useDebouncedRef } from '@/composables/useDebouncedRef';
  import { createColumnHelper, type ColumnDef, type ColumnHelper } from '@tanstack/vue-table';
  import { Eye } from 'lucide-vue-next';
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

  /* ------------------------------- Searching ------------------------------ */
  const {
    input: nameSearch, // bound to v-model
    debounced: name, // used in fetch
    setNow: commitNameNow, // call on Enter
    cancel: cancelNameDebounce, // cleanup on unmount
  } = useDebouncedRef('', 500);
  onBeforeUnmount(cancelNameDebounce);

  /* ------------------------------- Fetching ------------------------------- */
  type EmpQuery = { name?: string };

  // Detail cache (from server) and quick lookup map
  const employeeDetails = ref<EmployeeResponse[]>([]);
  const employeeDetailsById = computed(() => new Map(employeeDetails.value.map(e => [e.id, e])));

  const fetchEmployees = async ({
    page,
    pageSize,
    query,
  }: {
    page: number;
    pageSize: number;
    query?: EmpQuery;
  }) => {
    const params: GetEmployeesRequest = { page, pageSize, name: query?.name || undefined };
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
  watch(name, () => setQuery({ name: name.value || undefined }));

  /* ------------------------------- Dialogs/UX ----------------------------- */
  const viewEmployeeDialogIsOpen = ref(false);
  const selectedEmployee = ref<EmployeeResponse | null>(null);
  const editEmployeeDialogIsOpen = ref(false);

  /* -------------------------------- Handlers ------------------------------ */
  const handleViewEmployee = (summary: EmployeeSummaryResponse): void => {
    const detail = employeeDetailsById.value.get(summary.id) ?? null;
    selectedEmployee.value = detail;
    viewEmployeeDialogIsOpen.value = !!detail;
  };

  const handleEditEmployee = (summary: EmployeeSummaryResponse): void => {
    const detail = employeeDetailsById.value.get(summary.id) ?? null;
    selectedEmployee.value = detail;
    editEmployeeDialogIsOpen.value = !!detail;
  };
</script>
