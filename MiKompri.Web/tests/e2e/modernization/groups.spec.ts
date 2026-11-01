import { test, expect, type Page } from '@playwright/test'
import AxeBuilder from '@axe-core/playwright'
import { ids, modernFixtures, openHarness } from '../fixtures/modernization'

async function groupsFixture(page: Page, count: number) {
  await modernFixtures(page)
  const groups = Array.from({ length: count }, (_, i) => ({ id: i ? `group-${i}` : ids.group, name: i ? `Espacio de compras de la familia y amistades con un nombre muy largo ${i}` : 'Casa y familia', myRole: ['Owner', 'Admin', 'Member'][i % 3], memberCount: i + 1 }))
  const writes: unknown[] = []; let failCreate = false
  await page.route('**/api/v1/groups', async route => {
    if (route.request().method() === 'POST') {
      const input = route.request().postDataJSON(); writes.push(input)
      if (failCreate) return route.fulfill({ status: 409, json: {} })
      const created = { id: ids.group, ...input, myRole: 'Owner', memberCount: 1 }; groups.push(created)
      return route.fulfill({ status: 201, json: created })
    }
    return route.fulfill({ json: groups })
  })
  return { groups, writes, fail: (value: boolean) => { failCreate = value } }
}

for (const width of [320, 390, 430, 1280]) {
  test(`@smoke 011 group spaces, real metadata and detail ${width}px`, async ({ page }) => {
    await page.setViewportSize({ width, height: 900 })
    await groupsFixture(page, 10)
    await openHarness(page, '/groups')
    const list = page.getByRole('list', { name: 'Tus grupos' })
    await expect(list.getByRole('listitem')).toHaveCount(10)
    const first = list.getByRole('link', { name: /Casa y familia/ })
    await expect(list.getByRole('link').nth(1)).toContainText('Administrador')
    await expect(list.getByRole('link').nth(2)).toContainText('Miembro')
    await expect(first).toContainText('Propietario'); await expect(first).toContainText('1 persona')
    await expect(list.getByRole('link', { name: /muy largo 9/ })).toContainText('10 personas')
    expect(await first.locator('a,button,input').count()).toBe(0)
    expect((await first.boundingBox())!.height).toBeGreaterThanOrEqual(44)
    expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true)
    await first.click()
    await expect(page.getByRole('heading', { name: 'Casa y familia', exact: true })).toBeVisible()
    await expect(page.getByRole('region', { name: 'Personas del grupo' })).toContainText('Ana')
    await expect(page.getByRole('link', { name: 'Ver y crear listas de Casa y familia', exact: true })).toHaveAttribute('href', `/shared-lists?groupId=${ids.group}`)
    const nav = page.getByRole('navigation', { name: width >= 1024 ? 'Navegación principal' : 'Accesos rápidos', exact: true })
    await expect(nav.getByRole('link', { name: 'Grupos', exact: true })).toHaveAttribute('aria-current', 'page')
    await expect(page.getByRole('button', { name: /invitar|eliminar grupo|abandonar|transferir|cambiar rol/i })).toHaveCount(0)
    expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true)
  })
}

for (const count of [0, 1]) {
  test(`@smoke 011 intentional group state ${count}`, async ({ page }) => {
    await page.setViewportSize({ width: 390, height: 844 })
    await groupsFixture(page, count)
    await openHarness(page, '/groups')
    if (!count) {
      await expect(page.getByRole('heading', { name: 'Aún no tienes grupos' })).toBeVisible()
      await expect(page.getByRole('button', { name: 'Crear tu primer grupo' })).toBeEnabled()
    } else await expect(page.getByRole('list', { name: 'Tus grupos' }).getByRole('link')).toHaveCount(1)
    const result = await new AxeBuilder({ page }).withTags(['wcag2a', 'wcag2aa', 'wcag21aa', 'wcag22aa']).analyze()
    const violations = result.violations.map(v => v.id === 'aria-hidden-focus' ? { ...v, nodes: v.nodes.filter(n => !/^<i\s[^>]*data-tabster-dummy=/.test(n.html)) } : v).filter(v => v.nodes.length)
    expect(violations).toEqual([])
  })
}

test('@smoke 011 group creation keeps draft, errors, submit and focus', async ({ page }) => {
  await page.setViewportSize({ width: 320, height: 844 })
  const fixture = await groupsFixture(page, 0)
  await openHarness(page, '/groups')
  await page.getByRole('button', { name: 'Crear tu primer grupo' }).click()
  const dialog = page.getByRole('dialog', { name: 'Crear grupo', exact: true })
  const input = dialog.getByRole('textbox', { name: 'Nombre del grupo' })
  await expect(input).toBeFocused()
  await expect(dialog.getByRole('button', { name: 'Crear grupo', exact: true })).toBeDisabled()
  await input.fill('Casa QA'); fixture.fail(true)
  await input.press('Enter')
  await expect(dialog.getByRole('alert')).toBeVisible(); await expect(input).toHaveValue('Casa QA')
  fixture.fail(false); await input.press('Enter')
  await expect(dialog).toHaveCount(0)
  await expect(page.getByRole('button', { name: 'Crear grupo', exact: true })).toBeFocused()
  await expect(page.getByRole('link', { name: /Casa QA/ })).toBeVisible()
  expect(fixture.writes).toEqual([{ name: 'Casa QA' }, { name: 'Casa QA' }])
  await page.getByRole('button', { name: 'Crear grupo', exact: true }).click()
  await expect(dialog).toBeVisible()
  await expect(input).toBeFocused()
  await page.keyboard.press('Escape')
  await expect(dialog).toHaveCount(0)
  await expect(page.getByRole('button', { name: 'Crear grupo', exact: true })).toBeFocused()
})

test('@smoke 011 groups deep links and reload keep the selected section', async ({ page }) => {
  await groupsFixture(page, 1)
  await openHarness(page, `/groups/${ids.group}`)
  await page.getByRole('link', { name: 'Volver a grupos', exact: true }).click()
  await expect(page.getByRole('heading', { name: 'Grupos', exact: true })).toBeVisible()
  await openHarness(page, `/groups/${ids.group}`)
  await page.reload()
  await expect(page.getByRole('heading', { name: 'Casa y familia', exact: true })).toBeVisible()
  await expect(page.getByRole('navigation', { name: 'Navegación principal', exact: true }).getByRole('link', { name: 'Grupos', exact: true })).toHaveAttribute('aria-current', 'page')
})

test.describe('touch input', () => {
  test.use({ hasTouch: true })
  test('@smoke 011 touch can create and enter a group without hover help', async ({ page }) => {
    await page.setViewportSize({ width: 390, height: 844 })
    await groupsFixture(page, 1)
    await openHarness(page, '/groups')
    await page.getByRole('button', { name: 'Crear grupo', exact: true }).tap()
    const cancel = page.getByRole('dialog').getByRole('button', { name: 'Cancelar', exact: true })
    await cancel.tap()
    await page.getByRole('link', { name: /Casa y familia/ }).tap()
    await expect(page.getByRole('region', { name: 'Personas del grupo' })).toBeVisible()
  })
})
