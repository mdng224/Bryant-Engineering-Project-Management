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
  code?: string;
  message?: string; // for non-ProblemDetails fallbacks
  detail?: string; // ProblemDetails.detail
  error?: string; // optional legacy shape some libs use
  errors?: Record<string, string[] | string>;
};
