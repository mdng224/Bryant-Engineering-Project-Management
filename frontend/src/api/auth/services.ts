// src/api/auth/services.ts

import type {
  LoginRequest,
  LoginResponse,
  MeResponse,
  RegisterRequest,
  RegisterResponse,
} from '@/api/auth/contracts';
import type { AxiosError } from 'axios';
import apiClient from '..';
import { AuthRoutes } from './routes';

/* ----------------------- Auth Storage ----------------------- */
const TOKEN_KEY = 'authToken';
const EXP_KEY = 'authExp'; // ISO string (UTC)
const EXP_LEEWAY_MS = 60_000; // 60s clock skew

export const AuthStorage = {
  set(data: LoginResponse): void {
    if (!data?.token) return; // guard
    localStorage.setItem(TOKEN_KEY, data.token);
    if (data.expiresAtUtc) localStorage.setItem(EXP_KEY, data.expiresAtUtc);
  },
  clear(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(EXP_KEY);
  },
  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  },
  getExp(): string | null {
    return localStorage.getItem(EXP_KEY);
  },
  isExpired(now = Date.now()): boolean {
    const iso = this.getExp();
    if (!iso) return true;
    const ts = new Date(iso).getTime();
    if (Number.isNaN(ts)) return true;
    return ts <= now + EXP_LEEWAY_MS;
  },
};

/* ------------------------- Services ------------------------- */

/**
 * Logs in a user.
 *
 * @endpoint POST /auth/login
 * @description Exchanges email/password for a JWT token and stores it in localStorage.
 * @returns The login response containing the token and expiration timestamp.
 */
const login = async (request: LoginRequest): Promise<LoginResponse> => {
  const { data } = await apiClient.post<LoginResponse>(AuthRoutes.login, request);
  AuthStorage.set(data);
  return data;
};

/**
 * Retrieves the currently authenticated user.
 *
 * @endpoint GET /auth/me
 * @description Uses the stored JWT (via request interceptor) to fetch user identity.
 * @returns The decoded JWT claims (subject, email, etc.).
 */
const me = async (): Promise<MeResponse> => {
  try {
    const { data } = await apiClient.get<MeResponse>(AuthRoutes.me);
    return data;
  } catch (err) {
    const e = err as AxiosError;
    if (e.response?.status === 401) AuthStorage.clear();
    throw err;
  }
};

/**
 * Registers a new user account (pending admin approval).
 *
 * @endpoint POST /auth/register
 * @description Submits registration info and clears any existing auth token.
 * The backend marks the user as inactive (IsActive = false) until approved.
 * @returns The register response containing userId, status ("pending"), and message.
 */
const register = async (request: RegisterRequest): Promise<RegisterResponse> => {
  const { data } = await apiClient.post<RegisterResponse>(AuthRoutes.register, request);

  // Ensure no prior auth session remains after registration
  AuthStorage.clear();
  return data;
};

export const authService = { login, me, register };
