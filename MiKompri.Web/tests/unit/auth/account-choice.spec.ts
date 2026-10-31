import { expect, it } from 'vitest'
import type { AccountInfo, IPublicClientApplication } from '@azure/msal-browser'
import { currentAccount } from '../../../src/auth/session/accountIdentity'
import { env } from '../../../src/shared/config/env'

it('never bootstraps a legacy account exposed by shared MSAL cache under the CIAM route', () => {
  const before = { ...env }
  try {
    Object.assign(env, { ciamEnabled: true, ciamClientId: 'ciam-spa',
      ciamAuthority: 'https://qa.ciamlogin.com/qa', ciamTenantId: '22222222-2222-4222-8222-222222222222',
      ciamApiScope: 'api://qa/access_as_user' })
    sessionStorage.setItem('mikompri.auth-provider', 'google')
    const legacy = { homeAccountId: 'legacy', tenantId: env.entraTenantId } as AccountInfo
    expect(currentAccount({ getActiveAccount: () => legacy } as IPublicClientApplication, [legacy])).toBeUndefined()
  } finally { Object.assign(env, before); sessionStorage.clear() }
})

const account = (homeAccountId: string) => ({ homeAccountId, tenantId: env.entraTenantId }) as AccountInfo
it('uses the active account even when another account is first in the cache', () => {
  const a = account('A'), b = account('B')
  expect(currentAccount({ getActiveAccount: () => b } as IPublicClientApplication, [a, b])).toBe(b)
})
it('requires explicit selection when multiple cached accounts have no active choice', () => {
  expect(currentAccount({ getActiveAccount: () => null } as unknown as IPublicClientApplication, [account('A'), account('B')])).toBeUndefined()
})
