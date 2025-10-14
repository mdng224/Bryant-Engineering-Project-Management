// src/router/index.ts
import { createRouter, createWebHistory } from 'vue-router'
import { useAuth } from '@/composables/useAuth'
import HomeView from '../views/HomeView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/login', name: 'login', component: () => import('../views/LoginView.vue'), meta: { guestOnly: true } },
    { path: '/register', name: 'register', component: () => import('../views/RegisterView.vue'), meta: { guestOnly: true } },

    { path: '/', name: 'home', component: HomeView, },
    { path: '/users',  name: 'users',   component: () => import('../views/UsersView.vue') },
    { path: '/employees',  name: 'employees',  component: () => import('../views/EmployeesView.vue') },
    { path: '/clients',  name: 'clients',  component: () => import('../views/ClientsView.vue') },
    { path: '/projects',  name: 'projects',  component: () => import('../views/ProjectsView.vue') },
    { path: '/:pathMatch(.*)*', name: 'notFound',       component: () => import('../views/NotFoundView.vue') },
  ],
})

router.beforeEach(async (to) => {
  const { ensureAuthState } = useAuth()

  // public pages
  const publicPaths = new Set(['/login', '/register'])

  const requiresAuth = !publicPaths.has(to.path)
  const guestOnly = !!to.meta.guestOnly

  // only resolve auth when necessary
  let authed = false
  if (requiresAuth || guestOnly) {
    authed = await ensureAuthState()
  }

  if (requiresAuth && !authed) {
    return { path: '/login', query: { redirect: to.fullPath } }
  }

  if (guestOnly && authed) {
    // allow only same-origin paths
    const r = typeof to.query.redirect === 'string' ? to.query.redirect : '/'
    const safe = r.startsWith('/') ? r : '/'
    return safe
  }

  return true
})

export default router
