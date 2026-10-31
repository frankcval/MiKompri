import { afterEach, beforeEach, expect, it, vi } from 'vitest'
import type { IPublicClientApplication } from '@azure/msal-browser'
import { createAuthSessionService } from '../../../src/auth/session/authSessionService'
import { getProviderSettings, selectedProvider } from '../../../src/auth/msal/providerSettings'
import { env } from '../../../src/shared/config/env'

const baseline = { ...env }
beforeEach(() => Object.assign(env, { ...baseline, ciamEnabled: false }))
afterEach(() => { Object.assign(env, baseline); sessionStorage.clear() })
it('preserves Microsoft as the default route and its scope', () => {
  expect(selectedProvider()).toBe('microsoft')
  expect(getProviderSettings('microsoft')).not.toHaveProperty('domainHint')
  expect(getProviderSettings('microsoft').scope).toContain('access_as_user')
})
it('rejects an unconfigured CIAM route instead of falling back to Microsoft', () => {
  expect(() => getProviderSettings('google')).toThrow(/Google/)
})
it('ignores arbitrary route values from browser storage', () => {
  sessionStorage.setItem('mikompri.auth-provider', 'https://attacker.example')
  expect(selectedProvider()).toBe('microsoft')
})
it('routes Google through the hosted CIAM selector without the hint rejected with AADSTS90023', () => {
  const before = { ...env }
  try {
    Object.assign(env, { ciamEnabled: true, ciamClientId: 'qa-spa', ciamTenantId: 'qa-tenant',
      ciamAuthority: 'https://qa.ciamlogin.com/qa-tenant', ciamApiScope: 'api://qa-backend/access_as_user' })
    sessionStorage.setItem('mikompri.auth-provider', 'google')
    expect(selectedProvider()).toBe('google')
    expect(getProviderSettings('google')).toMatchObject({ scope: 'api://qa-backend/access_as_user' })
    expect(getProviderSettings('google')).not.toHaveProperty('domainHint')
    expect(getProviderSettings('microsoft').authority).toBe(before.entraAuthority)
  } finally { Object.assign(env, before) }
})
it('keeps Microsoft on its accredited route when Google CIAM is enabled', () => {
  const before = { ...env }
  try {
    Object.assign(env, { ciamEnabled: true, ciamClientId: 'qa-spa', ciamTenantId: 'qa-tenant',
      ciamAuthority: 'https://qa.ciamlogin.com/qa-tenant', ciamApiScope: 'api://qa-backend/access_as_user' })
    expect(getProviderSettings('microsoft')).toMatchObject({ clientId: before.entraClientId, authority: before.entraAuthority, scope: before.apiScope })
    expect(getProviderSettings('microsoft')).not.toHaveProperty('domainHint')
  } finally { Object.assign(env, before) }
})
it('opens hosted CIAM sign-in without a hint or a cached email account shortcut for Google', async () => {
  Object.assign(env, { ciamEnabled: true, ciamClientId: 'qa-spa', ciamTenantId: 'qa-tenant',
    ciamAuthority: 'https://qa.ciamlogin.com/qa-tenant', ciamApiScope: 'api://qa-backend/access_as_user' })
  sessionStorage.setItem('mikompri.auth-provider', 'google')
  const loginRedirect = vi.fn(async () => undefined)
  const msal = { loginRedirect, getConfiguration: () => ({ auth: {
    clientId: env.ciamClientId, authority: env.ciamAuthority,
  } }) } as unknown as IPublicClientApplication
  await createAuthSessionService(msal).login(['legacy-scope'], 'google')
  expect(loginRedirect).toHaveBeenCalledExactlyOnceWith({
    scopes: ['api://qa-backend/access_as_user'], prompt: 'login',
  })
})
