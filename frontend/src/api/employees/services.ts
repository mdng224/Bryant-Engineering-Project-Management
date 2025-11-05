// src/api/employees/services.ts

import api from '..';
import type { GetEmployeesRequest, GetEmployeesResponse } from './contracts';
import { EmployeesRoutes } from './routes';

const get = async (params: GetEmployeesRequest): Promise<GetEmployeesResponse> => {
  const { data } = await api.get<GetEmployeesResponse>(EmployeesRoutes.list, { params });
  return data;
};

/* ------------------------------ POST (restore) ------------------------------ */
const restore = async (id: string): Promise<void> => {
  await api.post<string>(EmployeesRoutes.restore(id));
};

export const employeeService = { get, restore };
