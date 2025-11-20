<template>
  <h2 class="pb-4 text-xl font-semibold text-slate-100">Positions</h2>

  <div class="flex items-center justify-between pb-4">
    <div class="flex gap-4">
      <table-search v-model="nameFilter" placeholder="Search position name..." @commit="commit" />
      <boolean-filter v-model="deletedFilter" :options="deletedOptions" />
    </div>

    <button
      class="flex items-center gap-2 rounded-md bg-indigo-600 px-3 py-2 text-sm font-medium text-white transition hover:bg-indigo-500"
      @click="addDialogIsOpen = true"
    >
      <circle-plus class="block h-4 w-4 shrink-0 self-center" aria-hidden="true" />
      <span class="text-white">Add Position</span>
    </button>
  </div>

  <app-alert v-if="errorMessage" :message="errorMessage" variant="error" :icon="AlertTriangle" />

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
            @click="handleEdit(cell.row.original as PositionRowResponse)"
          >
            <pencil class="h-4 w-4" />
          </button>

          <!-- Delete button -->
          <button
            v-if="!cell.row.original.deletedAtUtc"
            class="rounded-md bg-indigo-600 p-1.5 transition hover:bg-rose-200"
            aria-label="delete position"
            @click="handleOpenDeleteDialog(cell.row.original as PositionRowResponse)"
          >
            {{ cell.row.original.deletedAtUtc }}
            <Lock class="h-4 w-4 text-rose-500 hover:text-rose-400" />
          </button>

          <!-- Restore button -->
          <button
            v-else
            class="rounded-md bg-indigo-600 p-1.5 text-emerald-200 transition hover:bg-green-200"
            aria-label="reactivate position"
            @click="handleOpenRestoreDialog(cell.row.original as PositionRowResponse)"
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
    ListPositionsRequest,
    ListPositionsResponse,
    PositionRowResponse,
  } from '@/api/positions/contracts';
  import { positionService } from '@/api/positions/services';
  import BooleanFilter from '@/components/BooleanFilter.vue';
  import { AddPositionDialog, EditPositionDialog } from '@/components/dialogs/positions';
  import DeleteDialog from '@/components/dialogs/shared/DeleteDialog.vue';
  import RestoreDialog from '@/components/dialogs/shared/RestoreDialog.vue';
  import { CellRenderer, DataTable, TableFooter, TableSearch } from '@/components/table';
  import AppAlert from '@/components/ui/AppAlert.vue';
  import { useDataTable, type FetchParams } from '@/composables/useDataTable';
  import { useDebouncedRef } from '@/composables/useDebouncedRef';
  import { createColumnHelper, type ColumnDef, type ColumnHelper } from '@tanstack/vue-table';
  import {
    AlertTriangle,
    CheckCircle2,
    CirclePlus,
    Lock,
    LockOpen,
    Pencil,
    Trash2,
  } from 'lucide-vue-next';
  import { computed, onBeforeUnmount, ref } from 'vue';

  const errorMessage = ref<string | null>(null);
  const actionButtonClass = 'rounded-md bg-indigo-600 p-1.5  transition hover:bg-indigo-500';

  /* -------------------------------- Columns ------------------------------- */
  const col: ColumnHelper<PositionRowResponse> = createColumnHelper<PositionRowResponse>();
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const columns: ColumnDef<PositionRowResponse, any>[] = [
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
  const deletedFilter = ref<boolean | null>(false); // default Active (not deleted)

  const deletedOptions = [
    { value: false, label: 'Active', icon: CheckCircle2, color: 'text-emerald-400' },
    { value: true, label: 'Inactive', icon: Trash2, color: 'text-rose-400' },
  ];

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
  type PosQuery = { position: string | null; deletedFilter: boolean | null };

  const fetchPositions = async ({ page, pageSize, query }: FetchParams<PosQuery>) => {
    const request: ListPositionsRequest = {
      page,
      pageSize,
      nameFilter: query?.position || null,
      isDeleted: query?.deletedFilter ?? false,
    };
    console.log(request);
    const response: ListPositionsResponse = await positionService.get(request);
    console.log(response);
    return {
      items: response.positions,
      totalCount: response.totalCount,
      totalPages: response.totalPages,
      page: response.page,
      pageSize: response.pageSize,
    };
  };

  const query = computed<PosQuery>(() => ({
    position: position.value ?? null,
    deletedFilter: deletedFilter.value ?? null,
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
  } = useDataTable<PositionRowResponse, typeof query.value>(columns, fetchPositions, query);

  /* ------------------------------ Handlers ------------------------------- */
  const selectedPosition = ref<PositionRowResponse | null>(null);
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

  const handleOpenDeleteDialog = (position: PositionRowResponse): void => {
    selectedPosition.value = position;
    deleteDialogIsOpen.value = true;
  };

  const handleEdit = (position: PositionRowResponse): void => {
    selectedPosition.value = position;
    editPositionDialogIsOpen.value = true;
  };

  const handleOpenRestoreDialog = (position: PositionRowResponse): void => {
    restoreDialogIsOpen.value = true;
    selectedPosition.value = position;
  };
</script>
