// src/api/projects/contracts.ts

type ProjectBase = {
  id: string;
  firstName: string;
  lastName: string;
  preferredName?: string | null;
  employmentType?: string | null;
  hireDate: string | null;
  department: string | null;
};

export type GetProjectsRequest = {
  page: number;
  pageSize: number;
  name?: string;
  isDeleted?: boolean | null;
};

export type GetProjectsResponse = {
  projects: ProjectListItemResponse[];
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
export type ProjectSummaryResponse = ProjectBase & {
  isActive: boolean;
};

// For expanded detail look on an project
export type ProjectResponse = ProjectBase & {
  userId: string;
  salaryType: string | null;
  endDate: string | null;
  companyEmail: string | null;
  workLocation: string | null;
  licenseNotes: string | null;
  notes: string | null;
  recommendedRoleId: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
  deletedAtUtc: string | null;
};
