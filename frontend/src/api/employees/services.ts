// src/api/employees/services.ts

import api from '..';
import type { GetEmployeesRequest, GetEmployeesResponse } from './contracts';
import { EmployeesRoutes } from './routes';

async function getEmployees(params: GetEmployeesRequest): Promise<GetEmployeesResponse> {
  console.log(params);
  const { data } = await api.get<GetEmployeesResponse>(EmployeesRoutes.list, { params });
  return data;
}

export const services = { getEmployees };
