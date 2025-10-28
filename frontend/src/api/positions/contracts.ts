// src/api/positions/contracts.ts

export type GetPositionsRequest = {
  page: number;
  pageSize: number;
};

export type GetPositionsResponse = {
  positions: PositionResponse[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
};

export type PositionResponse = {
  id: string;
  name: string;
  code: string | null;
  requiresLicense: boolean;
};
