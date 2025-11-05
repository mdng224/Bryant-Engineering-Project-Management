// src/api/users/routes.ts

export const UsersRoutes = {
  delete: (id: string) => `/users/${id}`,
  list: '/users',
  byId: (id: string) => `/users/${id}`,
} as const;
