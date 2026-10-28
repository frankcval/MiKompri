import { test, expect, api, auth, makeShared, realContext } from '../fixtures/integrated'
test('dos sesiones reales colaboran y una membresía retirada queda bloqueada', async ({ request, browser, page }) => {
  const fixture = await makeShared(request)
  const url = `${api.shopping}/api/v1/shared-lists/${fixture.listId}`
  const memberContext = await realContext(browser, process.env.MIKOMPRI_MEMBER_STORAGE!)
  try {
    await page.goto(`/shared-lists/${fixture.listId}?tab=people`)
    await expect(page.getByRole('heading', { name: 'Personas del grupo' })).toBeVisible()
    const memberPage = await memberContext.newPage()
    await memberPage.goto(`${process.env.MIKOMPRI_INTEGRATED_URL}/shared-lists/${fixture.listId}`)
    await expect(memberPage.getByRole('button', { name: 'Añadir productos' })).toBeVisible()
    expect((await request.delete(`${api.users}/api/v1/groups/${fixture.groupId}/members/${fixture.memberId}`, { headers: auth() })).ok()).toBeTruthy()
    expect((await request.get(url, { headers: auth(true) })).status()).toBe(403)
    await memberPage.reload()
    await expect(memberPage.getByRole('alert').first()).toBeVisible()
    await expect(memberPage.getByRole('button', { name: 'Añadir productos' })).toHaveCount(0)
  } finally { await memberContext.close(); await request.delete(url, { headers: auth() }) }
})
