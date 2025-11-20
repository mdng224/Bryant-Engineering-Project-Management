// src/api/auth/contracts.ts

export type LoginRequest = {
  email: string;
  password: string;
};

export type LoginResponse = {
  token: string;
  expiresAtUtc: string;
};

export type GetMeResponse = {
  sub: string;
  email?: string;
};

export type RegisterRequest = {
  email: string;
  password: string;
};

export type RegisterResponse = {
  userId: string;
  status: string;
  message: string;
};
