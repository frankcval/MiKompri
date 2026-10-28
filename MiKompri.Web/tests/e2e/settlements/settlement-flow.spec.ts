import { test, expect, api, auth, makeShared } from '../fixtures/integrated'
test('registra gasto real, conserva balance al desmarcar y permite corregirlo', async ({ request, page }) => {
  const f = await makeShared(request)
  const base = `${api.shopping}/api/v1/shared-lists/${f.listId}`
  let expenseId = ''; let itemId = ''
  try {
    const added = await request.post(`${base}/items`, { headers: auth(), data: { productId: crypto.randomUUID(), name: 'Cena QA', quantity: 1 } }); expect(added.ok()).toBeTruthy(); itemId = await added.json()
    const paid = await request.post(`${base}/items/${itemId}/expenses`, { headers: auth(), data: { realPaidPrice: 10, paidBy: f.ownerId, purchasedBy: f.ownerId, participants: [f.ownerId, f.memberId], currency: 'EUR' } }); expect(paid.ok()).toBeTruthy(); expenseId = await paid.json()
    await page.goto(`/shared-lists/${f.listId}?tab=expenses`)
    await expect(page.getByRole('region', { name: 'Gasto de Cena QA' })).toBeVisible()
    const summary = await request.get(`${base}/settlement/summary`, { headers: auth() }); expect(summary.ok()).toBeTruthy()
    const before = await summary.json()
    expect((await request.patch(`${api.shopping}/api/v1/purchaselists/${f.listId}/items/${itemId}/purchase-state`, { headers: auth(), data: { isPurchased: false } })).ok()).toBeTruthy()
    expect(await (await request.get(`${base}/settlement/summary`, { headers: auth() })).json()).toEqual(before)
  } finally { if (expenseId) await request.delete(`${base}/items/${itemId}/expenses/${expenseId}`, { headers: auth() }); await request.delete(base, { headers: auth() }) }
})
