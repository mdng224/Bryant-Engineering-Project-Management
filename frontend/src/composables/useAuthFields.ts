import { ref, computed } from 'vue'

export function useAuthFields(opts?: { minPassword?: number }) {
  const min = opts?.minPassword ?? 8
  const email = ref('')
  const password = ref('')
  const showPassword = ref(false)

  const isEmailValid = computed(() => /\S+@\S+\.\S+/.test(email.value))
  const isPasswordValid = computed(() => password.value.length >= min)
  const normalizedEmail = computed(() => email.value.trim().toLowerCase())

  return {
    email, password, showPassword,
    isEmailValid, isPasswordValid, normalizedEmail, min
  }
}
