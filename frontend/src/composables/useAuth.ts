// src/composables/useAuth.ts
import { ref } from 'vue';
import { useRouter } from 'vue-router';

import { authServices } from '@/api/auth';

/* -------------------------------------------------------------------------- */
/*                                State / Cache                               */
/* -------------------------------------------------------------------------- */
/**
 * Reactive auth state.
 *  - `true`  → authenticated
 *  - `false` → unauthenticated
 *  - `null`  → unknown (not yet resolved)
 */
const isAuthed = ref<boolean | null>(null);

/**
 * Tracks any in-progress `authServices.me()` request to prevent duplicate API calls.
 */
let inFlight: Promise<void> | null = null;

/* -------------------------------------------------------------------------- */
/*                               Helper Functions                             */
/* -------------------------------------------------------------------------- */
function tokenExpired(): boolean {
  const exp = localStorage.getItem('authExp');
  return !exp || Date.parse(exp) <= Date.now() + 1000;
}

/* -------------------------------------------------------------------------- */
/*                               Auth Management                              */
/* -------------------------------------------------------------------------- */
/**
 * Ensures the authentication state is known.
 *
 * @param force - If `true`, clears any cached state and revalidates.
 * @returns `true` if authenticated, otherwise `false`.
 *
 * @description
 * - Returns immediately if already resolved (`isAuthed` is boolean).
 * - If no valid token exists, marks as unauthenticated.
 * - Otherwise calls `/auth/me` once (with concurrency control)
 *   to confirm the token is valid.
 */
export const ensureAuthState = async (force = false): Promise<boolean> => {
  if (force) {
    isAuthed.value = null;
    inFlight = null;
  }

  // No token or expired → definitely not authed
  const token = localStorage.getItem('authToken');
  if (!token || tokenExpired()) {
    localStorage.removeItem('authToken');
    localStorage.removeItem('authExp');
    isAuthed.value = false;
    return false;
  }

  // Already known
  if (typeof isAuthed.value === 'boolean') return isAuthed.value;

  // Avoid parallel /me calls
  if (!inFlight) {
    inFlight = authServices
      .me()
      .then(() => {
        isAuthed.value = true;
      })
      .catch(() => {
        isAuthed.value = false;
      })
      .finally(() => {
        inFlight = null;
      });
  }

  await inFlight;
  return isAuthed.value!;
};

/**
 * Composable for managing authentication state within Vue components.
 *
 * @example
 * ```ts
 * const { isAuthed, ensureAuthState, logout } = useAuth()
 * ```
 */
export const useAuth = () => {
  const router = useRouter();

  /**
   * Logs out the current user.
   *
   * @description
   * Clears all local auth data, resets the reactive state,
   * and redirects the user to `/login`.
   */
  const logout = async (): Promise<void> => {
    localStorage.removeItem('authToken');
    localStorage.removeItem('authExp');
    isAuthed.value = false;
    await router.push('/login');
  };

  return { isAuthed, ensureAuthState, logout };
};
