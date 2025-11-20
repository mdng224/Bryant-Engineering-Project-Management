<!-- src/views/RegisterView.vue -->
<template>
  <main class="-mt-60 grid min-h-screen place-items-center p-4">
    <!-- UNDERLAY: blurred background with radial fade -->
    <div
      class="pointer-events-none fixed inset-0 z-0 bg-slate-900/40 backdrop-blur-3xl [mask-image:radial-gradient(circle_at_center,white_38%,transparent_100%)]"
    ></div>

    <!-- Foreground content (card area) -->
    <section
      class="relative z-10 w-full max-w-md rounded-2xl border border-slate-800 bg-slate-900 p-6 text-slate-100 shadow-2xl"
    >
      <h1 class="mb-1 text-center text-2xl font-semibold tracking-tight">
        {{ success ? 'Account request submitted' : 'Create your account' }}
      </h1>

      <!-- Success state -->
      <div v-if="success" class="grid pb-4">
        <p
          class="flex items-start gap-3 rounded-lg border border-emerald-800 bg-emerald-900/30 px-3.5 py-3 text-sm leading-relaxed text-emerald-200"
          role="status"
          aria-live="polite"
        >
          <CheckCircle class="mt-[2px] h-5 w-5 shrink-0 text-emerald-400" aria-hidden="true" />
          <span class="grid">
            <span class="font-medium text-emerald-300">Status: {{ success.status }}</span>
            <span>{{ success.message }}</span>
          </span>
        </p>

        <p class="pt-6 text-sm text-slate-400">
          Already have an account?
          <RouterLink
            class="text-indigo-300 hover:underline"
            :to="{ name: 'login', query: { redirect: route.query.redirect ?? '/' } }"
          >
            Sign in
          </RouterLink>
        </p>
      </div>

      <form v-else novalidate class="grid gap-4" @submit.prevent="onSubmit">
        <!-- Email -->
        <div class="grid gap-1.5">
          <label for="email" class="text-sm text-slate-300">Email</label>

          <div class="relative">
            <Mail
              class="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400"
              aria-hidden="true"
            />
            <input
              id="email"
              ref="emailEl"
              v-model.trim="email"
              type="email"
              inputmode="email"
              autocomplete="email"
              autocapitalize="none"
              spellcheck="false"
              required
              :disabled="loading"
              :maxlength="maxEmail"
              placeholder="you@example.com"
              :class="['pl-10', formClass]"
            />
          </div>
        </div>

        <!-- Password -->
        <div class="grid gap-1.5">
          <label for="password" class="text-sm text-slate-300">Password</label>

          <div class="relative">
            <Lock
              class="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400"
              aria-hidden="true"
            />

            <input
              id="password"
              v-model="password"
              :type="showPassword ? 'text' : 'password'"
              autocomplete="new-password"
              required
              :minlength="minPassword"
              :maxlength="maxPassword"
              :disabled="loading"
              placeholder="••••••••"
              :class="['pl-10 pr-10', formClass]"
            />

            <button
              type="button"
              :aria-pressed="showPassword"
              :aria-label="showPassword ? 'Hide password' : 'Show password'"
              :disabled="loading"
              class="absolute right-2 top-1/2 inline-flex h-8 w-8 -translate-y-1/2 items-center justify-center rounded-md text-indigo-300 hover:text-indigo-200 disabled:opacity-50"
              @click="showPassword = !showPassword"
            >
              <component :is="showPassword ? EyeOff : Eye" class="h-4 w-4" aria-hidden="true" />
            </button>
          </div>
        </div>

        <!-- Confirm Password -->
        <div class="grid gap-1.5">
          <label for="password2" class="text-sm text-slate-300">Confirm Password</label>

          <div class="relative">
            <Lock
              class="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400"
              aria-hidden="true"
            />

            <input
              id="password2"
              v-model="password2"
              :type="showPassword2 ? 'text' : 'password'"
              autocomplete="new-password"
              required
              :minlength="minPassword"
              :maxlength="maxPassword"
              :disabled="loading"
              placeholder="••••••••"
              :class="['pl-10 pr-10', formClass]"
            />

            <button
              type="button"
              :aria-pressed="showPassword2"
              :aria-label="showPassword2 ? 'Hide confirm password' : 'Show confirm password'"
              :disabled="loading"
              class="absolute right-2 top-1/2 inline-flex h-8 w-8 -translate-y-1/2 items-center justify-center rounded-md text-indigo-300 hover:text-indigo-200 disabled:opacity-50"
              @click="showPassword2 = !showPassword2"
            >
              <component :is="showPassword2 ? EyeOff : Eye" class="h-4 w-4" aria-hidden="true" />
            </button>
          </div>

          <p
            v-if="password2 && password2 !== password"
            class="flex items-center gap-2 text-xs text-rose-300"
          >
            <AlertTriangle class="h-3.5 w-3.5 shrink-0" aria-hidden="true" />
            <span>Passwords do not match.</span>
          </p>
        </div>

        <!-- Error -->
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

        <!-- Submit -->
        <button
          type="submit"
          :disabled="!canSubmit || loading"
          :aria-busy="loading"
          class="mt-1 inline-flex items-center justify-center rounded-lg border border-indigo-500 bg-indigo-500 px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-indigo-600 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-500 disabled:cursor-not-allowed disabled:opacity-60"
        >
          {{ loading ? 'Creating…' : 'Create account' }}
        </button>

        <p class="mt-1 text-sm text-slate-400">
          Already have an account?
          <RouterLink
            class="text-indigo-300 hover:underline"
            :to="{ name: 'login', query: { redirect: route.query.redirect ?? '/' } }"
          >
            Sign in
          </RouterLink>
        </p>
      </form>
    </section>
  </main>
