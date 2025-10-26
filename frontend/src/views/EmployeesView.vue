<template>
  <div class="pb-4">
    <TableSearch v-model="nameSearch" placeholder="Search email…" @commit="commitNameNow" />
  </div>

  <div class="overflow-x-auto rounded-xl border border-slate-700 bg-slate-900/70 shadow">
    <table
      class="sticky top-0 z-10 min-w-full divide-y divide-slate-700 text-sm text-slate-200 backdrop-blur"
    >
      <thead class="bg-slate-800/80 text-slate-100">
        <tr v-for="hg in table.getHeaderGroups()" :key="hg.id">
          <th
            v-for="header in hg.headers"
            :key="header.id"
            class="px-4 py-3 text-left font-semibold uppercase tracking-wider"
          >
            {{ header.column.columnDef.header as string }}
          </th>
        </tr>
      </thead>

      <tbody class="divide-y divide-slate-800">
        <SkeletonRows v-if="loading" :rows="10" :cols="table.getAllLeafColumns().length" />

        <tr
          v-for="row in table.getRowModel().rows"
          v-else
          :key="row.id"
          class="odd:bg-slate-900/40 even:bg-slate-700/30 hover:bg-slate-800/40 ..."
        >
          <td
            v-for="cell in row.getVisibleCells()"
            :key="cell.id"
            class="whitespace-nowrap px-4 py-2.5"
          >
            <template v-if="(cell.column.columnDef.meta as ColMeta)?.kind === 'text'">
              <span class="text-md text-slate-100">{{ cell.getValue() as string }}</span>
            </template>

            <template v-else-if="(cell.column.columnDef.meta as ColMeta)?.kind === 'department'">
              <span
                :class="[
                  'rounded-full px-2 py-0.5 text-sm font-medium',
                  getDepartmentClass(cell.getValue() as string),
                ]"
              >
                {{ cell.getValue() as string }}
              </span>
            </template>

            <template v-else-if="(cell.column.columnDef.meta as ColMeta)?.kind === 'datetime'">
              <span class="text-[12px] tracking-wide text-slate-100">
                {{ formatUtc(cell.getValue() as string | null) }}
              </span>
            </template>
            <template v-else>{{ cell.getValue() as any }}</template>
          </td>
        </tr>

        <tr v-if="!loading && employees.length === 0">
          <td class="px-6 py-6 text-slate-400" :colspan="table.getAllLeafColumns().length">
            No results.
          </td>
        </tr>
        <tr v-if="loading">
          <td class="px-6 py-6 text-slate-400" :colspan="table.getAllLeafColumns().length">
            Loading…
          </td>
        </tr>
      </tbody>
    </table>
  </div>

  <TableFooter :table :totalCount :totalPages :pagination :setPageSize />
</template>

<script setup lang="ts">
  import { employeeService } from '@/api/employees';
  import type {
    EmployeeSummaryResponse,
    GetEmployeesRequest,
    GetEmployeesResponse,
  } from '@/api/employees/contracts';
  import SkeletonRows from '@/components/SkeletonRows.vue';
  import TableFooter from '@/components/TableFooter.vue';
  import TableSearch from '@/components/TableSearch.vue';

  import { useDataTable } from '@/composables/useDataTable';
  import { useDebouncedRef } from '@/composables/useDebouncedRef';
  import { createColumnHelper, type ColumnDef, type ColumnHelper } from '@tanstack/vue-table';
  import { onBeforeUnmount, watch } from 'vue';
  type ColMeta =
    | { kind: 'text' }
    | { kind: 'department' }
    | { kind: 'datetime' }
    | { kind: 'actions' };

  // formatting helpers
  const fmt = new Intl.DateTimeFormat(undefined, {
    year: 'numeric',
    month: 'short',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  });
  const formatUtc = (iso: string | null) => (iso ? fmt.format(new Date(iso)) : '—');

  const col: ColumnHelper<EmployeeSummaryResponse> = createColumnHelper<EmployeeSummaryResponse>();
  const columns: ColumnDef<EmployeeSummaryResponse>[] = [
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
      meta: { kind: 'department' as const },
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

  const {
    table,
    rows: employees,
    loading,
    totalCount,
    totalPages,
    pagination,
    setQuery,
    setPageSize,
  } = useDataTable<EmployeeSummaryResponse, EmpQuery>(columns, fetchEmployees, { name: undefined });

  watch(name, () => setQuery({ name: name.value || undefined }));

  /* ------------------------------ Handlers ------------------------------- */
</script>
