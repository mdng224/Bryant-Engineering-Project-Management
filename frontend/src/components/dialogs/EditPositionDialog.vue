<template>
  <Teleport to="body">
    <div v-if="open" class="fixed inset-0 z-[100]" role="dialog" aria-modal="true">
      <!-- Overlay -->
      <div class="absolute inset-0 bg-black/60 backdrop-blur-sm" @click="handleOnCancel"></div>

      <!-- Centered panel -->
      <div class="absolute inset-0 flex items-center justify-center p-4">
        <form
          class="w-full max-w-lg rounded-xl border border-slate-700 bg-slate-900/95 p-6 text-slate-100 shadow-2xl"
          @submit.prevent="handleOnSubmit"
          @keydown.esc.prevent="handleOnCancel"
        >
          <!-- Header -->
          <div class="mb-6 flex items-center justify-between gap-3">
            <h2 class="text-lg font-semibold">Edit Position</h2>
            <button
              class="rounded-md px-2 py-1 text-sm text-slate-300 hover:bg-slate-700/70"
              @click="$emit('close')"
            >
              <X class="block h-5 w-5" />
            </button>
            <p
              v-if="errorMessage"
              ref="errorEl"
              class="flex items-center gap-2 rounded-lg border border-rose-800 bg-rose-900/30 px-3.5 py-2 text-sm leading-tight text-rose-200"
              role="alert"
              aria-live="assertive"
              tabindex="-1"
            >
              <AlertTriangle class="block h-4 w-4 shrink-0 self-center" aria-hidden="true" />
              <span>{{ errorMessage }}</span>
            </p>
          </div>

          <!-- Fields -->
          <div class="grid grid-cols-1 gap-4">
            <!-- Name -->
            <div>
              <label class="text-xs text-slate-400" for="pos-name">Position Name</label>
              <input
                id="pos-name"
                ref="nameEl"
                v-model.trim="model.name"
                class="mt-1 w-full rounded-md border border-slate-700 bg-slate-800/70 px-3 py-2 text-sm"
                placeholder="e.g., Project Engineer"
                :disabled="saving"
                maxlength="100"
                autocomplete="off"
              />
              <p v-if="nameError" class="mt-1 text-xs text-rose-400">{{ nameError }}</p>
            </div>

            <!-- Code -->
            <div>
              <label class="text-xs text-slate-400" for="pos-code">Code</label>
              <input
                id="pos-code"
                v-model.trim="model.code"
                class="mt-1 w-full rounded-md border border-slate-700 bg-slate-800/70 px-3 py-2 text-sm"
                placeholder="e.g., PE1"
                :disabled="saving"
                maxlength="20"
                autocomplete="off"
              />
              <p v-if="codeError" class="mt-1 text-xs text-rose-400">{{ codeError }}</p>
            </div>

            <!-- Requires License -->
            <div class="flex items-center gap-2">
              <input
                id="pos-req-lic"
                v-model="model.requiresLicense"
                type="checkbox"
                :disabled="saving"
                class="h-4 w-4 rounded border-slate-700 bg-slate-800/70"
              />
              <label class="text-sm text-slate-200" for="pos-req-lic">Requires License</label>
            </div>
          </div>
          <div class="mt-6 flex justify-end gap-3">
            <button
              type="submit"
              class="flex gap-2 rounded-md bg-indigo-600 px-3 py-2 text-sm text-white transition hover:bg-indigo-500 disabled:cursor-not-allowed disabled:opacity-60"
              :disabled="saving || !isValid || bothEmpty"
            >
              <Save class="block h-4 w-4 shrink-0 self-center" />
              {{ saving ? 'Saving...' : 'Save' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
  import { extractApiError } from '@/api/error';
  import { positionService } from '@/api/positions';
  import type { PositionResponse, UpdatePositionRequest } from '@/api/positions/contracts';
  import { AlertTriangle, Save, X } from 'lucide-vue-next';
  import { computed, nextTick, onBeforeUnmount, onMounted, reactive, ref, watch } from 'vue';

  const props = defineProps<{
    open: boolean;
    selectedPosition: PositionResponse | null;
  }>();

  const emit = defineEmits<{
    (e: 'close'): void;
    (e: 'save'): void;
  }>();

  /******************** State ********************/
  const model = reactive<PositionResponse>({
    id: '',
    name: '',
    code: '',
    requiresLicense: false,
  });

  const saving = ref(false);
  const nameEl = ref<HTMLInputElement | null>(null);
  const errorEl = ref<HTMLElement | null>(null);

  /******************** Validation ********************/
  const errorMessage = ref<string | null>(null);

  const bothEmpty = computed(() => !model.name?.trim() && !model.code?.trim());

  const nameError = computed(() => {
    if (!model.name?.trim()) return 'Name is required.';
    return '';
  });

  const codeError = computed(() => {
    if (!model.code?.trim()) return 'Code is required.';
    return '';
  });

  const isValid = computed(() => !nameError.value && !codeError.value);

  /******************** Effects ********************/
  watch(
    () => [props.open, props.selectedPosition],
    async () => {
      if (!props.open || !props.selectedPosition) return;

      model.name = props.selectedPosition.name ?? '';
      model.code = props.selectedPosition.code ?? '';
      model.requiresLicense = !!props.selectedPosition.requiresLicense;
      errorMessage.value = null;
      await nextTick();
      nameEl.value?.focus();
    },
    { immediate: true },
  );

  /******************** Handlers ********************/
  const handleOnCancel = (): void => {
    if (saving.value) return;
    errorMessage.value = null;
    emit('close');
  };

  const handleOnSubmit = async (): Promise<void> => {
    if (!isValid.value || bothEmpty.value || saving.value) return;

    saving.value = true;
    errorMessage.value = null;

    try {
      const id = props.selectedPosition?.id;
      if (!id) return;

      const payload: UpdatePositionRequest = {
        name: model.name.trim(),
        code: model.code.trim(),
        requiresLicense: !!model.requiresLicense,
      };
      await positionService.update(id, payload);
      emit('save');
      emit('close');
    } catch (e: unknown) {
      let msg = extractApiError(e, 'name');

      if (!msg || msg === 'An unexpected error occurred.') {
        msg = extractApiError(e, 'code');
      }

      errorMessage.value = msg;
      await nextTick();
      errorEl.value?.focus();
    } finally {
      saving.value = false;
    }
  };

  /******************** Global Escape (optional) ********************/
  const onKeydown = (e: KeyboardEvent) => {
    if (e.key === 'Escape') handleOnCancel();
  };
  onMounted(() => window.addEventListener('keydown', onKeydown));
  onBeforeUnmount(() => window.removeEventListener('keydown', onKeydown));
</script>
