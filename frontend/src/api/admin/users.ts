import api from '@/api';
import type { GetUsersRequest, GetUsersResponse } from '@/types/api';

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
 * const users = await getUsers({ page: 1, pageSize: 25 });
 * console.log(users.totalCount);
 * ```
 */
export async function getUsers(params: GetUsersRequest): Promise<GetUsersResponse> {
  try {
    const { data } = await api.get<GetUsersResponse>('/admins/users', { params });

    return data;
  } catch (err: any) {
    if (err?.response?.status === 403) {
      // surface a nicer error or redirect
      throw new Error('You do not have permission to view users.');
    }
    throw err;
  }
}
