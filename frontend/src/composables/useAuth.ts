// src/composables/useAuth.ts
import { computed, ref } from 'vue';
import { useRouter } from 'vue-router';

import { authService, AuthStorage } from '@/api/auth';

type AuthUser = {
  id: string;
  email: string;
  roleName?: string;
};

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
const currentUser = ref<AuthUser | null>(null);
/**
 * Tracks any in-progress `authService.me()` request to prevent duplicate API calls.
 */
let inFlight: Promise<void> | null = null;

/* -------------------------------------------------------------------------- */
/*                               Helper Functions                             */
/* -------------------------------------------------------------------------- */
const tokenExpired = (): boolean => AuthStorage.isExpired();

const decodeJwtSub = (token: string | null): string | null => {
  if (!token) return null;
  try {
    const [, payload] = token.split('.');
    const json = JSON.parse(atob(payload.replace(/-/g, '+').replace(/_/g, '/')));
    const sub = json?.sub ?? json?.nameid ?? json?.uid; // common claim names
    return typeof sub === 'string' ? sub : null;
  } catch {
    return null;
  }
};

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
  const token = AuthStorage.getToken();
  if (!token || tokenExpired()) {
    AuthStorage.clear();
    isAuthed.value = false;
    return false;
  }

  // Already known
  if (typeof isAuthed.value === 'boolean') return isAuthed.value;

  // Avoid parallel /me calls
  if (!inFlight) {
    inFlight = authService
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

  const currentUserId = computed<string | null>(() => {
    if (currentUser.value?.id) return currentUser.value.id;
    // Fallback to JWT sub if we haven't loaded /me yet
    return decodeJwtSub(AuthStorage.getToken());
  });

  const canDeleteUser = (userId: string | null | undefined): boolean => {
    const me = currentUserId.value;
    if (!userId) return false; // nothing to delete
    if (!me) return true; // unknown me → allow UI but backend will auth check
    return userId !== me; // prevent self-delete
  };

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

  return {
    // state
    isAuthed,
    currentUser,
    currentUserId,

    // guards
    canDeleteUser,

    // actions
    ensureAuthState,
    logout,
  };
};
