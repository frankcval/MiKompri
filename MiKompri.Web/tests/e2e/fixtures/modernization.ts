import type { Page } from '@playwright/test'
export const ids = { user: '11111111-1111-4111-8111-111111111111', group: '22222222-2222-4222-8222-222222222222', personal: '33333333-3333-4333-8333-333333333333', product: '44444444-4444-4444-8444-444444444444', item: '55555555-5555-4555-8555-555555555555', shared: '66666666-6666-4666-8666-666666666666', expense: '77777777-7777-4777-8777-777777777777' }
export async function modernFixtures(page: Page, itemCount = 1) {
  const personal = { id: ids.personal, name: 'Compra de la semana', items: Array.from({ length: itemCount }, (_, i) => ({ id: i ? `item-${i}` : ids.item, productName: i ? `Producto de la despensa ${i}` : 'Tomates', quantity: 2, productPrice: 2.8 as number | null, isPurchased: false, addedBy: ids.user })) }
  const shared = { id: ids.shared, name: 'Compra de casa', groupId: ids.group, status: 'Active', items: [{ id: ids.item, productName: 'Tomates', quantity: 1, productPrice: 2.8 as number | null, isPurchased: false, addedBy: ids.user }] }
  const products = [{ id: ids.product, name: 'Tomates', purchaseUnit: 'kg', isActive: true }]
  let expenses: unknown[] = []
  let purchases: unknown[] = []
  const operations = new Map<string, unknown>()
  const calls: { path: string; method: string; body: unknown }[] = []
  await page.route('**/api/v1/**', async route => {
    const path = new URL(route.request().url()).pathname
    const method = route.request().method()
    const body = route.request().postDataJSON()
    calls.push({ path, method, body })
    if (method === 'GET') {
      let data: unknown
      if (path.endsWith('/purchaselists/mine')) data = [personal]
      else if (path.endsWith('/groups')) data = [{ id: ids.group, name: 'Casa', myRole: 'Owner' }]
      else if (path.endsWith('/members')) data = [{ userId: ids.user, displayName: 'Ana', role: 'Owner' }]
      else if (path.endsWith('/users/me')) data = { id: ids.user, displayName: 'Ana', email: 'ana@example.test' }
      else if (path.endsWith('/catalog-products')) data = products
      else if (path.endsWith('/shared-lists')) data = [{ ...shared }]
      else if (path.endsWith('/' + ids.shared)) data = shared
      else if (path.endsWith('/summary')) data = { balances: [{ userId: ids.user, totalPaid: expenses.length ? 4 : 0, totalOwed: expenses.length ? 4 : 0, netBalance: 0 }] }
      else if (path.endsWith('/proposal')) data = { transfers: [] }
      else if (path.endsWith('/expenses')) data = expenses
      else if (path.endsWith('/purchases')) data = purchases
      else if (path.endsWith('/markets')) data = []
      else return route.fulfill({ status: 404, json: {} })
      return route.fulfill({ json: data })
    }
    const list = path.includes(ids.shared) ? shared : personal
    if (path.endsWith('/purchase-state')) list.items.find(i => path.includes(i.id))!.isPurchased = body.isPurchased
    else if (path.endsWith('/mark-as-purchased')) list.items.find(i => path.includes(i.id))!.isPurchased = true
    else if (path.endsWith('/catalog-products') && method === 'POST') { const product = { id: '88888888-8888-4888-8888-888888888888', ...body, isActive: true }; products.push(product); return route.fulfill({ status: 201, json: product }) }
    else if (path.endsWith('/purchases') && method === 'POST') {
      if (!operations.has(body.operationId)) { purchases.push({ ...body, paidBy: ids.user, participants: [], version: ids.expense, expenseId: ids.expense, itemId: ids.item, productName: 'Tomates', createdAt: new Date().toISOString() }); operations.set(body.operationId, ids.expense) }
      return route.fulfill({ status: 201, json: ids.expense })
    }
    else if (path.endsWith('/expenses') && method === 'POST') { expenses.push({ ...body, version: ids.expense, expenseId: ids.expense, itemId: ids.item, productName: 'Tomates', createdAt: new Date().toISOString() }); return route.fulfill({ status: 201, json: ids.expense }) }
    else if (path.endsWith('/' + ids.expense)) { if (method === 'DELETE') expenses = []; else expenses = [{ ...body, expenseId: ids.expense, itemId: ids.item, productName: 'Tomates', createdAt: new Date().toISOString() }] }
    else if (path.endsWith('/items') && method === 'POST') { list.items.push({ id: '99999999-9999-4999-8999-999999999999', productName: body.productName ?? body.name, quantity: body.quantity, productPrice: body.price ?? body.estimatedPrice ?? null, isPurchased: false, addedBy: ids.user }); return route.fulfill({ status: 201, json: list.items.at(-1)!.id }) }
    else if (path.includes('/items/')) { const index = list.items.findIndex(i => path.endsWith(i.id)); if (method === 'DELETE') list.items.splice(index, 1); else Object.assign(list.items[index], { quantity: body.quantity, productPrice: body.price ?? body.estimatedPrice ?? null }) }
    else if (path.endsWith('/close')) shared.status = 'Closed'
    else if (method === 'PUT' || method === 'PATCH') list.name = body.name
    return route.fulfill({ status: 204 })
  })
  return { personal, shared, calls, products }
}

export async function openHarness(page: Page, path: string) {
  await page.goto(`/tests/visual/index.html?route=${encodeURIComponent(path)}`)
  await page.evaluate(() => document.fonts.ready)
}
