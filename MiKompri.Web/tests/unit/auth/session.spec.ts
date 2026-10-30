import type { AccountInfo, IPublicClientApplication } from '@azure/msal-browser'
import { describe, expect, it, vi } from 'vitest'
import { createAuthSessionService } from '../../../src/auth/session/authSessionService'

describe('authSessionService', () => {
  it('getAccessToken usa acquireTokenSilent con la cuenta recibida', async () => {
    const account = { homeAccountId: 'acc-1', username: 'ana@demo.com' } as AccountInfo
    const acquireTokenSilent = vi.fn(async () => ({ accessToken: 'token-abc' }))

    const msal = {
      loginRedirect: vi.fn(),
      logoutRedirect: vi.fn(),
      acquireTokenSilent,
      getActiveAccount: vi.fn(() => null),
      getAllAccounts: vi.fn(() => [account]),
    } as unknown as IPublicClientApplication

    const service = createAuthSessionService(msal)
    const token = await service.getAccessToken(['scope-a'], account)

    expect(token).toBe('token-abc')
    expect(acquireTokenSilent).toHaveBeenCalledWith({ scopes: ['scope-a'], account })
  })

  it('getAccessToken falla si no hay cuenta MSAL disponible', async () => {
    const msal = {
      loginRedirect: vi.fn(),
      logoutRedirect: vi.fn(),
      acquireTokenSilent: vi.fn(),
      getActiveAccount: vi.fn(() => null),
      getAllAccounts: vi.fn(() => []),
    } as unknown as IPublicClientApplication

    const service = createAuthSessionService(msal)

    await expect(service.getAccessToken(['scope-a'])).rejects.toThrow('No hay cuenta activa en MSAL para adquirir token.')
  })

  it('login y logout delegan en MSAL redirect', async () => {
    const account = { homeAccountId: 'acc-1', username: 'ana@demo.com' } as AccountInfo
    const loginRedirect = vi.fn(async () => undefined)
    const logoutRedirect = vi.fn(async () => undefined)

    const msal = {
      loginRedirect,
      logoutRedirect,
      acquireTokenSilent: vi.fn(),
      getActiveAccount: vi.fn(() => null),
      getAllAccounts: vi.fn(() => []),
    } as unknown as IPublicClientApplication

    const service = createAuthSessionService(msal)

    await service.login(['scope-a'])
    await service.logout(account)

    expect(loginRedirect).toHaveBeenCalledWith({ scopes: ['scope-a'], prompt: 'select_account' })
    expect(logoutRedirect).toHaveBeenCalledWith({ account })
  })
})
