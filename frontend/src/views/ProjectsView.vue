<template>
  <h2 class="pb-4 text-xl font-semibold text-slate-100">Projects</h2>

  <div class="flex items-center justify-between pb-4">
    <div class="flex gap-4">
      <table-search
        v-model="nameFilter"
        placeholder="Search by project name..."
        @commit="commitNameNow"
      />
      <deleted-filter
        v-model="deletedFilter"
        label-1="Open"
        label-2="Closed"
        @change="val => setQuery({ name: name || null, isDeleted: val ?? false })"
      />
    </div>
    <button
      class="flex items-center gap-2 rounded-md bg-indigo-600 px-3 py-2 text-sm font-medium text-white transition hover:bg-indigo-500"
      @click="addDialogIsOpen = true"
    >
      <circle-plus class="block h-4 w-4 shrink-0 self-center" aria-hidden="true" />
      <span class="text-white">Add Project</span>
    </button>
  </div>

  <data-table :table :loading :total-count empty-text="No projects found.">
    <!-- actions slot for this table only -->
    <template #cell="{ cell }">
      <template v-if="(cell.column.columnDef.meta as any)?.kind === 'actions'">
        <span class="flex gap-2">
          <!-- View button -->
          <button
            v-if="!cell.row.original.deletedAtUtc"
            :class="actionButtonClass"
            aria-label="view project"
            @click="handleView(cell.row.original.id as string)"
          >
            <eye class="h-4 w-4" />
          </button>

          <!-- Delete button -->
          <button
            v-if="!cell.row.original.deletedAtUtc"
            class="rounded-md bg-indigo-600 p-1.5 transition hover:bg-rose-200"
            aria-label="delete position"
            @click="handleOpenDeleteDialog(cell.row.original as ProjectSummaryResponse)"
          >
            {{ cell.row.original.deletedAtUtc }}
            <Lock class="h-4 w-4 text-rose-500 hover:text-rose-400" />
          </button>

          <!-- Reactivate button -->
          <button
            v-else
            class="rounded-md bg-indigo-600 p-1.5 text-emerald-200 transition hover:bg-green-200"
            aria-label="reactivate position"
            @click="handleOpenReactivateDialog(cell.row.original as ProjectSummaryResponse)"
          >
            <lock-open class="h-4 w-4 hover:text-green-400" />
          </button>
        </span>
      </template>
      <cell-renderer :cell />
    </template>
  </data-table>

  <table-footer :table :totalCount :totalPages :pagination :setPageSize />

  <details-dialog
    :open="openDetailsDialog"
    title="Project Details"
    :item="selectedProject as ProjectResponse"
    :fields
    :format-utc
    @close="openDetailsDialog = false"
  />
</template>

