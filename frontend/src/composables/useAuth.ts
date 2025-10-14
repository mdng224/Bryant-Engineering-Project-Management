// src/composables/useAuth.ts
import { ref } from 'vue'
import { me } from '@/api/auth'
import { useRouter } from 'vue-router'

const isAuthed = ref<boolean | null>(null)
let inFlight: Promise<void> | null = null

function tokenExpired() {
  const exp = localStorage.getItem('authExp')
  return !exp || Date.parse(exp) <= Date.now() + 1000 // 1s skew
}

export const ensureAuthState = async (force = false): Promise<boolean> => {
  if (force) { isAuthed.value = null; inFlight = null }

  // no token (or expired) -> definitely not authed
  const token = localStorage.getItem('authToken')
  if (!token || tokenExpired()) {
    localStorage.removeItem('authToken')
    localStorage.removeItem('authExp')
    isAuthed.value = false
    return false
  }

  if (typeof isAuthed.value === 'boolean') return isAuthed.value

  if (!inFlight) {
    inFlight = me()
      .then(() => { isAuthed.value = true })
      .catch(() => { isAuthed.value = false })
      .finally(() => { inFlight = null })
  }
  await inFlight
  return isAuthed.value!
}

export const useAuth = () => {
  const router = useRouter()

  const logout = async () => {
    // Clear auth data
    localStorage.removeItem('authToken')
    localStorage.removeItem('authExp')

    // Reset state so ensureAuthState() knows we’re logged out
    isAuthed.value = false

    // Redirect to login
    await router.push('/login')
  }

  return { isAuthed, ensureAuthState, logout }
}
