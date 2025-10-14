// src/api/auth.ts
import type {
  LoginPayload,
  LoginResponse,
  MeResponse,
  RegisterPayload,
  RegisterResponse,
} from '@/types/api';
import apiClient from '.';

/* --------------------------- Constants --------------------------- */
const TOKEN_KEY: string = 'authToken';
const EXP_KEY: string = 'authExp';

/* ----------------------- LocalStorage Helpers -------------------- */
export function clearAuth(): void {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(EXP_KEY);
}

function setAuth(data: LoginResponse): void {
  localStorage.setItem(TOKEN_KEY, data.token);
  localStorage.setItem(EXP_KEY, data.expiresAtUtc);
}

/* ----------------------------- API ------------------------------- */
/**
 * Logs in a user.
 *
 * @endpoint POST /auth/login
 * @description Exchanges email/password for a JWT token and stores it in localStorage.
 * @returns The login response containing the token and expiration timestamp.
 */
export async function login(loginPayload: LoginPayload): Promise<LoginResponse> {
  const { data }: { data: LoginResponse } = await apiClient.post<LoginResponse>(
    '/auth/login',
    loginPayload,
  );

  setAuth(data);
  return data;
}

/**
 * Retrieves the currently authenticated user.
 *
 * @endpoint GET /auth/me
 * @description Uses the stored JWT (via request interceptor) to fetch user identity.
 * @returns The decoded JWT claims (subject, email, etc.).
 */
export async function me(): Promise<MeResponse> {
  const { data }: { data: MeResponse } = await apiClient.get<MeResponse>('/auth/me');

  return data;
}

/**
 * Registers a new user account (pending admin approval).
 *
 * @endpoint POST /auth/register
 * @description Submits registration info and clears any existing auth token.
 * The backend marks the user as inactive (IsActive = false) until approved.
 * @returns The register response containing userId, status ("pending"), and message.
 */
export async function register(registerPayload: RegisterPayload): Promise<RegisterResponse> {
  const { data }: { data: RegisterResponse } = await apiClient.post<RegisterResponse>(
    '/auth/register',
    registerPayload,
  );

  // Ensure no prior auth session remains after registration
  clearAuth();
  return data;
}
