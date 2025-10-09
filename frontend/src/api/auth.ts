import apiClient from ".";

export type LoginPayload = {
  email: string
  password: string
}

export type LoginResponse = {
  token: string
  expiresAtUtc: string
}

export type MeResponse = { sub: string; email?: string }

/**
 * POST /auth/login → { token, expiresAtUtc }
 * Stores token in localStorage for the request interceptor.
 */
export async function login(payload: LoginPayload): Promise<LoginResponse> {
  const { data } = await apiClient.post<LoginResponse>('/auth/login', payload)
  localStorage.setItem('authToken', data.token)
  localStorage.setItem('authExp', data.expiresAtUtc)

  return data
}

/** Optional helper: GET /me */
export async function me(): Promise<MeResponse> {
  const { data } = await apiClient.get<MeResponse>('/auth/me')

  return data
}