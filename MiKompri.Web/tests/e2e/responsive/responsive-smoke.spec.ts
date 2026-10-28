import { test, expect } from '../fixtures/integrated'
test('navegación integrada responsive sin desbordamiento', async ({ page }) => {
  for (const width of [320, 390, 430, 768, 1280]) {
    await page.setViewportSize({ width, height: 900 })
    await page.goto('/lists')
    await expect(page.getByRole('heading', { name: 'Mis listas', exact: true })).toBeVisible()
    expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true)
  }
})
