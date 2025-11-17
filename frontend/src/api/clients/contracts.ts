// src/api/clients/contracts.ts

import type { Address } from '../common';

export interface ClientCategoryDto {
  id: string;
  name: string;
}

export interface ClientTypeDto {
  id: string;
  name: string;
  description: string;
  categoryId: string;
}

export interface GetClientLookupsResponse {
  categories: ClientCategoryDto[];
  types: ClientTypeDto[];
}

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

export type AddClientRequest = {
  name: string | null;
  namePrefix: string | null;
  firstName: string | null;
  lastName: string | null;
  nameSuffix: string | null;
  email: string | null;
  phone: string | null;
  address: Address | null;
  note: string | null;
};

export type AddClientResponse = {};

export type GetClientsRequest = {
  page: number;
  pageSize: number;
  nameFilter: string | null;
  hasActiveProject: boolean;
  categoryId: string | null;
  typeId: string | null;
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
