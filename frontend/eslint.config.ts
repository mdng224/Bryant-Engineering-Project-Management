// eslint.config.ts
import { globalIgnores } from 'eslint/config'
import { defineConfigWithVueTs, vueTsConfigs } from '@vue/eslint-config-typescript'
import pluginVue from 'eslint-plugin-vue'
import pluginVitest from '@vitest/eslint-plugin'
import pluginPlaywright from 'eslint-plugin-playwright'

// Runs Prettier via ESLint (flat-config friendly)
import prettierRecommended from 'eslint-plugin-prettier/recommended'

// If you want to **disable** style rules that conflict with Prettier but still
// run Prettier separately, you could keep skipFormatting. If you want ESLint to
// run Prettier directly (recommended), don't use skipFormatting.
// import skipFormatting from '@vue/eslint-config-prettier/skip-formatting'

export default defineConfigWithVueTs(
  // Files to lint
  {
    name: 'app/files-to-lint',
    files: ['**/*.{ts,tsx,vue}'],
  },

  // Global ignores
  globalIgnores(['**/dist/**', '**/dist-ssr/**', '**/coverage/**', '**/.vite/**']),

  // Vue essentials + TS rules
  pluginVue.configs['flat/essential'],
  vueTsConfigs.recommended,

  // Vitest tests
  {
    ...pluginVitest.configs.recommended,
    files: ['src/**/__tests__/**/*.{test,spec}.{ts,tsx}', 'src/**/*.{test,spec}.{ts,tsx}'],
  },

  // Playwright e2e
  {
    ...pluginPlaywright.configs['flat/recommended'],
    files: ['e2e/**/*.{test,spec}.{js,ts,jsx,tsx}'],
  },

  // Your project-level tweaks (rules/env/globals)
  {
    languageOptions: {
      ecmaVersion: 2022,
      sourceType: 'module',
    },
    rules: {
      // Vue niceties
      'vue/multi-word-component-names': 'off',
      'vue/html-self-closing': ['warn', {
        html: { void: 'always', normal: 'never', component: 'always' },
        svg: 'always',
        math: 'always'
      }],
      // TS niceties
      '@typescript-eslint/no-unused-vars': ['warn', { argsIgnorePattern: '^_', varsIgnorePattern: '^_' }],
    },
  },

  // Run Prettier as part of ESLint (autoformat on save via ESLint)
  prettierRecommended,
)
