<template>
  <!-- HEADER -->
  <header class="bg-gray-900 shadow-sm">
    <nav
      aria-label="Global"
      class="mx-auto flex max-w-7xl items-center justify-between p-6 lg:px-8"
    >
      <!-- Logo -->
      <div class="flex lg:flex-1">
        <RouterLink to="/" class="flex items-center gap-2">
          <img
            src="https://tailwindcss.com/plus-assets/img/logos/mark.svg?color=indigo&shade=500"
            alt="Logo"
            class="h-8 w-auto"
          />
          <span class="sr-only">Your Company</span>
        </RouterLink>
      </div>

      <!-- Navigation -->
      <div v-if="isAuthed" class="hidden lg:flex lg:gap-x-8">
        <RouterLink to="/users" :class="navLink" class="flex items-center gap-2">
          <Users class="h-4 w-4 text-indigo-400" />
          Users
        </RouterLink>

        <RouterLink to="/positions" :class="navLink" class="flex items-center gap-2">
          <Briefcase class="h-4 w-4 text-indigo-400" />
          Positions
        </RouterLink>

        <RouterLink to="/employees" :class="navLink" class="flex items-center gap-2">
          <BadgeCheck class="h-4 w-4 text-indigo-400" />
          Employees
        </RouterLink>

        <RouterLink to="/clients" :class="navLink" class="flex items-center gap-2">
          <Building2 class="h-4 w-4 text-indigo-400" />
          Clients
        </RouterLink>

        <RouterLink to="/projects" :class="navLink" class="flex items-center gap-2">
          <FolderKanban class="h-4 w-4 text-indigo-400" />
          Projects
        </RouterLink>
      </div>

      <!-- Right side (Logout) -->
      <div class="hidden lg:flex lg:flex-1 lg:justify-end">
        <button v-if="isAuthed" :class="['flex items-center gap-2', buttonClass]" @click="logout">
          <LogOut class="h-4 w-4 text-indigo-400" aria-hidden="true" />
          <span>Logout</span>
        </button>
      </div>
    </nav>
  </header>

  <!-- page wrapper -->
  <div class="min-h-screen bg-slate-950 text-slate-100">
    <main class="flex min-h-screen flex-col overflow-x-hidden transition-all duration-200">
      <section class="flex-1 p-6">
        <RouterView />
      </section>
    </main>
  </div>
</template>

<script setup lang="ts">
  import { BadgeCheck, Briefcase, Building2, FolderKanban, LogOut, Users } from 'lucide-vue-next';
  import { RouterView } from 'vue-router';
  import { useAuth } from './composables/useAuth';

  const { isAuthed, logout } = useAuth();

  const buttonClass =
    'rounded-md px-4 py-2 text-sm font-semibold text-white hover:text-indigo-400 transition-colors duration-150 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-500';
  const navLink =
    'text-sm font-semibold text-white transition-colors duration-150 hover:text-indigo-400';
</script>
