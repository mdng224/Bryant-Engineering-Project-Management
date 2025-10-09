import axios from "axios";
import type { AxiosInstance, InternalAxiosRequestConfig, AxiosError } from 'axios'
import { AxiosHeaders } from 'axios'

const apiClient: AxiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5000",
  headers: { "Content-Type": "application/json" },
  withCredentials: false   // not using cookies
});

// Attach Authorization if we have a token (Axios v1-safe)
apiClient.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const token = localStorage.getItem('authToken')
  if (token) {
    // Ensure headers is an AxiosHeaders instance (or at least an object)
    if (!config.headers) {
      config.headers = new AxiosHeaders()
    }
    // Works whether it's AxiosHeaders or a plain object
    if (config.headers instanceof AxiosHeaders) {
      config.headers.set('Authorization', `Bearer ${token}`)
    } else {
      ;(config.headers as any)['Authorization'] = `Bearer ${token}`
    }
  }
  return config
})

// optional: auto-logout on 401
apiClient.interceptors.response.use(undefined, (error: AxiosError) => {
  if (error.response?.status === 401) {
    localStorage.removeItem('authToken')
    localStorage.removeItem('authExp')
  }
  return Promise.reject(error)
})

export default apiClient;
