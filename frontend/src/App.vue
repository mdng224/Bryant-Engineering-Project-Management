<script setup lang="ts">
import { computed, ref } from 'vue'
import { RouterView, useRoute } from 'vue-router'
import SidebarMenu from './components/SidebarMenu.vue'
import HelloWorld from './components/HelloWorld.vue'

const expanded = ref(true)
const route = useRoute()

const subtitle = computed(() => {
  if (!route.name) return ''
  const name = String(route.name)
  const spaced = name.replace(/([A-Z])/g, ' $1').trim()
  return spaced.charAt(0).toUpperCase() + spaced.slice(1) + ' Page'
})
const title = 'Hello Daniel'
</script>

<template>
  <!-- page wrapper -->
  <div class="min-h-screen bg-slate-950 text-slate-100">
    <!-- fixed sidebar lives here -->
    <SidebarMenu :expanded="expanded" @toggle="expanded = !expanded" />

    <!-- content area shifts based on sidebar width -->
    <main
      :class="[
        'min-h-screen overflow-x-hidden flex flex-col transition-all duration-200',
        expanded
          ? 'ml-[240px] w-[calc(100vw-240px)]'  // sidebar open
          : 'ml-16 w-[calc(100vw-64px)]'        // sidebar collapsed
      ]"
    >
      <header class="px-5 py-4 border-b border-white/10">
        <!-- pass props explicitly -->
        <HelloWorld :title="title" :subtitle="subtitle" />
      </header>

      <section class="p-5">
        <RouterView />
      </section>
    </main>
  </div>
</template>
