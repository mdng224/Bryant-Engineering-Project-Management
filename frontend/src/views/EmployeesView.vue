<template>
  <div class="pb-4">
    <TableSearch v-model="nameSearch" placeholder="Search email…" @commit="commitNameNow" />
  </div>

  <DataTable :table :loading :total-count empty-text="No employees found.">
    <!-- actions slot for this table only -->
    <template #cell="{ cell }">
      <template v-if="(cell.column.columnDef.meta as any)?.kind === 'actions'">
        <button
          class="rounded-md bg-indigo-600 p-1.5 text-white transition hover:bg-indigo-500"
          aria-label="Edit employee"
          @click="handleEditEmployee(cell.row.original as EmployeeResponse)"
        >
          <Pencil class="h-4 w-4" />
        </button>
      </template>
      <CellRenderer :cell="cell" />
    </template>
  </DataTable>

  <TableFooter :table :totalCount :totalPages :pagination :setPageSize />
</template>

<script setup lang="ts">
  import { employeeService } from '@/api/employees';
  import type {
    EmployeeResponse,
    EmployeeSummaryResponse,
    GetEmployeesRequest,
    GetEmployeesResponse,
  } from '@/api/employees/contracts';
  import CellRenderer from '@/components/table/CellRenderer.vue';
  import DataTable from '@/components/table/DataTable.vue';
  import TableFooter from '@/components/table/TableFooter.vue';
  import TableSearch from '@/components/TableSearch.vue';
  import { useDataTable } from '@/composables/useDataTable';
  import { useDebouncedRef } from '@/composables/useDebouncedRef';
  import { createColumnHelper, type ColumnDef, type ColumnHelper } from '@tanstack/vue-table';
  import { Pencil } from 'lucide-vue-next';
  import { onBeforeUnmount, ref, watch } from 'vue';

  const editUserDialogIsOpen = ref(false);
  const userBeingEdited = ref<EmployeeResponse | null>(null);
  const col: ColumnHelper<EmployeeSummaryResponse> = createColumnHelper<EmployeeSummaryResponse>();
  const columns: ColumnDef<EmployeeSummaryResponse, any>[] = [
    col.accessor('lastName', {
      header: 'Last Name',
      meta: { kind: 'text' as const },
    }),
    col.accessor('firstName', {
      header: 'First Name',
      meta: { kind: 'text' as const },
    }),
    col.accessor('preferredName', {
      header: 'Preferred Name',
      meta: { kind: 'text' as const },
    }),
    col.accessor('department', {
      header: 'Department',
      meta: {
        kind: 'badge' as const,
        classFor: (val: string) => getDepartmentClass(val),
      },
    }),
    col.accessor('employmentType', {
      header: 'Employment Type',
      meta: { kind: 'text' as const },
    }),
    col.accessor('hireDate', {
      header: 'Hire Date',
      meta: { kind: 'datetime' as const },
    }),
    col.accessor('isActive', {
      header: 'Active Employee?',
      meta: { kind: 'boolean' as const },
    }),
    {
      id: 'actions',
      header: 'Actions',
      meta: { kind: 'actions' as const },
      enableSorting: false,
    },
  ];

  /* ------------------------------ Department ------------------------------- */
  type DepartmentType = 'Engineering' | 'Drafting' | 'Surveying' | 'OfficeAdmin';

  const departmentClasses: Record<DepartmentType, string> = {
    Engineering: 'bg-sky-700 text-sky-100',
    Drafting: 'bg-fuchsia-700 text-fuchsia-100',
    Surveying: 'bg-amber-700 text-amber-100',
    OfficeAdmin: 'bg-slate-700 text-slate-200',
  };

  const getDepartmentClass = (dept: string): string =>
    departmentClasses[dept as DepartmentType] ?? 'bg-slate-800/60 text-slate-300';

  /* ---------------------------- Search ---------------------------- */
  const {
    input: nameSearch, // bind to v-model
    debounced: name, // use in fetch
    setNow: commitNameNow, // optional: commit immediately on Enter
    cancel: cancelNameDebounce, // optional
  } = useDebouncedRef('', 500);

  onBeforeUnmount(cancelNameDebounce);

  /* ------------------------------ Fetching ------------------------------- */
  type EmpQuery = { name?: string };
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

    return {
      items: response.employees.map(e => e.summary),
      totalCount: response.totalCount,
      totalPages: response.totalPages,
      page: response.page,
      pageSize: response.pageSize,
    };
  };

  const { table, loading, totalCount, totalPages, pagination, setQuery, setPageSize } =
    useDataTable<EmployeeSummaryResponse, EmpQuery>(columns, fetchEmployees, { name: undefined });

  watch(name, () => setQuery({ name: name.value || undefined }));

  /* ------------------------------ Handlers ------------------------------- */
  const handleEditEmployee = (employee: EmployeeResponse): void => {
    //employeeBeingEdited.value = employee;
    //editemployeeDialogIsOpen.value = true;
  };
</script>
