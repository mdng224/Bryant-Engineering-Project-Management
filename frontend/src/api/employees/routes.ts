// src/api/employees/routes.ts

export const EmployeesRoutes = Object.freeze({
  add: '/employees',
  getDetails: (id: string) => `/employees/${id}`,
  list: '/employees',
  restore: (id: string) => `/employees/${id}/restore`, // POST - restore employee
} as const);
