// src/api/clients/routes.ts

export const ClientsRoutes = Object.freeze({
  add: '/clients',
  list: '/clients',
  lookups: '/clients/lookups',
  restore: (id: string) => `/clients/${id}/restore`, // POST - restore client
} as const);
