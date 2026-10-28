import { test, expect } from '@playwright/test'

for (const width of [320, 390, 430, 768, 1280]) {
  test(`@smoke portada pública ${width}px`, async ({ page }) => {
    await page.setViewportSize({ width, height: 900 })
    await page.goto('/')
    await expect(page.getByRole('heading', { level: 1 })).toHaveText(/La compra, mejor/)
    await page.evaluate(() => document.fonts.ready)
    expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true)
    await expect(page.getByRole('main')).toHaveCount(1)
    for (const link of await page.getByRole('link', { name: /Crear mi primera lista/ }).all()) {
      const box = await link.boundingBox()
      expect(box?.height).toBeGreaterThanOrEqual(44)
      expect(box?.width).toBeGreaterThanOrEqual(44)
    }
    await page.keyboard.press('Tab')
    await expect(page.getByRole('link', { name: 'Saltar al contenido' })).toBeFocused()
    await page.keyboard.press('Enter')
    await expect(page.getByRole('main')).toBeFocused()
    const tomatoes = page.getByRole('checkbox').first()
    await tomatoes.focus()
    await page.keyboard.press('Space')
    expect(await tomatoes.evaluate(el => getComputedStyle(el.closest('div')!).outlineStyle !== 'none' || getComputedStyle(el).outlineStyle !== 'none')).toBe(true)
    await expect(page.getByRole('status')).toContainText('3 de 5')
    await page.getByRole('button', { name: 'Restablecer ejemplo' }).click()
    await expect(page.getByRole('checkbox', { name: 'Marcar comprado: Tomates' })).not.toBeChecked()
    if (width < 1024) {
      const menu = page.getByRole('button', { name: 'Abrir menú' })
      await menu.click()
      await expect(page.getByRole('dialog')).toBeVisible()
      await expect(page.getByRole('dialog').locator(':focus')).toHaveCount(1)
      await page.keyboard.press('Escape')
      await expect(menu).toBeFocused()
      await menu.click()
      await page.getByRole('navigation', { name: 'Navegación móvil' }).getByRole('link', { name: 'Preguntas' }).click()
      await expect(page.getByRole('dialog')).toHaveCount(0)
    }
    await page.getByRole('button', { name: '¿MiKompri realiza pagos?' }).click()
    await expect(page.getByText('No. El reparto muestra balances', { exact: false })).toBeVisible()
    await page.screenshot({ path: `test-results/home-${width}.png`, fullPage: true, animations: 'disabled' })
    await page.getByRole('link', { name: /Crear mi primera lista/ }).first().click()
    await expect(page).toHaveURL(/\/login$/)
    await expect(page.getByRole('heading', { name: 'Inicio de sesión' })).toBeVisible()
  })
}
