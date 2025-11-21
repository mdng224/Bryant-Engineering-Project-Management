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

export type GetEmployeeDetailsResponse = EmployeeBase & {
  userId: string | null;
  fullName: string; // computed backend-side from First/Last
  salaryType: string | null;
  endDate: string | null;
  companyEmail: string | null;
  workLocation: string | null;

  // Notes
  notes: string | null;

  // Address
  line1: string | null;
  line2: string | null;
  city: string | null;
  state: string | null;
  postalCode: string | null;

  // Role recommendation / preapproval
  recommendedRole: string | null;
  isPreapproved: boolean;

  // Audit
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
  department?: string | null;
  hireDate?: string | null;
  companyEmail: string;
  workLocation?: string | null;
  licenseNotes?: string | null;
  notes?: string | null;
};
