// src/api/auth.ts
import apiClient from ".";

export type LoginPayload = { email: string; password: string }
export type LoginResponse   = { token: string; expiresAtUtc: string }
export type MeResponse      = { sub: string; email?: string }
export type RegisterPayload = { email: string; password: string }
export type RegisterResponse = { userId: string; status: string; message: string }

const TOKEN_KEY: string = 'authToken'
const EXP_KEY: string = 'authExp'

export function clearAuth(): void {
  localStorage.removeItem(TOKEN_KEY)
  localStorage.removeItem(EXP_KEY)
}

function setAuth(data: LoginResponse): void {
  localStorage.setItem(TOKEN_KEY, data.token)
  localStorage.setItem(EXP_KEY, data.expiresAtUtc)
}

/**
 * POST /auth/login → { token, expiresAtUtc }
 * Stores token in localStorage for the request interceptor.
 */
export async function login(payload: LoginPayload): Promise<LoginResponse> {
  const { data } = await apiClient.post<LoginResponse>('/auth/login', payload)
  setAuth(data)

  return data
}

/** Optional helper: GET /me */
export async function me(): Promise<MeResponse> {
  const { data } = await apiClient.get<MeResponse>('/auth/me')

  return data
}

/**
 * POST /auth/register
 * If the API returns a token, store it; otherwise, attempt to login with the same credentials.
 */
export async function register(payload: RegisterPayload): Promise<RegisterResponse> {
  const { data } = await apiClient.post<RegisterResponse>('/auth/register', payload)
  clearAuth() // make sure no token sticks around
  return data
}