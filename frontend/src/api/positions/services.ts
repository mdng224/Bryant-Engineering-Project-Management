// src/api/positions/services.ts

import type {
  AddPositionRequest,
  AddPositionResponse,
  GetPositionsRequest,
  GetPositionsResponse,
  UpdatePositionRequest,
} from '@/api/positions/contracts';
import { PositionsRoutes } from '@/api/positions/routes';
import api from '..';

/* ------------------------------ GET (list) ------------------------------ */
const get = async (params: GetPositionsRequest): Promise<GetPositionsResponse> => {
  const { data } = await api.get<GetPositionsResponse>(PositionsRoutes.list, { params });
  return data;
};

/* ------------------------------ POST (add) ------------------------------ */
const add = async (payload: AddPositionRequest): Promise<AddPositionResponse> => {
  const { data } = await api.post<AddPositionResponse>(PositionsRoutes.add, payload);
  return data;
};

/* ------------------------------ POST (delete) ------------------------------ */
const deletePosition = async (id: string): Promise<void> => {
  await api.delete<string>(PositionsRoutes.delete(id));
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
  update,
};
