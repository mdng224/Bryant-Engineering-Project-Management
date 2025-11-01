// src/api/positions/contracts.ts

// Shared base for all position shapes
type PositionBase = {
  name: string;
  code: string;
  requiresLicense: boolean;
};

export type AddPositionRequest = PositionBase;

export type AddPositionResponse = PositionBase & {
  id: string;
};

export type GetPositionsRequest = {
  page: number;
  pageSize: number;
  nameFilter?: string;
};

export type GetPositionsResponse = {
  positions: PositionResponse[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
};

// Response for each individual position (used in table)
export type PositionResponse = PositionBase & {
  id: string;
};

export type UpdatePositionRequest = PositionBase;
