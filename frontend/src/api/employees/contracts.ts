// src/api/employees/contracts.ts

type EmployeeBase = {
  id: string;
  firstName: string;
  lastName: string;
  preferredName?: string | null;
  employmentType?: string | null;
  hireDate: string | null;
  department: string | null;
  positionNames: string[];
};

export type GetEmployeesRequest = {
  page: number;
  pageSize: number;
  nameFilter: string | null;
  isDeleted?: boolean | null;
};

export type GetEmployeesResponse = {
  employees: EmployeeListItemResponse[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
};

export type EmployeeListItemResponse = {
  summary: EmployeeSummaryResponse;
  details: EmployeeResponse;
};

// This is for the non detail view in the table for an employee
export type EmployeeSummaryResponse = EmployeeBase & {
  isActive: boolean;
};

// For expanded detail look on an employee
export type EmployeeResponse = EmployeeBase & {
  userId: string;
  fullName: string;
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
  createdById: string | null;
  updatedById: string | null;
  deletedById: string | null;
};
