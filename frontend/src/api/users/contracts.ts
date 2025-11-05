// src/types/users/contracts.ts

export type RoleName = 'Administrator' | 'Manager' | 'User';
export type UserStatus = 'PendingEmail' | 'PendingApproval' | 'Active' | 'Denied' | 'Disabled';

export type GetUsersRequest = {
  page: number;
  pageSize: number;
  email?: string;
  isDeleted?: boolean | null;
};

export type GetUsersResponse = {
  users: UserResponse[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
};

export type UpdateUserRequest = {
  roleName?: RoleName;
  status?: UserStatus;
};

// Each row in the table
export type UserResponse = {
  id: string;
  email: string;
  roleName: RoleName;
  status: UserStatus;
  createdAtUtc: string;
  updatedAtUtc: string;
  deletedAtUtc: string | null;
};