</template>

<script setup lang="ts">
  import type { RegisterRequest, RegisterResponse } from '@/api/auth';
  import { authService } from '@/api/auth';
  import { extractApiError } from '@/api/error';
  import { useAuthFields } from '@/composables/useAuthFields';
  import { AlertTriangle, CheckCircle, Eye, EyeOff, Lock, Mail } from 'lucide-vue-next';
  import { computed, nextTick, onMounted, ref } from 'vue';
  import { useRoute } from 'vue-router';

  const route = useRoute();
  const {
    email,
    password,
    showPassword,
    isEmailValid,
    isPasswordValid,
    normalizedEmail,
    minPassword,
    maxEmail,
    maxPassword,
  } = useAuthFields();

  const formClass =
    'w-full rounded-lg border border-slate-700 bg-slate-950 px-3.5 py-2.5 text-sm text-slate-100 outline-none transition placeholder:text-slate-500 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/40 disabled:cursor-not-allowed disabled:opacity-60';

  const password2 = ref('');
  const emailEl = ref<HTMLInputElement | null>(null);
  const errorEl = ref<HTMLElement | null>(null);
  const errorMessage = ref<string | null>(null);
  const loading = ref(false);
  const showPassword2 = ref(false);
  const success = ref<RegisterResponse | null>(null);

  onMounted(() => emailEl.value?.focus());

  const passwordsMatch = computed(() => password2.value === password.value);
  const canSubmit = computed(
    () => isEmailValid.value && isPasswordValid.value && passwordsMatch.value,
  );

  const onSubmit = async (): Promise<void> => {
    if (!canSubmit.value || loading.value) return;

    loading.value = true;
    errorMessage.value = null;

    try {
      const payload: RegisterRequest = { email: normalizedEmail.value, password: password.value };
      const registerResponse: RegisterResponse = await authService.register(payload);
      success.value = registerResponse;
    } catch (err: unknown) {
      const msg =
        extractApiError(err, 'email') || 'We couldn’t create your account. Please try again.';
      errorMessage.value = msg;
      await resetForms(false);
    } finally {
      loading.value = false;
    }
  };

  const resetForms = async (clearEmail = false): Promise<void> => {
    showPassword.value = false;
    showPassword2.value = false;
    password.value = '';
    password2.value = '';
    if (clearEmail) email.value = '';

    await nextTick();

    const targetEl = errorMessage.value ? errorEl.value : emailEl.value;
    targetEl?.focus();
  };
</script>
