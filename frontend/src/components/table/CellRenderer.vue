<!-- components/table/CellRenderer.vue -->
<template>
  <span v-if="kind === 'text'" class="text-md text-slate-100">{{ val as string }}</span>

  <span v-else-if="kind === 'datetime'" class="text-[12px] tracking-wide text-slate-100">
    {{ formatUtc(val as string | null) }}
  </span>

  <span v-else-if="kind === 'boolean'" class="text-sm">
    {{ (val as boolean) ? 'Yes' : 'No' }}
  </span>

  <span
    v-else-if="kind === 'badge'"
    :class="[
      'rounded-full px-2 py-0.5 text-sm font-medium',
      meta.classFor?.(val as string) ?? 'bg-slate-800/60 text-slate-300',
    ]"
  >
    {{ val as string }}
  </span>

  <slot v-else-if="kind === 'actions'"></slot>

  <span v-else>{{ val as any }}</span>
</template>

<script setup lang="ts">
  import { useDateFormat } from '@/composables/useDateFormat';
  import type { Cell } from '@tanstack/table-core';

  type ColMeta =
    | { kind: 'text' | 'datetime' | 'boolean' | 'actions' }
    | { kind: 'badge'; classFor?: (value: string) => string };

  const props = defineProps<{ cell: Cell<any, unknown> }>();

  const meta = props.cell.column.columnDef.meta as ColMeta | undefined;
  const kind = meta?.kind ?? 'text';
  const val = props.cell.getValue();

  const { formatUtc } = useDateFormat();
</script>
