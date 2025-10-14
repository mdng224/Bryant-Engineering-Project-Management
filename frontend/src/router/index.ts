// src/router/index.ts
import { createRouter, createWebHistory } from 'vue-router';
import { useAuth } from '@/composables/useAuth';
import HomeView from '../views/HomeView.vue';

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    // --- Auth routes ---
    {
      path: '/login',
      name: 'login',
      component: () => import('@/views/LoginView.vue'),
      meta: { guestOnly: true },
    },
    {
      path: '/register',
      name: 'register',
      component: () => import('@/views/RegisterView.vue'),
      meta: { guestOnly: true },
    },

    // --- Public / root routes ---
    { path: '/', name: 'home', component: HomeView },

    // --- Protected routes ---
    { path: '/clients', name: 'clients', component: () => import('../views/ClientsView.vue') },
    {
      path: '/employees',
      name: 'employees',
      component: () => import('../views/EmployeesView.vue'),
    },
    { path: '/projects', name: 'projects', component: () => import('../views/ProjectsView.vue') },
    { path: '/users', name: 'users', component: () => import('../views/UsersView.vue') },

    // --- Fallback / 404 ---
    {
      path: '/:pathMatch(.*)*',
      name: 'notFound',
      component: () => import('@/views/NotFoundView.vue'),
      meta: { guestOnly: true }, // avoid auth checks on 404
    },
  ],
});

/** Allow only same-origin, absolute app paths (mitigates open redirect). */
function isSafePath(path: unknown): path is string {
  return typeof path === 'string' && path.startsWith('/');
}

router.beforeEach(async to => {
  const { ensureAuthState } = useAuth();

  // If a route is not guest-only, we treat it as requiring auth.
  const guestOnly = !!to.meta.guestOnly;
  const requiresAuth = !guestOnly;

  let authed = false;
  if (requiresAuth || guestOnly) {
    authed = await ensureAuthState();
  }

  // Block protected routes if not authenticated
  if (requiresAuth && !authed) {
    return { path: '/login', query: { redirect: to.fullPath } };
  }

  // Prevent authed users from seeing guest-only routes (login/register)
  if (guestOnly && authed) {
    const redirect = to.query.redirect;
    const safe = isSafePath(redirect) ? redirect : '/';
    // Use object return to preserve replace semantics (no extra history entry)
    return { path: safe, replace: true };
  }

  return true;
});

export default router;
