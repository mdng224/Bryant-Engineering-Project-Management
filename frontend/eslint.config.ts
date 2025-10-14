// eslint.config.ts
import pluginVitest from '@vitest/eslint-plugin';
import { defineConfigWithVueTs, vueTsConfigs } from '@vue/eslint-config-typescript';
import pluginImport from 'eslint-plugin-import'; // only for no-duplicates, not sorting
import pluginPlaywright from 'eslint-plugin-playwright';
import pluginVue from 'eslint-plugin-vue';
import { globalIgnores } from 'eslint/config';

export default defineConfigWithVueTs(
  // Which files to lint
  {
    name: 'app/files-to-lint',
    files: ['**/*.{ts,tsx,vue}'],
  },

  // Global ignores
  globalIgnores(['**/dist/**', '**/dist-ssr/**', '**/coverage/**', '**/.vite/**']),

  // Base Vue + TS configs
  pluginVue.configs['flat/essential'],
  vueTsConfigs.recommended,

  // Vitest
  {
    ...pluginVitest.configs.recommended,
    files: ['src/**/__tests__/**/*.{test,spec}.{ts,tsx}', 'src/**/*.{test,spec}.{ts,tsx}'],
  },

  // Playwright
  {
    ...pluginPlaywright.configs['flat/recommended'],
    files: ['e2e/**/*.{test,spec}.{js,ts,jsx,tsx}'],
  },

  // Project rules
  {
    plugins: {
      import: pluginImport,
    },
    languageOptions: {
      ecmaVersion: 2022,
      sourceType: 'module',
    },
    rules: {
      // ---- Vue structure & ordering ----
      'vue/attributes-order': [
        'warn',
        {
          order: [
            'DEFINITION', // is, v-is
            'LIST_RENDERING', // v-for
            'CONDITIONALS', // v-if, v-else-if, v-else, v-show, v-cloak
            'RENDER_MODIFIERS', // v-once, v-pre
            'GLOBAL', // id
            'UNIQUE', // ref, key, slot
            'TWO_WAY_BINDING', // v-model
            'OTHER_DIRECTIVES', // other custom directives
            'OTHER_ATTR', // normal attributes
            'EVENTS', // v-on
            'CONTENT', // v-text, v-html
          ],
          alphabetical: false,
        },
      ],
      'vue/order-in-components': [
        'warn',
        {
          order: [
            'el',
            'name',
            'key',
            'parent',
            'functional',
            ['delimiters', 'comments'],
            ['components', 'directives', 'filters'],
            'extends',
            'mixins',
            ['provide', 'inject'],
            'ROUTER_GUARDS',
            'layout',
            'middleware',
            'validate',
            'scrollToTop',
            'transition',
            'loading',
            'inheritAttrs',
            'model',
            ['props', 'propsData'],
            'emits',
            'setup',
            'asyncData',
            'data',
            'computed',
            'watch',
            'watchQuery',
            'methods',
            ['template', 'render'],
            'renderError',
          ],
        },
      ],

      // ---- TypeScript niceties ----
      '@typescript-eslint/no-unused-vars': [
        'warn',
        { argsIgnorePattern: '^_', varsIgnorePattern: '^_' },
      ],

      // ---- Imports: duplicates only (sorting handled by Prettier plugin) ----
      'import/no-duplicates': 'error',
    },
  },
);
