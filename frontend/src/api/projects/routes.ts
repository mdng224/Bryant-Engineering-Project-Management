// src/api/projects/routes.ts

export const ProjectsRoutes = Object.freeze({
  add: '/projects',
  getDetails: (id: string) => `/projects/${id}`,
  list: '/projects',
  lookups: '/projects/lookups',
  restore: (id: string) => `/projects/${id}/restore`,
} as const);
