// src/api/positions/services.ts

import type {
  AddPositionRequest,
  ListPositionsRequest,
  ListPositionsResponse,
  UpdatePositionRequest,
} from '@/api/positions/contracts';
import { PositionsRoutes } from '@/api/positions/routes';
import api from '..';

/* ------------------------------ GET (list) ------------------------------ */
const get = async (params: ListPositionsRequest): Promise<ListPositionsResponse> => {
  const { data } = await api.get<ListPositionsResponse>(PositionsRoutes.list, { params });
  return data;
};

/* ------------------------------ POST (add) ------------------------------ */
const add = async (payload: AddPositionRequest): Promise<string> => {
  const { data } = await api.post<string>(PositionsRoutes.add, payload);
  return data;
};

/* ------------------------------ DELETE (delete) ------------------------------ */
const deletePosition = async (id: string): Promise<void> => {
  await api.delete<string>(PositionsRoutes.delete(id));
};

/* ------------------------------ POST (restore) ------------------------------ */
const restore = async (id: string): Promise<void> => {
  await api.post(PositionsRoutes.restore(id));
};

/* ------------------------------ PATCH (update) ------------------------------ */
export async function update(id: string, payload: UpdatePositionRequest): Promise<void> {
  await api.patch(PositionsRoutes.update(id), payload);
}

/* ----------------------------- Export Service ---------------------------- */
export const positionService = {
  add,
  deletePosition,
  get,
  restore,
  update,
};
