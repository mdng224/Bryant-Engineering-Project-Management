<template>
  <div class="pb-4">
    <TableSearch v-model="emailSearch" placeholder="Search email…" @commit="commitEmailNow" />
  </div>

  <div class="overflow-x-auto rounded-xl border border-slate-700 bg-slate-900/70 shadow">
    <table
      class="sticky top-0 z-10 min-w-full divide-y divide-slate-700 text-sm text-slate-200 backdrop-blur"
    >
      <thead class="bg-slate-800/80 text-slate-100">
        <tr v-for="hg in table.getHeaderGroups()" :key="hg.id">
          <th
            v-for="header in hg.headers"
            :key="header.id"
            class="px-4 py-3 text-left font-semibold uppercase tracking-wider"
          >
            {{ header.column.columnDef.header as string }}
          </th>
        </tr>
      </thead>

      <tbody class="divide-y divide-slate-800">
        <SkeletonRows v-if="loading" :rows="10" :cols="table.getAllLeafColumns().length" />

        <tr
          v-for="row in table.getRowModel().rows"
          v-else
          :key="row.id"
          class="odd:bg-slate-900/40 even:bg-slate-700/30 hover:bg-slate-800/40 ..."
        >
          <td
            v-for="cell in row.getVisibleCells()"
            :key="cell.id"
            class="whitespace-nowrap px-4 py-2.5"
          >
            <template v-if="(cell.column.columnDef.meta as ColMeta)?.kind === 'text'">
              <span class="text-md text-slate-100">{{ cell.getValue() as string }}</span>
            </template>

            <template v-else-if="(cell.column.columnDef.meta as ColMeta)?.kind === 'status'">
              <span
                :class="[
                  'rounded-full px-2 py-0.5 text-sm font-medium',
                  getStatusClass(cell.getValue() as string),
                ]"
              >
                {{ cell.getValue() as string }}
              </span>
            </template>

            <template v-else-if="(cell.column.columnDef.meta as ColMeta)?.kind === 'datetime'">
              <span class="text-[12px] tracking-wide text-slate-100">
                {{ formatUtc(cell.getValue() as string | null) }}
              </span>
            </template>

            <template v-else-if="(cell.column.columnDef.meta as ColMeta)?.kind === 'actions'">
              <button
                class="rounded-md bg-indigo-600 p-1.5 text-white transition hover:bg-indigo-500"
                aria-label="Edit user"
                @click="handleEditUser(row.original as UserResponse)"
              >
                <Pencil class="h-4 w-4" />
              </button>
            </template>

            <template v-else>{{ cell.getValue() as any }}</template>
          </td>
        </tr>

        <tr v-if="!loading && users.length === 0">
          <td class="px-6 py-6 text-slate-400" :colspan="table.getAllLeafColumns().length">
            No results.
          </td>
        </tr>
        <tr v-if="loading">
          <td class="px-6 py-6 text-slate-400" :colspan="table.getAllLeafColumns().length">
            Loading…
          </td>
        </tr>
      </tbody>
    </table>
  </div>

  <TableFooter :table :totalCount :totalPages :pagination :setPageSize />

  <EditUserDialog
    :open="editUserDialogIsOpen"
    :user="userBeingEdited"
    @close="editUserDialogIsOpen = false"
    @saved="refetch"
  />
</template>

<script setup lang="ts">
  import { userService } from '@/api/users';
  import SkeletonRows from '@/components/SkeletonRows.vue';
  import type { GetUsersRequest, GetUsersResponse, UserResponse } from '../api/users';

  import EditUserDialog from '@/components/EditUserDialog.vue';
  import TableFooter from '@/components/TableFooter.vue';
  import TableSearch from '@/components/TableSearch.vue';
  import { useDataTable } from '@/composables/useDataTable';
  import { createColumnHelper, type ColumnDef, type ColumnHelper } from '@tanstack/vue-table';
  import { Pencil } from 'lucide-vue-next';
  import { onBeforeUnmount, ref, watch } from 'vue';
  import { useDebouncedRef } from '../composables/useDebouncedRef';

  type ColMeta = { kind: 'text' } | { kind: 'status' } | { kind: 'datetime' } | { kind: 'actions' };

  const editUserDialogIsOpen = ref(false);
  const userBeingEdited = ref<UserResponse | null>(null);

  /* ------------------------------ Status ------------------------------- */
  type Status = 'PendingEmail' | 'PendingApproval' | 'Active' | 'Denied' | 'Disabled';

  const statusClasses: Record<Status, string> = {
    Active: 'bg-emerald-700 text-emerald-100',
    PendingEmail: 'bg-amber-600/90 text-amber-100',
    PendingApproval: 'bg-blue-600/80 text-blue-100',
    Denied: 'bg-rose-700 text-rose-100',
    Disabled: 'bg-slate-700 text-slate-300',
  } satisfies Record<Status, string>;

  const getStatusClass = (status: string): string =>
    statusClasses[status as Status] ?? 'bg-slate-800/60 text-slate-400';

  // formatting helpers
  const fmt = new Intl.DateTimeFormat(undefined, {
    year: 'numeric',
    month: 'short',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  });
  const formatUtc = (iso: string | null) => (iso ? fmt.format(new Date(iso)) : '—');

  const col: ColumnHelper<UserResponse> = createColumnHelper<UserResponse>();
  const columns: ColumnDef<UserResponse>[] = [
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
      meta: { kind: 'status' as const },
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
    rows: users,
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
  const handleEditUser = (user: UserResponse): void => {
    userBeingEdited.value = user;
    editUserDialogIsOpen.value = true;
  };
</script>
