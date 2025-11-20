<template>
  <div
    class="mt-4 flex flex-wrap items-center justify-between gap-3 border-t border-slate-800 pt-3 text-xs text-slate-300 md:text-sm"
  >
    <!-- Left: counts -->
    <div class="flex flex-wrap items-center gap-3">
      <span v-if="totalCount > 0">
        Showing
        <span class="font-semibold text-slate-100">{{ startIndex }}</span>
        –
        <span class="font-semibold text-slate-100">{{ endIndex }}</span>
        of
        <span class="font-semibold text-slate-100">{{ totalCount }}</span>
      </span>
      <span v-else>No results</span>

      <span v-if="totalPages > 0" class="hidden text-slate-400 md:inline">
        Page {{ pagination.pageIndex + 1 }} of {{ totalPages }}
      </span>
    </div>

    <!-- Right: page size + pager -->
    <div class="flex flex-wrap items-center gap-3">
      <div class="flex items-center gap-2">
        <label class="text-slate-400">Rows:</label>
        <select
          class="rounded-md border border-slate-700 bg-slate-900 px-2 py-1 text-xs focus:outline-none focus:ring-1 focus:ring-indigo-500 md:text-sm"
          :value="pagination.pageSize"
          @change="setPageSize(Number(($event.target as HTMLSelectElement).value))"
        >
          <option v-for="s in [10, 25, 50]" :key="s" :value="s">
            {{ s }}
          </option>
        </select>
      </div>

      <div class="flex items-center gap-2">
        <button
          class="rounded-md border border-slate-700 bg-slate-900/70 p-1.5 text-slate-200 hover:bg-slate-800/80 disabled:cursor-default disabled:opacity-40 md:p-2"
          :disabled="!table.getCanPreviousPage()"
          aria-label="Previous page"
          @click="table.previousPage()"
        >
          <ChevronLeft class="h-4 w-4" />
        </button>
        <button
          class="rounded-md border border-slate-700 bg-slate-900/70 p-1.5 text-slate-200 hover:bg-slate-800/80 disabled:cursor-default disabled:opacity-40 md:p-2"
          :disabled="!table.getCanNextPage()"
          aria-label="Next page"
          @click="table.nextPage()"
        >
          <ChevronRight class="h-4 w-4" />
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  import type { Table } from '@tanstack/vue-table';
  import { ChevronLeft, ChevronRight } from 'lucide-vue-next';
  import { computed } from 'vue';

  type PagedTable = Pick<
    Table<unknown>,
    'getCanPreviousPage' | 'previousPage' | 'getCanNextPage' | 'nextPage'
  >;

  const props = defineProps<{
    table: PagedTable;
    totalCount: number;
    totalPages: number;
    pagination: { pageIndex: number; pageSize: number };
    setPageSize: (n: number) => void;
  }>();

  const startIndex = computed(() =>
    props.totalCount === 0 ? 0 : props.pagination.pageIndex * props.pagination.pageSize + 1,
  );

  const endIndex = computed(() =>
    props.totalCount === 0
      ? 0
      : Math.min(props.totalCount, (props.pagination.pageIndex + 1) * props.pagination.pageSize),
  );
</script>
