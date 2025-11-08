// src/api/clients/contracts.ts

import type { Address } from '../common';

// Client is required to have Name or contact first
// TODO: List client-> many projects (maybe)
type ClientBase = {
  id: string;
  name: string | null;
  contactFirst: string | null;
  contactMiddle: string | null;
  contactLast: string | null;
  email: string | null;
  phone: string | null;
};

export type GetClientsRequest = {
  page: number;
  pageSize: number;
  nameFilter: string | null;
  isDeleted: boolean | null;
};

export type GetClientsResponse = {
  clientListItemResponses: ClientListItemResponses[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
};

export type ClientListItemResponses = {
  summary: ClientSummaryResponse;
  details: ClientResponse;
};

// This is for the non detail view in the table for an employee
export type ClientSummaryResponse = ClientBase;

// For expanded detail look on an employee
export type ClientResponse = ClientBase & {
  address: Address | null;
  note: string | null;
  CreatedAtUtc: string | null;
  UpdatedAtUtc: string | null;
  DeletedAtUtc: string | null;
  CreatedById: string | null;
  UpdatedById: string | null;
  DeletedById: string | null;
};
