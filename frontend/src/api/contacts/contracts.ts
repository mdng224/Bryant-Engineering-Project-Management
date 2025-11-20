// src/api/contacts/contracts.ts

// --- Shared base for row & details (minimal table view) ---
export type ContactRowResponse = {
  id: string;
  clientId: string | null;

  firstName: string;
  lastName: string;

  company: string | null;
  jobTitle: string | null;

  email: string | null;
  businessPhone: string | null;

  isPrimaryForClient: boolean;

  deletedAtUtc: string | null;
};

// --- List / paging ---

export type ListContactsRequest = {
  page: number;
  pageSize: number;
  nameFilter: string | null;
  isDeleted?: boolean | null;
};

export type ListContactsResponse = {
  contacts: ContactRowResponse[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
};

// --- Details response (matches C# GetContactDetailsResponse) ---

export type GetContactDetailsResponse = {
  id: string;

  // Client
  clientId: string | null;

  // Person name
  namePrefix: string | null;
  firstName: string;
  middleName: string | null;
  lastName: string;
  nameSuffix: string | null;

  // Company / Role
  company: string | null;
  department: string | null;
  jobTitle: string | null;

  // Business Address
  addressLine1: string | null;
  addressLine2: string | null;
  addressCity: string | null;
  addressState: string | null;
  addressPostalCode: string | null;
  country: string | null;

  // Phones
  businessPhone: string | null;
  mobilePhone: string | null;
  primaryPhone: string; // PrimaryPhoneKind enum as string from backend

  // Contact info
  email: string | null;
  webPage: string | null;

  // Other
  isPrimaryForClient: boolean;

  // Audit
  createdAtUtc: string;
  updatedAtUtc: string;
  deletedAtUtc: string | null;
  createdById: string | null;
  updatedById: string | null;
  deletedById: string | null;
};

// --- Create / edit ---

export type AddContactRequest = {
  clientId?: string | null;

  // Person name
  namePrefix?: string | null;
  firstName: string;
  middleName?: string | null;
  lastName: string;
  nameSuffix?: string | null;

  // Company / Role
  company?: string | null;
  department?: string | null;
  jobTitle?: string | null;

  // Business address
  addressLine1?: string | null;
  addressLine2?: string | null;
  addressCity?: string | null;
  addressState?: string | null;
  addressPostalCode?: string | null;
  country?: string | null;

  // Phones
  businessPhone?: string | null;
  mobilePhone?: string | null;
  primaryPhone?: string | null; // "Business" | "Mobile" | "Unknown", etc.

  // Contact info
  email?: string | null;
  webPage?: string | null;

  // Other
  isPrimaryForClient?: boolean;
};
