// src/api/projects/routes.ts

export const ProjectsRoutes = Object.freeze({
  get: '/projects',
  lookups: '/projects/lookups',
  restore: (id: string) => `/projects/${id}/restore`,
} as const);
