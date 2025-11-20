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

export type ListProjectsRequest = {
  page: number;
  pageSize: number;
  nameFilter: string | null;
  isDeleted: boolean | null;
  clientId: string | null;
  manager: string | null;
};

export type ListProjectsResponse = {
  projects: ProjectRowResponse[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
};

export type ProjectRowResponse = ProjectBase;

// For expanded detail look on an project
export type GetProjectDetailsResponse = ProjectBase & {
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

export type ListProjectLookupsResponse = {
  managers: string[];
};

export type AddProjectRequest = {
  code: string;
  name: string;
  clientId: string | null;
  scopeId: string | null;
  manager: string;
  type: string | null;
  location: string | null;
  year: number | null;
  number: number | null;
};
