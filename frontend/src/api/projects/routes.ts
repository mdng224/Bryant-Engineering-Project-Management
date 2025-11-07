// src/api/projects/routes.ts

export const ProjectsRoutes = Object.freeze({
  list: '/projects',
  restore: (id: string) => `/projects/${id}/restore`,
} as const);
