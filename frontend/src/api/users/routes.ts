// src/api/users/routes.ts

export const UsersRoutes = Object.freeze({
  delete: (id: string) => `/users/${id}`,
  list: '/users',
  byId: (id: string) => `/users/${id}`,
  restore: (id: string) => `/users/${id}/restore`, // POST - restore user
} as const);
