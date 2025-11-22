<template>
  <h2 class="pb-4 text-xl font-semibold text-slate-100">Clients</h2>

  <div class="flex items-center justify-between pb-4">
    <div class="flex gap-4">
      <table-search v-model="nameFilter" placeholder="Search client name..." @commit="commit" />
      <boolean-filter v-model="hasActiveProject" :options="activeOptions" />

      <!-- Client Category Filter -->
      <select-filter
        v-model="selectedCategoryId"
        :options="categoryOptions"
        placeholder="All Categories"
      />

      <!-- Client Type Filter -->
      <select-filter v-model="selectedTypeId" :options="typeOptions" placeholder="All Types" />
    </div>

    <button
      class="flex items-center gap-2 rounded-md bg-indigo-600 px-3 py-2 text-sm font-medium text-white transition hover:bg-indigo-500"
      @click="addDialogIsOpen = true"
    >
      <circle-plus class="block h-4 w-4 shrink-0 self-center" aria-hidden="true" />
      <span class="text-white">Add Client</span>
    </button>
  </div>

  <app-alert v-if="errorMessage" :message="errorMessage" variant="error" :icon="AlertTriangle" />

  <data-table :table :loading :total-count empty-text="No clients found.">
    <!-- actions slot for this table only -->
    <template #cell="{ cell }">
      <template v-if="(cell.column.columnDef.meta as any)?.kind === 'actions'">
        <span class="flex gap-2">
          <!-- View button -->
          <button
            :class="actionButtonClass"
            aria-label="View client"
            @click="handleOpenClientDetails(cell.row.original.id as string)"
          >
            <eye class="h-4 w-4" />
          </button>
        </span>
      </template>
      <cell-renderer :cell />
    </template>
  </data-table>

  <table-footer :table :totalCount :totalPages :pagination :setPageSize />

  <!-- Dialogs -->
  <add-client-dialog
    :open="addDialogIsOpen"
    @close="addDialogIsOpen = false"
    @saved="handleClientSaved"
  />

  <details-dialog
    :open="openDetailsDialog"
    title="Client Details"
    :item="selectedClient"
    :fields
    :format-utc
    @close="openDetailsDialog = false"
  />
  <!--
  <EditClientDialog
    :open="editClientDialogIsOpen"
    :selected-client
    @close="editClientDialogIsOpen = false"
  />-->
</template>

