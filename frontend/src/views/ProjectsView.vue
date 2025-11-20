<template>
  <h2 class="pb-4 text-xl font-semibold text-slate-100">Projects</h2>

  <div class="flex items-center justify-between pb-4">
    <div class="flex gap-4">
      <table-search
        v-model="nameFilter"
        placeholder="Search by project name..."
        @commit="commitNameNow"
      />
      <boolean-filter v-model="deletedFilter" :options="deletedOptions" />
      <!-- 🔽 Project Manager filter -->
      <select
        v-model="selectedManager"
        class="rounded-md border border-slate-700 bg-slate-800 px-2 py-1 text-sm text-slate-100"
      >
        <option :value="null">All Project Managers</option>
        <option v-for="m in managers" :key="m" :value="m">
          {{ m }}
        </option>
      </select>
    </div>
    <button
      class="flex items-center gap-2 rounded-md bg-indigo-600 px-3 py-2 text-sm font-medium text-white transition hover:bg-indigo-500"
      @click="addDialogIsOpen = true"
    >
      <circle-plus class="block h-4 w-4 shrink-0 self-center" aria-hidden="true" />
      <span class="text-white">Add Project</span>
    </button>
  </div>

  <app-alert v-if="errorMessage" :message="errorMessage" variant="error" :icon="AlertTriangle" />

  <data-table :table :loading :total-count empty-text="No projects found.">
    <!-- actions slot for this table only -->
    <template #cell="{ cell }">
      <template v-if="(cell.column.columnDef.meta as any)?.kind === 'actions'">
        <span class="flex gap-2">
          <!-- View button -->
          <button
            :class="actionButtonClass"
            aria-label="view project"
            @click="handleOpenProjectDetails(cell.row.original.id as string)"
          >
            <eye class="h-4 w-4" />
          </button>
        </span>
      </template>
      <cell-renderer :cell />
    </template>
  </data-table>

  <table-footer :table :totalCount :totalPages :pagination :setPageSize />
  <add-project-dialog
    :open="addDialogIsOpen"
    @close="addDialogIsOpen = false"
    @saved="handleProjectSaved"
  />

  <details-dialog
    :open="openDetailsDialog"
    title="Project Details"
    :item="selectedProject as GetProjectDetailsResponse"
    :fields
    :format-utc
    @close="openDetailsDialog = false"
  />
</template>

