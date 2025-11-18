// src/api/projects/contracts.ts

type ProjectBase = {
  code: string;
  id: string;
  name: string;
  clientName: string;
  scopeName: string;
  manager: string;
  type: string;
  location: string;
  deletedAtUtc: string | null;
};

export type GetProjectsRequest = {
  page: number;
  pageSize: number;
  nameFilter: string | null;
  isDeleted: boolean | null;
  clientId: string | null;
  manager: string | null;
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
  scopeId: string;
  year: number;
  number: number;
  createdAtUtc: string | null;
  updatedAtUtc: string | null;
  createdById: string | null;
  updatedById: string | null;
  deletedBy: string | null;
};

export type GetProjectLookupsResponse = {
  managers: string[];
};
