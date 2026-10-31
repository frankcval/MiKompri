import { test, expect } from '@playwright/test'
import AxeBuilder from '@axe-core/playwright'
import { ids, modernFixtures, openHarness } from '../fixtures/modernization'

for (const width of [320, 390, 430, 768, 1280]) {
  test(`@smoke 011 navigation, notes, history and reflow ${width}px`, async ({ page }) => {
    await page.setViewportSize({ width, height: 900 })
    const fixture = await modernFixtures(page)
    fixture.personal.name = 'Compra para toda la familia: frutas, verduras y productos de la despensa para el fin de semana'
    fixture.personal.items[0].productPrice = null
    await openHarness(page, '/lists?filter=personal&sort=oldest')
    const nav = page.getByRole('navigation', { name: width >= 1024 ? 'Navegación principal' : 'Accesos rápidos', exact: true })
    await expect(nav.getByRole('link', { name: 'Mis listas' })).toHaveAttribute('aria-current', 'page')
    for (const label of ['Mis listas', 'Grupos', 'Productos']) {
      const link = nav.getByRole('link', { name: label, exact: true })
      await expect(link).toBeVisible()
      const box = (await link.boundingBox())!
      expect(box.height).toBeGreaterThanOrEqual(44); expect(box.width).toBeGreaterThanOrEqual(44)
      await expect(link.locator('svg')).toHaveAttribute('aria-hidden', 'true')
    }
    const note = page.getByRole('link', { name: /Personal Compra para toda la familia/ })
    await expect(note).toContainText('1 producto sin precio')
    expect(await note.locator('button,a,input').count()).toBe(0)
    await page.getByRole('link', { name: 'Historial', exact: true }).click()
    await expect(page.getByTestId('test-route')).toHaveText('/lists?filter=personal&sort=oldest&view=history')
    await expect(page.getByRole('heading', { name: 'Sin listas en el historial' })).toBeVisible()
    await page.getByRole('link', { name: 'Activas', exact: true }).click()
    await note.click()
    await expect(nav.getByRole('link', { name: 'Mis listas' })).toHaveAttribute('aria-current', 'page')
    await expect(page.getByRole('checkbox', { name: 'Marcar comprado: Tomates' })).toBeVisible()
    expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true)
    await page.screenshot({ path: `test-results/011-detail-${width}.png`, fullPage: true, animations: 'disabled' })
  })
}

test('@smoke 011 menu tooltip, keyboard and destructive confirmation preserve focus', async ({ page }) => {
  await page.setViewportSize({ width: 390, height: 844 })
  const fixture = await modernFixtures(page)
  await openHarness(page, `/personal-lists/${ids.personal}`)
  const menu = page.getByRole('button', { name: 'Abrir menú', exact: true })
  await menu.focus()
  await expect(page.getByRole('tooltip')).toContainText('Abrir menú')
  await page.keyboard.press('Escape'); await expect(page.getByRole('tooltip')).toHaveCount(0)
  await expect(menu).toBeFocused()
  await page.keyboard.press('Enter')
  await expect(page.getByRole('button', { name: 'Cerrar menú', exact: true })).toBeVisible()
  // Escape belongs to the topmost help while a Tooltip is open. Focus a
  // labelled navigation link to test the Drawer's own Escape independently.
  await page.getByRole('navigation', { name: 'Navegación principal', exact: true }).getByRole('link', { name: 'Mis listas', exact: true }).focus()
  await page.keyboard.press('Escape'); await expect(menu).toBeFocused()
  const options = page.getByLabel('Opciones de Tomates', { exact: true })
  await options.focus(); await page.keyboard.press('Enter')
  await expect(page.getByRole('button', { name: 'Eliminar Tomates', exact: true })).toBeVisible()
  await page.keyboard.press('Escape'); await expect(options).toBeFocused()
  await expect(page.getByRole('button', { name: 'Eliminar Tomates', exact: true })).toHaveCount(0)
  const remove = page.getByRole('button', { name: 'Eliminar lista', exact: true })
  await remove.click()
  const confirm = page.getByRole('dialog', { name: 'Eliminar lista', exact: true })
  await expect(confirm).toContainText('Compra de la semana')
  await confirm.getByRole('button', { name: 'Cancelar', exact: true }).click()
  await expect(remove).toBeFocused()
  expect(fixture.calls.filter(c => c.method === 'DELETE')).toHaveLength(0)
})

test('@smoke 011 partial failure, forced colors, reduced motion and a hundred items', async ({ page }) => {
  await page.setViewportSize({ width: 320, height: 844 })
  await page.emulateMedia({ reducedMotion: 'reduce', forcedColors: 'active' })
  const fixture = await modernFixtures(page, 100)
  await page.route('**/api/v1/shared-lists?*', route => route.fulfill({ status: 503, json: {} }))
  await openHarness(page, '/lists')
  await expect(page.getByRole('alert')).toContainText('Algunas listas compartidas')
  await expect(page.getByRole('link', { name: /Personal Compra de la semana/ })).toBeVisible()
  await page.getByRole('link', { name: /Personal Compra de la semana/ }).click()
  await expect(page.getByRole('checkbox')).toHaveCount(100)
  const last = page.getByRole('checkbox', { name: 'Marcar comprado: Producto de la despensa 99' })
  await last.focus(); await page.keyboard.press('Space')
  await page.getByRole('button', { name: 'Ahora no', exact: true }).click()
  await expect(page.getByRole('checkbox', { name: 'Desmarcar comprado: Producto de la despensa 99' })).toBeFocused()
  expect(fixture.calls.filter(c => c.path.endsWith('/purchase-state'))).toHaveLength(1)
  expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true)
})

test('@smoke 011 implemented list notes pass automated accessibility', async ({ page }) => {
  await page.setViewportSize({ width: 390, height: 844 })
  await modernFixtures(page)
  await openHarness(page, '/lists')
  await expect(page.getByRole('link', { name: /Personal Compra de la semana/ })).toBeVisible()
  const results = await new AxeBuilder({ page }).withTags(['wcag2a', 'wcag2aa', 'wcag21aa', 'wcag22aa']).analyze()
  const violations = results.violations.map(v => v.id === 'aria-hidden-focus' ? { ...v, nodes: v.nodes.filter(n => !/^<i\s[^>]*data-tabster-dummy=/.test(n.html)) } : v).filter(v => v.nodes.length)
  expect(violations).toEqual([])
  await page.screenshot({ path: 'test-results/011-lists-mobile.png', fullPage: true, animations: 'disabled' })
  await page.setViewportSize({ width: 1280, height: 900 })
  await page.screenshot({ path: 'test-results/011-lists-desktop.png', fullPage: true, animations: 'disabled' })
})
