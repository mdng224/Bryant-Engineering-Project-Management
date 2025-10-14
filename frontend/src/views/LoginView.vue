<!-- src/views/LoginView.vue -->
<template>
  <main class="grid min-h-[50dvh] place-items-center p-4">
    <section
      class="w-full max-w-md rounded-2xl border border-slate-800 bg-slate-900/80 p-6 text-slate-100 shadow-xl backdrop-blur"
    >
      <h1 class="mb-4 text-2xl font-semibold tracking-tight">Sign in</h1>

      <form @submit.prevent="onSubmit" novalidate class="grid gap-4">
        <!-- Email -->
        <div class="grid gap-1.5">
          <label for="email" class="text-sm text-slate-300">Email</label>
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
            class="w-full rounded-lg border border-slate-700 bg-slate-950 px-3.5 py-2.5 text-slate-100 outline-none transition placeholder:text-slate-500 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/40 disabled:cursor-not-allowed disabled:opacity-60"
          />
        </div>

        <!-- Password -->
        <div class="grid gap-1.5">
          <label for="password" class="text-sm text-slate-300">Password</label>

          <div class="grid grid-cols-[1fr_auto] items-center gap-2">
            <input
              id="password"
              v-model="password"
              :type="showPassword ? 'text' : 'password'"
              autocomplete="current-password"
              required
              minlength="6"
              :disabled="loading"
              placeholder="••••••••"
              class="w-full rounded-lg border border-slate-700 bg-slate-950 px-3.5 py-2.5 text-slate-100 outline-none transition placeholder:text-slate-500 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/40 disabled:cursor-not-allowed disabled:opacity-60"
            />
            <button
              type="button"
              class="px-2 text-sm font-medium text-indigo-300 hover:text-indigo-200 hover:underline disabled:opacity-50"
              @click="showPassword = !showPassword"
              :aria-pressed="showPassword"
              :disabled="loading"
            >
              {{ showPassword ? 'Hide' : 'Show' }}
            </button>
          </div>
        </div>

        <!-- Error -->
        <p
          v-if="errorMessage"
          class="rounded-lg border border-rose-800 bg-rose-900/30 px-3.5 py-2 text-sm text-rose-200"
          role="alert"
        >
          {{ errorMessage }}
        </p>

        <!-- Submit -->
        <button
          type="submit"
          :disabled="!canSubmit || loading"
          :aria-busy="loading"
          class="inline-flex items-center justify-center rounded-lg border border-indigo-500 bg-indigo-500 px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-indigo-600 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-500 disabled:cursor-not-allowed disabled:opacity-60"
        >
          {{ loading ? 'Signing in…' : 'Sign in' }}
        </button>

        <p class="text-sm text-slate-400">
          New here?
          <RouterLink class="text-indigo-300 hover:underline" to="/register">
            Create an account
          </RouterLink>
        </p>

        <!-- TODO: ADD FORGOT PASSWORD FEATURE -->
      </form>
    </section>
  </main>
</template>

<script setup lang="ts">
  import { ref, computed, onMounted } from 'vue';
  import { useRoute, useRouter } from 'vue-router';
  import { login } from '@/api/auth';
  import { useAuth } from '@/composables/useAuth';
  import type { LoginPayload } from '@/api/auth';

  const route = useRoute();
  const router = useRouter();
  const { ensureAuthState } = useAuth();

  const email = ref('');
  const password = ref('');
  const showPassword = ref(false);
  const loading = ref(false);
  const errorMessage = ref<string | null>(null);

  const emailEl = ref<HTMLInputElement | null>(null);
  onMounted(() => emailEl.value?.focus());

  const isEmailValid = computed(() => /\S+@\S+\.\S+/.test(email.value));
  const canSubmit = computed(() => isEmailValid && password.value.length >= 8);

  const onSubmit = async () => {
    if (!canSubmit.value || loading.value) return;
    loading.value = true;
    errorMessage.value = null;

    try {
      const loginPayload: LoginPayload = {
        email: email.value.trim().toLocaleLowerCase(),
        password: password.value,
      };
      await login(loginPayload);
      const authed: boolean = await ensureAuthState(true);

      if (authed) {
        const redirectTo = (route.query.redirect as string) || '/';
        router.replace(redirectTo.startsWith('/') ? redirectTo : '/');
      } else {
        errorMessage.value = 'Unexpected auth error. Please try again.';
        password.value = '';
      }
    } catch (err: any) {
      const msg =
        err?.response?.data?.message ??
        err?.response?.data?.error ??
        err?.message ??
        'Login failed. Please try again.';
      errorMessage.value = msg;
      password.value = '';
    } finally {
      loading.value = false;
    }
  };
</script>
