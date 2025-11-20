<template>
  <app-dialog :open="open" :title width="max-w-4xl" @close="emit('close')">
    <div v-if="item" class="space-y-6 text-sm">
      <!-- Header block -->
      <slot name="header">
        <div class="mb-4">
          <div class="flex flex-wrap items-start justify-between gap-3">
            <div class="min-w-0">
              <h3 v-if="nameKey" class="truncate text-xl font-semibold text-slate-50 sm:text-2xl">
                {{ toTitleCase(getItemVal(nameKey)) }}
              </h3>

              <p v-if="idKey" class="mt-2 flex items-center gap-2 font-mono text-xs text-slate-400">
                <span class="uppercase tracking-wide text-slate-500">{{ idLabel }}</span>
                <span class="max-w-[18rem] truncate">
                  {{ getItemVal(idKey) }}
                </span>
                <button
                  v-if="getItemVal(idKey) !== '—'"
                  class="inline-flex items-center rounded border border-slate-700 px-1.5 py-0.5 text-[10px] text-slate-300 hover:bg-slate-700/60"
                  @click="copy(String((item as any)[idKey]))"
                  title="Copy ID"
                >
                  Copy
                </button>
              </p>
            </div>
          </div>

          <div class="mt-4 h-px w-full bg-slate-700/60" />
        </div>
      </slot>

      <!-- Grid -->
      <dl class="grid grid-cols-12 gap-2 md:gap-3">
        <template v-for="f in fields" :key="f.key">
          <div
            class="col-span-12 rounded-lg bg-slate-800/30 px-3 py-2 md:px-4 md:py-2.5"
            :class="f.span === 2 ? 'md:col-span-12' : 'md:col-span-6'"
          >
            <div class="flex items-baseline justify-between gap-4">
              <dt
                class="truncate text-[11px] font-semibold uppercase tracking-wide text-slate-400 md:text-[12px]"
              >
                {{ f.label }}
              </dt>

              <dd
                class="flex-1 break-words text-right leading-relaxed text-slate-100"
                :class="{
                  'font-mono text-[13px]': f.type === 'mono',
                  'text-[15px] md:text-[16px]': f.type === 'text',
                }"
              >
                <template v-if="f.type === 'multiline'">
                  <span class="whitespace-pre-wrap">{{ getDisplayValue(f) }}</span>
                </template>
                <template v-else>
                  <span>{{ getDisplayValue(f) }}</span>
                </template>

                <button
                  v-if="f.type === 'mono' && getDisplayValue(f) !== '—'"
                  class="ml-2 inline-flex items-center rounded border border-slate-700 px-1.5 py-0.5 text-[10px] text-slate-300 hover:bg-slate-700/60"
                  @click="copy(getDisplayValue(f))"
                  title="Copy"
                >
                  Copy
                </button>
              </dd>
            </div>
          </div>
        </template>
      </dl>

      <!-- Extra custom content (chips, subtables, etc.) -->
      <slot name="extra" />
    </div>
  </app-dialog>
</template>

<script setup lang="ts">
  import { AppDialog } from '@/components/ui';
  import { computed } from 'vue';

  type FieldType = 'text' | 'date' | 'mono' | 'multiline' | 'phone';

  export interface FieldDef {
    key: string;
    label: string;
    type?: FieldType;
    span?: 1 | 2;
    format?: (value: unknown, record: Record<string, unknown>) => string;
    get?: (record: Record<string, unknown>) => unknown;
  }

  const props = withDefaults(
    defineProps<{
      open: boolean;
      title: string;
      item: Record<string, unknown> | null;
      fields: FieldDef[];
      formatUtc?: (s: string) => string;
      idKey?: string;
      nameKey?: string;
    }>(),
    {
      idKey: 'id',
      nameKey: 'name',
    },
  );

  const emit = defineEmits<{ (e: 'close'): void }>();

  const idKey = computed(() => props.idKey ?? 'id');
  const nameKey = computed(() => props.nameKey ?? 'name');
  const idLabel = 'ID';

  const normalizeEmpty = (v: unknown) => {
    if (v === null || v === undefined) return '—';
    const s = String(v).trim();
    if (s === '' || s.toLowerCase() === 'unknown' || s.toLowerCase() === 'n/a') return '—';
    return s;
  };

  const val = normalizeEmpty;

  const getItemVal = (k?: string | undefined) =>
    props.item && k ? normalizeEmpty((props.item as any)[k]) : '—';

  const formatPhone = (raw: unknown): string => {
    const normalized = normalizeEmpty(raw);
    if (normalized === '—') return normalized;

    const digits = normalized.replace(/\D+/g, '');
    if (!digits) return normalized;

    let d = digits;

    if (d.length === 11 && d.startsWith('1')) {
      d = d.slice(1);
    }

    if (d.length === 10) {
      const area = d.slice(0, 3);
      const prefix = d.slice(3, 6);
      const line = d.slice(6);
      return `(${area}) ${prefix}-${line}`;
    }

    if (d.length === 7) {
      const prefix = d.slice(0, 3);
      const line = d.slice(3);
      return `${prefix}-${line}`;
    }

    return normalized;
  };

  const getDisplayValue = (f: FieldDef): string => {
    if (!props.item) return '—';
    const raw = f.get ? f.get(props.item) : (props.item as any)[f.key];

    if (f.format) return val(f.format(raw, props.item));

    let result: string;
    switch (f.type) {
      case 'date':
        result = raw ? (props.formatUtc ? props.formatUtc(String(raw)) : String(raw)) : '—';
        break;
      case 'phone':
        result = formatPhone(raw);
        break;
      default:
        result = val(raw);
    }

    if (f.type === 'text' && result !== '—') {
      result = toTitleCase(result);
    }

    return result;
  };

  const toTitleCase = (s: string) =>
    s.replace(/\w\S*/g, w => w.charAt(0).toUpperCase() + w.slice(1).toLowerCase());

  const copy = async (text: string) => {
    try {
      await navigator.clipboard.writeText(text);
    } catch {
      // swallow
    }
  };
</script>
