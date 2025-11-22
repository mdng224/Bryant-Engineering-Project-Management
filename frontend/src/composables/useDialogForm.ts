// src/composables/useDialogForm.ts
import { ref, type Ref } from 'vue';

export function useDialogForm<T>(createInitialForm: () => T) {
  const form: Ref<T> = ref(createInitialForm()) as Ref<T>;
  const submitting = ref(false);
  const errorMessage = ref<string | null>(null);
  const touched = ref(false);

  const reset = () => {
    form.value = createInitialForm();
    submitting.value = false;
    errorMessage.value = null;
    touched.value = false;
  };

  return {
    form,
    submitting,
    errorMessage,
    touched,
    reset,
  };
}
