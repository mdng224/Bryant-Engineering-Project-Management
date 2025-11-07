<template>
  <h2 class="pb-4 text-xl font-semibold text-slate-100">Clients</h2>

  <div class="flex items-center justify-between pb-4">
    <div class="flex gap-4">
      <TableSearch v-model="nameFilter" placeholder="Search client name..." @commit="commit" />
      <DeletedFilter v-model="deletedFilter" @change="handleDeletedFilterChange" />
    </div>

    <button
      class="flex items-center gap-2 rounded-md bg-indigo-600 px-3 py-2 text-sm font-medium text-white transition hover:bg-indigo-500"
      @click="addDialogIsOpen = true"
    >
      <CirclePlus class="block h-4 w-4 shrink-0 self-center" aria-hidden="true" />
      <span class="text-white">Add Client</span>
    </button>
  </div>

  <DataTable :table :loading :total-count empty-text="No clients found.">
    <!-- actions slot for this table only -->
    <template #cell="{ cell }">
      <template v-if="(cell.column.columnDef.meta as any)?.kind === 'actions'">
        <span class="flex gap-2">
          <!-- View button -->
          <button
            :class="actionButtonClass"
            aria-label="View client"
            @click="handleView(cell.row.original as ClientSummaryResponse)"
          >
            <Eye class="h-4 w-4" />
          </button>
        </span>
      </template>
      <CellRenderer :cell="cell" />
    </template>
  </DataTable>

  <TableFooter :table :totalCount :totalPages :pagination :setPageSize />
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
  import DeletedFilter from '@/components/DeletedFilter.vue';
  import CellRenderer from '@/components/table/CellRenderer.vue';
  import DataTable from '@/components/table/DataTable.vue';
  import TableFooter from '@/components/table/TableFooter.vue';
  import TableSearch from '@/components/TableSearch.vue';
  import { useDataTable, type FetchParams } from '@/composables/useDataTable';
  import { useDebouncedRef } from '@/composables/useDebouncedRef';
  import { createColumnHelper, type ColumnDef, type ColumnHelper } from '@tanstack/vue-table';
  import { CirclePlus, Eye } from 'lucide-vue-next';
  import { computed, onBeforeUnmount, ref, watch } from 'vue';

  /* ------------------------------- Constants ------------------------------ */
  // Buttons
  const actionButtonClass =
    'rounded-md bg-indigo-600 p-1.5 text-white transition hover:bg-indigo-500';

  /* -------------------------------- Columns ------------------------------- */
  const col: ColumnHelper<ClientSummaryResponse> = createColumnHelper<ClientSummaryResponse>();
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const columns: ColumnDef<ClientSummaryResponse, any>[] = [
    col.accessor('name', { header: 'Last Name', meta: { kind: 'text' as const } }),
    col.accessor('contactLast', { header: 'Last Name', meta: { kind: 'text' as const } }),
    col.accessor('contactFirst', { header: 'First Name', meta: { kind: 'text' as const } }),
    col.accessor('contactMiddle', { header: 'Middle Name', meta: { kind: 'text' as const } }),
    col.accessor('email', { header: 'Email', meta: { kind: 'text' as const } }),
    col.accessor('phone', { header: 'Phone', meta: { kind: 'text' as const } }),
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
    setNow: commit, // call on Enter
    cancel: cancelNameDebounce, // cleanup on unmount
  } = useDebouncedRef('', 500);
  onBeforeUnmount(cancelNameDebounce);

  /* ------------------------------- Fetching ------------------------------- */
  type ClientQuery = { name?: string; isDeleted?: boolean | null };

  const clientDetails = ref<ClientResponse[]>([]);
  const clientDetailsById = computed(() => new Map(clientDetails.value.map(e => [e.id, e])));

  const fetchClients = async ({ page, pageSize, query }: FetchParams<ClientQuery>) => {
    const params: GetClientsRequest = {
      page,
      pageSize,
      name: query?.name || null,
      isDeleted: query?.isDeleted || null,
    };
    const response: GetClientsResponse = await clientService.get(params);

    return {
      items: response.clientListItemResponses.map(clir => clir.summary), // summaries are the table rows
      totalCount: response.totalCount,
      totalPages: response.totalPages,
      page: response.page,
      pageSize: response.pageSize,
    };
  };

  const { table, loading, totalCount, totalPages, pagination, setQuery, setPageSize } =
    useDataTable<ClientSummaryResponse, ClientQuery>(columns, fetchClients, { name: undefined });

  // Update query when search changes
  watch(name, () => setQuery({ name: name.value || undefined }));

  /* -------------------------------- Handlers ------------------------------ */
  const addDialogIsOpen = ref(false);
  const viewClientDialogIsOpen = ref(false);
  const selectedClient = ref<ClientResponse | null>(null);
  const editClientDialogIsOpen = ref(false);

  const handleView = (summary: ClientSummaryResponse): void => {
    const detail = clientDetailsById.value.get(summary.id) ?? null;
    selectedClient.value = detail;
    viewClientDialogIsOpen.value = !!detail;
  };

  const handleEditClient = (summary: ClientSummaryResponse): void => {
    const detail = clientDetailsById.value.get(summary.id) ?? null;
    selectedClient.value = detail;
    editClientDialogIsOpen.value = !!detail;
  };
</script>
