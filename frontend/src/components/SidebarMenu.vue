<script setup lang="ts">
import { defineEmits } from 'vue';

const props = defineProps<{ expanded: boolean }>();
const emit = defineEmits(['toggle']);
</script>

<template>
    <aside class="sidebar" :class="{ collapsed: !props.expanded }">
      <button class="toggle-btn" @click="emit('toggle')">
        <svg v-if="expanded" xmlns="http://www.w3.org/2000/svg" class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
        </svg>
        <svg v-else xmlns="http://www.w3.org/2000/svg" class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
        </svg>
      </button>

      <nav class="menu">
       <RouterLink to="/users" class="item" aria-label="Users">
        <span class="icon">
          <!-- Server icon -->
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
            <rect x="3" y="4" width="18" height="6" rx="2" stroke-width="2"/>
            <rect x="3" y="14" width="18" height="6" rx="2" stroke-width="2"/>
            <circle cx="8" cy="7" r="1" stroke-width="2"/>
            <circle cx="8" cy="17" r="1" stroke-width="2"/>
          </svg>
        </span>
        <span class="label">Users</span>
      </RouterLink>

      <RouterLink to="/employees" class="item" aria-label="Employees">
        <span class="icon">
          <!-- Triangle warning icon -->
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
            <polygon points="12,2 22,20 2,20" stroke-width="2" fill="none"/>
            <line x1="12" y1="8" x2="12" y2="13" stroke-width="2"/>
            <circle cx="12" cy="16" r="1" stroke-width="2"/>
          </svg>
        </span>
        <span class="label">Employees</span>
      </RouterLink>

      <RouterLink to="/clients" class="item" aria-label="Clients">
        <span class="icon">
          <!-- Send/arrow icon -->
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
            <line x1="22" y1="2" x2="11" y2="13" stroke-width="2"/>
            <polygon points="22,2 15,22 11,13 2,9" stroke-width="2" fill="none"/>
          </svg>
        </span>
        <span class="label">Clients</span>
      </RouterLink>

      <RouterLink to="/projects" class="item" aria-label="Projects">
        <span class="icon">
          <!-- Send/arrow icon -->
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
            <line x1="22" y1="2" x2="11" y2="13" stroke-width="2"/>
            <polygon points="22,2 15,22 11,13 2,9" stroke-width="2" fill="none"/>
          </svg>
        </span>
        <span class="label">Projects</span>
      </RouterLink>
    </nav>
  </aside>
</template>

<style scoped>
.icon svg { width: 20px; height: 20px; }

/* each link */
.item {
  display: flex;
  align-items: center;
  gap: .625rem;
  padding: .625rem .75rem;
  border-radius: .5rem;
  color: #e5e7eb;
  text-decoration: none;
  line-height: 1;
  transition: background-color .15s ease, color .15s ease, transform .05s ease;
}

/* optional: subtle press effect */
.item:active { transform: translateY(1px); }
.item:hover { background: #273244; }                 /* hover */
.item.router-link-active {
  background: #111827;                               /* active */
  color: #ffffff;
  font-weight: 600;
}

.sidebar.collapsed ~ .content {
  margin-left: 64px;
}

.sidebar {
  position: fixed;
  top: 0;
  left: 0;
  height: 100vh;
  width: 240px;
  background: #1f2937;
  color: #fff;
  transition: width 0.2s ease;
  padding: 1rem;
  box-sizing: border-box;
  overflow-x: hidden; /* avoid text peeking */
}

.sidebar.collapsed {
  width: 64px;
}

.sidebar.collapsed .brand-text,
.sidebar.collapsed .label {
  display: none;
}

/* keep nav links centered when collapsed */
.sidebar.collapsed nav {
  display: flex;
  flex-direction: column;
  align-items: center;
}

/* hide text when collapsed */
.sidebar.collapsed .brand-text,
.sidebar.collapsed .label { display: none; }

.toggle-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 36px;
  height: 36px;
  border: none;
  border-radius: 8px;
  background-color: #111827; /* dark slate */
  color: #e5e7eb;            /* light text */
  cursor: pointer;
  transition: background-color 0.2s, transform 0.2s;
  margin: 0.5rem;
}

.toggle-btn:hover {
  background-color: #1f2937; /* slightly lighter */
  transform: scale(1.05);
}

.toggle-btn:active {
  transform: scale(0.95);
}

.toggle-btn .icon {
  width: 20px;
  height: 20px;
  stroke-width: 2;
}
</style>
