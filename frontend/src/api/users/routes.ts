// src/api/users/routes.ts

export const UsersRoutes = {
  delete: (id: string) => `/users/${id}`,
  list: '/users',
  byId: (id: string) => `/users/${id}`,
  restore: (id: string) => `/users/${id}/restore`, // POST - restore user
} as const;
