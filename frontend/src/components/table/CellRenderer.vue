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

  <button
    v-else-if="kind === 'link'"
    type="button"
    class="text-xs font-medium text-indigo-400 underline-offset-2 hover:text-indigo-300 hover:underline disabled:cursor-default disabled:opacity-40"
    :disabled="isLinkDisabled"
    @click="handleLinkClick"
  >
    {{ asNumber }}
  </button>

  <span v-else>{{ asString }}</span>
</template>

<script setup lang="ts">
  import { useDateFormat } from '@/composables/UseDateFormat';
  import type { Cell } from '@tanstack/table-core';
  import { computed } from 'vue';

  type ColMeta =
    | { kind: 'text' | 'datetime' | 'boolean' | 'actions' }
    | { kind: 'badge'; classFor?: (value: string) => string }
    | {
        kind: 'link';
        onClick?: (ctx: { cell: Cell<unknown, unknown>; value: unknown }) => void;
        disableWhenZero?: boolean;
      };

  const props = defineProps<{ cell: Cell<unknown, unknown> }>();
  const meta = props.cell.column.columnDef.meta as ColMeta | undefined;
  const kind = meta?.kind ?? 'text';

  const rawVal = props.cell.getValue();
  const { formatUtc } = useDateFormat();

  const asString = computed(() => (rawVal == null ? '' : String(rawVal)));
  const asBoolean = computed(() => Boolean(rawVal));
  const asNumber = computed(() => {
    const n = Number(rawVal);
    return Number.isNaN(n) ? null : n;
  });

  const normalizeEmpty = (v: unknown) => {
    if (v === null || v === undefined) return '—';
    const s = String(v).trim();
    const lower = s.toLowerCase();
    if (s === '' || lower === 'n/a' || lower === 'unknown') return '—';
    return s;
  };

  const toTitleCase = (s: string) =>
    s.replace(/\w\S*/g, w => w.charAt(0).toUpperCase() + w.slice(1).toLowerCase());

  const displayText = computed((): string => {
    const normalized = normalizeEmpty(rawVal);
    if (normalized === '—') return normalized;

    // Title case first
    const titled = toTitleCase(normalized);

    // Then correct legal suffixes
    return fixLegalSuffixes(titled);
  });

  const displayDate = computed((): string => {
    const v = rawVal == null ? null : String(rawVal);
    if (!v) return '—';
    return formatUtc(v);
  });

  const badgeClass = computed(() => {
    if (meta?.kind !== 'badge') return '';
    const fn = meta.classFor;
    return fn ? fn(asString.value) : 'bg-slate-800/60 text-slate-300';
  });

  const isLinkDisabled = computed(() => {
    if (meta?.kind !== 'link') return false;
    if (!meta.disableWhenZero) return false;
    const n = asNumber.value;
    return n === null || n === 0;
  });

  const handleLinkClick = (): void => {
    if (meta?.kind !== 'link') return;
    if (isLinkDisabled.value) return;
    meta.onClick?.({ cell: props.cell, value: rawVal });
  };

  const fixLegalSuffixes = (s: string): string => {
    const LEGAL_SUFFIXES = ['llc', 'pllc', 'llp', 'lp', 'inc', 'co', 'pc', 'ltd', 'corp', 'psc'];

    return s
      .split(/\s+/)
      .map(word => {
        const lower = word.toLowerCase();
        return LEGAL_SUFFIXES.includes(lower) ? lower.toUpperCase() : word;
      })
      .join(' ');
  };
</script>
