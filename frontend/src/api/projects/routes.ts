// src/api/projects/routes.ts

export const ProjectsRoutes = Object.freeze({
  add: '/projects',
  get: '/projects',
  lookups: '/projects/lookups',
  restore: (id: string) => `/projects/${id}/restore`,
} as const);
