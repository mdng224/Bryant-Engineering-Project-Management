// src/api/auth/routes.ts

export const AuthRoutes = Object.freeze({
  login: '/auth/login',
  me: '/auth/me',
  register: `/auth/register`,
} as const);
