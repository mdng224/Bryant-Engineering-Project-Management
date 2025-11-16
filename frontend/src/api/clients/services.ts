// src/api/clients/services.ts

import api from '..';
import type {
  AddClientRequest,
  AddClientResponse,
  GetClientsRequest,
  GetClientsResponse,
} from './contracts';
import { ClientsRoutes } from './routes';

const get = async (params: GetClientsRequest): Promise<GetClientsResponse> => {
  const { data } = await api.get<GetClientsResponse>(ClientsRoutes.list, { params });
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

export const clientService = { add, get, restore };
