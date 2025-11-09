<template>
  <span v-if="kind === 'text'" class="text-slate-100">
    {{ displayText }}
  </span>

  <span v-else-if="kind === 'datetime'" class="text-[12px] tracking-wide text-slate-100">
    {{ displayDate }}
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

  <span v-else>{{ asString }}</span>
</template>

<script setup lang="ts">
  import { useDateFormat } from '@/composables/UseDateFormat';
  import type { Cell } from '@tanstack/table-core';
  import { computed } from 'vue';

  type ColMeta =
    | { kind: 'text' | 'datetime' | 'boolean' | 'actions' }
    | { kind: 'badge'; classFor?: (value: string) => string };

  const props = defineProps<{ cell: Cell<unknown, unknown> }>();
  const meta = props.cell.column.columnDef.meta as ColMeta | undefined;
  const kind = meta?.kind ?? 'text';

  const rawVal = props.cell.getValue();
  const { formatUtc } = useDateFormat();

  const asString = computed(() => (rawVal == null ? '' : String(rawVal)));
  const asBoolean = computed(() => Boolean(rawVal));

  const normalizeEmpty = (v: unknown) => {
    if (v === null || v === undefined) return '—';
    const s = String(v).trim();
    const lower = s.toLowerCase();
    if (s === '' || lower === 'n/a' || lower === 'unknown') return '—';
    return s;
  };

  const toTitleCase = (s: string) =>
    s.replace(/\w\S*/g, w => w.charAt(0).toUpperCase() + w.slice(1).toLowerCase());

  const displayText = computed(() => {
    const normalized = normalizeEmpty(rawVal);
    return normalized === '—' ? normalized : toTitleCase(normalized);
  });

  const displayDate = computed(() => {
    const v = rawVal == null ? null : String(rawVal);
    if (!v) return '—';
    return formatUtc(v);
  });

  const badgeClass = computed(() => {
    if (meta?.kind !== 'badge') return '';
    const fn = meta.classFor;
    return fn ? fn(asString.value) : 'bg-slate-800/60 text-slate-300';
  });
</script>
