// src/api/projects/services.ts

import api from '..';
import type {
  AddProjectRequest,
  ListProjectLookupsResponse,
  ListProjectsRequest,
  ListProjectsResponse,
} from './contracts';
import { ProjectsRoutes } from './routes';

/* ------------------------------ GET (list) ------------------------------ */
const get = async (params: ListProjectsRequest): Promise<ListProjectsResponse> => {
  const { data } = await api.get<ListProjectsResponse>(ProjectsRoutes.get, { params });
  return data;
};

/* ------------------------------ GET (lookups) ------------------------------ */
const getLookups = async (): Promise<ListProjectLookupsResponse> => {
  const { data } = await api.get<ListProjectLookupsResponse>(ProjectsRoutes.lookups);
  return data;
};

/* ------------------------------ POST (add) ------------------------------ */
const add = async (payload: AddProjectRequest): Promise<AddProjectRequest> => {
  const { data } = await api.post<AddProjectRequest>(ProjectsRoutes.add, payload);
  return data;
};

/* ------------------------------ POST (restore) ------------------------------ */
const restore = async (id: string): Promise<void> => {
  await api.post<string>(ProjectsRoutes.restore(id));
};

export const projectService = { add, get, getLookups, restore };
