import { test, expect, api, auth, makeShared } from '../fixtures/integrated'
test('Member no puede registrar pagos de otra persona ni borrar lista', async ({ request }) => {
  const f = await makeShared(request); const base = `${api.shopping}/api/v1/shared-lists/${f.listId}`
  try {
    const item = await request.post(`${base}/items`, { headers: auth(), data: { productId: crypto.randomUUID(), name: 'QA', quantity: 1 } }); expect(item.ok()).toBeTruthy()
    expect((await request.post(`${base}/items/${await item.json()}/expenses`, { headers: auth(true), data: { realPaidPrice: 5, paidBy: f.ownerId, participants: [f.memberId], currency: 'EUR' } })).status()).toBe(403)
    expect((await request.delete(base, { headers: auth(true) })).status()).toBe(403)
  } finally { await request.delete(base, { headers: auth() }) }
})
