<!-- src/components/EditUserDialog.vue -->
<template>
  <div v-if="open && user" class="fixed inset-0 z-50 grid place-items-center">
    <div class="absolute inset-0 bg-black/60 backdrop-blur-sm" @click="$emit('close')"></div>

    <section
      class="relative w-full max-w-md rounded-2xl border border-slate-700 bg-slate-900/90 p-5 text-slate-100 shadow-2xl"
      role="dialog"
      aria-modal="true"
      aria-labelledby="editUserTitle"
    >
      <header class="mb-3">
        <h3 id="editUserTitle" class="pb-2 text-center text-lg font-semibold">Edit user</h3>
        <span class="truncate text-slate-400">{{ user?.email }}</span>
      </header>

      <div class="grid gap-4">
        <!-- ROLE -->
        <div class="pb-2">
          <label class="mb-1 block text-slate-400">Role</label>
          <select
            v-model="form.roleName"
            :disabled="saving"
            class="w-full appearance-none rounded-md border border-slate-700 bg-slate-900/70 px-3 py-2 pr-10 text-sm text-slate-100 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/30 disabled:opacity-60"
          >
            <option v-for="r in roles" :key="r" :value="r">{{ r }}</option>
          </select>
          <ChevronDown
            class="pointer-events-none absolute right-7 top-[48%] h-[18px] w-[18px] -translate-y-[45%] text-slate-400"
          />
        </div>

        <!-- STATUS -->
        <div class="flex items-center gap-3">
          <label class="text-slate-400">Status</label>
          <input
            class="h-4 w-4 rounded-sm border-gray-300 bg-gray-100 text-blue-600 focus:ring-2 focus:ring-blue-500 dark:border-gray-600 dark:bg-gray-700 dark:ring-offset-gray-800 dark:focus:ring-blue-600"
            type="checkbox"
            v-model="form.isActive"
            :disabled="saving"
          />
          <span class="text-sm" :class="form.isActive ? 'text-emerald-300' : 'text-slate-300'">
            {{ form.isActive ? 'Active' : 'Inactive' }}
          </span>
        </div>
      </div>

      <footer class="mt-6 flex justify-end gap-2">
        <button class="rounded-md border border-slate-700 px-3 py-1.5" @click="$emit('close')">
          Cancel
        </button>
        <button
          class="rounded-md bg-indigo-600 px-3 py-1.5 text-white disabled:opacity-60"
          :disabled="saving || isNoop"
          @click="save"
        >
          {{ saving ? 'Saving…' : 'Save' }}
        </button>
      </footer>
    </section>
  </div>
</template>

<script setup lang="ts">
  import { updateUser } from '@/api/admin/users';
  import type { UserResponse } from '@/types';
  import { ChevronDown } from 'lucide-vue-next';
  import { computed, ref, watch } from 'vue';

  const props = defineProps<{
    open: boolean;
    user: UserResponse | null;
    roles?: string[];
  }>();

  const emit = defineEmits<{
    (e: 'close'): void;
    (e: 'saved', user: UserResponse): void;
  }>();

  const roles = computed(() => props.roles ?? ['User', 'Manager', 'Administrator']);

  const form = ref({ roleName: '', isActive: false });
  const saving = ref(false);

  watch(
    () => props.user,
    u => {
      if (!u) return;
      form.value = { roleName: u.roleName, isActive: u.isActive };
    },
    { immediate: true },
  );

  const isNoop = computed(
    () =>
      !props.user ||
      (form.value.roleName === props.user.roleName && form.value.isActive === props.user.isActive),
  );

  async function save() {
    if (!props.user || isNoop.value) {
      emit('close');
      return;
    }
    saving.value = true;
    try {
      const payload: any = {};
      if (form.value.roleName !== props.user.roleName) payload.roleName = form.value.roleName;
      if (form.value.isActive !== props.user.isActive) payload.isActive = form.value.isActive;

      const updated = await updateUser(props.user.id, payload);
      // Expect backend to return updated user with updatedAtUtc set
      emit('saved', { ...props.user, ...updated });
      emit('close');
    } catch (e) {
      // TODO: toast the error if you have a toaster
      console.error(e);
    } finally {
      saving.value = false;
    }
  }
</script>
