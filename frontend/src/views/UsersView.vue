<template>
  <h2 class="pb-4 text-xl font-semibold text-slate-100">Users</h2>

  <div class="flex gap-4 pb-4">
    <table-search v-model="emailFilter" placeholder="Search email…" @commit="commit" />
    <boolean-filter v-model="deletedFilter" :options="deletedOptions" />
  </div>

  <p
    v-if="errorMessage"
    ref="errorEl"
    class="mb-4 flex items-center gap-2 rounded-lg border border-rose-800 bg-rose-900/30 px-3.5 py-2 text-sm leading-tight text-rose-200"
    role="alert"
    aria-live="assertive"
    tabindex="-1"
  >
    <alert-triangle class="block h-4 w-4 shrink-0 self-center" aria-hidden="true" />
    <span>{{ errorMessage }}</span>
  </p>

  <data-table
    :table="table as unknown as import('@tanstack/vue-table').Table<unknown>"
    :loading
    :total-count
    empty-text="No users found."
  >
    <!-- actions slot for this table only -->
    <template #cell="{ cell }">
      <template v-if="(cell.column.columnDef.meta as any)?.kind === 'actions'">
        <template class="flex gap-2">
          <button
            v-if="!cell.row.original.deletedAtUtc"
            class="rounded-md bg-indigo-600 p-1.5 text-white transition hover:bg-indigo-500"
            aria-label="Edit user"
            @click="handleEditUser(cell.row.original as UserResponse)"
          >
            <Pencil class="h-4 w-4" />
          </button>

          <!-- Delete button -->
          <button
            v-if="!cell.row.original.deletedAtUtc && canDelete(cell.row.original)"
            class="rounded-md bg-indigo-600 p-1.5 transition hover:bg-rose-200"
            aria-label="delete user"
            @click="handleOpenDeleteDialog(cell.row.original as UserResponse)"
          >
            <Lock class="h-4 w-4 text-rose-500 hover:text-rose-400" />
          </button>

          <!-- Reactivate button -->
          <button
            v-else-if="cell.row.original.deletedAtUtc"
            class="rounded-md bg-indigo-600 p-1.5 text-emerald-200 transition hover:bg-green-200"
            aria-label="reactivate position"
            @click="handleOpenRestoreDialog(cell.row.original as UserResponse)"
          >
            <lock-open class="h-4 w-4 hover:text-green-400" />
          </button>
        </template>
      </template>
      <cell-renderer :cell />
    </template>
  </data-table>

  <table-footer :table :total-count :total-pages :pagination :set-page-size />

  <!-- Dialogs -->
  <delete-dialog
    :open="deleteDialogIsOpen"
    title="Delete user"
    message="This action cannot be undone. This will permanently delete the selected user."
    @confirm="handleDelete"
    @close="deleteDialogIsOpen = false"
  />

  <edit-user-dialog
    :open="editUserDialogIsOpen"
    :selected-user
    @close="editUserDialogIsOpen = false"
    @saved="refetch"
  />

  <restore-dialog
    :open="restoreDialogIsOpen"
    title="Restore position"
    message="This action will restore position."
    @confirm="handleRestore"
    @close="deleteDialogIsOpen = false"
  />
</template>

