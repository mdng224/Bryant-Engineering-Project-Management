<template>
  <h2 class="pb-4 text-xl font-semibold text-slate-100">Contacts</h2>

  <div class="flex items-center justify-between pb-4">
    <div class="flex gap-4">
      <table-search v-model="nameFilter" placeholder="Search contact name..." @commit="commit" />
      <boolean-filter v-model="hasActiveContact" :options="activeOptions" />
    </div>

    <button
      class="flex items-center gap-2 rounded-md bg-indigo-600 px-3 py-2 text-sm font-medium text-white transition hover:bg-indigo-500"
      @click="addDialogIsOpen = true"
    >
      <circle-plus class="block h-4 w-4 shrink-0 self-center" aria-hidden="true" />
      <span class="text-white">Add Contact</span>
    </button>
  </div>

  <app-alert v-if="errorMessage" :message="errorMessage" variant="error" :icon="AlertTriangle" />

  <data-table :table :loading :total-count empty-text="No contacts found.">
    <!-- actions slot for this table only -->
    <template #cell="{ cell }">
      <template v-if="(cell.column.columnDef.meta as any)?.kind === 'actions'">
        <span class="flex gap-2">
          <!-- View button -->
          <button
            :class="actionButtonClass"
            aria-label="View contact"
            @click="handleOpenContactDetails(cell.row.original.id as string)"
          >
            <eye class="h-4 w-4" />
          </button>
        </span>
      </template>
      <cell-renderer :cell />
    </template>
  </data-table>

  <table-footer :table :totalCount :totalPages :pagination :setPageSize />

  <!-- Dialogs -->

  <details-dialog
    :open="openDetailsDialog"
    title="Contact Details"
    :item="selectedContact"
    :fields
    :format-utc
    @close="openDetailsDialog = false"
  />
  <!--
  <EditContactDialog
    :open="editContactDialogIsOpen"
    :selected-contact
    @close="editContactDialogIsOpen = false"
  />-->
</template>

