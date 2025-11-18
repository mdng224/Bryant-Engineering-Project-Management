// src/composables/useClientLookups.ts
import {
  clientService,
  type ClientCategoryDto,
  type ClientTypeDto,
  type GetClientLookupsResponse,
} from '@/api/clients';
import { ref } from 'vue';

// module-level refs so all components share the same cache
const categories = ref<ClientCategoryDto[]>([]);
const types = ref<ClientTypeDto[]>([]);
const loaded = ref(false);
const loading = ref(false);

export const useClientLookups = () => {
  const loadLookups = async () => {
    if (loaded.value || loading.value) return;

    loading.value = true;
    try {
      const lookups: GetClientLookupsResponse = await clientService.getLookups();

      categories.value = lookups.categories;
      types.value = lookups.types;
      loaded.value = true;
    } finally {
      loading.value = false;
    }
  };

  const getTypesForCategory = (categoryId: string | null) =>
    categoryId ? types.value.filter(t => t.categoryId === categoryId) : types.value;

  return {
    categories,
    types,
    loaded,
    loading,
    loadLookups,
    getTypesForCategory,
  };
};
