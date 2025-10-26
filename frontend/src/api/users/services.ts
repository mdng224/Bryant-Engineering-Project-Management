import api from '@/api';
import type {
  GetUsersRequest,
  GetUsersResponse,
  UpdateUserRequest,
  UserResponse,
} from '@/api/users/contracts';
import type { AxiosError } from 'axios';
import { UsersRoutes } from './routes';

/**
 * Fetches a paginated list of users from the admin API.
 *
 * Backend endpoint: GET /admins/users
 *
 * Query parameters:
 *  - page (number, 1-based): the current page index
 *  - pageSize (number): number of users per page
 *
 * Returns:
 *  - users (UserResponse[]): array of user records
 *  - totalCount (number): total number of users
 *  - page (number): current page returned by backend
 *  - pageSize (number): actual page size used by backend
 *  - totalPages (number): total number of available pages
 *
 * Authorization:
 *  Requires "AdminOnly" policy (403 if not authorized)
 *
 * Example:
 * ```ts
 * const users = await get({ page: 1, pageSize: 25 });
 * console.log(users.totalCount);
 * ```
 */
const get = async (params: GetUsersRequest): Promise<GetUsersResponse> => {
  try {
    const { data } = await api.get<GetUsersResponse>(UsersRoutes.list, { params });

    return data;
  } catch (err) {
    const e = err as AxiosError;
    if (e.response?.status === 403) {
      // surface a nicer error or redirect
      throw new Error('You do not have permission to view users.');
    }
    throw err;
  }
};

const update = async (id: string, body: UpdateUserRequest): Promise<void> => {
  await api.patch<UserResponse>(UsersRoutes.byId(id), body);
};

export const userService = { get, update };
