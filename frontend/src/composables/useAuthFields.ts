// src/composables/useAuthFields.ts
import { computed, ref } from 'vue';

/**
 * Composable providing reactive state and validation logic for authentication forms.
 *
 * @param options             - Optional configuration.
 * @param options.minPassword - Minimum password length (default: 8).
 * @param options.maxPassword - Maximum password length (default: 72).
 * @param options.maxEmail    - Maximum email length (default: 254).
 *
 * @returns An object containing:
 * - **email**                — Reactive string bound to the user's email input.
 * - **password**             — Reactive string bound to the user's password input.
 * - **showPassword**         — Boolean flag to toggle password visibility.
 * - **isEmailValid**         — Computed boolean indicating if the email matches a basic pattern and does not exceed `maxEmail`.
 * - **isPasswordValid**      — Computed boolean indicating if the password length is within `[minPassword, maxPassword]`.
 * - **normalizedEmail**      — Computed, trimmed and lowercased email string suitable for API submission.
 * - **min**                  — The minimum password length used for validation.
 * - **maxEmail**             — The maximum allowed email length.
 * - **maxPassword**          — The maximum allowed password length.
 *
 * @example
 * ```ts
 * const {
 *   email,
 *   password,
 *   showPassword,
 *   isEmailValid,
 *   isPasswordValid,
 *   normalizedEmail,
 *   min,
 *   maxEmail,
 *   maxPassword
 * } = useAuthFields({ minPassword: 8, maxPassword: 72, maxEmail: 254 })
 * ```
 */
export function useAuthFields(options?: {
  minPassword?: number;
  maxPassword?: number;
  maxEmail?: number;
}) {
  const minPassword = options?.minPassword ?? 8;
  const maxPassword = options?.maxPassword ?? 72;
  const maxEmail = options?.maxEmail ?? 254;

  const email = ref('');
  const password = ref('');
  const showPassword = ref(false);

  const isEmailValid = computed(
    () => /\S+@\S+\.\S+/.test(email.value) && email.value.length <= maxEmail,
  );
  const isPasswordValid = computed(
    () => password.value.length >= minPassword && password.value.length <= maxPassword,
  );
  const normalizedEmail = computed(() => email.value.trim().toLowerCase());

  return {
    email,
    password,
    showPassword,
    isEmailValid,
    isPasswordValid,
    normalizedEmail,
    minPassword,
    maxPassword,
    maxEmail,
  };
}
