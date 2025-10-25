<template>
  <div class="pb-4">
    <input
      v-model="emailSearchTerm"
      placeholder="Search email…"
      class="h-9 min-w-[260px] rounded-md border border-slate-700 bg-slate-900/70 px-3 text-sm focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/30 sm:w-1/2 lg:w-1/3"
    />
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
        <template v-if="loading">
          <tr v-for="i in 10" :key="i">
            <td class="px-4 py-3"><div :class="loadingTd" /></td>
            <td class="px-4 py-3"><div :class="loadingTd" /></td>
            <td class="px-4 py-3"><div :class="loadingTd" /></td>
            <td class="px-4 py-3"><div :class="loadingTd" /></td>
            <td class="px-4 py-3"><div :class="loadingTd" /></td>
            <td class="px-4 py-3"><div :class="loadingTd" /></td>
          </tr>
        </template>

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
                v-if="cell.getValue() as boolean"
                class="rounded-full bg-emerald-700 px-2 py-0.5 text-sm text-emerald-200"
              >
                Active
              </span>
              <span v-else class="rounded-full bg-slate-800/60 px-2 py-0.5 text-sm text-slate-400">
                Inactive
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
                @click="onEdit(row.original as UserResponse)"
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

  <!-- Pagination / info -->
  <div class="mt-4 flex flex-wrap items-center justify-between gap-3 text-sm text-slate-300">
    <div class="flex items-center gap-3">
      <span>Total: {{ totalCount }}</span>
      <span>Page {{ pagination.pageIndex + 1 }} / {{ totalPages }}</span>
    </div>

    <div class="flex items-center gap-2">
      <label>Rows:</label>
      <select
        class="rounded-md border border-slate-700 bg-slate-900 px-2 py-1"
        :value="pagination.pageSize"
        @change="setPageSize(($event.target as HTMLSelectElement).value)"
      >
        <option v-for="s in [10, 25, 50]" :key="s" :value="s">{{ s }}</option>
      </select>

      <div class="flex items-center gap-2">
        <button
          class="flex items-center justify-center rounded-md border border-slate-700 p-2 disabled:opacity-50"
          :disabled="!table.getCanPreviousPage()"
          aria-label="Previous page"
          @click="table.previousPage()"
        >
          <ChevronLeft class="h-4 w-4" />
        </button>
        <button
          class="flex items-center justify-center rounded-md border border-slate-700 p-2 disabled:opacity-50"
          :disabled="!table.getCanNextPage()"
          aria-label="Next page"
          @click="table.nextPage()"
        >
          <ChevronRight class="h-4 w-4" />
        </button>
      </div>
    </div>
  </div>

  <EditUserDialog
    :open="editUserDialogIsOpen"
    :user="userBeingEdited"
    @close="editUserDialogIsOpen = false"
    @saved="fetchUsers"
  />
</template>

<script setup lang="ts">
  import type { GetUsersRequest, GetUsersResponse, UserResponse } from '@/api/users';
  import { userService } from '@/api/users';

  import EditUserDialog from '@/components/EditUserDialog.vue';
  import {
    createColumnHelper,
    getCoreRowModel,
    useVueTable,
    type ColumnDef,
    type ColumnHelper,
  } from '@tanstack/vue-table';
  import { ChevronLeft, ChevronRight, Pencil } from 'lucide-vue-next';
  import { reactive, ref, watch, watchEffect } from 'vue';

  type ColMeta = { kind: 'text' } | { kind: 'status' } | { kind: 'datetime' } | { kind: 'actions' };

  const editUserDialogIsOpen = ref(false);
  const emailSearchTerm = ref('');
  const loading = ref(false);
  let reqSeq = 0;
  let searchTimeout: ReturnType<typeof setTimeout> | null = null;
  const totalCount = ref(0);
  const totalPages = ref(0);
  const users = ref<UserResponse[]>([]);
  const loadingTd = 'h-3 w-40 animate-pulse rounded bg-slate-700/50';
  const userBeingEdited = ref<UserResponse | null>(null);

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
    col.accessor('isActive', {
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

  // TanStack state (server mode)
  const pagination = reactive({ pageIndex: 0, pageSize: 25 });

  const table = useVueTable<UserResponse>({
    get data() {
      return users.value;
    },
    columns,
    get pageCount() {
      return totalPages.value;
    },
    state: { pagination },
    manualPagination: true,
    onPaginationChange: updater => {
      if (typeof updater === 'function') Object.assign(pagination, updater(pagination));
      else Object.assign(pagination, updater);
    },
    getCoreRowModel: getCoreRowModel(),
    initialState: { pagination },
    getRowId: row => String(row.id),
  });

  // --------------------- FUNCTIONS --------------------------------
  async function fetchUsers(): Promise<void> {
    loading.value = true;
    const seq = ++reqSeq; // capture this call's sequence

    try {
      const page = pagination.pageIndex + 1; // backend is 1-based
      const pageSize = pagination.pageSize;

      const params: GetUsersRequest = { page, pageSize };
      const response: GetUsersResponse = await userService.get(params);

      // Ignore stale responses
      if (seq !== reqSeq) return;

      // TODO: Maybe map this to a user model
      // assign from response
      users.value = response.users;
      totalCount.value = response.totalCount;
      totalPages.value = response.totalPages;

      // sync pagination if server adjusted it (cap/normalize)
      pagination.pageIndex = Math.max(0, (response.page ?? page) - 1);
      pagination.pageSize = response.pageSize ?? pageSize;
    } catch (err) {
      console.error('Failed to fetch users', err);
      // optional safety resets:
      users.value = [];
      totalCount.value = 0;
      totalPages.value = 0;
    } finally {
      loading.value = false;
    }
  }

  watchEffect(fetchUsers);

  watch(emailSearchTerm, val => {
    if (searchTimeout) clearTimeout(searchTimeout);

    searchTimeout = setTimeout(() => {
      search(val.trim());
    }, 500);
  });

  const search = async (email: string): Promise<void> => {
    loading.value = true;
    const seq = ++reqSeq;

    try {
      const page = 1;
      const pageSize = pagination.pageSize;
      const request: GetUsersRequest = { page, pageSize, email };
      const response: GetUsersResponse = await userService.get(request);

      if (seq !== reqSeq) return; // ignore stale results

      users.value = response.users;
      totalCount.value = response.totalCount;
      totalPages.value = response.totalPages;
    } catch (err) {
      console.error('Search failed', err);
      users.value = [];
    } finally {
      loading.value = false;
    }
  };

  const onEdit = (user: UserResponse): void => {
    userBeingEdited.value = user;
    editUserDialogIsOpen.value = true;
  };

  const setPageSize = (val: string): void => {
    pagination.pageSize = Number(val);
    pagination.pageIndex = 0;
  };
</script>
