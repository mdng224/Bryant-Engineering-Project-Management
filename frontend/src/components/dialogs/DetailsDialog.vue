<template>
  <Teleport to="body">
    <div
      v-if="open"
      class="fixed inset-0 z-[100]"
      role="dialog"
      aria-modal="true"
      :aria-labelledby="headingId"
    >
      <div class="absolute inset-0 bg-black/60 backdrop-blur-sm" @click="$emit('close')"></div>

      <div class="absolute inset-0 flex items-center justify-center p-4">
        <div
          class="w-full max-w-xl rounded-xl border border-slate-700 bg-slate-900/95 p-6 text-slate-100 shadow-2xl outline-none"
          @keydown.esc="$emit('close')"
          ref="panel"
        >
          <div class="mb-4 flex items-center justify-between">
            <h2 :id="headingId" class="text-2xl font-bold">{{ title }}</h2>
            <button
              class="rounded-md px-2 py-1 text-sm text-slate-300 hover:bg-slate-700/70"
              @click="$emit('close')"
              aria-label="Close dialog"
            >
              <X class="block h-5 w-5" />
            </button>
          </div>

          <div v-if="item" class="space-y-6 text-sm">
            <!-- Header block -->
            <slot name="header">
              <div v-if="item" class="mb-4">
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
                <!-- one visual row per field -->
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

                    <!-- copy button for mono/ID-like fields -->
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
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
  import { X } from 'lucide-vue-next';
  import { computed, onBeforeUnmount, onMounted, ref } from 'vue';

  type FieldType = 'text' | 'date' | 'mono' | 'multiline';

  export interface FieldDef {
    key: string; // accept simple string keys at the boundary
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

  const idKey = computed(() => props.idKey ?? 'id');
  console.log(idKey);
  const nameKey = computed(() => props.nameKey ?? 'name');

  const getItemVal = (k?: string | undefined) =>
    props.item && k ? normalizeEmpty((props.item as any)[k]) : '—';

  const idLabel = 'ID';
  const emit = defineEmits<{ (e: 'close'): void }>();

  const headingId = `details_heading_${Math.random().toString(36).slice(2)}`;
  const panel = ref<HTMLElement | null>(null);

  const normalizeEmpty = (v: unknown) => {
    if (v === null || v === undefined) return '—';
    const s = String(v).trim();
    if (s === '' || s.toLowerCase() === 'unknown' || s.toLowerCase() === 'n/a') return '—';
    return s;
  };

  const val = normalizeEmpty;

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

    // apply title case only for 'text' type
    if (f.type === 'text' && result !== '—') {
      result = toTitleCase(result);
    }

    return result;
  };
  const toTitleCase = (s: string) =>
    s.replace(/\w\S*/g, w => w.charAt(0).toUpperCase() + w.slice(1).toLowerCase());

  // tiny clipboard util
  const copy = async (text: string) => {
    try {
      await navigator.clipboard.writeText(text);
    } catch {}
  };

  /* Escape handling (optional since we catch on panel) */
  const handleKeydown = (e: KeyboardEvent) => {
    if (e.key === 'Escape') emit('close');
  };

  onMounted(() => window.addEventListener('keydown', handleKeydown));
  onBeforeUnmount(() => window.removeEventListener('keydown', handleKeydown));
</script>
