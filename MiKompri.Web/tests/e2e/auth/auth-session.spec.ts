import { test, expect } from '../fixtures/integrated'
test('restaura una sesión Entra real y un enlace protegido tras recargar', async ({ page }) => {
  await page.goto('/profile')
  await expect(page.getByRole('heading', { name: 'Perfil', exact: true })).toBeVisible()
  await page.reload()
  await expect(page.getByRole('button', { name: 'Cerrar sesión' })).toBeVisible()
  await page.getByRole('button', { name: 'Cerrar sesión' }).click()
  await expect(page).toHaveURL(/login\.microsoftonline\.com|\/login|^.*\/$/)
  await page.goto('/personal-lists')
  await expect(page.getByRole('heading', { name: 'Inicio de sesión' })).toBeVisible()
})
