// src/api/positions/services.ts

import type { GetPositionsRequest, GetPositionsResponse } from '@/api/positions/contracts';
import { PositionsRoutes } from '@/api/positions/routes';
import api from '..';

const get = async (params: GetPositionsRequest): Promise<GetPositionsResponse> => {
  console.log(params);
  const { data } = await api.get<GetPositionsResponse>(PositionsRoutes.list, { params });
  console.log(data);
  return data;
};

export const positionService = { get };
