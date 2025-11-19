<template>
  <h2 class="pb-4 text-xl font-semibold text-slate-100">Clients</h2>

  <div class="flex items-center justify-between pb-4">
    <div class="flex gap-4">
      <table-search v-model="nameFilter" placeholder="Search client name..." @commit="commit" />
      <boolean-filter v-model="hasActiveProject" :options="activeOptions" />

      <!-- Client Category Filter -->
      <select
        v-model="selectedCategoryId"
        class="rounded-md border border-slate-700 bg-slate-800 px-2 py-1 text-sm text-slate-100"
      >
        <option :value="null">All Categories</option>
        <option v-for="cat in categories" :key="cat.id" :value="cat.id">
          {{ cat.name }}
        </option>
      </select>

      <!-- Client Type Filter -->
      <select
        v-model="selectedTypeId"
        class="rounded-md border border-slate-700 bg-slate-800 px-2 py-1 text-sm text-slate-100"
      >
        <option :value="null">All Types</option>
        <option v-for="t in filteredTypes" :key="t.id" :value="t.id">
          {{ t.name }}
        </option>
      </select>
    </div>

    <button
      class="flex items-center gap-2 rounded-md bg-indigo-600 px-3 py-2 text-sm font-medium text-white transition hover:bg-indigo-500"
      @click="addDialogIsOpen = true"
    >
      <circle-plus class="block h-4 w-4 shrink-0 self-center" aria-hidden="true" />
      <span class="text-white">Add Client</span>
    </button>
  </div>

  <data-table :table :loading :total-count empty-text="No clients found.">
    <!-- actions slot for this table only -->
    <template #cell="{ cell }">
      <template v-if="(cell.column.columnDef.meta as any)?.kind === 'actions'">
        <span class="flex gap-2">
          <!-- View button -->
          <button
            :class="actionButtonClass"
            aria-label="View client"
            @click="handleView(cell.row.original.id as string)"
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
  <add-client-dialog :open="addDialogIsOpen" @close="addDialogIsOpen = false" @saved="refetch" />

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
    type ClientDetailsResponse,
    type ClientSummaryResponse,
    type GetClientsRequest,
    type GetClientsResponse,
  } from '@/api/clients';
  import type { Address } from '@/api/common';
  import BooleanFilter from '@/components/BooleanFilter.vue';
  import { AddClientDialog } from '@/components/dialogs/clients';
  import DetailsDialog, { type FieldDef } from '@/components/dialogs/shared/DetailsDialog.vue';
  import { CellRenderer, DataTable, TableFooter, TableSearch } from '@/components/table';
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
  import { CheckCircle2, CirclePlus, Eye, Lock } from 'lucide-vue-next';
  import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue';

  /* ------------------------------- Constants ------------------------------ */
  // Buttons
  const actionButtonClass =
    'rounded-md bg-indigo-600 p-1.5 text-white transition hover:bg-indigo-500';

  /* ------------------------------- Details ------------------------------ */
  const fieldDef = <K extends keyof ClientDetailsResponse>(
    key: K,
    label: string,
    type: 'text' | 'date' | 'mono' | 'multiline' = 'text',
    span: 1 | 2 = 1,
  ) => ({ key: key as string, label, type, span }) satisfies FieldDef;

  const { formatUtc } = useDateFormat();

  const fields: FieldDef[] = [
    fieldDef('totalActiveProjects', 'Total Active Projects'),
    fieldDef('totalProjects', 'Total Projects'),
    fieldDef('firstName', 'Contact First Name'),
    fieldDef('middleName', 'Contact Middle Name'),
    fieldDef('lastName', 'Contact Last Name'),
    fieldDef('email', 'Email'),
    fieldDef('phone', 'Phone Number'),
    fieldDef('categoryName', 'Client Category'),
    fieldDef('typeName', 'Client Type'),
    {
      key: 'address.line1',
      label: 'Address Line 1',
      get: (r: { address: Address }) => r.address?.line1,
    },
    {
      key: 'address.line2',
      label: 'Address Line 2',
      get: (r: { address: Address }) => r.address?.line2,
    },
    {
      key: 'address.city',
      label: 'City',
      get: (r: { address: Address }) => r.address?.city,
    },
    {
      key: 'address.state',
      label: 'State',
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
    fieldDef('deletedAtUtc', 'Deleted', 'date'),
    fieldDef('deletedById', 'Deleted By', 'mono'),
  ] as any;

  /* -------------------------------- Columns ------------------------------- */
  const col: ColumnHelper<ClientSummaryResponse> = createColumnHelper<ClientSummaryResponse>();
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const columns: ColumnDef<ClientSummaryResponse, any>[] = [
    col.accessor('name', { header: 'Client Name', meta: { kind: 'text' as const } }),
    col.accessor('totalActiveProjects', {
      header: 'Active Projects',
      meta: {
        kind: 'link' as const,
        disableWhenZero: true,
        onClick: ({ cell }: { cell: Cell<ClientSummaryResponse, unknown> }) => {
          const row = cell.row.original as ClientSummaryResponse;
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
        onClick: ({ cell }: { cell: Cell<ClientSummaryResponse, unknown> }) => {
          const row = cell.row.original as ClientSummaryResponse;
          router.push({
            name: 'projects',
            query: { clientId: row.id, status: 'all' },
          });
        },
      },
    }),
    col.accessor('lastName', { header: 'Last Name', meta: { kind: 'text' as const } }),
    col.accessor('firstName', { header: 'First Name', meta: { kind: 'text' as const } }),
    col.accessor('phone', { header: 'Phone', meta: { kind: 'text' as const } }),
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

  const {
    input: nameFilter, // bound to v-model
    debounced: name, // used in fetch
    setNow: commit, // call on Enter
    cancel: cancelNameDebounce, // cleanup on unmount
  } = useDebouncedRef<string | null>(null, 500);

  onBeforeUnmount(() => {
    cancelNameDebounce();
    destroy();
  });

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

  const clientDetails = ref<ClientDetailsResponse[]>([]);
  const clientDetailsById = computed(() => {
    const map = new Map<string, ClientDetailsResponse>();
    for (const item of clientDetails.value) {
      map.set(item.id, item);
    }
    return map;
  });

  const fetchClients = async ({ page, pageSize, query }: FetchParams<ClientQuery>) => {
    const request: GetClientsRequest = {
      page,
      pageSize,
      nameFilter: query?.name || null,
      hasActiveProject: query?.hasActiveProject ?? true,
      categoryId: query?.categoryId ?? null,
      typeId: query?.typeId ?? null,
    };
    const response: GetClientsResponse = await clientService.get(request);
    clientDetails.value = response.clientListItemResponses.map(clir => clir.clientDetailsResponse);

    return {
      items: response.clientListItemResponses.map(clir => clir.clientSummaryResponse),
      totalCount: response.totalCount,
      totalPages: response.totalPages,
      page: response.page,
      pageSize: response.pageSize,
    };
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
  } = useDataTable<ClientSummaryResponse, typeof query.value>(columns, fetchClients, query);

  /* -------------------------------- Handlers ------------------------------ */
  const addDialogIsOpen = ref(false);
  const editClientDialogIsOpen = ref(false);
  const openDetailsDialog = ref(false);
  const selectedClient = ref<ClientDetailsResponse | null>(null);

  const handleView = (id: string): void => {
    const detail = clientDetailsById.value.get(id) ?? null;
    selectedClient.value = detail;
    openDetailsDialog.value = !!detail;
  };

  const handleEditClient = (summary: ClientSummaryResponse): void => {
    const detail = clientDetailsById.value.get(summary.id) ?? null;
    selectedClient.value = detail;
    editClientDialogIsOpen.value = !!detail;
  };

  onMounted(loadLookups);
</script>
