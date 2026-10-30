import { test, expect, type Page } from '@playwright/test'

const userId = '11111111-1111-4111-8111-111111111111'
const groupId = '22222222-2222-4222-8222-222222222222'
const listId = '33333333-3333-4333-8333-333333333333'
const productId = '44444444-4444-4444-8444-444444444444'
const itemId = '55555555-5555-4555-8555-555555555555'
const marketId = '66666666-6666-4666-8666-666666666666'
const longName = 'Compra de la semana para casa y la cena del sábado con toda la familia'
async function fixtures(page: Page) {
  await page.route('**/api/v1/**', async route => {
    const path = new URL(route.request().url()).pathname
    let body: unknown = []
    if (path.endsWith('/users/me')) body = { id: userId, displayName: 'Persona de prueba', email: 'persona@example.test' }
    else if (path.endsWith('/groups')) body = [{ id: groupId, name: 'Casa y familia' }]
    else if (path.endsWith('/members')) body = [{ userId, displayName: 'Persona de prueba', role: 'Owner' }]
    else if (path.endsWith('/purchaselists/mine')) body = [{ id: listId, name: longName, items: [{ id: itemId, productName: 'Tomates de temporada para ensalada y cocina', quantity: 2, isPurchased: false }] }]
    else if (path.endsWith('/catalog-products')) body = [{ id: productId, name: 'Tomates de temporada para ensalada y cocina', purchaseUnit: 'kg', isActive: true }]
    else if (path.endsWith('/markets')) body = [{ id: marketId, name: 'Mercado del barrio' }]
    else if (path.endsWith('/price-history')) body = { catalogProductId: productId, records: [{ marketId, effectiveDate: '2026-10-27', priceAmount: 2.8 }] }
    else if (path.endsWith('/settlement/summary')) body = { balances: [{ userId, totalPaid: 12.5, totalOwed: 12.5, netBalance: 0 }], transfers: [] }
    else if (path.endsWith('/settlement/proposal')) body = { transfers: [] }
    else if (path.endsWith('/shared-lists')) body = [{ id: listId, name: longName, groupId }]
    else if (path.endsWith(`/shared-lists/${listId}`)) body = { id: listId, name: longName, groupId, items: [{ id: itemId, productName: 'Tomates', quantity: 2, isPurchased: true }] }
    if (route.request().method() === 'POST') body = path.endsWith('/catalog-products') ? { id: productId, name: 'Producto de prueba', purchaseUnit: 'kg' } : listId
    await route.fulfill({ status: route.request().method() === 'POST' ? 201 : 200, json: body })
  })
}
async function open(page: Page, path: string, anonymous = false) {
  await page.goto(`/tests/visual/index.html?route=${encodeURIComponent(path)}${anonymous ? '&anonymous' : ''}`)
  await page.evaluate(() => document.fonts.ready)
}
async function check(page: Page, section: string, width: number) {
  // A modal correctly hides the background main/h1 from the accessibility tree.
  await expect(page.locator('main h1')).toHaveCount(1)
  await expect(page.locator('main')).toHaveCount(1)
  expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true)
  await page.screenshot({ path: `test-results/app-${section}-${width}.png`, fullPage: true, animations: 'disabled' })
}
for (const width of [320, 390, 430, 768, 1280]) {
  test(`@smoke coherencia visual de secciones a ${width}px`, async ({ page }) => {
    await page.setViewportSize({ width, height: 900 })
    await fixtures(page)
    await open(page, '/profile')
    await expect(page.getByText('persona@example.test')).toBeVisible()
    await check(page, 'profile', width)
    await page.keyboard.press('Tab')
    await expect(page.getByRole('link', { name: 'Saltar al contenido' })).toBeFocused()
    await page.keyboard.press('Enter')
    await expect(page.getByRole('main')).toBeFocused()
    if (width < 1024) {
      const menu = page.getByRole('button', { name: 'Abrir menú' })
      await menu.click()
      const dialog = page.getByRole('dialog')
      await expect(dialog).toBeVisible()
      await expect(dialog.locator(':focus')).toHaveCount(1)
      await page.keyboard.press('Escape')
      await expect(menu).toBeFocused()
      await menu.click()
      await dialog.getByRole('link', { name: 'Grupos', exact: true }).click()
      await expect(dialog).toHaveCount(0)
    } else await page.getByRole('link', { name: 'Grupos', exact: true }).click()
    await expect(page.getByRole('heading', { name: 'Grupos', exact: true })).toBeVisible()
    await expect(page.getByText('Casa y familia')).toBeVisible()
    await check(page, 'groups', width)
    const groupInput = page.getByRole('textbox', { name: 'Nombre del grupo' })
    await groupInput.focus()
    expect(await groupInput.evaluate(el => el.getBoundingClientRect().height)).toBeGreaterThanOrEqual(40)
    await open(page, `/personal-lists/${listId}`)
    await expect(page.getByRole('heading', { name: longName, exact: true })).toBeVisible()
    await expect(page.getByRole('progressbar')).toHaveAttribute('aria-valuenow', '0')
    await check(page, 'personal', width)
    await page.getByRole('button', { name: 'Añadir productos' }).click()
    await page.getByRole('button', { name: 'Tomates de temporada para ensalada y cocina (kg)', exact: true }).click()
    await page.getByRole('spinbutton', { name: 'Cantidad', exact: true }).fill('2')
    await expect(page.getByRole('button', { name: 'Agregar ítem' })).toBeEnabled()
    await open(page, '/product-catalog')
    await expect(page.getByRole('heading', { name: 'Productos', exact: true })).toBeVisible()
    await page.getByRole('button', { name: 'Ver historial' }).click()
    await expect(page.getByText('2,80 €')).toBeVisible()
    await page.getByLabel('Filtrar por mercado (opcional)').selectOption(marketId)
    await check(page, 'catalog', width)
    await open(page, `/shared-lists?groupId=${groupId}`)
    await expect(page.getByRole('heading', { name: 'Listas de Casa y familia' })).toBeVisible()
    await page.getByRole('button', { name: 'Crear lista compartida' }).click()
    await expect(page.getByRole('dialog')).toBeVisible()
    await check(page, 'shared-dialog', width)
    await page.keyboard.press('Escape')
    await expect(page.getByRole('button', { name: 'Crear lista compartida' })).toBeFocused()
    await page.getByRole('link', { name: `Ver reparto de gastos de ${longName}` }).click()
    await expect(page.getByRole('heading', { name: 'Balance por persona' })).toBeVisible()
    await check(page, 'settlement', width)
    await page.getByRole('combobox', { name: 'Ítem de la lista', exact: true }).selectOption(itemId)
    await page.getByRole('spinbutton', { name: 'Importe pagado (EUR)', exact: true }).fill('12.50')
    await page.getByRole('combobox', { name: 'Pagado por', exact: true }).selectOption(userId)
    await page.getByRole('checkbox', { name: 'Persona de prueba' }).check()
    await expect(page.getByRole('button', { name: 'Registrar gasto', exact: true })).toBeEnabled()
    await open(page, '/login', true)
    await expect(page.getByRole('heading', { name: 'Inicio de sesión' })).toBeVisible()
    await check(page, 'login', width)
  })
}

