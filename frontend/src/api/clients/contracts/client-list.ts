import type { Address } from '@/api/common';
import type { ClientBase } from './client-base';
import type { ClientSummaryResponse } from './client-summary';

export interface ClientListItemResponse {
  clientSummaryResponse: ClientSummaryResponse;
  clientDetailsResponse: ClientDetailsResponse;
}

export interface GetClientsRequest {
  page: number;
  pageSize: number;
  nameFilter: string | null;
  hasActiveProject: boolean;
  categoryId: string | null;
  typeId: string | null;
}

export interface GetClientsResponse {
  clientListItemResponses: ClientListItemResponse[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export type ClientDetailsResponse = ClientBase & {
  middleName: string | null;
  address: Address | null;
  note: string | null;

  createdAtUtc: string;
  updatedAtUtc: string;
  deletedAtUtc: string | null;

  createdById: string | null;
  updatedById: string | null;
  deletedById: string | null;
};
