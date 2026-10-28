import { test, expect } from '@playwright/test'
import AxeBuilder from '@axe-core/playwright'
import { ids, modernFixtures, openHarness } from '../fixtures/modernization'

for (const width of [320, 390, 430, 768, 1280]) {
  test(`@smoke 008 compra reversible, edición y alta contextual ${width}px`, async ({ page }) => {
    await page.setViewportSize({ width, height: 900 })
    const fixture = await modernFixtures(page)
    await openHarness(page, '/lists')
    await page.getByRole('link', { name: /Personal Compra de la semana/ }).click()
    await expect(page.getByRole('heading', { name: 'Compra de la semana' })).toBeVisible()
    const item = page.getByRole('checkbox', { name: 'Marcar comprado: Tomates' })
    await item.focus(); await page.keyboard.press('Space')
    await expect(page.getByRole('checkbox', { name: 'Desmarcar comprado: Tomates' })).toBeFocused()
    await page.keyboard.press('Space')
    await expect(item).not.toBeChecked()
    await page.getByText('Más opciones', { exact: true }).first().click()
    await page.getByRole('button', { name: 'Editar Tomates', exact: true }).click()
    const edit = page.getByRole('dialog')
    await edit.getByRole('spinbutton', { name: 'Cantidad' }).fill('3')
    await edit.getByRole('button', { name: 'Guardar producto' }).click()
    await expect(page.getByText(/Cantidad: 3/)).toBeVisible()
    await page.getByRole('button', { name: 'Añadir productos' }).click()
    const add = page.getByRole('dialog')
    await add.getByRole('button', { name: 'Crear producto aquí' }).click()
    await add.getByRole('textbox', { name: 'Nombre del producto' }).fill('Café')
    await add.getByRole('textbox', { name: 'Unidad de compra' }).fill('paquete')
    await add.getByRole('button', { name: 'Crear y añadir', exact: true }).click()
    await expect(add.getByRole('status')).toContainText('Café se ha añadido')
    await add.getByRole('button', { name: 'Terminar' }).click()
    await expect(add).not.toBeVisible()
    await expect(page.getByText('Café', { exact: true })).toBeVisible()
    expect(fixture.calls.filter(c => c.method === 'POST' && c.path.endsWith('/catalog-products'))).toHaveLength(1)
    expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true)
    await page.screenshot({ path: `test-results/008-shopping-${width}.png`, fullPage: true, animations: 'disabled' })
  })
}

test('@smoke 008 grupos, personas, gastos corregibles y navegación', async ({ page }) => {
  await page.setViewportSize({ width: 390, height: 844 })
  await modernFixtures(page)
  await openHarness(page, `/groups/${ids.group}`)
  await expect(page.getByRole('heading', { name: 'Personas del grupo' })).toBeVisible()
  await expect(page.getByText('Ana', { exact: true })).toBeVisible()
  await openHarness(page, `/shared-lists/${ids.shared}?tab=people`)
  await expect(page.getByRole('tab', { name: 'Personas' })).toHaveAttribute('aria-selected', 'true')
  await page.getByRole('tab', { name: 'Gastos' }).click()
  await page.getByRole('combobox', { name: 'Ítem de la lista' }).selectOption(ids.item)
  await page.getByRole('spinbutton', { name: 'Importe pagado (EUR)' }).fill('4')
  await page.getByRole('checkbox', { name: 'Ana', exact: true }).check()
  await page.getByRole('button', { name: 'Registrar gasto', exact: true }).click()
  await expect(page.getByRole('region', { name: 'Gasto de Tomates' })).toBeVisible()
  await page.getByRole('button', { name: 'Corregir gasto de Tomates' }).click()
  const history = page.getByRole('region', { name: 'Gasto de Tomates' })
  await history.getByRole('spinbutton', { name: 'Importe pagado (EUR)' }).fill('5')
  await history.getByRole('button', { name: 'Guardar corrección' }).click()
  await expect(history).toContainText('5,00')
  await page.getByRole('button', { name: 'Eliminar gasto de Tomates' }).click()
  await page.getByRole('dialog').getByRole('button', { name: 'Confirmar' }).click()
  await expect(page.getByText('No hay gastos registrados todavía.')).toBeVisible()
})

for (const path of ['/lists', `/personal-lists/${ids.personal}`, `/shared-lists/${ids.shared}?tab=people`]) {
  test(`@smoke 008 accesibilidad automatizada ${path}`, async ({ page }, testInfo) => {
    await page.setViewportSize({ width: 390, height: 844 })
    await modernFixtures(page)
    await openHarness(page, path)
    await expect(page.getByRole('heading', { level: 1 })).toBeVisible()
    const results = await new AxeBuilder({ page }).withTags(['wcag2a', 'wcag2aa', 'wcag21aa', 'wcag22aa']).analyze()
    await testInfo.attach('axe-raw-results', { body: JSON.stringify(results), contentType: 'application/json' })
    // Tabster's intentional focus sentinels are a documented false positive:
    // https://github.com/microsoft/tabster/issues/288 . Keep the raw report and
    // every rule enabled; exempt only these non-content infrastructure nodes.
    const violations = results.violations.map(v => v.id === 'aria-hidden-focus' ? { ...v, nodes: v.nodes.filter(n => !/^<i\s[^>]*data-tabster-dummy=/.test(n.html)) } : v).filter(v => v.nodes.length)
    expect(violations).toEqual([])
  })
}
