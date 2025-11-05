// src/api/positions/routes.ts

export const PositionsRoutes = Object.freeze({
  list: '/positions',
  add: '/positions',
  update: (id: string) => `/positions/${id}`,
  delete: (id: string) => `/positions/${id}`,
  restore: (id: string) => `/positions/${id}/restore`,
} as const);
