// src/types/api.ts

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
  message?: string;
  error?: string;
  errors?: Record<string, string[] | string>;
};
