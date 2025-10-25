// src/api/index.ts
import type { AxiosError, AxiosInstance, InternalAxiosRequestConfig } from 'axios';
import axios, { AxiosHeaders } from 'axios';
import { AuthStorage } from './auth/services';

/**
 * Axios client preconfigured for your backend API.
 *
 * - Sets baseURL from VITE_API_BASE_URL or defaults to localhost.
 * - Injects Bearer token from localStorage automatically.
 * - Clears auth token on 401 responses.
 */
const apiClient: AxiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000',
  headers: { 'Content-Type': 'application/json' },
  withCredentials: false, // not using cookies
});

/* -------------------------------------------------------------------------- */
/*                              Request Interceptor                            */
/* -------------------------------------------------------------------------- */

/**
 * Attaches Authorization header if a JWT token is stored.
 */
apiClient.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const token = AuthStorage.getToken();

  if (token) {
    config.headers = new AxiosHeaders(config.headers);
    config.headers.set('Authorization', `Bearer ${token}`);
  }

  return config;
});

/* -------------------------------------------------------------------------- */
/*                              Response Interceptor                           */
/* -------------------------------------------------------------------------- */

/**
 * Automatically logs out the user on 401 Unauthorized.
 */
apiClient.interceptors.response.use(
  response => response,
  (err: AxiosError) => {
    if (err.response?.status === 401) AuthStorage.clear();

    return Promise.reject(err);
  },
);

export default apiClient;
