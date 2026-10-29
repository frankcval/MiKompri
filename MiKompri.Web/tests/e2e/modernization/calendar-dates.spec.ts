import { test, expect } from '@playwright/test'
import { ids, modernFixtures, openHarness } from '../fixtures/modernization'

for (const scenario of [
  { zone: 'Europe/Madrid', instant: '2026-10-29T23:30:00Z', day: '2026-10-30', tomorrow: '2026-10-31', offset: '60', display: '30/10/2026' },
  { zone: 'Etc/GMT+2', instant: '2026-10-30T00:30:00Z', day: '2026-10-29', tomorrow: '2026-10-30', offset: '-120', display: '29/10/2026' },
]) {
  test.describe(`civil dates ${scenario.zone}`, () => {
    test.use({ timezoneId: scenario.zone })
    test(`@smoke 009 local calendar date in price, purchases and history (${scenario.offset})`, async ({ page }) => {
      await page.setViewportSize({ width: 390, height: 844 })
      await page.clock.setFixedTime(new Date(scenario.instant))
      await modernFixtures(page)
      await page.route('**/api/v1/markets**', route => route.fulfill({ json: [{ id: ids.group, name: 'Mercado del barrio', isActive: true }] }))
      const headers: string[] = []
      page.on('request', request => {
        if (request.method() === 'POST' && /\/(product-prices|purchases|expenses)$/.test(new URL(request.url()).pathname))
          headers.push(request.headers()['x-client-utc-offset-minutes'])
      })
      const prices: Record<string, unknown>[] = []
      await page.route('**/api/v1/product-prices', async route => {
        const price = route.request().postDataJSON()
        prices.push({ ...price, marketName: 'Mercado del barrio', currency: 'EUR' })
        await route.fulfill({ status: 201, json: ids.expense })
      })
      await page.route('**/api/v1/catalog-products/*/price-history**', route => route.fulfill({ json: { catalogProductId: ids.product, catalogProductName: 'Tomates', records: prices } }))
      await openHarness(page, '/product-catalog')
      await page.getByRole('button', { name: 'Ver historial', exact: true }).click()
      const price = page.getByRole('form', { name: 'Registrar precio de catálogo' })
      await expect(price.getByLabel('Fecha del precio')).toHaveValue(scenario.day)
      await expect(price.getByLabel('Fecha del precio')).toHaveAttribute('max', scenario.day)
      await price.getByLabel('Mercado del precio').selectOption(ids.group)
      await price.getByLabel('Precio unitario (EUR)').fill('1.25')
      await price.getByRole('button', { name: 'Registrar precio', exact: true }).click()
      await expect(page.getByText(new RegExp(scenario.display))).toBeVisible()
      expect(prices[0].effectiveDate).toBe(scenario.day)

      for (const shared of [false, true]) {
        await openHarness(page, shared ? `/shared-lists/${ids.shared}` : `/personal-lists/${ids.personal}`)
        await page.getByRole('checkbox', { name: 'Marcar comprado: Tomates' }).click()
        const capture = page.getByRole('dialog', { name: 'Datos de compra de Tomates' })
        const date = capture.getByLabel('Fecha de compra (opcional)')
        await expect(date).toHaveAttribute('max', scenario.day)
        await expect(date).toHaveValue('')
        await capture.getByRole('spinbutton', { name: 'Importe pagado (EUR)' }).fill('2.30')
        if (shared) await capture.getByRole('checkbox', { name: 'Ana', exact: true }).check()
        await date.fill(scenario.tomorrow)
        await expect(capture.getByRole('button', { name: 'Guardar datos de compra' })).toBeDisabled()
        await date.fill(scenario.day)
        await capture.getByRole('button', { name: 'Guardar datos de compra' }).click()
        await expect(capture).toHaveCount(0)
        await page.getByRole('tab', { name: 'Gastos', exact: true }).click()
        await expect(page.getByRole('region', { name: 'Gasto de Tomates' })).toContainText(scenario.day)
      }
      expect(headers).toEqual([scenario.offset, scenario.offset, scenario.offset])
    })
  })
}
