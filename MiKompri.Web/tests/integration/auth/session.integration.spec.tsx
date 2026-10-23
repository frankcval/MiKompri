import type { AccountInfo, IPublicClientApplication } from '@azure/msal-browser'
import { describe, expect, it, vi } from 'vitest'
import { createAuthSessionService } from '../../../src/auth/session/authSessionService'
import { fetchMyProfile } from '../../../src/features/profile/api/profileApi'

describe('session integration', () => {
  it('obtiene token con MSAL y consulta /users/me', async () => {
    const account = { homeAccountId: 'acc-1', username: 'ana@demo.com' } as AccountInfo

    const msal = {
      loginRedirect: vi.fn(),
      logoutRedirect: vi.fn(),
      acquireTokenSilent: vi.fn(async () => ({ accessToken: 'token-abc' })),
      getActiveAccount: vi.fn(() => account),
      getAllAccounts: vi.fn(() => [account]),
    } as unknown as IPublicClientApplication

    const sessionService = createAuthSessionService(msal)
    const token = await sessionService.getAccessToken(['scope-a'])

    const fetchMock = vi
      .spyOn(globalThis, 'fetch')
      .mockResolvedValueOnce(
        new Response(JSON.stringify({ id: 'u-1', displayName: 'Ana', email: 'ana@demo.com' }), {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        }),
      )

    const profile = await fetchMyProfile('http://localhost:8082', token)

    expect(profile.displayName).toBe('Ana')
    expect(fetchMock).toHaveBeenCalledWith('http://localhost:8082/api/v1/users/me', expect.any(Object))

    fetchMock.mockRestore()
  })
})
