<template>
  <h2 class="pb-4 text-xl font-semibold text-slate-100">Clients</h2>

  <div class="flex items-center justify-between pb-4">
    <div class="flex gap-4">
      <table-search v-model="nameFilter" placeholder="Search client name..." @commit="commit" />
      <boolean-filter v-model="hasActiveProject" :options="activeOptions" />
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

  <details-dialog
    :open="openDetailsDialog"
    title="Client Details"
    :item="selectedClient"
    :fields
    :format-utc
    @close="openDetailsDialog = false"
  />
  <!--
  <ViewClientDialog
    :open="viewClientDialogIsOpen"
    :selected-client
    @close="viewClientDialogIsOpen = false"
  />

  <EditClientDialog
    :open="editClientDialogIsOpen"
    :selected-client
    @close="editClientDialogIsOpen = false"
  />-->
</template>

<script setup lang="ts">
  import type { ClientResponse } from '@/api/clients';
  import { clientService } from '@/api/clients';
  import type {
    ClientSummaryResponse,
    GetClientsRequest,
    GetClientsResponse,
  } from '@/api/clients/contracts';
  import type { Address } from '@/api/common';
  import BooleanFilter from '@/components/BooleanFilter.vue';
  import DetailsDialog, { type FieldDef } from '@/components/dialogs/DetailsDialog.vue';
  import CellRenderer from '@/components/table/CellRenderer.vue';
  import DataTable from '@/components/table/DataTable.vue';
  import TableFooter from '@/components/table/TableFooter.vue';
  import TableSearch from '@/components/TableSearch.vue';
  import { useDataTable, type FetchParams } from '@/composables/useDataTable';
  import { useDateFormat } from '@/composables/UseDateFormat';
  import { useDebouncedRef } from '@/composables/useDebouncedRef';
  import { createColumnHelper, type ColumnDef, type ColumnHelper } from '@tanstack/vue-table';
  import { CheckCircle2, CirclePlus, Eye, Lock } from 'lucide-vue-next';
  import { computed, onBeforeUnmount, ref } from 'vue';

  /* ------------------------------- Constants ------------------------------ */
  // Buttons
  const actionButtonClass =
    'rounded-md bg-indigo-600 p-1.5 text-white transition hover:bg-indigo-500';

  /* ------------------------------- Details ------------------------------ */
  const fieldDef = <K extends keyof ClientResponse>(
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
    fieldDef('lastName', 'Contact Last Name'),
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
      meta: { kind: 'text' as const },
    }),
    col.accessor('totalProjects', { header: 'Total Projects', meta: { kind: 'text' as const } }),
    col.accessor('lastName', { header: 'Last Name', meta: { kind: 'text' as const } }),
    col.accessor('firstName', { header: 'First Name', meta: { kind: 'text' as const } }),
    col.accessor('email', { header: 'Email', meta: { kind: 'text' as const } }),
    col.accessor('phone', { header: 'Phone', meta: { kind: 'text' as const } }),
    { id: 'actions', header: 'Actions', meta: { kind: 'actions' as const }, enableSorting: false },
  ];

  /* ------------------------------- Filtering ------------------------------ */
  const hasActiveProject = ref<boolean>(true); // default Active

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

  /* ------------------------------- Fetching ------------------------------- */
  type ClientQuery = { name: string | null; hasActiveProject: boolean };

  const clientDetails = ref<ClientResponse[]>([]);
  const clientDetailsById = computed(() => {
    const map = new Map<string, ClientResponse>();
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
    };
    const response: GetClientsResponse = await clientService.get(request);
    clientDetails.value = response.clientListItemResponses.map(clir => clir.details);

    return {
      items: response.clientListItemResponses.map(clir => clir.summary), // summaries are the table rows
      totalCount: response.totalCount,
      totalPages: response.totalPages,
      page: response.page,
      pageSize: response.pageSize,
    };
  };

  const query = computed(() => ({
    name: name.value ?? null,
    hasActiveProject: hasActiveProject.value,
  }));

  const { table, loading, totalCount, totalPages, pagination, setPageSize, destroy } = useDataTable<
    ClientSummaryResponse,
    typeof query.value
  >(columns, fetchClients, query);

  /* -------------------------------- Handlers ------------------------------ */
  const addDialogIsOpen = ref(false);
  const editClientDialogIsOpen = ref(false);
  const openDetailsDialog = ref(false);
  const selectedClient = ref<ClientResponse | null>(null);

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
</script>
