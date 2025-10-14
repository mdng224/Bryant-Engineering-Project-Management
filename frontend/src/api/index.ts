// src/api/index.ts
import axios from 'axios';
import type { AxiosInstance, InternalAxiosRequestConfig, AxiosError } from 'axios';
import { AxiosHeaders } from 'axios';

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
  const token = localStorage.getItem('authToken');
  if (!token) return config;

  // Ensure headers is an AxiosHeaders instance or plain object
  if (!config.headers) {
    config.headers = new AxiosHeaders();
  }

  if (config.headers instanceof AxiosHeaders) {
    config.headers.set('Authorization', `Bearer ${token}`);
  } else {
    (config.headers as any)['Authorization'] = `Bearer ${token}`;
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
  (error: AxiosError) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('authToken');
      localStorage.removeItem('authExp');
    }
    return Promise.reject(error);
  },
);

export default apiClient;
