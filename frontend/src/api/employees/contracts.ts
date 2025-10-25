// src/api/employees/contracts.ts

type EmployeeBase = {
  id: string;
  firstName: string;
  lastName: string;
  preferredName?: string | null;
  employmentType?: string | null;
  hireDate: string | null;
  department: string | null;
};

export type GetEmployeesRequest = {
  page: number;
  pageSize: number;
  name?: string;
};

export type GetEmployeesResponse = {
  employees: EmployeeListItem[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
};

export type EmployeeListItem = {
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
