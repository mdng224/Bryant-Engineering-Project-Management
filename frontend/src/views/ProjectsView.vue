<template>
  <h2 class="pb-4 text-xl font-semibold text-slate-100">Projects</h2>

  <div class="flex items-center justify-between pb-4">
    <div class="flex gap-4">
      <TableSearch
        v-model="nameFilter"
        placeholder="Search by project name..."
        @commit="commitNameNow"
      />
      <DeletedFilter
        v-model="deletedFilter"
        label-1="Open"
        label-2="Closed"
        @change="val => setQuery({ name: name || null, isDeleted: val ?? null })"
      />
    </div>
    <button
      class="flex items-center gap-2 rounded-md bg-indigo-600 px-3 py-2 text-sm font-medium text-white transition hover:bg-indigo-500"
      @click="addDialogIsOpen = true"
    >
      <CirclePlus class="block h-4 w-4 shrink-0 self-center" aria-hidden="true" />
      <span class="text-white">Add Project</span>
    </button>
  </div>

  <DataTable :table :loading :total-count empty-text="No projects found.">
    <!-- actions slot for this table only -->
    <template #cell="{ cell }">
      <template v-if="(cell.column.columnDef.meta as any)?.kind === 'actions'">
        <span class="flex gap-2">
          <!-- View button -->
          <button
            v-if="!cell.row.original.deletedAtUtc"
            :class="actionButtonClass"
            aria-label="view project"
            @click="handleView(cell.row.original as ProjectSummaryResponse)"
          >
            <Eye class="h-4 w-4" />
          </button>

          <!-- Delete button -->
          <button
            v-if="!cell.row.original.deletedAtUtc"
            class="rounded-md bg-indigo-600 p-1.5 transition hover:bg-rose-200"
            aria-label="delete position"
            @click="handleOpenDeleteDialog(cell.row.original as ProjectSummaryResponse)"
          >
            {{ cell.row.original.deletedAtUtc }}
            <Trash2 class="h-4 w-4 text-rose-500 hover:text-rose-400" />
          </button>

          <!-- Reactivate button -->
          <button
            v-else
            class="rounded-md bg-indigo-600 p-1.5 text-emerald-200 transition hover:bg-green-200"
            aria-label="reactivate position"
            @click="handleOpenReactivateDialog(cell.row.original as ProjectSummaryResponse)"
          >
            <RotateCcw class="h-4 w-4 hover:text-green-400" />
          </button>
        </span>
      </template>
      <CellRenderer :cell />
    </template>
  </DataTable>

  <TableFooter :table :totalCount :totalPages :pagination :setPageSize />
</template>

<script setup lang="ts">
  import {
    projectService,
    type GetProjectsRequest,
    type GetProjectsResponse,
    type ProjectResponse,
    type ProjectSummaryResponse,
  } from '@/api/projects';
  import DeletedFilter from '@/components/DeletedFilter.vue';
  import CellRenderer from '@/components/table/CellRenderer.vue';
  import DataTable from '@/components/table/DataTable.vue';
  import TableFooter from '@/components/table/TableFooter.vue';
  import TableSearch from '@/components/TableSearch.vue';
  import { useDataTable, type FetchParams } from '@/composables/useDataTable';
  import { useDebouncedRef } from '@/composables/useDebouncedRef';
  import { createColumnHelper, type ColumnDef, type ColumnHelper } from '@tanstack/vue-table';
  import { CirclePlus, Eye, RotateCcw, Trash2 } from 'lucide-vue-next';
  import { computed, onBeforeUnmount, ref, watch } from 'vue';

  const actionButtonClass =
    'rounded-md bg-indigo-600 p-1.5 text-white transition hover:bg-indigo-500';

  /* -------------------------------- Columns ------------------------------- */
  const col: ColumnHelper<ProjectSummaryResponse> = createColumnHelper<ProjectSummaryResponse>();

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const columns: ColumnDef<ProjectSummaryResponse, any>[] = [
    col.accessor('name', { header: 'Project Name', meta: { kind: 'text' as const } }),
    col.accessor('clientName', { header: 'Client Name', meta: { kind: 'text' as const } }),
    col.accessor('newCode', { header: 'Code', meta: { kind: 'text' as const } }),
    col.accessor('scope', { header: 'Scope', meta: { kind: 'text' as const } }),
    col.accessor('manager', { header: 'Project Manager', meta: { kind: 'text' as const } }),
    col.accessor('type', { header: 'Project Type', meta: { kind: 'text' as const } }),
    { id: 'actions', header: 'Actions', meta: { kind: 'actions' as const }, enableSorting: false },
  ];

  /* ------------------------------- Filtering ------------------------------ */
  const deletedFilter = ref<boolean | null>(false); // default: Active Only
  const {
    input: nameFilter, // bound to v-model
    debounced: name, // used in fetch
    setNow: commitNameNow, // call on Enter
    cancel: cancelNameDebounce, // cleanup on unmount
  } = useDebouncedRef('', 500);
  onBeforeUnmount(cancelNameDebounce);

  watch([name, deletedFilter], ([n, del]) => {
    setQuery({
      name: n || null,
      isDeleted: del ?? null, // keep false as false, only null stays null
    });
  });

  /* ------------------------------- Fetching ------------------------------- */
  type EmpQuery = { name: string | null; isDeleted?: boolean | null };

  const projectDetails = ref<ProjectResponse[]>([]);
  const projectDetailsById = computed(() => new Map(projectDetails.value.map(p => [p.id, p])));

  const fetchProjects = async ({ page, pageSize, query }: FetchParams<EmpQuery>) => {
    const params: GetProjectsRequest = {
      page,
      pageSize,
      nameFilter: query?.name || null,
      isDeleted: query?.isDeleted ?? null,
    };
    console.log(params);
    const response: GetProjectsResponse = await projectService.get(params);
    console.log(response);
    // Cache details for action dialogs
    projectDetails.value = response.projectListItemResponses.map(plir => plir.details);

    return {
      items: response.projectListItemResponses.map(plir => plir.summary), // summaries are the table rows
      totalCount: response.totalCount,
      totalPages: response.totalPages,
      page: response.page,
      pageSize: response.pageSize,
    };
  };

  const { table, loading, totalCount, totalPages, pagination, setQuery, setPageSize } =
    useDataTable<ProjectSummaryResponse, EmpQuery>(columns, fetchProjects, { name: null });

  /* ------------------------------- Dialogs/UX ----------------------------- */
  const addDialogIsOpen = ref(false);
  const deleteDialogIsOpen = ref(false);
  const editProjectDialogIsOpen = ref(false);
  const reactivateDialogIsOpen = ref(false);
  const selectedProject = ref<ProjectResponse | null>(null);
  const viewProjectDialogIsOpen = ref(false);

  /* -------------------------------- Handlers ------------------------------ */
  const handleView = (summary: ProjectSummaryResponse): void => {
    const detail = projectDetailsById.value.get(summary.id) ?? null;
    selectedProject.value = detail;
    viewProjectDialogIsOpen.value = !!detail;
  };

  const handleOpenDeleteDialog = (summary: ProjectSummaryResponse): void => {
    //selectedProject.value = summary;
    //deleteDialogIsOpen.value = true;
  };

  const handleOpenReactivateDialog = (position: ProjectSummaryResponse): void => {
    // TODO: Call reactivate service
  };
</script>
