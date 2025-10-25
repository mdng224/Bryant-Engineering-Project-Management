// src/api/users/routes.ts

export const UsersRoutes = {
  list: '/users',
  byId: (id: string) => `/users/${id}`,
} as const;
