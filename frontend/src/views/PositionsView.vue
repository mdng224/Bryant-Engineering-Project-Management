<template>
  <div class="pb-4">
    <TableSearch
      v-model="positionNameFilter"
      placeholder="Search by position name..."
      @commit="commitPositionNameNow"
    />
  </div>

  <div class="flex items-center justify-between pb-4">
    <h2 class="text-xl font-semibold text-slate-100">Positions</h2>
    <button
      class="flex items-center gap-2 rounded-md bg-indigo-600 px-3 py-2 text-sm font-medium text-white transition hover:bg-indigo-500"
      @click="addDialogIsOpen = true"
    >
      <CirclePlus class="block h-4 w-4 shrink-0 self-center" aria-hidden="true" />

      <span class="text-white">Add Position</span>
    </button>
  </div>
  <p
    v-if="errorMessage"
    ref="errorEl"
    class="flex items-center gap-2 rounded-lg border border-rose-800 bg-rose-900/30 px-3.5 py-2 text-sm leading-tight text-rose-200"
    role="alert"
    aria-live="assertive"
    tabindex="-1"
  >
    <AlertTriangle class="block h-4 w-4 shrink-0 self-center" aria-hidden="true" />
    <span>{{ errorMessage }}</span>
  </p>

  <DataTable
    :table="table as unknown as import('@tanstack/vue-table').Table<unknown>"
    :loading
    :total-count
    empty-text="No positions found."
  >
    <!-- actions slot for this table only -->
    <template #cell="{ cell }">
      <template v-if="(cell.column.columnDef.meta as any)?.kind === 'actions'">
        <template class="flex gap-2">
          <button
            :class="actionButtonClass"
            aria-label="Edit position"
            @click="handleEditPosition(cell.row.original as PositionResponse)"
          >
            <Pencil class="h-4 w-4" />
          </button>

          <button
            class="rounded-md bg-indigo-600 p-1.5 transition hover:bg-rose-200"
            aria-label="delete position"
            @click="handleOpenDeleteDialog(cell.row.original as PositionResponse)"
          >
            <Trash2 class="h-4 w-4 text-rose-500 hover:text-rose-400" />
          </button>
        </template>
      </template>
      <CellRenderer :cell="cell" />
    </template>
  </DataTable>

  <TableFooter :table :total-count :total-pages :pagination :set-page-size />

  <!-- Dialogs -->
  <AddPositionDialog :open="addDialogIsOpen" @close="addDialogIsOpen = false" @saved="refetch" />

  <DeleteDialog
    :open="deleteDialogIsOpen"
    title="Delete position"
    message="This action cannot be undone. This will permanently delete the selected position."
    @confirm="handleDeletePosition"
    @close="deleteDialogIsOpen = false"
  />

  <EditPositionDialog
    :open="editPositionDialogIsOpen"
    :selected-position
    @close="editPositionDialogIsOpen = false"
    @save="refetch"
  />
</template>

<script setup lang="ts">
  import { extractApiError } from '@/api/error';
  import type {
    GetPositionsRequest,
    GetPositionsResponse,
    PositionResponse,
  } from '@/api/positions/contracts';
  import { positionService } from '@/api/positions/services';
  import AddPositionDialog from '@/components/dialogs/AddPositionDialog.vue';
  import DeleteDialog from '@/components/dialogs/DeleteDialog.vue';
  import EditPositionDialog from '@/components/dialogs/EditPositionDialog.vue';
  import CellRenderer from '@/components/table/CellRenderer.vue';
  import DataTable from '@/components/table/DataTable.vue';
  import TableFooter from '@/components/table/TableFooter.vue';
  import TableSearch from '@/components/TableSearch.vue';
  import { useDataTable, type FetchParams } from '@/composables/useDataTable';
  import { useDebouncedRef } from '@/composables/useDebouncedRef';
  import { createColumnHelper, type ColumnDef, type ColumnHelper } from '@tanstack/vue-table';
  import { AlertTriangle, CirclePlus, Pencil, Trash2 } from 'lucide-vue-next';
  import { onBeforeUnmount, ref, watch } from 'vue';

  const errorMessage = ref<string | null>(null);
  const actionButtonClass =
    'rounded-md bg-indigo-600 p-1.5 text-white transition hover:bg-indigo-500';

  /* -------------------------------- Columns ------------------------------- */
  const col: ColumnHelper<PositionResponse> = createColumnHelper<PositionResponse>();
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const columns: ColumnDef<PositionResponse, any>[] = [
    col.accessor('name', {
      header: 'Position Name',
      meta: { kind: 'text' as const },
    }),
    col.accessor('code', {
      header: 'Code',
      meta: { kind: 'text' as const },
    }),
    col.accessor('requiresLicense', {
      header: 'Requires License?',
      meta: { kind: 'boolean' as const },
    }),
    {
      id: 'actions',
      header: 'Actions',
      meta: { kind: 'actions' as const },
      enableSorting: false,
    },
  ];

  /* ------------------------------- Searching ------------------------------ */
  const {
    input: positionNameFilter, // bound to v-model
    debounced: position, // used in fetch
    setNow: commitPositionNameNow, // call on Enter
    cancel: cancelNameDebounce, // cleanup on unmount
  } = useDebouncedRef('', 500);
  onBeforeUnmount(cancelNameDebounce);

  /* ------------------------------ Fetching ------------------------------- */
  type PosQuery = { position?: string };

  const fetchPositions = async ({ page, pageSize, query }: FetchParams<PosQuery>) => {
    const params: GetPositionsRequest = {
      page,
      pageSize,
      nameFilter: query?.position || undefined,
    };
    const response: GetPositionsResponse = await positionService.get(params);

    return {
      items: response.positions,
      totalCount: response.totalCount,
      totalPages: response.totalPages,
      page: response.page,
      pageSize: response.pageSize,
    };
  };

  const {
    table,
    loading,
    totalCount,
    totalPages,
    pagination,
    setQuery,
    setPageSize,
    fetchNow: refetch,
  } = useDataTable<PositionResponse, PosQuery>(columns, fetchPositions, { position: undefined });

  watch(position, () => setQuery({ position: position.value || undefined }));

  /* ------------------------------ Handlers ------------------------------- */
  const selectedPosition = ref<PositionResponse | null>(null);
  const addDialogIsOpen = ref(false);
  const deleteDialogIsOpen = ref(false);
  const editPositionDialogIsOpen = ref(false);

  const handleDeletePosition = async (): Promise<void> => {
    const id = selectedPosition.value?.id;
    if (!id) return;

    try {
      await positionService.deletePosition(id);

      await refetch();
    } catch (err: unknown) {
      const msg = extractApiError(err, 'position');
      errorMessage.value = msg;
    } finally {
      deleteDialogIsOpen.value = false;
      selectedPosition.value = null;
    }
  };

  const handleOpenDeleteDialog = (position: PositionResponse): void => {
    selectedPosition.value = position;
    deleteDialogIsOpen.value = true;
  };

  const handleEditPosition = (position: PositionResponse): void => {
    selectedPosition.value = position;
    editPositionDialogIsOpen.value = true;
  };
</script>
