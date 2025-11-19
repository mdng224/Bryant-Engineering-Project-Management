<!-- src/views/LoginView.vue -->
<template>
  <main class="grid min-h-[50dvh] place-items-center p-4">
    <!-- UNDERLAY: blurred background with radial fade -->
    <div
      class="pointer-events-none fixed inset-0 z-0 bg-slate-900/40 backdrop-blur-3xl [mask-image:radial-gradient(circle_at_center,white_38%,transparent_100%)]"
    ></div>

    <!-- Foreground content (card area) -->
    <section
      class="relative z-10 w-full max-w-md rounded-2xl border border-slate-800 bg-slate-900 p-6 text-slate-100 shadow-2xl"
    >
      <h1 class="mb-4 text-2xl font-semibold tracking-tight">Sign in</h1>

      <form novalidate class="grid gap-4" @submit.prevent="onSubmit">
        <!-- Email -->
        <div class="grid gap-1.5">
          <label for="email" class="text-sm text-slate-300">Email</label>

          <div class="relative">
            <!-- icon -->
            <Mail
              class="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400"
              aria-hidden="true"
            />
            <!-- input -->
            <input
              id="email"
              ref="emailEl"
              v-model.trim="email"
              type="email"
              inputmode="email"
              autocomplete="email"
              :maxlength="maxEmail"
              spellcheck="false"
              required
              :disabled="loading"
              placeholder="you@example.com"
              :class="[formClass, 'pl-10']"
            />
          </div>
        </div>

        <!-- Password -->
        <div class="grid gap-1.5">
          <label for="password" class="text-sm text-slate-300">Password</label>

          <div class="relative">
            <!-- left icon -->
            <Lock
              class="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400"
              aria-hidden="true"
            />

            <!-- input -->
            <input
              id="password"
              v-model="password"
              :type="showPassword ? 'text' : 'password'"
              autocomplete="current-password"
              required
              :maxlength="maxPassword"
              :disabled="loading"
              placeholder="••••••••"
              :class="[formClass, 'pl-10 pr-10']"
            />

            <!-- right toggle button -->
            <button
              type="button"
              :aria-pressed="showPassword"
              :disabled="loading"
              class="absolute right-2 top-1/2 inline-flex h-8 w-8 -translate-y-1/2 items-center justify-center rounded-md text-indigo-300 hover:text-indigo-200 disabled:opacity-50"
              :aria-label="showPassword ? 'Hide password' : 'Show password'"
              @click="showPassword = !showPassword"
            >
              <component :is="showPassword ? EyeOff : Eye" class="h-4 w-4" aria-hidden="true" />
            </button>
          </div>
        </div>

        <app-alert
          v-if="errorMessage"
          :message="errorMessage"
          variant="error"
          :icon="AlertTriangle"
        />

        <!-- Submit -->
        <button
          type="submit"
          :disabled="!canSubmit || loading"
          :aria-busy="loading"
          class="inline-flex items-center justify-center rounded-lg border border-indigo-500 bg-indigo-500 px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-indigo-600 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-500 disabled:cursor-not-allowed disabled:opacity-60"
        >
          <span>{{ loading ? 'Signing in…' : 'Sign in' }}</span>
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
  import type { LoginRequest } from '@/api/auth';
  import { authService } from '@/api/auth/';
  import type { ApiErrorResponse } from '@/api/error';
  import AppAlert from '@/components/ui/AppAlert.vue';
  import { useAuth } from '@/composables/useAuth';
  import { useAuthFields } from '@/composables/useAuthFields';
  import { isAxiosError } from 'axios';
  import { AlertTriangle, Eye, EyeOff, Lock, Mail } from 'lucide-vue-next';
  import { computed, nextTick, onMounted, ref } from 'vue';
  import { useRoute, useRouter } from 'vue-router';
  const route = useRoute();
  const router = useRouter();
  const { ensureAuthState } = useAuth();
  const { email, password, showPassword, isEmailValid, normalizedEmail, maxEmail, maxPassword } =
    useAuthFields();

  const loading = ref(false);
  const errorMessage = ref<string | null>(null);
  const emailEl = ref<HTMLInputElement | null>(null);
  const errorEl = ref<HTMLElement | null>(null);
  const formClass: string = `
      w-full rounded-lg border border-slate-700 bg-slate-950
      px-3.5 py-2.5 text-slate-100 outline-none transition
      placeholder:text-slate-500
      focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/40
      disabled:opacity-60 disabled:cursor-not-allowed
    `;
  onMounted(() => emailEl.value?.focus());

  const canSubmit = computed(() => isEmailValid.value && password.value);

  const onSubmit = async (): Promise<void> => {
    if (!canSubmit.value || loading.value) return;
    loading.value = true;
    errorMessage.value = null;

    try {
      const LoginRequest: LoginRequest = {
        email: normalizedEmail.value, // locale-insensitive for emails
        password: password.value,
      };
      await authService.login(LoginRequest);
      const authed = await ensureAuthState(true);

      if (authed) {
        const redirectTo = (route.query.redirect as string) || '/';
        router.replace(redirectTo.startsWith('/') ? redirectTo : '/');
      } else {
        errorMessage.value = 'Unexpected auth error. Please try again.';
        password.value = '';
      }
    } catch (err: unknown) {
      let msg = 'Login failed. Please try again.';

      if (isAxiosError<ApiErrorResponse>(err)) {
        const { status, data } = err.response ?? {};

        // Prefer ProblemDetails.detail, then message, then legacy error
        const detail = data?.detail ?? data?.message ?? data?.error;

        if (status === 401) {
          msg = detail ?? 'Invalid email or password.';
        } else {
          msg = detail ?? msg;
        }
      } else if (err instanceof Error) {
        msg = err.message || msg;
      }

      errorMessage.value = msg;
      resetForms();
    } finally {
      loading.value = false;
    }
  };

  const resetForms = async (): Promise<void> => {
    showPassword.value = false;
    password.value = '';
    await nextTick();

    if (errorMessage.value) errorEl.value?.focus();
    else emailEl.value?.focus();
  };
</script>
