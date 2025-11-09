// src/api/projects/contracts.ts

import type { Address } from '../common';

type ProjectBase = {
  id: string;
  clientName: string;
  name: string;
  newCode: string;
  scope: string | null;
  manager: string | null;
  type: string | null;
};

export type GetProjectsRequest = {
  page: number;
  pageSize: number;
  nameFilter: string | null;
  isDeleted: boolean | null;
};

export type GetProjectsResponse = {
  projectListItemResponses: ProjectListItemResponse[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
};

export type ProjectListItemResponse = {
  summary: ProjectSummaryResponse;
  details: ProjectResponse;
};

// This is for the non detail view in the table for an project
export type ProjectSummaryResponse = ProjectBase;

// For expanded detail look on an project
export type ProjectResponse = ProjectBase & {
  clientId: string;
  address: Address | null;
  code: string | null;
  year: number;
  number: number;
  CreatedAtUtc: string | null;
  UpdatedAtUtc: string | null;
  DeletedAtUtc: string | null;
  CreatedById: string | null;
  UpdatedById: string | null;
  DeletedById: string | null;
};
