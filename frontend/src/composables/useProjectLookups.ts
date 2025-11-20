// src/composables/useProjectLookups.ts
import { projectService, type ListProjectLookupsResponse } from '@/api/projects';
import { ref } from 'vue';

const managers = ref<string[]>([]);
const loaded = ref(false);
const loading = ref(false);

export const useProjectLookups = () => {
  const loadLookups = async (): Promise<void> => {
    if (loaded.value || loading.value) return;

    loading.value = true;
    try {
      const lookups: ListProjectLookupsResponse = await projectService.getLookups();

      managers.value = lookups.managers;
      loaded.value = true;
    } finally {
      loading.value = false;
    }
  };

  return {
    managers,
    loaded,
    loading,
    loadLookups,
  };
};
