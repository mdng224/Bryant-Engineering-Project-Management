<!-- components/table/CellRenderer.vue -->
<template>
  <span v-if="kind === 'text'" class="text-slate-100">
    {{ asString }}
  </span>

  <span v-else-if="kind === 'datetime'" class="text-[12px] tracking-wide text-slate-100">
    {{ formatUtc(val as string | null) }}
  </span>

  <span v-else-if="kind === 'boolean'" class="text-sm">
    {{ asBoolean ? 'Yes' : 'No' }}
  </span>

  <span
    v-else-if="kind === 'badge'"
    class="inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium"
    :class="badgeClass"
  >
    {{ asString }}
  </span>

  <slot v-else-if="kind === 'actions'"></slot>

  <span v-else>{{ val as string }}</span>
</template>

<script setup lang="ts">
  import { useDateFormat } from '@/composables/UseDateFormat';
  import type { Cell } from '@tanstack/table-core';
  import { computed } from 'vue';

  type ColMeta =
    | { kind: 'text' | 'datetime' | 'boolean' | 'actions' }
    | { kind: 'badge'; classFor?: (value: string) => string };

  const props = defineProps<{ cell: Cell<unknown, unknown> }>();
  const rawMeta = props.cell.column.columnDef.meta as ColMeta | undefined;
  const rawVal = props.cell.getValue();
  const asString = computed(() => (rawVal == null ? '' : String(rawVal)));
  const asBoolean = computed(() => Boolean(rawVal));
  const meta = props.cell.column.columnDef.meta as ColMeta | undefined;
  const kind = meta?.kind ?? 'text';
  const val = props.cell.getValue();

  const badgeClass = computed(() => {
    const fn = (rawMeta as Extract<ColMeta, { kind: 'badge' }>)?.classFor;

    return fn ? fn(asString.value) : 'bg-slate-800/60 text-slate-300';
  });

  const { formatUtc } = useDateFormat();
</script>
