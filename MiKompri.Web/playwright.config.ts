import { defineConfig, devices } from '@playwright/test'

export default defineConfig({
  testDir: './tests/e2e',
  testMatch: ['home/**/*.spec.ts', 'visual/**/*.spec.ts', 'modernization/**/*.spec.ts'],
  fullyParallel: true,
  workers: 2,
  retries: 0,
  use: {
    channel: process.env.PLAYWRIGHT_CHANNEL,
    baseURL: 'http://127.0.0.1:5173',
    trace: 'retain-on-failure',
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    ...(process.env.PLAYWRIGHT_ALL_BROWSERS ? [{ name: 'firefox', use: { ...devices['Desktop Firefox'], channel: undefined } }, { name: 'webkit', use: { ...devices['Desktop Safari'], channel: undefined } }] : []),
  ],
  webServer: {
    command: 'npm run dev -- --host 127.0.0.1 --port 5173',
    url: 'http://127.0.0.1:5173',
    reuseExistingServer: true,
  },
})