for (const section of [
    { path: '/profile', loading: 'Cargando perfil', empty: 'Perfil vacío.' },
    { path: '/groups', loading: 'Cargando grupos', empty: 'Sin grupos todavía.' },
    { path: '/personal-lists', loading: 'Cargando listas…', empty: 'Sin listas personales todavía. Crea una lista para empezar.' },
    { path: '/shared-lists', loading: 'Cargando tus grupos', empty: 'Todavía no perteneces a ningún grupo.' },
    { path: '/product-catalog', loading: 'Cargando...', empty: 'Sin productos' },
  ]) {
  test(`@smoke estados de carga, vacío y errores ${section.path}`, async ({ page }) => {
    await page.setViewportSize({ width: 390, height: 900 })
    let release!: () => void
    const responseReady = new Promise<void>(resolve => { release = resolve })
    await page.route('**/api/v1/**', async route => {
      await responseReady
      await route.fulfill({ json: section.path === '/profile' ? null : [] })
    })
    await open(page, section.path)
    await expect(page.getByText(section.loading, { exact: true }).first()).toBeVisible()
    release()
    await expect(page.getByText(section.empty, { exact: false }).first()).toBeVisible()
    expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true)
    await page.unroute('**/api/v1/**')
    for (const status of [401, 403, 503]) {
      await page.route('**/api/v1/**', route => route.fulfill({ status, json: {} }))
      await open(page, section.path)
      await expect(page.getByRole('alert').first(), `${section.path}: HTTP ${status}`).toBeVisible()
      await expect(page.getByRole('heading', { level: 1 })).toHaveCount(1)
      expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true)
      await page.unroute('**/api/v1/**')
    }
  })
}

