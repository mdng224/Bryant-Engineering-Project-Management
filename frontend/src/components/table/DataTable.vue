<!-- components/DataTable.vue -->
<template>
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
          class="odd:bg-slate-900/40 even:bg-slate-700/30 hover:bg-slate-800/40"
        >
          <td
            v-for="cell in row.getVisibleCells()"
            :key="cell.id"
            class="whitespace-nowrap px-4 py-2.5"
          >
            <!-- Delegate actual content to a slot with sensible defaults -->
            <slot name="cell" :cell>
              <!-- default renderer using column meta -->
              <CellRenderer :cell />
            </slot>
          </td>
        </tr>

        <tr v-if="!loading && totalCount === 0">
          <td class="px-6 py-6 text-slate-400" :colspan="table.getAllLeafColumns().length">
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
