<!-- components/DataTable.vue -->
<template>
  <div class="overflow-hidden rounded-xl border border-slate-700 bg-slate-900/70 shadow">
    <table class="min-w-full divide-y divide-slate-700 text-sm text-slate-200">
      <!-- Sticky header -->
      <thead class="sticky top-0 z-10 bg-slate-800/90 text-slate-100 backdrop-blur">
        <tr v-for="hg in table.getHeaderGroups()" :key="hg.id">
          <th
            v-for="header in hg.headers"
            :key="header.id"
            class="px-4 py-2 text-left text-[11px] font-semibold uppercase tracking-wide text-slate-300"
          >
            {{ header.column.columnDef.header as string }}
          </th>
        </tr>
      </thead>

      <tbody class="divide-y divide-slate-800">
        <!-- Loading skeleton -->
        <SkeletonRows v-if="loading" :rows="10" :cols="table.getAllLeafColumns().length" />

        <!-- Data rows -->
        <tr
          v-else
          v-for="row in table.getRowModel().rows"
          :key="row.id"
          class="transition-colors odd:bg-slate-900/40 even:bg-slate-800/30 hover:bg-slate-800/70"
        >
          <td
            v-for="cell in row.getVisibleCells()"
            :key="cell.id"
            class="max-w-xs truncate px-4 py-2.5 align-middle"
          >
            <!-- Delegate actual content to a slot with sensible defaults -->
            <slot name="cell" :cell>
              <!-- default renderer using column meta -->
              <cell-renderer :cell />
            </slot>
          </td>
        </tr>

        <!-- Empty state -->
        <tr v-if="!loading && totalCount === 0">
          <td
            :colspan="table.getAllLeafColumns().length"
            class="px-6 py-6 text-center text-slate-400"
          >
            {{ emptyText ?? 'No results.' }}
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script setup lang="ts">
  import type { Table } from '@tanstack/table-core';
  import CellRenderer from './CellRenderer.vue';
  import SkeletonRows from './SkeletonRows.vue';

  type Props = {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    table: Table<any>;
    loading: boolean;
    totalCount: number;
    emptyText?: string;
  };

  defineProps<Props>();
</script>
