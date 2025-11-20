import type { ClientBase } from './client-base';
import type { ClientRowResponse } from './client-summary';

export interface ListClientsRequest {
  page: number;
  pageSize: number;
  nameFilter: string | null;
  hasActiveProject: boolean;
  categoryId: string | null;
  typeId: string | null;
}

export interface ListClientsResponse {
  clients: ClientRowResponse[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export type GetClientDetailsResponse = ClientBase & {
  createdAtUtc: string;
  updatedAtUtc: string;
  deletedAtUtc: string | null;
  createdById: string | null;
  updatedById: string | null;
  deletedById: string | null;
};
