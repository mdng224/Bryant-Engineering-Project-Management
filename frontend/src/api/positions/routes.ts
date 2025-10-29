// src/api/positions/routes.ts

export const PositionsRoutes = {
  list: '/positions', // GET  - paginated list
  add: '/positions', // POST - create new position
  update: (id: string) => `/positions/${id}`, // PATCH - update position
  delete: (id: string) => `/positions/${id}`, // DELETE - remove position
} as const;