<script setup lang="ts">
  import {
    type ContactRowResponse,
    type GetContactDetailsResponse,
    type ListContactsRequest,
    type ListContactsResponse,
  } from '@/api/contacts/contracts';
  import { contactService } from '@/api/contacts/services';
  import { extractApiError } from '@/api/error';
  import DetailsDialog, { type FieldDef } from '@/components/dialogs/shared/DetailsDialog.vue';
  import { CellRenderer, DataTable, TableFooter, TableSearch } from '@/components/table';
  import { AppAlert } from '@/components/ui';
  import BooleanFilter from '@/components/ui/BooleanFilter.vue';
  import { useDataTable, type FetchParams } from '@/composables/useDataTable';
  import { useDateFormat } from '@/composables/UseDateFormat';
  import { useDebouncedRef } from '@/composables/useDebouncedRef';
  import { createColumnHelper, type ColumnDef, type ColumnHelper } from '@tanstack/vue-table';
  import { AlertTriangle, CheckCircle2, CirclePlus, Eye, Lock } from 'lucide-vue-next';
  import { computed, onBeforeUnmount, ref, watch } from 'vue';

  const errorMessage = ref<string | null>(null);

  /* ------------------------------- Constants ------------------------------ */
  // Buttons
  const actionButtonClass =
    'rounded-md bg-indigo-600 p-1.5 text-white transition hover:bg-indigo-500';

  /* ------------------------------- Details ------------------------------ */
  const fieldDef = <K extends keyof GetContactDetailsResponse>(
    key: K,
    label: string,
    type: 'text' | 'phone' | 'date' | 'mono' | 'multiline' = 'text',
    span: 1 | 2 = 1,
  ) => ({ key: key as string, label, type, span }) satisfies FieldDef;

  const { formatUtc } = useDateFormat();

  const fields: FieldDef[] = [
    // --- Person name ---
    fieldDef('namePrefix', 'Title'),
    fieldDef('firstName', 'First Name'),
    fieldDef('middleName', 'Middle Name'),
    fieldDef('lastName', 'Last Name'),
    fieldDef('nameSuffix', 'Suffix'),

    // --- Company / Role ---
    fieldDef('company', 'Company'),
    fieldDef('department', 'Department'),
    fieldDef('jobTitle', 'Job Title'),

    // --- Contact info ---
    fieldDef('email', 'Email'),
    fieldDef('webPage', 'Web Page'),

    // --- Phones ---
    fieldDef('businessPhone', 'Business Phone', 'phone'),
    fieldDef('mobilePhone', 'Mobile Phone', 'phone'),
    fieldDef('primaryPhone', 'Primary Phone', 'phone'),

    // --- Address (flat from backend) ---
    fieldDef('addressLine1', 'Address Line 1'),
    fieldDef('addressLine2', 'Address Line 2'),
    fieldDef('addressCity', 'City'),
    fieldDef('addressState', 'State'),
    fieldDef('addressPostalCode', 'ZIP'),
    fieldDef('country', 'Country'),

    // --- Other ---
    fieldDef('isPrimaryForClient', 'Primary For Client'),

    // --- Audit ---
    fieldDef('createdAtUtc', 'Created', 'date'),
    fieldDef('createdById', 'Created By', 'mono'),
    fieldDef('updatedAtUtc', 'Updated', 'date'),
    fieldDef('updatedById', 'Updated By', 'mono'),
    fieldDef('deletedAtUtc', 'Deleted', 'date'),
    fieldDef('deletedById', 'Deleted By', 'mono'),
  ] as any;

  /* -------------------------------- Columns ------------------------------- */
  const col: ColumnHelper<ContactRowResponse> = createColumnHelper<ContactRowResponse>();
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const columns: ColumnDef<ContactRowResponse, any>[] = [
    // Computed contact name: "Last, First"
    col.accessor(row => `${row.lastName}, ${row.firstName}`, {
      id: 'name',
      header: 'Contact Name',
      meta: { kind: 'text' as const },
    }),

    col.accessor('company', {
      header: 'Company',
      meta: { kind: 'text' as const },
    }),

    col.accessor('jobTitle', {
      header: 'Job Title',
      meta: { kind: 'text' as const },
    }),

    col.accessor('email', {
      header: 'Email',
      meta: { kind: 'text' as const },
    }),

    col.accessor('businessPhone', {
      header: 'Phone',
      meta: { kind: 'phone' as const },
    }),

    col.accessor('isPrimaryForClient', {
      header: 'Primary?',
      meta: { kind: 'text' as const },
    }),

    {
      id: 'actions',
      header: 'Actions',
      meta: { kind: 'actions' as const },
      enableSorting: false,
    },
  ];

  /* ------------------------------- Filtering ------------------------------ */
  const hasActiveContact = ref<boolean>(true); // default Active
  const selectedCategoryId = ref<string | null>(null);
  const selectedTypeId = ref<string | null>(null);

  const activeOptions = [
    { value: true, label: 'Active', icon: CheckCircle2, color: 'text-emerald-400' },
    { value: false, label: 'Inactive', icon: Lock, color: 'text-rose-400' },
  ];

  const {
    input: nameFilter, // bound to v-model
    debounced: name, // used in fetch
    setNow: commit, // call on Enter
    cancel: cancelNameDebounce, // cleanup on unmount
  } = useDebouncedRef<string | null>(null, 500);

  onBeforeUnmount(() => {
    cancelNameDebounce();
    destroy();
  });

  watch(selectedCategoryId, () => {
    selectedTypeId.value = null;
  });

  /* ------------------------------- Fetching ------------------------------- */
  type ContactQuery = {
    name: string | null;
    isDeleted: boolean | null;
  };

  const isDeletedFilter = ref<boolean | null>(false);
  const contactDetails = ref<GetContactDetailsResponse[]>([]);
  const contactDetailsById = computed(() => {
    const map = new Map<string, GetContactDetailsResponse>();
    for (const item of contactDetails.value) {
      map.set(item.id, item);
    }
    return map;
  });

  const fetchContacts = async ({ page, pageSize, query }: FetchParams<ContactQuery>) => {
    const request: ListContactsRequest = {
      page,
      pageSize,
      nameFilter: query?.name ?? null,
      isDeleted: query?.isDeleted ?? null,
    };
    try {
      const response: ListContactsResponse = await contactService.list(request);

      return {
        items: response.contacts,
        totalCount: response.totalCount,
        totalPages: response.totalPages,
        page: response.page,
        pageSize: response.pageSize,
      };
    } catch (err: unknown) {
      const msg: string = extractApiError(err, 'contact');
      errorMessage.value = msg;

      return {
        items: [],
        totalCount: 0,
        totalPages: 0,
        page,
        pageSize,
      };
    }
  };

  const query = computed<ContactQuery>(() => ({
    name: name.value ?? null,
    isDeleted: isDeletedFilter.value,
  }));

  const {
    table,
    loading,
    totalCount,
    totalPages,
    pagination,
    setPageSize,
    fetchNow: refetch,
    destroy,
  } = useDataTable<ContactRowResponse, ContactQuery>(columns, fetchContacts, query);

  /* -------------------------------- Handlers ------------------------------ */
  const addDialogIsOpen = ref(false);
  const editContactDialogIsOpen = ref(false);
  const openDetailsDialog = ref(false);
  const selectedContact = ref<GetContactDetailsResponse | null>(null);

  const handleOpenContactDetails = async (id: string): Promise<void> => {
    errorMessage.value = null;

    if (selectedContact.value?.id === id) {
      openDetailsDialog.value = true;
      return;
    }

    try {
      const response: GetContactDetailsResponse = await contactService.getDetails(id);
      selectedContact.value = response;
      openDetailsDialog.value = Boolean(response);
    } catch (err: unknown) {
      const msg: string = extractApiError(err, 'contact');
      errorMessage.value = msg;
    }
  };

  const handleEditContact = (summary: ContactRowResponse): void => {
    const detail = contactDetailsById.value.get(summary.id) ?? null;
    selectedContact.value = detail;
    editContactDialogIsOpen.value = !!detail;
  };
</script>
