import { readFileSync } from 'node:fs'
import { test as base, expect, type BrowserContext } from '@playwright/test'

export const test = base.extend<{ integrationReady: void }>({
  integrationReady: [async ({ browserName }, provide) => {
    void browserName
    const names = ['MIKOMPRI_INTEGRATED_URL', 'MIKOMPRI_OWNER_TOKEN', 'MIKOMPRI_MEMBER_TOKEN', 'MIKOMPRI_OWNER_STORAGE', 'MIKOMPRI_MEMBER_STORAGE']
    const missing = names.filter(name => !process.env[name])
    if (missing.length) throw new Error(`Faltan prerrequisitos de integración real: ${missing.join(', ')}`)
    await provide()
  }, { auto: true }],
  context: async ({ browser }, provide) => {
    const names = ['MIKOMPRI_INTEGRATED_URL', 'MIKOMPRI_OWNER_TOKEN', 'MIKOMPRI_MEMBER_TOKEN', 'MIKOMPRI_OWNER_STORAGE', 'MIKOMPRI_MEMBER_STORAGE']
    const missing = names.filter(name => !process.env[name])
    if (missing.length) throw new Error(`Faltan prerrequisitos de integración real: ${missing.join(', ')}`)
    const context = await realContext(browser, process.env.MIKOMPRI_OWNER_STORAGE!)
    await provide(context)
    await context.close()
  },
})
export { expect }

export async function realContext(browser: { newContext: (options: object) => Promise<BrowserContext> }, path: string) {
  // Exported by a person after real Entra login. Session storage is needed
  // because MSAL intentionally does not use localStorage in this application.
  const state = JSON.parse(readFileSync(path, 'utf8'))
  const context = await browser.newContext({ storageState: { cookies: state.cookies ?? [], origins: state.origins ?? [] } })
  if (state.sessionStorage) await context.addInitScript(({ origin, values }) => {
    if (location.origin === origin) for (const [key, value] of Object.entries(values)) sessionStorage.setItem(key, String(value))
  }, { origin: new URL(process.env.MIKOMPRI_INTEGRATED_URL!).origin, values: state.sessionStorage })
  return context
}

export const api = {
  shopping: process.env.MIKOMPRI_SHOPPING_API || 'http://localhost:8080',
  users: process.env.MIKOMPRI_USERS_API || 'http://localhost:8082',
  catalog: process.env.MIKOMPRI_CATALOG_API || 'http://localhost:8083',
}
export const auth = (member = false) => ({ Authorization: `Bearer ${process.env[member ? 'MIKOMPRI_MEMBER_TOKEN' : 'MIKOMPRI_OWNER_TOKEN'] ?? ''}` })

export async function makeShared(request: import('@playwright/test').APIRequestContext) {
  const profiles = await Promise.all([false, true].map(async member => {
    const response = await request.get(`${api.users}/api/v1/users/me`, { headers: auth(member) }); expect(response.ok()).toBeTruthy(); return response.json()
  }))
  const group = await request.post(`${api.users}/api/v1/groups`, { headers: auth(), data: { name: `QA 008 ${Date.now()}` } }); expect(group.ok()).toBeTruthy()
  const groupId = (await group.json()).id
  const membership = await request.post(`${api.users}/api/v1/groups/${groupId}/members`, { headers: auth(), data: { userId: profiles[1].id, role: 'Member' } }); expect(membership.ok()).toBeTruthy()
  const created = await request.post(`${api.shopping}/api/v1/shared-lists`, { headers: auth(), data: { name: `Compra QA ${Date.now()}`, groupId } }); expect(created.ok()).toBeTruthy()
  return { groupId, listId: await created.json() as string, ownerId: profiles[0].id, memberId: profiles[1].id }
}
