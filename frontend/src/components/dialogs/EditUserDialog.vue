<!-- src/components/EditUserDialog.vue -->
<template>
  <app-dialog
    :open="open && !!selectedUser"
    title="Edit User"
    width="max-w-md"
    :loading="saving"
    @close="handleClose"
  >
    <header class="mb-3">
      <span class="truncate text-slate-400">{{ selectedUser?.email }}</span>
    </header>

    <app-alert v-if="errorMessage" :message="errorMessage" variant="error" :icon="AlertTriangle" />

    <form id="edit-user-form" class="grid gap-4" @submit.prevent="save">
      <!-- ROLE -->
      <div class="pb-2">
        <label class="mb-1 block text-slate-400" for="role">Role</label>
        <select
          id="role"
          ref="firstField"
          v-model="form.roleName"
          :disabled="saving"
          class="w-full rounded-md border border-slate-700 bg-slate-900/70 px-3 py-2 pr-10 text-sm text-slate-100 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/30 disabled:opacity-60"
        >
          <option v-for="role in roles" :key="role" :value="role">
            {{ role }}
          </option>
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
          <option v-for="status in statuses" :key="status" :value="status">
            {{ status }}
          </option>
        </select>
      </div>
    </form>

    <template #footer>
      <button
        type="button"
        class="rounded-md px-3 py-2 text-sm text-slate-300 hover:bg-slate-700/70 disabled:opacity-60"
        :disabled="saving"
        @click="handleClose"
      >
        Cancel
      </button>

      <button
        type="submit"
        form="edit-user-form"
        class="flex gap-2 rounded-md bg-indigo-600 px-3 py-2 text-sm text-white hover:bg-indigo-500 disabled:opacity-60"
        :disabled="saving || isNoop"
      >
        <Save class="block h-4 w-4 shrink-0 self-center" />
        {{ saving ? 'Saving…' : 'Save' }}
      </button>
    </template>
  </app-dialog>
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
  import { AlertTriangle, Save } from 'lucide-vue-next';
  import { computed, onMounted, ref, watch } from 'vue';
  import AppAlert from '../AppAlert.vue';
  import AppDialog from '../ui/AppDialog.vue';

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

  const form = ref<{ roleName: RoleName; status: UserStatus }>({
    roleName: 'User',
    status: 'PendingEmail',
  });

  const firstField = ref<HTMLSelectElement | null>(null);

  /** Autofocus when dialog opens */
  onMounted(() => {
    if (props.open) firstField.value?.focus();
  });

  watch(
    () => props.open,
    isOpen => {
      if (isOpen) {
        errorMessage.value = null;
        setTimeout(() => firstField.value?.focus(), 0);
      }
    },
  );

  /** Hydrate form from selectedUser */
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
      if (isOpen) errorMessage.value = null;
    },
  );

  const isNoop = computed(
    () =>
      !props.selectedUser ||
      (form.value.roleName === props.selectedUser.roleName &&
        form.value.status === props.selectedUser.status),
  );

  /** Don’t allow closing while saving */
  function handleClose() {
    if (saving.value) return;
    emit('close');
  }

  const save = async (): Promise<void> => {
    if (!props.selectedUser || isNoop.value || saving.value) return;

    saving.value = true;
    errorMessage.value = null;

    try {
      const request: Partial<UpdateUserRequest> = {};

      if (form.value.roleName !== props.selectedUser.roleName) {
        request.roleName = form.value.roleName;
      }
      if (form.value.status !== props.selectedUser.status) {
        request.status = form.value.status;
      }

      await userService.update(props.selectedUser.id, request);
      emit('save');
      emit('close');
    } catch (e: unknown) {
      // Prefer field-level errors if your API supports them; otherwise fall back
      let msg = extractApiError(e, 'roleName');
      if (!msg || msg === 'An unexpected error occurred.') {
        msg = extractApiError(e, 'status');
      }
      errorMessage.value = msg;
    } finally {
      saving.value = false;
    }
  };
</script>
