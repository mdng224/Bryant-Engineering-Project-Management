// src/api/projects/services.ts

import api from '..';
import type {
  AddProjectRequest,
  GetProjectDetailsResponse,
  ListProjectLookupsResponse,
  ListProjectsRequest,
  ListProjectsResponse,
} from './contracts';
import { ProjectsRoutes } from './routes';

const getDetails = async (id: string): Promise<GetProjectDetailsResponse> => {
  const { data } = await api.get<GetProjectDetailsResponse>(ProjectsRoutes.getDetails(id));
  return data;
};

/* ------------------------------ GET (list) ------------------------------ */
const list = async (params: ListProjectsRequest): Promise<ListProjectsResponse> => {
  const { data } = await api.get<ListProjectsResponse>(ProjectsRoutes.list, { params });
  return data;
};

/* ------------------------------ GET (lookups) ------------------------------ */
const getLookups = async (): Promise<ListProjectLookupsResponse> => {
  const { data } = await api.get<ListProjectLookupsResponse>(ProjectsRoutes.lookups);
  return data;
};

/* ------------------------------ POST (add) ------------------------------ */
const add = async (payload: AddProjectRequest): Promise<string> => {
  const { data } = await api.post<string>(ProjectsRoutes.add, payload);
  return data;
};

/* ------------------------------ POST (restore) ------------------------------ */
const restore = async (id: string): Promise<void> => {
  await api.post<string>(ProjectsRoutes.restore(id));
};

export const projectService = { add, getDetails, list, getLookups, restore };
