// src/types/api.ts

// --- Users ---

export type GetUsersRequest = {
  page: number;
  pageSize: number;
};

export type GetUsersResponse = {
  users: UserResponse[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
};

export type UserResponse = {
  id: string;
  email: string;
  roleName: string;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
  deletedAtUtc: string | null;
};

/* ----------------------------- Auth Types ----------------------------- */
export type LoginPayload = {
  email: string;
  password: string;
};

export type LoginResponse = {
  token: string;
  expiresAtUtc: string;
};

export type MeResponse = {
  sub: string;
  email?: string;
};

export type RegisterPayload = {
  email: string;
  password: string;
};

export type RegisterResponse = {
  userId: string;
  status: string;
  message: string;
};

/* ----------------------------- Error Types ---------------------------- */
export type ApiErrorData = {
  code?: string;
  message?: string; // for non-ProblemDetails fallbacks
  detail?: string; // ProblemDetails.detail
  error?: string; // optional legacy shape some libs use
  errors?: Record<string, string[] | string>;
};
