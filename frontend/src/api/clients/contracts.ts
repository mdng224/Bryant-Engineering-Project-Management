// src/api/clients/contracts.ts

import type { Address } from '../common';

// Client is required to have Name or contact first
type ClientBase = {
  id: string;
  name: string | null;
  totalActiveProjects: number;
  totalProjects: number;
  firstName: string | null;
  lastName: string | null;
  email: string | null;
  phone: string | null;
};

export type GetClientsRequest = {
  page: number;
  pageSize: number;
  nameFilter: string | null;
  hasActiveProject: boolean;
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
  createdAtUtc: string;
  updatedAtUtc: string;
  deletedAtUtc: string | null;
  createdById: string | null;
  updatedById: string | null;
  deletedById: string | null;
};
