<!-- src/components/EditUserDialog.vue -->
<template>
  <div v-if="open && selectedUser" class="fixed inset-0 z-50 grid place-items-center">
    <!-- backdrop -->
    <div class="absolute inset-0 bg-black/60 backdrop-blur-sm" @click="$emit('close')"></div>

    <!-- dialog -->
    <section
      class="relative w-full max-w-md rounded-2xl border border-slate-700 bg-slate-900/90 p-5 text-slate-100 shadow-2xl"
      role="dialog"
      aria-modal="true"
      aria-labelledby="editUserTitle"
    >
      <div class="mb-4 flex items-center justify-between">
        <h2 class="text-lg font-semibold">Edit Position</h2>
        <button
          class="rounded-md px-2 py-1 text-sm text-slate-300 hover:bg-slate-700/70"
          @click="$emit('close')"
        >
          <X class="block h-5 w-5" />
        </button>
      </div>

      <header class="mb-3">
        <span class="truncate text-slate-400">{{ selectedUser?.email }}</span>
      </header>

      <div class="grid gap-4">
        <!-- ROLE -->
        <div class="pb-2">
          <label class="mb-1 block text-slate-400">Role</label>
          <select
            id="role"
            ref="firstField"
            v-model="form.roleName"
            :disabled="saving"
            class="w-full rounded-md border border-slate-700 bg-slate-900/70 px-3 py-2 pr-10 text-sm text-slate-100 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/30 disabled:opacity-60"
          >
            <option v-for="role in roles" :key="role" :value="role">{{ role }}</option>
          </select>
        </div>

        <!-- STATUS -->
        <div class="pb-2">
          <label for="status" class="mb-1 block text-slate-400">Status</label>
          <select
            id="status"
            v-model="form.status"
            :disabled="saving"
            class="w-full rounded-md border border-slate-700 bg-slate-900/70 px-3 py-2 pr-10 text-sm text-slate-100 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/30 disabled:opacity-60"
          >
            <option v-for="status in statuses" :key="status" :value="status">{{ status }}</option>
          </select>
        </div>
      </div>

      <footer class="mt-6 flex justify-end gap-2 pb-4">
        <button
          class="flex gap-2 rounded-md bg-indigo-600 px-3 py-2 text-white disabled:opacity-60"
          :disabled="saving || isNoop"
          @click="save"
        >
          <Save class="block h-4 w-4 shrink-0 self-center" />

          {{ saving ? 'Saving…' : 'Save' }}
        </button>
      </footer>

      <!-- Error -->
      <p
        v-if="errorMessage"
        class="flex items-center gap-2 rounded-lg border border-rose-800 bg-rose-900/30 px-3.5 py-2 text-sm leading-tight text-rose-200"
        role="alert"
        aria-live="assertive"
        tabindex="-1"
      >
        <AlertTriangle class="block h-4 w-4 shrink-0 self-center" aria-hidden="true" />
        <span>{{ errorMessage }}</span>
      </p>
    </section>
  </div>
</template>

<script setup lang="ts">
  import { extractApiError } from '@/api/error';
  import { userService } from '@/api/users';
  import type {
    RoleName,
    UpdateUserRequest,
    UserResponse,
    UserStatus,
  } from '@/api/users/contracts';
  import { AlertTriangle, Save, X } from 'lucide-vue-next';
  import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue';

  const roles: RoleName[] = ['Administrator', 'Manager', 'User'];
  const statuses: UserStatus[] = [
    'PendingEmail',
    'PendingApproval',
    'Active',
    'Denied',
    'Disabled',
  ];

  const props = defineProps<{
    open: boolean;
    selectedUser: UserResponse | null;
  }>();

  const emit = defineEmits<{
    (e: 'close'): void;
    (e: 'save'): void;
  }>();

  const errorMessage = ref<string | null>(null);
  const saving = ref(false);

  // Form state (typed)
  const form = ref<{ roleName: RoleName; status: UserStatus }>({
    roleName: 'User',
    status: 'PendingEmail',
  });

  // autofocus first field
  const firstField = ref<HTMLSelectElement | null>(null);
  onMounted(() => {
    if (props.open) firstField.value?.focus();
  });
  watch(
    () => props.open,
    isOpen => {
      if (isOpen) {
        errorMessage.value = null;
        // next tick not necessary here; simple focus retry
        setTimeout(() => firstField.value?.focus(), 0);
      }
    },
  );

  // hydrate form from prop
  watch(
    () => props.selectedUser,
    u => {
      if (!u) return;
      form.value = { roleName: u.roleName, status: u.status };
    },
    { immediate: true },
  );

  watch(
    () => props.open,
    isOpen => {
      if (isOpen) errorMessage.value = null; // reset error each time dialog opens
    },
  );
  // ESC to close (if not saving)
  const onKey = (e: KeyboardEvent) => {
    if (!props.open) return;
    if (e.key === 'Escape' && !saving.value) emit('close');
  };
  onMounted(() => window.addEventListener('keydown', onKey));
  onBeforeUnmount(() => window.removeEventListener('keydown', onKey));

  const isNoop = computed(
    () =>
      !props.selectedUser ||
      (form.value.roleName === props.selectedUser.roleName &&
        form.value.status === props.selectedUser.status),
  );

  const save = async (): Promise<void> => {
    if (!props.selectedUser || isNoop.value || saving.value) return;

    saving.value = true;
    errorMessage.value = null;

    try {
      const request: Partial<UpdateUserRequest> = {};
      if (form.value.roleName !== props.selectedUser.roleName)
        request.roleName = form.value.roleName;
      if (form.value.status !== props.selectedUser.status) request.status = form.value.status;

      await userService.update(props.selectedUser.id, request);
      emit('save');
      emit('close');
    } catch (e: unknown) {
      let msg = extractApiError(e, 'name');

      if (!msg || msg === 'An unexpected error occurred.') {
        msg = extractApiError(e, 'code');
      }

      errorMessage.value = msg;
    } finally {
      saving.value = false;
    }
  };
</script>
