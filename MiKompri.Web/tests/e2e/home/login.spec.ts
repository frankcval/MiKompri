import { test, expect } from '@playwright/test'
import { modernFixtures, openHarness } from '../fixtures/modernization'

test('@smoke login does not expose Microsoft as a sign-in option', async ({ page }) => {
  await page.goto('/tests/visual/index.html?anonymous&route=%2Flogin')
  await expect(page.getByRole('heading', { name: 'Inicio de sesión' })).toBeVisible()
  await expect(page.getByRole('button', { name: 'Continuar con Microsoft' })).toHaveCount(0)
  await expect(page.getByText('continúa con Microsoft', { exact: false })).toHaveCount(0)
})

test('@smoke failed account preparation can retry without repeating provider login', async ({ page }) => {
  await modernFixtures(page)
  let syncAttempts = 0
  await page.route('**/api/v1/users/me/sync', async route => {
    syncAttempts++
    await route.fulfill(syncAttempts === 1
      ? { status: 503, json: {} }
      : { status: 200, json: { id: '11111111-1111-4111-8111-111111111111', displayName: 'QA' } })
  })
  await openHarness(page, '/auth/callback')
  await expect(page.getByRole('status')).toContainText('El servicio no está disponible temporalmente.')
  expect(syncAttempts).toBe(1)
  await page.getByRole('button', { name: 'Reintentar acceso' }).click()
  await expect(page.getByRole('heading', { name: 'Mis listas', exact: true })).toBeVisible()
  expect(syncAttempts).toBe(2)
})
