// src/api/clients/services.ts

import api from '..';
import type {
  AddClientRequest,
  AddClientResponse,
  GetClientDetailsResponse,
  ListClientLookupsResponse,
  ListClientsRequest,
  ListClientsResponse,
} from './';
import { ClientsRoutes } from './routes';

const getDetails = async (id: string): Promise<GetClientDetailsResponse> => {
  const { data } = await api.get<GetClientDetailsResponse>(ClientsRoutes.getDetails(id));
  return data;
};

/* ------------------------------ GET (list) ------------------------------ */
const list = async (params: ListClientsRequest): Promise<ListClientsResponse> => {
  const { data } = await api.get<ListClientsResponse>(ClientsRoutes.list, { params });
  return data;
};

/* ------------------------------ GET (lookups) ------------------------------ */
const getLookups = async (): Promise<ListClientLookupsResponse> => {
  const { data } = await api.get<ListClientLookupsResponse>(ClientsRoutes.lookups);
  return data;
};

/* ------------------------------ POST (add) ------------------------------ */
const add = async (payload: AddClientRequest): Promise<AddClientResponse> => {
  const { data } = await api.post<AddClientResponse>(ClientsRoutes.add, payload);
  return data;
};

/* ------------------------------ POST (restore) ------------------------------ */
const restore = async (id: string): Promise<void> => {
  await api.post<string>(ClientsRoutes.restore(id));
};

export const clientService = { add, getDetails, list, getLookups, restore };
