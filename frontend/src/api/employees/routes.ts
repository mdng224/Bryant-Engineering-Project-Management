// src/api/employees/routes.ts

export const EmployeesRoutes = {
  list: '/employees',
  restore: (id: string) => `/employees/${id}/restore`, // POST - restore employee
} as const;