<script setup lang="ts">
  import { extractApiError } from '@/api/error';
  import {
    projectService,
    type GetProjectDetailsResponse,
    type ListProjectsRequest,
    type ListProjectsResponse,
    type ProjectRowResponse,
  } from '@/api/projects';
  import BooleanFilter from '@/components/BooleanFilter.vue';
  import { AddProjectDialog } from '@/components/dialogs/projects';
  import { DetailsDialog } from '@/components/dialogs/shared';
  import type { FieldDef } from '@/components/dialogs/shared/DetailsDialog.vue';
  import { CellRenderer, DataTable, TableFooter, TableSearch } from '@/components/table';
  import { AppAlert } from '@/components/ui';
  import { useDataTable, type FetchParams } from '@/composables/useDataTable';
  import { useDateFormat } from '@/composables/UseDateFormat';
  import { useDebouncedRef } from '@/composables/useDebouncedRef';
  import { useProjectLookups } from '@/composables/useProjectLookups';
  import { createColumnHelper, type ColumnDef, type ColumnHelper } from '@tanstack/vue-table';
  import { AlertTriangle, CheckCircle2, CirclePlus, Eye, Trash2 } from 'lucide-vue-next';
  import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
  import { useRoute } from 'vue-router';

  const errorMessage = ref<string | null>(null);

  // ───────────────────────────── Types ─────────────────────────────
  type ProjectStatus = 'active' | 'inactive' | 'all';

  type ProjectQuery = {
    name: string | null;
    isDeleted: boolean | null;
    clientId: string | null;
    manager: string | null;
  };

  // ───────────────────────── Routing + Lookups ───────────────────────
  const route = useRoute();

  // Project manager lookups
  const { managers, loadLookups } = useProjectLookups();
  onMounted(loadLookups);

  // ───────────────────────── Details Dialog Config ───────────────────
  const actionButtonClass =
    'rounded-md bg-indigo-600 p-1.5 text-white transition hover:bg-indigo-500';

  const fieldDef = <K extends keyof GetProjectDetailsResponse>(
    key: K,
    label: string,
    type: 'text' | 'date' | 'mono' | 'multiline' = 'text',
    span: 1 | 2 = 1,
  ) =>
    ({
      key: key as string,
      label,
      type,
      span,
    }) satisfies FieldDef;

  const { formatUtc } = useDateFormat();

  const fields: FieldDef[] = [
    fieldDef('code', 'Code'),
    fieldDef('year', 'Year'),
    fieldDef('number', 'Number'),
    fieldDef('clientName', 'Client'),
    fieldDef('clientId', 'Client ID', 'mono'),
    fieldDef('manager', 'PM'),
    fieldDef('scopeId', 'Scope Id', 'mono'),
    fieldDef('scopeName', 'Scope Name'),
    fieldDef('type', 'Type'),
    fieldDef('location', 'Location'),
    fieldDef('createdAtUtc', 'Created', 'date'),
    fieldDef('createdById', 'Created By', 'mono'),
    fieldDef('updatedAtUtc', 'Updated', 'date'),
    fieldDef('updatedById', 'Updated By', 'mono'),
    fieldDef('deletedAtUtc', 'Closed', 'date'),
    fieldDef('deletedBy', 'Closed By', 'text'),
  ] as any;

  // ───────────────────────────── Table Columns ──────────────────────
  const col: ColumnHelper<ProjectRowResponse> = createColumnHelper<ProjectRowResponse>();

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const columns: ColumnDef<ProjectRowResponse, any>[] = [
    col.accessor('code', { header: 'Code', meta: { kind: 'text' as const } }),
    col.accessor('name', { header: 'Project Name', meta: { kind: 'text' as const } }),
    col.accessor('clientName', { header: 'Client Name', meta: { kind: 'text' as const } }),
    col.accessor('scopeName', { header: 'Scope', meta: { kind: 'text' as const } }),
    col.accessor('manager', { header: 'Project Manager', meta: { kind: 'text' as const } }),
    col.accessor('type', { header: 'Project Type', meta: { kind: 'text' as const } }),
    col.accessor('location', { header: 'Location', meta: { kind: 'text' as const } }),
    { id: 'actions', header: 'Actions', meta: { kind: 'actions' as const }, enableSorting: false },
  ];

  // ───────────────────────────── Filters & Search ────────────────────
  const selectedManager = ref<string | null>(null);
  const deletedFilter = ref<boolean | null>(null);

  const deletedOptions = [
    { value: null, label: 'All', icon: Eye, color: 'text-indigo-300' },
    { value: false, label: 'Active', icon: CheckCircle2, color: 'text-emerald-400' },
    { value: true, label: 'Deleted', icon: Trash2, color: 'text-rose-400' },
  ];

  const {
    input: nameFilter, // bound to v-model
    debounced: name, // used in fetch
    setNow: commitNameNow, // call on Enter
    cancel: cancelNameDebounce, // cleanup on unmount
  } = useDebouncedRef('', 500);

  // ───────────────────────── Route → Filter Mapping ──────────────────
  const statusFromRoute = computed<ProjectStatus | undefined>(() => {
    const raw = route.query.status;
    if (!raw) return undefined;
    const s = String(raw).toLowerCase();
    if (s === 'active') return 'active';
    if (s === 'inactive') return 'inactive';
    if (s === 'all') return 'all';
    return undefined;
  });

  const clientIdFromRoute = computed<string | null>(() => {
    const raw = route.query.clientId;
    return raw ? String(raw) : null;
  });

  const mapStatusToIsDeleted = (status: ProjectStatus | undefined): boolean | null => {
    if (status === 'active') return false;
    if (status === 'inactive') return true;
    if (status === 'all') return null;
    return null;
  };

  const query = computed<ProjectQuery>(() => {
    const status = statusFromRoute.value;
    const clientId = clientIdFromRoute.value;
    const isDeletedFromStatus = mapStatusToIsDeleted(status);

    const effectiveIsDeleted =
      status !== undefined ? isDeletedFromStatus : (deletedFilter.value as boolean | null);

    return {
      name: name.value ?? null,
      isDeleted: effectiveIsDeleted,
      clientId,
      manager: selectedManager.value,
    };
  });

  // ─────────────────────────── Data Fetching ─────────────────────────
  const projectDetails = ref<GetProjectDetailsResponse[]>([]);
  const projectDetailsById = computed(() => new Map(projectDetails.value.map(p => [p.id, p])));

  const fetchProjects = async ({ page, pageSize, query }: FetchParams<ProjectQuery>) => {
    const request: ListProjectsRequest = {
      page,
      pageSize,
      nameFilter: query?.name ?? null,
      isDeleted: query?.isDeleted ?? null,
      clientId: query?.clientId ?? null,
      manager: query?.manager ?? null,
    };

    try {
      const response: ListProjectsResponse = await projectService.list(request);
      return {
        items: response.projects,
        totalCount: response.totalCount,
        totalPages: response.totalPages,
        page: response.page,
        pageSize: response.pageSize,
      };
    } catch (err: unknown) {
      const msg: string = extractApiError(err, 'project');
      errorMessage.value = msg;

      // Fallback so the table can still render gracefully
      return {
        items: [],
        totalCount: 0,
        totalPages: 0,
        page,
        pageSize,
      };
    }
  };

  const { table, loading, totalCount, totalPages, pagination, setPageSize, destroy } = useDataTable<
    ProjectRowResponse,
    ProjectQuery
  >(columns, fetchProjects, query);

  // ───────────────────────────── Dialog State ─────────────────────────
  const addDialogIsOpen = ref(false);
  const deleteDialogIsOpen = ref(false);
  const editProjectDialogIsOpen = ref(false);
  const openDetailsDialog = ref(false);
  const reactivateDialogIsOpen = ref(false);
  const selectedProject = ref<GetProjectDetailsResponse | null>(null);

  // ───────────────────────────── Handlers ────────────────────────────
  const handleOpenProjectDetails = async (id: string): Promise<void> => {
    errorMessage.value = null;

    if (selectedProject.value?.id === id) {
      openDetailsDialog.value = true;
      return;
    }

    try {
      const response: GetProjectDetailsResponse = await projectService.getDetails(id);
      selectedProject.value = response;
      openDetailsDialog.value = Boolean(response);
    } catch (err: unknown) {
      const msg: string = extractApiError(err, 'project');
      errorMessage.value = msg;
    }
  };

  const handleOpenDeleteDialog = (summary: ProjectRowResponse): void => {
    // selectedProject.value = summary;
    // deleteDialogIsOpen.value = true;
  };

  const handleOpenReactivateDialog = (position: ProjectRowResponse): void => {
    // TODO: Call reactivate service
  };

  const handleProjectSaved = () => {
    // however you currently refresh; if useDataTable returns a `reload`, call that
    pagination.pageIndex = 0; // or call your own reload function
  };

  // ───────────────────────────── Cleanup ──────────────────────────────
  onBeforeUnmount(() => {
    cancelNameDebounce();
    destroy();
  });
</script>
