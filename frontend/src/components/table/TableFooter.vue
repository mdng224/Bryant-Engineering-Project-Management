<template>
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
        @change="setPageSize(Number(($event.target as HTMLSelectElement).value))"
      >
        <option v-for="s in [10, 25, 50]" :key="s" :value="s">{{ s }}</option>
      </select>

      <div class="flex items-center gap-2">
        <button
          class="rounded-md border border-slate-700 p-2 disabled:opacity-50"
          :disabled="!table.getCanPreviousPage()"
          aria-label="Previous page"
          @click="table.previousPage()"
        >
          <ChevronLeft class="h-4 w-4" />
        </button>
        <button
          class="rounded-md border border-slate-700 p-2 disabled:opacity-50"
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
  type PagedTable = Pick<
    Table<unknown>,
    'getCanPreviousPage' | 'previousPage' | 'getCanNextPage' | 'nextPage'
  >;
  defineProps<{
    table: PagedTable;
    totalCount: number;
    totalPages: number;
    pagination: { pageIndex: number; pageSize: number };
    setPageSize: (n: number) => void;
  }>();
</script>
