<template>
  <app-dialog :open="open" :title="title" width="max-w-2xl" @close="emit('close')">
    <div v-if="item" class="space-y-6 text-sm">
      <!-- Header block -->
      <slot name="header">
        <div class="mb-4">
          <div class="flex flex-wrap items-end justify-between gap-3">
            <div class="min-w-0">
              <div class="mt-1 items-center gap-2 text-sm">
                <div
                  v-if="nameKey"
                  class="col-span-12 min-w-0 truncate font-semibold text-slate-100 sm:col-span-8"
                >
                  {{ toTitleCase(getItemVal(nameKey)) }}
                </div>

                <div
                  v-if="idKey"
                  class="col-span-12 justify-self-end pt-1 text-right font-mono text-slate-400 sm:col-span-4"
                >
                  <span class="pr-4 text-xs uppercase tracking-wide">{{ idLabel }}:</span>
                  <span class="max-w-[12rem] truncate">
                    {{ getItemVal(idKey) }}
                    <button
                      v-if="getItemVal(idKey) !== '—'"
                      class="ml-1 inline-flex items-center rounded border border-slate-700 px-1.5 text-[10px] text-slate-300 hover:bg-slate-700/60"
                      @click="copy(String((item as any)[idKey]))"
                      title="Copy ID"
                    >
                      Copy
                    </button>
                  </span>
                </div>
              </div>
            </div>
          </div>
          <div class="mt-4 h-px w-full bg-slate-700/60"></div>
        </div>
      </slot>

      <!-- Grid -->
      <dl class="grid grid-cols-12 gap-y-2">
        <template v-for="f in fields" :key="f.key">
          <div
            class="col-span-12 grid grid-cols-12 items-start rounded-lg bg-slate-800/30 p-1"
            :class="{ 'md:col-span-12': f.span === 2 }"
          >
            <dt
              class="col-span-4 pl-1 pr-4 pt-[1px] text-[12px] font-semibold uppercase tracking-wide text-slate-400"
            >
              {{ f.label }}
            </dt>

            <dd
              class="col-span-8 break-words leading-relaxed text-slate-100"
              :class="{
                'font-mono text-[13px]': f.type === 'mono',
                'text-[16px]': f.type === 'text',
              }"
            >
              <template v-if="f.type === 'multiline'">
                <span class="whitespace-pre-wrap">{{ displayValue(f) }}</span>
              </template>
              <template v-else>
                <span>{{ displayValue(f) }}</span>
              </template>

              <button
                v-if="f.type === 'mono' && displayValue(f) !== '—'"
                class="ml-2 inline-flex items-center rounded border border-slate-700 px-1.5 py-0.5 text-[10px] text-slate-300 hover:bg-slate-700/60"
                @click="copy(displayValue(f))"
                title="Copy"
              >
                Copy
              </button>
            </dd>
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

  type FieldType = 'text' | 'date' | 'mono' | 'multiline';

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

  const displayValue = (f: FieldDef) => {
    if (!props.item) return '—';
    const raw = f.get ? f.get(props.item) : (props.item as any)[f.key];

    if (f.format) return val(f.format(raw, props.item));

    let result: string;
    switch (f.type) {
      case 'date':
        result = raw ? (props.formatUtc ? props.formatUtc(String(raw)) : String(raw)) : '—';
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
