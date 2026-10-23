import { test } from '@playwright/test'

test.describe('auth session flow', () => {
  test.skip(true, 'Requiere tenant Entra de integración para login real')

  test('login logout restore', async ({ page }) => {
    await page.goto('/')
  })
})
