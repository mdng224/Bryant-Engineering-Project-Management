// src/api/clients/services.ts

import api from '..';
import type { GetClientsRequest, GetClientsResponse } from './contracts';
import { ClientsRoutes } from './routes';

const get = async (params: GetClientsRequest): Promise<GetClientsResponse> => {
  const { data } = await api.get<GetClientsResponse>(ClientsRoutes.list, { params });
  return data;
};

export const clientService = { get };
