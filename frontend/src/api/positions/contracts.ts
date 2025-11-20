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

export type ListPositionsRequest = {
  page: number;
  pageSize: number;
  nameFilter: string | null;
  isDeleted: boolean;
};

export type ListPositionsResponse = {
  positions: PositionRowResponse[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  deletedAtUtc: string | null;
};

// Response for each individual position (used in table)
export type PositionRowResponse = PositionBase & {
  id: string;
};

export type UpdatePositionRequest = PositionBase;
