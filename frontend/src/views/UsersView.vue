<template>
  <div class="p-6">
    <div class="overflow-x-auto rounded-xl border border-slate-700 bg-slate-900/70 shadow">
      <table class="min-w-full divide-y divide-slate-700 text-sm text-slate-200">
        <thead class="bg-slate-800/80 text-slate-300">
          <tr v-for="hg in table.getHeaderGroups()" :key="hg.id">
            <th
              v-for="header in hg.headers"
              :key="header.id"
              class="px-4 py-3 text-left font-semibold uppercase tracking-wider"
            >
              <FlexRender :render="header.column.columnDef.header" :props="header.getContext()" />
            </th>
          </tr>
        </thead>

        <tbody class="divide-y divide-slate-800">
          <tr v-for="row in table.getRowModel().rows" :key="row.id" class="hover:bg-slate-800/40">
            <td
              v-for="cell in row.getVisibleCells()"
              :key="cell.id"
              class="whitespace-nowrap px-4 py-3"
            >
              <FlexRender :render="cell.column.columnDef.cell" :props="cell.getContext()" />
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
        <label class="mr-1">Rows:</label>
        <select
          class="rounded-md border border-slate-700 bg-slate-900 px-2 py-1"
          :value="pagination.pageSize"
          @change="setPageSize(($event.target as HTMLSelectElement).value)"
        >
          <option v-for="s in [10, 25, 50]" :key="s" :value="s">{{ s }}</option>
        </select>

        <button
          class="rounded-md border border-slate-700 px-3 py-1 disabled:opacity-50"
          :disabled="!table.getCanPreviousPage()"
          @click="table.previousPage()"
        >
          Prev
        </button>
        <button
          class="rounded-md border border-slate-700 px-3 py-1 disabled:opacity-50"
          :disabled="!table.getCanNextPage()"
          @click="table.nextPage()"
        >
          Next
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  import api from '@/api'; // your axios instance
  import {
    FlexRender,
    createColumnHelper,
    getCoreRowModel,
    useVueTable,
    type ColumnDef,
  } from '@tanstack/vue-table';
  import { h, reactive, ref, watchEffect } from 'vue';

  type UserDto = {
    Id: string;
    Email: string;
    RoleName: string;
    IsActive: boolean;
    CreatedAtUtc: string;
    UpdatedAtUtc: string;
    DeletedAtUtc: string | null;
  };

  type GetUsersResult = {
    Users: UserDto[];
    TotalCount: number;
    Page: number;
    PageSize: number;
    TotalPages: number;
  };

  const users = ref<UserDto[]>([]);
  const totalCount = ref(0);
  const totalPages = ref(0);
  const loading = ref(false);

  // formatting helpers
  const fmt = new Intl.DateTimeFormat(undefined, {
    year: 'numeric',
    month: 'short',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  });
  const formatUtc = (iso: string | null) => (iso ? fmt.format(new Date(iso)) : '—');
  const shortId = (id: string | undefined) =>
    id && id.length > 8 ? id.slice(0, 8) + '…' : (id ?? '—');

  const col = createColumnHelper<UserDto>();
  const columns: ColumnDef<UserDto, unknown>[] = [
    col.accessor('Id', {
      header: 'Id',
      cell: info =>
        h('code', { class: 'font-mono text-xs text-slate-300' }, shortId(info.getValue())),
      enableSorting: false,
    }),
    col.accessor('Email', { header: 'Email', cell: i => i.getValue() }),
    col.accessor('RoleName', { header: 'Role', cell: i => i.getValue() }),
    col.accessor('IsActive', {
      header: 'Status',
      cell: i =>
        i.getValue()
          ? h(
              'span',
              { class: 'rounded-full bg-emerald-800/30 px-2 py-0.5 text-xs text-emerald-300' },
              'Active',
            )
          : h(
              'span',
              { class: 'rounded-full bg-slate-700 px-2 py-0.5 text-xs text-slate-300' },
              'Inactive',
            ),
    }),
    col.accessor('CreatedAtUtc', { header: 'Created', cell: i => formatUtc(i.getValue()) }),
    col.accessor('UpdatedAtUtc', { header: 'Updated', cell: i => formatUtc(i.getValue()) }),
    col.accessor('DeletedAtUtc', { header: 'Deleted', cell: i => formatUtc(i.getValue()) }),
    {
      id: 'actions',
      header: 'Actions',
      cell: ({ row }) =>
        h(
          'button',
          {
            class:
              'rounded-md bg-indigo-600 px-3 py-1 text-xs font-semibold text-white hover:bg-indigo-500',
            onClick: () => onEdit(row.original),
          },
          'Edit',
        ),
      enableSorting: false,
    },
  ];

  // TanStack state (server mode)
  const pagination = reactive({ pageIndex: 0, pageSize: 25 });

  const table = useVueTable<UserDto>({
    get data() {
      return users.value;
    },
    columns,
    get pageCount() {
      return totalPages.value;
    }, // server total pages
    state: { pagination },
    manualPagination: true,
    onPaginationChange: updater => {
      if (typeof updater === 'function') Object.assign(pagination, updater(pagination));
      else Object.assign(pagination, updater);
    },
    getCoreRowModel: getCoreRowModel(),
    initialState: { pagination },
  });

  // Fetch from API whenever page or size changes
  async function fetchUsers() {
    loading.value = true;
    try {
      const page = pagination.pageIndex + 1; // backend 1-based
      const pageSize = pagination.pageSize;

      const { data } = await api.get<GetUsersResult>('/api/admin/users', {
        params: { page, pageSize },
      });

      users.value = data.Users;
      totalCount.value = data.TotalCount;
      totalPages.value = data.TotalPages;

      // sync if backend adjusted values
      pagination.pageIndex = Math.max(0, (data.Page ?? page) - 1);
      pagination.pageSize = data.PageSize ?? pageSize;
    } finally {
      loading.value = false;
    }
  }

  watchEffect(fetchUsers);

  function onEdit(user: UserDto) {
    console.log('edit', user.Id);
  }

  function setPageSize(val: string) {
    pagination.pageSize = Number(val);
    pagination.pageIndex = 0;
  }
</script>
