// src/api/employees/services.ts

import api from '..';
import type { AddEmployeeRequest, GetEmployeesRequest, GetEmployeesResponse } from './contracts';
import { EmployeesRoutes } from './routes';

const get = async (params: GetEmployeesRequest): Promise<GetEmployeesResponse> => {
  const { data } = await api.get<GetEmployeesResponse>(EmployeesRoutes.list, { params });
  return data;
};

/* ------------------------------ POST (add) ------------------------------ */
const add = async (payload: AddEmployeeRequest): Promise<AddEmployeeRequest> => {
  const { data } = await api.post<AddEmployeeRequest>(EmployeesRoutes.add, payload);
  return data;
};

/* ------------------------------ POST (restore) ------------------------------ */
const restore = async (id: string): Promise<void> => {
  await api.post<string>(EmployeesRoutes.restore(id));
};

export const employeeService = { add, get, restore };
