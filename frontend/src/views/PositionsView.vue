<template>
  <h2 class="pb-4 text-xl font-semibold text-slate-100">Positions</h2>

  <div class="flex items-center justify-between pb-4">
    <div class="flex gap-4">
      <table-search v-model="nameFilter" placeholder="Search position name..." @commit="commit" />
      <deleted-filter v-model="deletedFilter" label-1="Active" label-2="Deleted" />
    </div>

    <button
      class="flex items-center gap-2 rounded-md bg-indigo-600 px-3 py-2 text-sm font-medium text-white transition hover:bg-indigo-500"
      @click="addDialogIsOpen = true"
    >
      <circle-plus class="block h-4 w-4 shrink-0 self-center" aria-hidden="true" />
      <span class="text-white">Add Position</span>
    </button>
  </div>

  <span
    v-if="errorMessage"
    ref="errorEl"
    class="mb-4 flex items-center gap-2 rounded-lg border border-rose-800 bg-rose-900/30 px-3.5 py-2 text-sm leading-tight text-rose-200"
    role="alert"
    aria-live="assertive"
    tabindex="-1"
  >
    <alter-triangle class="block h-4 w-4 shrink-0 self-center" aria-hidden="true" />
    <span>{{ errorMessage }}</span>
  </span>

  <data-table
    :table="table as unknown as import('@tanstack/vue-table').Table<unknown>"
    :loading
    :total-count
    empty-text="No positions found."
  >
    <!-- actions slot for this table only -->
    <template #cell="{ cell }">
      <template v-if="(cell.column.columnDef.meta as any)?.kind === 'actions'">
        <!-- Edit button -->
        <template class="flex gap-2">
          <button
            v-if="!cell.row.original.deletedAtUtc"
            :class="actionButtonClass"
            aria-label="Edit position"
            @click="handleEdit(cell.row.original as PositionResponse)"
          >
            <pencil class="h-4 w-4" />
          </button>

          <!-- Delete button -->
          <button
            v-if="!cell.row.original.deletedAtUtc"
            class="rounded-md bg-indigo-600 p-1.5 transition hover:bg-rose-200"
            aria-label="delete position"
            @click="handleOpenDeleteDialog(cell.row.original as PositionResponse)"
          >
            {{ cell.row.original.deletedAtUtc }}
            <Lock class="h-4 w-4 text-rose-500 hover:text-rose-400" />
          </button>

          <!-- Reactivate button -->
          <button
            v-else
            class="rounded-md bg-indigo-600 p-1.5 text-emerald-200 transition hover:bg-green-200"
            aria-label="reactivate position"
            @click="handleOpenRestoreDialog(cell.row.original as PositionResponse)"
          >
            <lock-open class="h-4 w-4 hover:text-green-400" />
          </button>
        </template>
      </template>
      <cell-renderer :cell="cell" />
    </template>
  </data-table>

  <table-footer :table :total-count :total-pages :pagination :set-page-size />

  <!-- Dialogs -->
  <add-position-dialog :open="addDialogIsOpen" @close="addDialogIsOpen = false" @saved="refetch" />

  <delete-dialog
    :open="deleteDialogIsOpen"
    title="Delete Position"
    message="This action will soft delete a position."
    @confirm="handleDelete"
    @close="deleteDialogIsOpen = false"
  />

  <edit-position-dialog
    :open="editPositionDialogIsOpen"
    :selected-position
    @close="editPositionDialogIsOpen = false"
    @save="refetch"
  />

  <restore-dialog
    :open="restoreDialogIsOpen"
    title="Restore position"
    message="This action will restore position."
    @confirm="handleRestore"
    @close="restoreDialogIsOpen = false"
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
  import DeletedFilter from '@/components/DeletedFilter.vue';
  import AddPositionDialog from '@/components/dialogs/AddPositionDialog.vue';
  import DeleteDialog from '@/components/dialogs/DeleteDialog.vue';
  import EditPositionDialog from '@/components/dialogs/EditPositionDialog.vue';
  import RestoreDialog from '@/components/dialogs/RestoreDialog.vue';
  import CellRenderer from '@/components/table/CellRenderer.vue';
  import DataTable from '@/components/table/DataTable.vue';
  import TableFooter from '@/components/table/TableFooter.vue';
  import TableSearch from '@/components/TableSearch.vue';
  import { useDataTable, type FetchParams } from '@/composables/useDataTable';
  import { useDebouncedRef } from '@/composables/useDebouncedRef';
  import { createColumnHelper, type ColumnDef, type ColumnHelper } from '@tanstack/vue-table';
  import { CirclePlus, Lock, LockOpen, Pencil } from 'lucide-vue-next';
  import { computed, onBeforeUnmount, ref } from 'vue';

  const errorMessage = ref<string | null>(null);
  const actionButtonClass = 'rounded-md bg-indigo-600 p-1.5  transition hover:bg-indigo-500';

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

  /* ------------------------------- Filtering ------------------------------ */
  const deletedFilter = ref(false); // default: show only active (not deleted)

  const {
    input: nameFilter, // bound to v-model
    debounced: position, // used in fetch
    setNow: commit, // call on Enter
    cancel: cancelNameDebounce, // cleanup on unmount
  } = useDebouncedRef('', 500);

  onBeforeUnmount(() => {
    cancelNameDebounce();
    destroy();
  });

  /* ------------------------------ Fetching ------------------------------- */
  type PosQuery = { position: string | null; isDeleted: boolean };

  const fetchPositions = async ({ page, pageSize, query }: FetchParams<PosQuery>) => {
    const params: GetPositionsRequest = {
      page,
      pageSize,
      nameFilter: query?.position || null,
      isDeleted: query?.isDeleted ?? false,
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

  const query = computed(() => ({
    position: position.value ?? null,
    isDeleted: deletedFilter.value,
  }));

  const {
    table,
    loading,
    totalCount,
    totalPages,
    pagination,
    setQuery,
    setPageSize,
    fetchNow: refetch,
    destroy,
  } = useDataTable<PositionResponse, typeof query.value>(columns, fetchPositions, query);

  /* ------------------------------ Handlers ------------------------------- */
  const selectedPosition = ref<PositionResponse | null>(null);
  const addDialogIsOpen = ref(false);
  const deleteDialogIsOpen = ref(false);
  const editPositionDialogIsOpen = ref(false);
  const restoreDialogIsOpen = ref(false);

  const handleDelete = async (): Promise<void> => {
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

  const handleRestore = async (): Promise<void> => {
    const id = selectedPosition.value?.id;
    if (!id) return;

    try {
      await positionService.restore(id);
      await refetch();
    } catch (err: unknown) {
      const msg = extractApiError(err, 'position');
      errorMessage.value = msg;
    } finally {
      restoreDialogIsOpen.value = false;
      selectedPosition.value = null;
    }
  };

  const handleOpenDeleteDialog = (position: PositionResponse): void => {
    selectedPosition.value = position;
    deleteDialogIsOpen.value = true;
  };

  const handleEdit = (position: PositionResponse): void => {
    selectedPosition.value = position;
    editPositionDialogIsOpen.value = true;
  };

  const handleOpenRestoreDialog = (position: PositionResponse): void => {
    restoreDialogIsOpen.value = true;
    selectedPosition.value = position;
  };
</script>
