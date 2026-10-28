import { test, expect } from '../fixtures/integrated'
test('consulta catálogo e historial con autenticación real', async ({ page }) => {
  await page.goto('/product-catalog')
  await expect(page.getByRole('heading', { name: 'Catálogo de productos' })).toBeVisible()
  await expect(page.getByRole('heading', { name: 'Productos', exact: true })).toBeVisible()
  await expect(page.getByRole('button', { name: 'Crear producto', exact: true })).toBeVisible()
})
