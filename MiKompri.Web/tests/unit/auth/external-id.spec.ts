import { afterEach, beforeEach, expect, it } from 'vitest'
import { getProviderSettings, selectedProvider } from '../../../src/auth/msal/providerSettings'
import { env } from '../../../src/shared/config/env'

const baseline = { ...env }
beforeEach(() => Object.assign(env, { ...baseline, ciamEnabled: false }))
afterEach(() => { Object.assign(env, baseline); sessionStorage.clear() })
it('preserves Microsoft as the default route and its scope', () => {
  expect(selectedProvider()).toBe('microsoft')
  expect(getProviderSettings('microsoft').domainHint).toBeUndefined()
  expect(getProviderSettings('microsoft').scope).toContain('access_as_user')
})
it('rejects an unconfigured CIAM route instead of falling back to Microsoft', () => {
  expect(() => getProviderSettings('google')).toThrow(/Google/)
})
it('ignores arbitrary route values from browser storage', () => {
  sessionStorage.setItem('mikompri.auth-provider', 'https://attacker.example')
  expect(selectedProvider()).toBe('microsoft')
})
it('routes Google to CIAM with an exact provider hint and the CIAM resource scope', () => {
  const before = { ...env }
  try {
    Object.assign(env, { ciamEnabled: true, ciamClientId: 'qa-spa', ciamTenantId: 'qa-tenant',
      ciamAuthority: 'https://qa.ciamlogin.com/qa-tenant', ciamApiScope: 'api://qa-backend/access_as_user' })
    sessionStorage.setItem('mikompri.auth-provider', 'google')
    expect(selectedProvider()).toBe('google')
    expect(getProviderSettings('google')).toMatchObject({ domainHint: 'google', scope: 'api://qa-backend/access_as_user' })
    expect(getProviderSettings('microsoft').authority).toBe(before.entraAuthority)
  } finally { Object.assign(env, before) }
})
it('keeps Microsoft on its accredited route when Google CIAM is enabled', () => {
  const before = { ...env }
  try {
    Object.assign(env, { ciamEnabled: true, ciamClientId: 'qa-spa', ciamTenantId: 'qa-tenant',
      ciamAuthority: 'https://qa.ciamlogin.com/qa-tenant', ciamApiScope: 'api://qa-backend/access_as_user' })
    expect(getProviderSettings('microsoft')).toMatchObject({ clientId: before.entraClientId, authority: before.entraAuthority, scope: before.apiScope })
    expect(getProviderSettings('microsoft').domainHint).toBeUndefined()
  } finally { Object.assign(env, before) }
})
