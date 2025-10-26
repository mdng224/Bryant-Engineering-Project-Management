// src/types/users/contracts.ts

export type GetUsersRequest = {
  page: number;
  pageSize: number;
  email?: string;
};

export type GetUsersResponse = {
  users: UserResponse[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
};

export type UpdateUserRequest = {
  roleName?: string;
  isActive?: boolean;
};

// Each row in the table
export type UserResponse = {
  id: string;
  email: string;
  roleName: string;
  status: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
  deletedAtUtc: string | null;
};
