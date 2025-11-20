<template>
  <!-- HEADER -->
  <header class="bg-slate-950/95 shadow-sm">
    <nav
      aria-label="Global"
      class="mx-auto flex max-w-7xl items-center justify-between px-4 py-4 lg:px-8"
    >
      <!-- Logo -->
      <div class="flex lg:flex-1">
        <RouterLink to="/" class="flex items-center gap-2">
          <img
            src="https://tailwindcss.com/plus-assets/img/logos/mark.svg?color=indigo&shade=500"
            alt="Logo"
            class="h-8 w-auto"
          />
          <span class="sr-only">Bryant Engineering Admin</span>
        </RouterLink>
      </div>

      <!-- Navigation -->
      <div v-if="isAuthed" class="hidden items-center gap-6 lg:flex">
        <RouterLink
          v-for="item in navItems"
          :key="item.path"
          :to="item.path"
          class="flex items-center gap-2 rounded-md px-2 py-1.5 text-sm font-semibold transition-colors duration-150"
          :class="navClasses(item.path)"
        >
          <component :is="item.icon" class="h-4 w-4" />
          <span>{{ item.label }}</span>
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
  import {
    BadgeCheck,
    Briefcase,
    Building2,
    Contact2,
    FolderKanban,
    LogOut,
    Users,
  } from 'lucide-vue-next';
  import { RouterView, useRoute } from 'vue-router';
  import { useAuth } from './composables/useAuth';

  const { isAuthed, logout } = useAuth();
  const route = useRoute();

  const buttonClass =
    'rounded-md px-4 py-2 text-sm font-semibold text-white hover:text-indigo-400 transition-colors duration-150 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-500';

  const navBaseClass = 'text-slate-200 hover:text-indigo-300';
  const navActiveClass = 'text-indigo-400 bg-slate-800/70';

  const navItems = [
    { path: '/users', label: 'Users', icon: Users },
    { path: '/positions', label: 'Positions', icon: Briefcase },
    { path: '/employees', label: 'Employees', icon: BadgeCheck },
    { path: '/clients', label: 'Clients', icon: Building2 },
    { path: '/projects', label: 'Projects', icon: FolderKanban },
    { path: '/contacts', label: 'Contacts', icon: Contact2 },
  ];

  const navClasses = (path: string) => {
    const isActive = route.path.startsWith(path);
    return [
      navBaseClass,
      isActive && navActiveClass,
      'border-b-2 border-transparent',
      isActive && 'border-indigo-400',
    ];
  };
</script>