<script setup lang="ts">
  import { extractApiError } from '@/api/error';
  import { userService } from '@/api/users';
  import BooleanFilter from '@/components/BooleanFilter.vue';
  import { DeleteDialog, RestoreDialog } from '@/components/dialogs/shared';
  import { EditUserDialog } from '@/components/dialogs/users';
  import { CellRenderer, DataTable, TableFooter, TableSearch } from '@/components/table';
  import { useAuth } from '@/composables/useAuth';
  import { useDataTable, type FetchParams } from '@/composables/useDataTable';
  import { createColumnHelper, type ColumnDef, type ColumnHelper } from '@tanstack/vue-table';
  import { AlertTriangle, CheckCircle2, Lock, LockOpen, Pencil, Trash2 } from 'lucide-vue-next';
  import { computed, onBeforeUnmount, ref, watch } from 'vue';
  import type { GetUsersRequest, GetUsersResponse, UserResponse, UserStatus } from '../api/users';
  import { useDebouncedRef } from '../composables/useDebouncedRef';

  const errorMessage = ref<string | null>(null);

  /* ------------------------------ Auth ------------------------------- */
  const { currentUserId, canDeleteUser } = useAuth();

  // helper so we can reuse it in template/handlers
  const canDelete = (u: UserResponse) =>
    currentUserId.value == null || u.id !== currentUserId.value;

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

  /* -------------------------------- Columns ------------------------------- */
  const col: ColumnHelper<UserResponse> = createColumnHelper<UserResponse>();
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const columns: ColumnDef<UserResponse, any>[] = [
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

  /* ---------------------------- Filtering ---------------------------- */
  const deletedFilter = ref(false); // default: show only active (not deleted)

  const deletedOptions = [
    { value: false, label: 'Active', icon: CheckCircle2, color: 'text-emerald-400' },
    { value: true, label: 'Inactive', icon: Trash2, color: 'text-rose-400' },
  ];

  const {
    input: emailFilter, // bind to v-model
    debounced: email, // use in fetch
    setNow: commit, // optional: commit immediately on Enter
    cancel: cancelEmailDebounce, // optional
  } = useDebouncedRef('', 500);

  onBeforeUnmount(cancelEmailDebounce);

  watch([email, deletedFilter], ([e, del]) => {
    setQuery({
      email: e || null,
      isDeleted: del, // keep false as false, only null stays null
    });
  });
  /* ------------------------------ Fetching ------------------------------- */
  type UserQuery = { email: string | null; isDeleted: boolean };

  const fetchUsers = async ({ page, pageSize, query }: FetchParams<UserQuery>) => {
    const params: GetUsersRequest = {
      page,
      pageSize,
      email: query?.email || null,
      isDeleted: query?.isDeleted ?? false,
    };
    const response: GetUsersResponse = await userService.get(params);

    return {
      items: response.users,
      totalCount: response.totalCount,
      totalPages: response.totalPages,
      page: response.page,
      pageSize: response.pageSize,
    };
  };
  const query = computed(() => ({
    email: email.value ?? null,
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
  } = useDataTable<UserResponse, UserQuery>(columns, fetchUsers, query);

  /* ------------------------------ Handlers ------------------------------- */
  const deleteDialogIsOpen = ref(false);
  const editUserDialogIsOpen = ref(false);
  const selectedUser = ref<UserResponse | null>(null);
  const restoreDialogIsOpen = ref(false);

  const handleDelete = async (): Promise<void> => {
    const id = selectedUser.value?.id;
    if (!id || id === currentUserId.value) return; // double guard

    try {
      await userService.deleteUser(id);
      await refetch();
    } catch (err: unknown) {
      const msg = extractApiError(err, 'user');
      errorMessage.value = msg;
    } finally {
      deleteDialogIsOpen.value = false;
      selectedUser.value = null;
    }
  };

  const handleRestore = async (): Promise<void> => {
    const id = selectedUser.value?.id;
    if (!id) return;

    try {
      await userService.restore(id);
      await refetch();
    } catch (err: unknown) {
      const msg = extractApiError(err, 'user');
      errorMessage.value = msg;
    } finally {
      restoreDialogIsOpen.value = false;
      selectedUser.value = null;
    }
  };

  const handleOpenDeleteDialog = (user: UserResponse): void => {
    if (!canDeleteUser(user.id)) return; // no self-delete
    selectedUser.value = user;
    deleteDialogIsOpen.value = true;
  };

  const handleEditUser = (user: UserResponse): void => {
    selectedUser.value = user;
    editUserDialogIsOpen.value = true;
  };

  const handleOpenRestoreDialog = (user: UserResponse): void => {
    restoreDialogIsOpen.value = true;
    selectedUser.value = user;
  };
</script>
