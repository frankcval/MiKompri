import { test, expect, api, auth } from '../fixtures/integrated'
test('CRUD personal, compra reversible y recarga contra backend real', async ({ request, page }) => {
  const created = await request.post(`${api.shopping}/api/v1/purchaselists`, { headers: auth(), data: { name: `Compra QA ${Date.now()}` } }); expect(created.ok()).toBeTruthy()
  const id = await created.json()
  try {
    const added = await request.post(`${api.shopping}/api/v1/purchaselists/${id}/items`, { headers: auth(), data: { productId: crypto.randomUUID(), productName: 'Pan QA', quantity: 1 } }); expect(added.ok()).toBeTruthy()
    await page.goto(`/personal-lists/${id}`)
    await page.getByRole('checkbox', { name: 'Marcar comprado: Pan QA' }).check()
    await page.reload()
    await expect(page.getByRole('checkbox', { name: 'Desmarcar comprado: Pan QA' })).toBeChecked()
    await page.getByRole('checkbox', { name: 'Desmarcar comprado: Pan QA' }).uncheck()
    await page.getByText('Más opciones', { exact: true }).first().click()
    await page.getByRole('button', { name: 'Editar Pan QA' }).click()
    await page.getByRole('dialog').getByRole('spinbutton', { name: 'Cantidad' }).fill('3')
    await page.getByRole('dialog').getByRole('button', { name: 'Guardar producto' }).click()
    await expect(page.getByText(/Cantidad: 3/)).toBeVisible()
  } finally { const removed = await request.delete(`${api.shopping}/api/v1/purchaselists/${id}`, { headers: auth() }); expect(removed.ok()).toBeTruthy() }
})
