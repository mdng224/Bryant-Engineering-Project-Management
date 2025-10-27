<!-- components/ViewEmployeeDialog.vue -->
<template>
  <Teleport to="body">
    <div v-if="open" class="fixed inset-0 z-[100]" role="dialog" aria-modal="true">
      <!-- Overlay (click to close if you want) -->
      <div class="absolute inset-0 bg-black/60 backdrop-blur-sm" @click="$emit('close')"></div>

      <!-- Centered panel -->
      <div class="absolute inset-0 flex items-center justify-center p-4">
        <div
          class="w-full max-w-lg rounded-xl border border-slate-700 bg-slate-900/95 p-6 text-slate-100 shadow-2xl"
          @keydown.esc="$emit('close')"
        >
          <div class="mb-4 flex items-center justify-between">
            <h2 class="text-lg font-semibold">Employee Details</h2>
            <button
              class="rounded-md px-2 py-1 text-sm text-slate-300 hover:bg-slate-700/70"
              @click="$emit('close')"
            >
              Close
            </button>
          </div>

          <!-- Details -->
          <div v-if="employee" class="space-y-6 text-sm">
            <!-- Header -->
            <div>
              <h3 class="text-base font-semibold">
                {{ employee.lastName }}, {{ employee.firstName }}
                <span v-if="employee.preferredName" class="ml-2 text-slate-400">
                  ({{ employee.preferredName }})
                </span>
              </h3>
              <p class="text-xs text-slate-400">
                Employee ID:
                <span class="font-mono">{{ employee.id }}</span>
              </p>
              <p class="text-xs text-slate-400">
                User ID:
                <span class="font-mono">{{ employee.userId }}</span>
              </p>
            </div>

            <!-- Property / value grid -->
            <dl class="grid grid-cols-1 gap-x-6 gap-y-3 sm:grid-cols-2">
              <div>
                <dt class="text-slate-400">Email</dt>
                <dd class="text-slate-100">{{ val(employee.companyEmail) }}</dd>
              </div>

              <div>
                <dt class="text-slate-400">Department</dt>
                <dd class="text-slate-100">{{ val(employee.department) }}</dd>
              </div>

              <div>
                <dt class="text-slate-400">Employment Type</dt>
                <dd class="text-slate-100">{{ val(employee.employmentType) }}</dd>
              </div>

              <div>
                <dt class="text-slate-400">Salary Type</dt>
                <dd class="text-slate-100">{{ val(employee.salaryType) }}</dd>
              </div>

              <div>
                <dt class="text-slate-400">Work Location</dt>
                <dd class="text-slate-100">{{ val(employee.workLocation) }}</dd>
              </div>

              <div>
                <dt class="text-slate-400">Recommended Role Id</dt>
                <dd class="font-mono">{{ val(employee.recommendedRoleId) }}</dd>
              </div>

              <div>
                <dt class="text-slate-400">Hire Date</dt>
                <dd class="text-slate-100">{{ fmt(employee.hireDate) }}</dd>
              </div>

              <div>
                <dt class="text-slate-400">End Date</dt>
                <dd class="text-slate-100">{{ fmt(employee.endDate) }}</dd>
              </div>

              <div>
                <dt class="text-slate-400">Created</dt>
                <dd class="text-slate-100">{{ fmt(employee.createdAtUtc) }}</dd>
              </div>

              <div>
                <dt class="text-slate-400">Updated</dt>
                <dd class="text-slate-100">{{ fmt(employee.updatedAtUtc) }}</dd>
              </div>

              <div>
                <dt class="text-slate-400">Deleted</dt>
                <dd class="text-slate-100">{{ fmt(employee.deletedAtUtc) }}</dd>
              </div>

              <div class="sm:col-span-2">
                <dt class="text-slate-400">License Notes</dt>
                <dd class="whitespace-pre-wrap text-slate-100">
                  {{ val(employee.licenseNotes) }}
                </dd>
              </div>

              <div class="sm:col-span-2">
                <dt class="text-slate-400">Notes</dt>
                <dd class="whitespace-pre-wrap text-slate-100">
                  {{ val(employee.notes) }}
                </dd>
              </div>
            </dl>
          </div>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
  import type { EmployeeResponse } from '@/api/employees/contracts';
  import { useDateFormat } from '@/composables/UseDateFormat';
  import { onBeforeUnmount, onMounted, watch } from 'vue';
  const props = defineProps<{
    open: boolean;
    employee: EmployeeResponse | null;
  }>();
  const emit = defineEmits<{ (e: 'close'): void }>();
  const { formatUtc } = useDateFormat();
  const val = (s: string | null | undefined) => s ?? '—';
  const fmt = (s: string | null | undefined) => (s ? formatUtc(s) : '—');
  console.log(props);
  /* ------------------------------ Escape key ------------------------------- */
  const handleKeydown = (e: KeyboardEvent): void => {
    if (e.key === 'Escape') emit('close');
  };

  onMounted(() => window.addEventListener('keydown', handleKeydown));
  onBeforeUnmount(() => window.removeEventListener('keydown', handleKeydown));
  watch(
    () => props.open,
    isOpen => {
      if (isOpen) window.addEventListener('keydown', handleKeydown);
      else window.removeEventListener('keydown', handleKeydown);
    },
    { immediate: true },
  );
</script>
