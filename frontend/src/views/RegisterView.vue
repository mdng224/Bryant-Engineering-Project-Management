<!-- src/views/RegisterView.vue -->
<template>
  <main class="grid min-h-[50dvh] place-items-center p-4">
    <section
      class="w-full max-w-md rounded-2xl border border-slate-800 bg-slate-900/80 p-6 text-slate-100 shadow-xl backdrop-blur"
    >
      <h1 class="mb-4 text-2xl font-semibold tracking-tight">
        {{ success ? 'Request submitted' : 'Create account' }}
      </h1>

      <!-- Success state -->
      <div v-if="success" class="grid pb-4">
        <p
          class="rounded-lg border border-emerald-800 bg-emerald-900/30 px-3.5 py-2 text-sm text-emerald-200"
          role="status"
          aria-live="polite"
        >
          (Status: {{ success.status }}) {{ success.message }}
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

      <form v-else @submit.prevent="onSubmit" novalidate class="grid gap-4">
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
            autocapitalize="none"
            spellcheck="false"
            required
            :disabled="loading"
            :maxlength="maxEmail"
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
              autocomplete="new-password"
              required
              :minlength="minPassword"
              :maxlength="maxPassword"
              :disabled="loading"
              placeholder="••••••••"
              :class="formClass"
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

        <!-- Confirm Password -->
        <div class="grid gap-1.5">
          <label for="password2" class="text-sm text-slate-300">Confirm Password</label>
          <div class="grid grid-cols-[1fr_auto] items-center gap-2">
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
              :class="formClass"
            />
            <button
              type="button"
              class="px-2 text-sm font-medium text-indigo-300 hover:text-indigo-200 hover:underline disabled:opacity-50"
              @click="showPassword2 = !showPassword2"
              :aria-pressed="showPassword2"
              :disabled="loading"
            >
              {{ showPassword2 ? 'Hide' : 'Show' }}
            </button>
          </div>
          <p v-if="password2 && password2 !== password" class="text-xs text-rose-300">
            Passwords do not match.
          </p>
        </div>
        <!-- Error -->
        <p
          v-if="errorMessage"
          class="rounded-lg border border-rose-800 bg-rose-900/30 px-3.5 py-2 text-sm text-rose-200"
          role="alert"
          aria-live="polite"
        >
          {{ errorMessage }}
        </p>

        <!-- Submit -->
        <button
          type="submit"
          :disabled="!canSubmit || loading"
          :aria-busy="loading"
          :class="formClass"
        >
          {{ loading ? 'Creating…' : 'Create account' }}
        </button>

        <p class="text-sm text-slate-400">
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
  import { ref, computed, onMounted } from 'vue';
  import { useRoute } from 'vue-router';
  import { register, type RegisterPayload, type RegisterResponse } from '@/api/auth';
  import { useAuthFields } from '@/composables/useAuthFields';

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

  // Register-specific local state
  const password2 = ref('');
  const showPassword2 = ref(false);
  const loading = ref(false);
  const errorMessage = ref<string | null>(null);
  const success = ref<RegisterResponse | null>(null);
  const emailEl = ref<HTMLInputElement | null>(null);
  const formClass: string = `
  w-full rounded-lg border border-slate-700 bg-slate-950
  px-3.5 py-2.5 text-slate-100 outline-none transition
  placeholder:text-slate-500
  focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/40
  disabled:opacity-60 disabled:cursor-not-allowed
`;
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
      const payload: RegisterPayload = { email: normalizedEmail.value, password: password.value };
      const registerResponse: RegisterResponse = await register(payload);
      success.value = registerResponse;
    } catch (err: any) {
      errorMessage.value =
        err?.response?.data?.message ??
        err?.response?.data?.error ??
        err?.message ??
        'Registration failed. Please try again.';
      password.value = '';
      password2.value = '';
    } finally {
      loading.value = false;
    }
  };
</script>
