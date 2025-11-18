// src/api/projects/services.ts

import api from '..';
import type {
  GetProjectLookupsResponse,
  GetProjectsRequest,
  GetProjectsResponse,
} from './contracts';
import { ProjectsRoutes } from './routes';

/* ------------------------------ GET (list) ------------------------------ */
const get = async (params: GetProjectsRequest): Promise<GetProjectsResponse> => {
  const { data } = await api.get<GetProjectsResponse>(ProjectsRoutes.get, { params });
  return data;
};

/* ------------------------------ GET (lookups) ------------------------------ */
const getLookups = async (): Promise<GetProjectLookupsResponse> => {
  const { data } = await api.get<GetProjectLookupsResponse>(ProjectsRoutes.lookups);
  return data;
};

/* ------------------------------ POST (restore) ------------------------------ */
const restore = async (id: string): Promise<void> => {
  await api.post<string>(ProjectsRoutes.restore(id));
};

export const projectService = { get, getLookups, restore };
