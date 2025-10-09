<!-- src/views/LoginView.vue -->
<template>
  <main class="login-page">
    <section class="card">
      <h1>Sign in</h1>

      <form @submit.prevent="onSubmit" novalidate>
        <div class="field">
          <label for="email">Email</label>
          <input
            id="email"
            ref="emailEl"
            v-model.trim="email"
            type="email"
            inputmode="email"
            autocomplete="email"
            spellcheck="false"
            required
            :disabled="loading"
            placeholder="you@example.com"
          />
        </div>

        <div class="field">
          <label for="password">Password</label>
          <div class="password-wrap">
            <input
              id="password"
              v-model="password"
              :type="showPassword ? 'text' : 'password'"
              autocomplete="current-password"
              required
              minlength="6"
              :disabled="loading"
              placeholder="••••••••"
            />
            <button
              type="button"
              class="link"
              @click="showPassword = !showPassword"
              :aria-pressed="showPassword"
            >
              {{ showPassword ? 'Hide' : 'Show' }}
            </button>
          </div>
        </div>

        <p v-if="errorMessage" class="error" role="alert">{{ errorMessage }}</p>

        <button
          type="submit"
          class="primary"
          :disabled="!canSubmit || loading"
          :aria-busy="loading"
        >
          {{ loading ? 'Signing in…' : 'Sign in' }}
        </button>
      </form>
    </section>
  </main>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { login } from '@/api/auth'
import { useAuth } from '../composables/useAuth' // uses /me check as discussed
import type { LoginPayload } from '@/api/auth'

const route = useRoute()
const router = useRouter()
const { ensureAuthState } = useAuth()

const email = ref('')
const password = ref('')
const showPassword = ref(false)
const loading = ref(false)
const errorMessage = ref<string | null>(null)

const emailEl = ref<HTMLInputElement | null>(null)
onMounted(() => emailEl.value?.focus())

const canSubmit = computed(() => {
  // lightweight client-side validation
  const hasEmail = /\S+@\S+\.\S+/.test(email.value)
  return hasEmail && password.value.length >= 1
})

const onSubmit = async () => {
  if (!canSubmit.value || loading.value) return
  loading.value = true
  errorMessage.value = null

  try {
    const loginPayload: LoginPayload = {
      email: email.value,
      password: password.value
    }
    await login(loginPayload)
    const authed = await ensureAuthState(true)   // 🔁 force refresh
    
    if (authed) {
      const redirectTo = (route.query.redirect as string) || '/'
      router.replace(redirectTo)
    } else {
      errorMessage.value = 'Unexpected auth error. Please try again.'
    }
  } catch (err: any) {
    const msg =
      err?.response?.data?.message ??
      err?.response?.data?.error ??
      err?.message ??
      'Login failed. Please try again.'
    errorMessage.value = msg
  } finally {
    loading.value = false
  }
}

</script>

<style scoped>
.login-page {
  min-height: 50dvh;
  display: grid;
  place-items: center;
  padding: 1rem;
}
.card {
  width: 100%;
  max-width: 420px;
  background: #11151c;
  border: 1px solid #1f2630;
  border-radius: 16px;
  padding: 24px;
  color: #e6ebf2;
  box-shadow: 0 10px 30px rgba(0,0,0,.35);
}
h1 {
  margin: 0 0 1rem;
  font-size: 1.5rem;
  letter-spacing: .2px;
}
form { display: grid; gap: 14px; }
.field { display: grid; gap: 6px; }
label { font-size: .9rem; color: #c9d3e1; }
input {
  appearance: none;
  width: 100%;
  padding: 12px 14px;
  border-radius: 10px;
  border: 1px solid #2a3340;
  background: #0e131a;
  color: #e6ebf2;
  outline: none;
}
input:focus { border-color: #4b89ff; box-shadow: 0 0 0 3px rgba(75,137,255,.2); }
.password-wrap {
  display: grid;
  grid-template-columns: 1fr auto;
  align-items: center;
  gap: 8px;
}
.link {
  background: transparent;
  border: none;
  color: #8fb0ff;
  cursor: pointer;
  padding: 0 6px;
}
.link:hover { text-decoration: underline; }

.primary {
  padding: 12px 16px;
  border-radius: 10px;
  border: 1px solid #4b89ff;
  background: #4b89ff;
  color: white;
  font-weight: 600;
  cursor: pointer;
}
.primary[disabled] {
  opacity: .6;
  cursor: not-allowed;
}
.error {
  color: #ffa8a8;
  background: #2a1212;
  border: 1px solid #6b1d1d;
  padding: 10px 12px;
  border-radius: 10px;
}
.hint { margin-top: 10px; color: #9fb0c6; font-size: .9rem; }
.hint a { color: #c7d7ff; }
</style>
