<template>
  <div class="flex items-center justify-between pb-4">
    <h2 class="text-xl font-semibold text-slate-100">Positions</h2>
    <button
      class="flex items-center gap-2 rounded-md bg-indigo-600 px-3 py-2 text-sm font-medium text-white transition hover:bg-indigo-500"
      @click="addDialogIsOpen = true"
    >
      <span class="text-white">+ Add Position</span>
    </button>
  </div>

  <DataTable
    :table="table as unknown as import('@tanstack/vue-table').Table<unknown>"
    :loading
    :total-count
    empty-text="No users found."
  >
    <!-- actions slot for this table only -->
    <template #cell="{ cell }">
      <template v-if="(cell.column.columnDef.meta as any)?.kind === 'actions'">
        <button
          class="rounded-md bg-indigo-600 p-1.5 text-white transition hover:bg-indigo-500"
          aria-label="Edit user"
          @click="handleEditPosition(cell.row.original as PositionResponse)"
        >
          <Pencil class="h-4 w-4" />
        </button>
      </template>
      <CellRenderer :cell="cell" />
    </template>
  </DataTable>

  <TableFooter :table :total-count :total-pages :pagination :set-page-size />

  <AddPositionDialog
    :open="addDialogIsOpen"
    @close="addDialogIsOpen = false"
    @saved="
      addDialogIsOpen = false;
      refetch();
    "
  />

  <!--
  <EditPositionDialog
    :open="editUserDialogIsOpen"
    :selected-user
    @close="editUserDialogIsOpen = false"
    @saved="refetch"
  />
  -->
</template>

<script setup lang="ts">
  import type {
    GetPositionsRequest,
    GetPositionsResponse,
    PositionResponse,
  } from '@/api/positions/contracts';
  import { positionService } from '@/api/positions/services';
  import AddPositionDialog from '@/components/dialogs/AddPositionDialog.vue';
  import CellRenderer from '@/components/table/CellRenderer.vue';
  import DataTable from '@/components/table/DataTable.vue';
  import TableFooter from '@/components/table/TableFooter.vue';
  import { useDataTable } from '@/composables/useDataTable';
  import { createColumnHelper, type ColumnDef, type ColumnHelper } from '@tanstack/vue-table';
  import { Pencil } from 'lucide-vue-next';
  import { ref } from 'vue';

  const addDialogIsOpen = ref(false);

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

  /* ------------------------------ Fetching ------------------------------- */
  const fetchUsers = async ({ page, pageSize }: { page: number; pageSize: number }) => {
    const params: GetPositionsRequest = { page, pageSize };
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
    setPageSize,
    fetchNow: refetch,
  } = useDataTable<PositionResponse>(columns, fetchUsers, { email: undefined });

  /* ------------------------------ Handlers ------------------------------- */

  const selectedUser = ref<PositionResponse | null>(null);
  const editUserDialogIsOpen = ref(false);

  const handleEditPosition = (user: PositionResponse): void => {
    selectedUser.value = user;
    editUserDialogIsOpen.value = true;
  };
</script>
