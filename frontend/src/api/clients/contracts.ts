// src/api/clients/contracts.ts

type ClientBase = {
  id: string;
  firstName: string;
  lastName: string;
  preferredName?: string | null;
  employmentType?: string | null;
  hireDate: string | null;
  department: string | null;
};

export type GetClientsRequest = {
  page: number;
  pageSize: number;
  name?: string;
};

export type GetClientsResponse = {
  clients: ClientListItem[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
};

export type ClientListItem = {
  summary: ClientSummaryResponse;
  details: ClientResponse;
};

// This is for the non detail view in the table for an employee
export type ClientSummaryResponse = ClientBase & {
  isActive: boolean;
};

// For expanded detail look on an employee
export type ClientResponse = ClientBase & {
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