<script setup lang="ts">
  import type { Address } from '@/api/common';
  import {
    projectService,
    type GetProjectsRequest,
    type GetProjectsResponse,
    type ProjectResponse,
    type ProjectSummaryResponse,
  } from '@/api/projects';
  import DeletedFilter from '@/components/DeletedFilter.vue';
  import type { FieldDef } from '@/components/dialogs/DetailsDialog.vue';
  import DetailsDialog from '@/components/dialogs/DetailsDialog.vue';
  import CellRenderer from '@/components/table/CellRenderer.vue';
  import DataTable from '@/components/table/DataTable.vue';
  import TableFooter from '@/components/table/TableFooter.vue';
  import TableSearch from '@/components/TableSearch.vue';
  import { useDataTable, type FetchParams } from '@/composables/useDataTable';
  import { useDateFormat } from '@/composables/UseDateFormat';
  import { useDebouncedRef } from '@/composables/useDebouncedRef';
  import { createColumnHelper, type ColumnDef, type ColumnHelper } from '@tanstack/vue-table';
  import { CirclePlus, Eye, Lock, LockOpen } from 'lucide-vue-next';
  import { computed, onBeforeUnmount, ref, watch } from 'vue';

  const actionButtonClass =
    'rounded-md bg-indigo-600 p-1.5 text-white transition hover:bg-indigo-500';

  /* ------------------------------- Details ------------------------------ */
  const fieldDef = <K extends keyof ProjectResponse>(
    key: K,
    label: string,
    type: 'text' | 'date' | 'mono' | 'multiline' = 'text',
    span: 1 | 2 = 1,
  ) => ({ key: key as string, label, type, span }) satisfies FieldDef;

  const { formatUtc } = useDateFormat();

  const fields: FieldDef[] = [
    fieldDef('code', 'Code'),
    fieldDef('year', 'Year'),
    fieldDef('number', 'Number'),
    fieldDef('clientName', 'Client'),
    fieldDef('clientId', 'Client ID', 'mono'),
    fieldDef('manager', 'PM'),
    fieldDef('scope', 'Scope'),
    fieldDef('type', 'Type'),
    {
      key: 'address.line1',
      label: 'Address Line 1',
      type: 'text',
      get: (r: { address: Address }) => r.address?.line1,
    },
    {
      key: 'address.line2',
      label: 'Address Line 2',
      type: 'text',
      get: (r: { address: Address }) => r.address?.line2,
    },
    {
      key: 'address.city',
      label: 'City',
      type: 'text',
      get: (r: { address: Address }) => r.address?.city,
    },
    {
      key: 'address.state',
      label: 'State',
      type: 'text',
      get: (r: { address: Address }) => r.address?.state,
    },
    {
      key: 'address.postalCode',
      label: 'ZIP',
      get: (r: { address: Address }) => r.address?.postalCode,
    },
    fieldDef('createdAtUtc', 'Created', 'date'),
    fieldDef('createdById', 'Created By', 'mono'),
    fieldDef('updatedAtUtc', 'Updated', 'date'),
    fieldDef('updatedById', 'Updated By', 'mono'),
    fieldDef('deletedAtUtc', 'Closed', 'date'),
    fieldDef('deletedBy', 'Closed By', 'text'),
  ] as any;

  /* -------------------------------- Columns ------------------------------- */
  const col: ColumnHelper<ProjectSummaryResponse> = createColumnHelper<ProjectSummaryResponse>();

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const columns: ColumnDef<ProjectSummaryResponse, any>[] = [
    col.accessor('code', { header: 'Code', meta: { kind: 'text' as const } }),
    col.accessor('name', { header: 'Project Name', meta: { kind: 'text' as const } }),
    col.accessor('clientName', { header: 'Client Name', meta: { kind: 'text' as const } }),
    col.accessor('manager', { header: 'Project Manager', meta: { kind: 'text' as const } }),
    col.accessor('scope', { header: 'Scope', meta: { kind: 'text' as const } }),
    col.accessor('type', { header: 'Project Type', meta: { kind: 'text' as const } }),
    { id: 'actions', header: 'Actions', meta: { kind: 'actions' as const }, enableSorting: false },
  ];

  /* ------------------------------- Filtering ------------------------------ */
  const deletedFilter = ref(false); // default: show only active (not deleted)

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
      isDeleted: del, // keep false as false, only null stays null
    });
  });

  /* ------------------------------- Fetching ------------------------------- */
  type ProjectQuery = { name: string | null; isDeleted: boolean };

  const projectDetails = ref<ProjectResponse[]>([]);
  const projectDetailsById = computed(() => new Map(projectDetails.value.map(p => [p.id, p])));

  const fetchProjects = async ({ page, pageSize, query }: FetchParams<ProjectQuery>) => {
    const request: GetProjectsRequest = {
      page,
      pageSize,
      nameFilter: query?.name || null,
      isDeleted: query?.isDeleted ?? false,
    };
    const response: GetProjectsResponse = await projectService.get(request);
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
    useDataTable<ProjectSummaryResponse, ProjectQuery>(columns, fetchProjects, {
      name: null,
      isDeleted: false,
    });

  /* ------------------------------- Dialogs/UX ----------------------------- */
  const addDialogIsOpen = ref(false);
  const deleteDialogIsOpen = ref(false);
  const editProjectDialogIsOpen = ref(false);
  const openDetailsDialog = ref(false);
  const reactivateDialogIsOpen = ref(false);
  const selectedProject = ref<ProjectResponse | null>(null);

  /* -------------------------------- Handlers ------------------------------ */
  const handleView = (id: string): void => {
    const detail = projectDetailsById.value.get(id) ?? null;
    selectedProject.value = detail;
    openDetailsDialog.value = !!detail;
  };

  const handleOpenDeleteDialog = (summary: ProjectSummaryResponse): void => {
    //selectedProject.value = summary;
    //deleteDialogIsOpen.value = true;
  };

  const handleOpenReactivateDialog = (position: ProjectSummaryResponse): void => {
    // TODO: Call reactivate service
  };
</script>
