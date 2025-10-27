<template>
  <div class="pb-4">
    <TableSearch v-model="emailSearch" placeholder="Search email…" @commit="commitEmailNow" />
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
          @click="handleEditUser(cell.row.original as UserResponse)"
        >
          <Pencil class="h-4 w-4" />
        </button>
      </template>
      <CellRenderer :cell="cell" />
    </template>
  </DataTable>

  <TableFooter :table :total-count :total-pages :pagination :set-page-size />

  <EditUserDialog
    :open="editUserDialogIsOpen"
    :selected-user
    @close="editUserDialogIsOpen = false"
    @saved="refetch"
  />
</template>

<script setup lang="ts">
  import { userService } from '@/api/users';
  import EditUserDialog from '@/components/dialogs/EditUserDialog.vue';
  import CellRenderer from '@/components/table/CellRenderer.vue';
  import DataTable from '@/components/table/DataTable.vue';
  import TableFooter from '@/components/table/TableFooter.vue';
  import TableSearch from '@/components/TableSearch.vue';
  import { useDataTable } from '@/composables/useDataTable';
  import { createColumnHelper, type ColumnDef, type ColumnHelper } from '@tanstack/vue-table';
  import { Pencil } from 'lucide-vue-next';
  import { onBeforeUnmount, ref, watch } from 'vue';
  import type { GetUsersRequest, GetUsersResponse, UserResponse, UserStatus } from '../api/users';
  import { useDebouncedRef } from '../composables/useDebouncedRef';

  const editUserDialogIsOpen = ref(false);

  /* ------------------------------ Status ------------------------------- */

  const statusClasses: Record<UserStatus, string> = {
    Active: 'bg-emerald-700 text-emerald-100',
    PendingEmail: 'bg-amber-600/90 text-amber-100',
    PendingApproval: 'bg-blue-600/80 text-blue-100',
    Denied: 'bg-rose-700 text-rose-100',
    Disabled: 'bg-slate-700 text-slate-300',
  } satisfies Record<UserStatus, string>;

  const getStatusClass = (status: string): string =>
    statusClasses[status as UserStatus] ?? 'bg-slate-800/60 text-slate-400';

  const col: ColumnHelper<UserResponse> = createColumnHelper<UserResponse>();
  const columns: ColumnDef<UserResponse, unknown>[] = [
    col.accessor('email', {
      header: 'Email',
      meta: { kind: 'text' as const },
    }),
    col.accessor('roleName', {
      header: 'Role Name',
      meta: { kind: 'text' as const },
    }),
    col.accessor('status', {
      header: 'Status',
      meta: {
        kind: 'badge' as const,
        classFor: (val: string) => getStatusClass(val),
      },
    }),
    col.accessor('createdAtUtc', {
      header: 'Created',
      meta: { kind: 'datetime' as const },
    }),
    col.accessor('updatedAtUtc', {
      header: 'Last Modified',
      meta: { kind: 'datetime' as const },
    }),
    col.accessor('deletedAtUtc', {
      header: 'Delete Date',
      meta: { kind: 'datetime' as const },
    }),
    {
      id: 'actions',
      header: 'Actions',
      meta: { kind: 'actions' as const },
      enableSorting: false,
    },
  ];

  /* ---------------------------- Search ---------------------------- */
  const {
    input: emailSearch, // bind to v-model
    debounced: email, // use in fetch
    setNow: commitEmailNow, // optional: commit immediately on Enter
    cancel: cancelEmailDebounce, // optional
  } = useDebouncedRef('', 500);

  onBeforeUnmount(cancelEmailDebounce);

  /* ------------------------------ Fetching ------------------------------- */
  type UserQuery = { email?: string };

  const fetchUsers = async ({
    page,
    pageSize,
    query,
  }: {
    page: number;
    pageSize: number;
    query?: UserQuery;
  }) => {
    const params: GetUsersRequest = { page, pageSize, email: query?.email || undefined };
    const response: GetUsersResponse = await userService.get(params);

    return {
      items: response.users,
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
  } = useDataTable<UserResponse, UserQuery>(columns, fetchUsers, { email: undefined });

  // keep page at 1 when search changes
  watch(email, () => setQuery({ email: email.value || undefined }));

  /* ------------------------------ Handlers ------------------------------- */
  const selectedUser = ref<UserResponse | null>(null);

  const handleEditUser = (user: UserResponse): void => {
    selectedUser.value = user;
    editUserDialogIsOpen.value = true;
  };
</script>
