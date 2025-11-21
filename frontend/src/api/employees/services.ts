// src/api/employees/services.ts

import api from '..';
import type {
  AddEmployeeRequest,
  GetEmployeeDetailsResponse,
  ListEmployeesRequest,
  ListEmployeesResponse,
} from './contracts';
import { EmployeesRoutes } from './routes';

const getDetails = async (id: string): Promise<GetEmployeeDetailsResponse> => {
  const { data } = await api.get<GetEmployeeDetailsResponse>(EmployeesRoutes.getDetails(id));
  return data;
};

const list = async (params: ListEmployeesRequest): Promise<ListEmployeesResponse> => {
  const { data } = await api.get<ListEmployeesResponse>(EmployeesRoutes.list, { params });
  return data;
};

/* ------------------------------ POST (add) ------------------------------ */
const add = async (payload: AddEmployeeRequest): Promise<string> => {
  const { data } = await api.post<string>(EmployeesRoutes.add, payload);
  return data;
};

/* ------------------------------ POST (restore) ------------------------------ */
const restore = async (id: string): Promise<void> => {
  await api.post<string>(EmployeesRoutes.restore(id));
};

export const employeeService = { add, getDetails, list, restore };
