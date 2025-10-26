// composables/useDebouncedRef.ts
import { ref, watch } from 'vue';

/**
 * Creates a debounced reactive pair:
 * - `input`: updates immediately (bind this to v-model)
 * - `debounced`: updates only after `delayMs` of no changes
 * - `setNow()`: immediately syncs debounced to input
 * - `cancel()`: clears any pending timer
 */
export function useDebouncedRef<T = string>(initialValue: T, delayMs = 500) {
  const input = ref<T>(initialValue);
  const debounced = ref<T>(initialValue);

  let timerId: ReturnType<typeof setTimeout> | null = null;

  const cancel = () => {
    if (timerId) {
      clearTimeout(timerId);
      timerId = null;
    }
  };

  const setNow = () => {
    cancel();
    debounced.value = input.value;
  };

  watch(input, (newVal) => {
    cancel();
    timerId = setTimeout(() => {
      debounced.value = newVal;
      timerId = null;
    }, delayMs);
  });

  return { input, debounced, setNow, cancel };
}