for (const width of [320, 390, 430, 768, 1280]) {
  test(`@smoke preparar y comprar con contexto ${width}px`, async ({ page }) => {
    test.setTimeout(60000)
    await page.setViewportSize({ width, height: 900 })
    const personal = { id: listId, name: 'Compra preparada en diálogo', items: [] as { id: string; productName: string; quantity: number; productPrice: number; isPurchased: boolean }[] }
    await fixtures(page)
    await page.route('**/api/v1/purchaselists**', async route => {
      const path = new URL(route.request().url()).pathname
      if (route.request().method() === 'GET') return route.fulfill({ json: [personal] })
      if (path.endsWith('/purchase-state')) { personal.items[0].isPurchased = route.request().postDataJSON().isPurchased; return route.fulfill({ status: 204 }) }
      if (path.endsWith('/items')) {
        const body = route.request().postDataJSON()
        expect(body.productId).toBe(productId)
        personal.items.push({ id: itemId, productName: body.productName, quantity: body.quantity, productPrice: body.price, isPurchased: false })
        return route.fulfill({ status: 201, json: itemId })
      }
      return route.fulfill({ status: 201, json: listId })
    })
    await open(page, '/personal-lists')
    await page.getByRole('button', { name: 'Crear lista', exact: true }).click()
    const create = page.getByRole('dialog')
    await create.getByRole('textbox', { name: 'Nombre de lista' }).fill(personal.name)
    await create.getByRole('button', { name: 'Crear lista', exact: true }).click()
    await expect(page.getByRole('heading', { name: personal.name })).toBeVisible()
    await expect(page.getByTestId('test-route')).toHaveText(`/personal-lists/${listId}`)
    await page.getByRole('button', { name: 'Añadir productos' }).click()
    const add = page.getByRole('dialog')
    await add.getByRole('button', { name: 'Tomates de temporada para ensalada y cocina (kg)', exact: true }).click()
    await add.getByRole('spinbutton', { name: 'Cantidad', exact: true }).fill('2')
    await add.getByRole('button', { name: 'Agregar ítem', exact: true }).click()
    await expect(add.getByRole('status')).toContainText('se agregó a esta lista')
    await add.getByRole('button', { name: 'Terminar' }).click()
    await expect(page.getByRole('button', { name: 'Añadir productos' })).toBeFocused()
    const mark = page.getByRole('checkbox', { name: 'Marcar comprado: Tomates de temporada para ensalada y cocina' })
    await mark.focus()
    await page.keyboard.press('Space')
    const purchased = page.getByRole('checkbox', { name: 'Desmarcar comprado: Tomates de temporada para ensalada y cocina' })
    await page.getByRole('button', { name: 'Ahora no' }).click()
    await expect(purchased).toBeChecked()
    await expect(purchased).toBeFocused()
    await expect(page.getByRole('progressbar')).toHaveAttribute('aria-valuenow', '1')
    await check(page, 'private-shopping', width)
    await open(page, `/shared-lists/${listId}?groupId=${groupId}&tab=expenses`)
    await expect(page.getByRole('tab', { name: 'Gastos', exact: true })).toHaveAttribute('aria-selected', 'true')
    await expect(page.getByRole('heading', { name: 'Balance por persona' })).toBeVisible()
    await page.getByRole('tab', { name: 'Compra', exact: true }).click()
    await expect(page.getByTestId('test-route')).toContainText('tab=shopping')
    await expect(page.getByRole('checkbox', { name: 'Desmarcar comprado: Tomates', exact: true })).toBeChecked()
    await check(page, 'private-shared', width)
    await page.getByRole('link', { name: 'Volver a listas compartidas' }).click()
    await expect(page.getByRole('heading', { name: 'Listas de Casa y familia' })).toBeVisible()
  })
}
