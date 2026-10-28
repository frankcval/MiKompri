import { defineConfig, devices } from '@playwright/test'
export default defineConfig({
  testDir: './tests/e2e',
  testMatch: ['auth/auth-session.spec.ts', 'lists/personal-lists.spec.ts', 'lists/shared-lists-authorization.spec.ts', 'settlements/*.spec.ts', 'product-catalog/product-catalog-readonly.spec.ts', 'responsive/responsive-smoke.spec.ts'],
  workers: 1,
  fullyParallel: false,
  retries: 0,
  use: { ...devices['Desktop Chrome'], baseURL: process.env.MIKOMPRI_INTEGRATED_URL, trace: 'off', screenshot: 'off' },
})