<script setup lang="ts">
  import {
    clientService,
    type ClientRowResponse,
    type GetClientDetailsResponse,
    type ListClientsRequest,
    type ListClientsResponse,
  } from '@/api/clients';
  import { extractApiError } from '@/api/error';
  import { AddClientDialog } from '@/components/dialogs/clients';
  import DetailsDialog, { type FieldDef } from '@/components/dialogs/shared/DetailsDialog.vue';
  import { CellRenderer, DataTable, TableFooter, TableSearch } from '@/components/table';
  import { AppAlert } from '@/components/ui';
  import BooleanFilter from '@/components/ui/BooleanFilter.vue';
  import SelectFilter from '@/components/ui/SelectFilter.vue';
  import { useClientLookups } from '@/composables/useClientLookups';
  import { useDataTable, type FetchParams } from '@/composables/useDataTable';
  import { useDateFormat } from '@/composables/UseDateFormat';
  import { useDebouncedRef } from '@/composables/useDebouncedRef';
  import router from '@/router';
  import {
    createColumnHelper,
    type Cell,
    type ColumnDef,
    type ColumnHelper,
  } from '@tanstack/vue-table';
  import {
    AlertTriangle,
    CheckCircle2,
    CirclePlus,
    Eye,
    FolderTree,
    Lock,
    Tag,
  } from 'lucide-vue-next';
  import { computed, onBeforeUnmount, onMounted, ref, watch, type Component } from 'vue';

  const errorMessage = ref<string | null>(null);

  /* ------------------------------- Constants ------------------------------ */
  // Buttons
  const actionButtonClass =
    'rounded-md bg-indigo-600 p-1.5 text-white transition hover:bg-indigo-500';

  /* ------------------------------- Details ------------------------------ */
  const fieldDef = <K extends keyof GetClientDetailsResponse>(
    key: K,
    label: string,
    type: 'text' | 'date' | 'mono' | 'multiline' = 'text',
    span: 1 | 2 = 1,
  ) => ({ key: key as string, label, type, span }) satisfies FieldDef;

  const { formatUtc } = useDateFormat();

  const fields: FieldDef[] = [
    fieldDef('totalActiveProjects', 'Total Active Projects'),
    fieldDef('totalProjects', 'Total Projects'),
    fieldDef('categoryName', 'Client Category'),
    fieldDef('typeName', 'Client Type'),
    fieldDef('createdAtUtc', 'Created', 'date'),
    fieldDef('createdById', 'Created By', 'mono'),
    fieldDef('updatedAtUtc', 'Updated', 'date'),
    fieldDef('updatedById', 'Updated By', 'mono'),
    fieldDef('deletedAtUtc', 'Deleted', 'date'),
    fieldDef('deletedById', 'Deleted By', 'mono'),
  ] as any;

  /* -------------------------------- Columns ------------------------------- */
  const col: ColumnHelper<ClientRowResponse> = createColumnHelper<ClientRowResponse>();
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const columns: ColumnDef<ClientRowResponse, any>[] = [
    col.accessor('name', { header: 'Client Name', meta: { kind: 'text' as const } }),
    col.accessor('totalActiveProjects', {
      header: 'Active Projects',
      meta: {
        kind: 'link' as const,
        disableWhenZero: true,
        onClick: ({ cell }: { cell: Cell<ClientRowResponse, unknown> }) => {
          const row = cell.row.original as ClientRowResponse;
          router.push({
            name: 'projects',
            query: { clientId: row.id, status: 'active' },
          });
        },
      },
    }),

    col.accessor('totalProjects', {
      header: 'Total Projects',
      meta: {
        kind: 'link' as const,
        disableWhenZero: true,
        onClick: ({ cell }: { cell: Cell<ClientRowResponse, unknown> }) => {
          const row = cell.row.original as ClientRowResponse;
          router.push({
            name: 'projects',
            query: { clientId: row.id, status: 'all' },
          });
        },
      },
    }),
    col.accessor('categoryName', { header: 'Category', meta: { kind: 'text' as const } }),
    col.accessor('typeName', { header: 'Type', meta: { kind: 'text' as const } }),
    { id: 'actions', header: 'Actions', meta: { kind: 'actions' as const }, enableSorting: false },
  ];

  /* ------------------------------- Filtering ------------------------------ */
  const hasActiveProject = ref<boolean>(true); // default Active
  const { categories, types, loadLookups, getTypesForCategory } = useClientLookups();
  const filteredTypes = computed(() => getTypesForCategory(selectedCategoryId.value));
  const selectedCategoryId = ref<string | null>(null);
  const selectedTypeId = ref<string | null>(null);

  const activeOptions = [
    { value: true, label: 'Active', icon: CheckCircle2, color: 'text-emerald-400' },
    { value: false, label: 'Inactive', icon: Lock, color: 'text-rose-400' },
  ];

  type SelectOption = { value: string | null; label: string; icon?: Component };

  const categoryOptions = computed<SelectOption[]>(() => [
    { value: null, label: 'All Categories', icon: FolderTree },
    ...categories.value.map(cat => ({
      value: cat.id,
      label: cat.name,
    })),
  ]);

  const typeOptions = computed<SelectOption[]>(() => [
    { value: null, label: 'All Types', icon: Tag },
    ...filteredTypes.value.map(t => ({
      value: t.id,
      label: t.name,
    })),
  ]);

  const {
    input: nameFilter, // bound to v-model
    debounced: name, // used in fetch
    setNow: commit, // call on Enter
    cancel: cancelNameDebounce, // cleanup on unmount
  } = useDebouncedRef<string | null>(null, 500);

  watch(selectedCategoryId, () => {
    selectedTypeId.value = null;
  });

  /* ------------------------------- Fetching ------------------------------- */
  type ClientQuery = {
    name: string | null;
    hasActiveProject: boolean;
    categoryId: string | null;
    typeId: string | null;
  };

  const clientDetails = ref<GetClientDetailsResponse[]>([]);
  const clientDetailsById = computed(() => {
    const map = new Map<string, GetClientDetailsResponse>();
    for (const item of clientDetails.value) {
      map.set(item.id, item);
    }
    return map;
  });

  const fetchClients = async ({ page, pageSize, query }: FetchParams<ClientQuery>) => {
    const request: ListClientsRequest = {
      page,
      pageSize,
      nameFilter: query?.name || null,
      hasActiveProject: query?.hasActiveProject ?? true,
      categoryId: query?.categoryId ?? null,
      typeId: query?.typeId ?? null,
    };
    try {
      const response: ListClientsResponse = await clientService.list(request);

      return {
        items: response.clients,
        totalCount: response.totalCount,
        totalPages: response.totalPages,
        page: response.page,
        pageSize: response.pageSize,
      };
    } catch (err: unknown) {
      const msg: string = extractApiError(err, 'client');
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
    hasActiveProject: hasActiveProject.value,
    categoryId: selectedCategoryId.value,
    typeId: selectedTypeId.value,
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
  } = useDataTable<ClientRowResponse, typeof query.value>(columns, fetchClients, query);

  /* -------------------------------- Handlers ------------------------------ */
  const addDialogIsOpen = ref(false);
  const editClientDialogIsOpen = ref(false);
  const openDetailsDialog = ref(false);
  const selectedClient = ref<GetClientDetailsResponse | null>(null);

  const handleClientSaved = () => {
    // ensure we show the fresh data from the first page
    pagination.pageIndex = 0;
    refetch();
  };

  const handleOpenClientDetails = async (id: string): Promise<void> => {
    errorMessage.value = null;

    if (selectedClient.value?.id === id) {
      openDetailsDialog.value = true;
      return;
    }

    try {
      const response: GetClientDetailsResponse = await clientService.getDetails(id);
      selectedClient.value = response;
      openDetailsDialog.value = Boolean(response);
    } catch (err: unknown) {
      const msg: string = extractApiError(err, 'project');
      errorMessage.value = msg;
    }
  };

  const handleEditClient = (summary: ClientRowResponse): void => {
    const detail = clientDetailsById.value.get(summary.id) ?? null;
    selectedClient.value = detail;
    editClientDialogIsOpen.value = !!detail;
  };

  onMounted(loadLookups);

  onBeforeUnmount(() => {
    cancelNameDebounce();
    destroy();
  });
</script>
