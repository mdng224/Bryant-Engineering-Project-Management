// src/api/clients/routes.ts

export const ClientsRoutes = {
  list: '/clients',
  restore: (id: string) => `/clients/${id}/restore`, // POST - restore client
} as const;
