// src/api/positions/services.ts

import type {
  AddPositionRequest,
  AddPositionResponse,
  GetPositionsRequest,
  GetPositionsResponse,
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
  console.log(payload);
  const { data } = await api.post<AddPositionResponse>(PositionsRoutes.add, payload);
  return data;
};

/* ----------------------------- Export Service ---------------------------- */
export const positionService = {
  get,
  add,
};
