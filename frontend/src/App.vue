<script setup lang="ts">
import { RouterView } from 'vue-router'
import { useAuth } from './composables/useAuth'

const { isAuthed, logout } = useAuth()

const buttonClass =
  'rounded-md px-4 py-2 text-sm font-semibold text-white hover:text-indigo-400 transition-colors duration-150 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-500'
const navLink = 'text-sm font-semibold text-white transition-colors duration-150 hover:text-indigo-400'
</script>

<template>
  <!-- HEADER -->
  <header class="bg-gray-900 shadow-sm">
    <nav aria-label="Global" class="mx-auto flex max-w-7xl items-center justify-between p-6 lg:px-8">
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
      
      <!-- Authenticated Navigation -->
      <div v-if="isAuthed" class="hidden lg:flex lg:gap-x-8">
        <RouterLink to="/users"     :class="navLink">Users</RouterLink>
        <RouterLink to="/employees" :class="navLink">Employees</RouterLink>
        <RouterLink to="/clients"   :class="navLink">Clients</RouterLink>
        <RouterLink to="/projects"  :class="navLink">Projects</RouterLink>
      </div>
      
        <!-- Right side (Auth actions) -->
      <div class="hidden lg:flex lg:flex-1 lg:justify-end">
        <button v-if="isAuthed" :class="buttonClass" @click="logout">Logout</button>
      </div>
  </nav>
  </header>

  <!-- page wrapper -->
  <div class="min-h-screen bg-slate-950 text-slate-100">
    <main class="min-h-screen overflow-x-hidden flex flex-col transition-all duration-200">
      <section class="flex-1 p-6">
        <RouterView />
      </section>
    </main>
  </div>
</template>
