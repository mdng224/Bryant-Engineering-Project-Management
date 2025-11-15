<template>
  <app-dialog :open="open" title="Edit Position" width="max-w-lg" @close="emit('close')">
    <app-alert v-if="errorMessage" :message="errorMessage" variant="error" :icon="AlertTriangle" />

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
    <template #footer>
      <button
        type="submit"
        class="flex gap-2 rounded-md bg-indigo-600 px-3 py-2 text-sm text-white transition hover:bg-indigo-500 disabled:cursor-not-allowed disabled:opacity-60"
        :disabled="saving || !isValid || bothEmpty"
        @click="handleUpdate"
      >
        <Save class="block h-4 w-4 shrink-0 self-center" />
        {{ saving ? 'Saving...' : 'Save' }}
      </button>
    </template>
  </app-dialog>
</template>

<script setup lang="ts">
  import { extractApiError } from '@/api/error';
  import { positionService } from '@/api/positions';
  import type { PositionResponse, UpdatePositionRequest } from '@/api/positions/contracts';
  import AppAlert from '@/components/AppAlert.vue';
  import { AlertTriangle, Save } from 'lucide-vue-next';
  import { computed, nextTick, onBeforeUnmount, onMounted, reactive, ref, watch } from 'vue';
  import AppDialog from '../ui/AppDialog.vue';

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

  const handleUpdate = async (): Promise<void> => {
    if (!isValid.value || bothEmpty.value || saving.value) return;

    saving.value = true;
    errorMessage.value = null;

    try {
      const id = props.selectedPosition?.id;
      if (!id) return;

      const request: UpdatePositionRequest = {
        name: model.name.trim(),
        code: model.code.trim(),
        requiresLicense: !!model.requiresLicense,
      };
      console.log(id, request);
      await positionService.update(id, request);
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
