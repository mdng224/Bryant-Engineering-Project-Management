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
  deletedAtUtc: string;
};

export type ListEmployeesRequest = {
  page: number;
  pageSize: number;
  nameFilter: string | null;
  isDeleted?: boolean | null;
};

export type ListEmployeesResponse = {
  employees: EmployeeRowResponse[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
};

export type EmployeeRowResponse = EmployeeBase;

// For expanded detail look on an employee
export type GetEmployeeDetailsResponse = EmployeeBase & {
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

export type AddEmployeeRequest = {
  firstName: string;
  lastName: string;
  preferredName?: string | null;
  employmentType?: string | null;
  salaryType?: string | null;
  hireDate?: string | null;
  endDate?: string | null;
  department?: string | null;
  companyEmail?: string | null;
  workLocation?: string | null;
  licenseNotes?: string | null;
  notes?: string | null;
};
