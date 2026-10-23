import { describe, expect, it, vi } from 'vitest'
import { fetchMyProfile } from '../../../src/features/profile/api/profileApi'
import {
  AUTH_SESSION_STORAGE_KEY,
  createSessionSnapshot,
  restoreSessionSnapshot,
} from '../../../src/auth/session/authSessionService'

describe('session integration', () => {
  it('restaura sesión y consulta /users/me', async () => {
    const storage = new Map<string, string>()
    const snapshot = createSessionSnapshot('acc-1', 'ana@demo.com', 'token-abc', Date.now() + 60_000)
    storage.set(AUTH_SESSION_STORAGE_KEY, JSON.stringify(snapshot))

    const restored = restoreSessionSnapshot(
      {
        getItem: (key: string) => storage.get(key) ?? null,
        setItem: (key: string, value: string) => storage.set(key, value),
        removeItem: (key: string) => storage.delete(key),
      },
      AUTH_SESSION_STORAGE_KEY,
      Date.now(),
    )

    expect(restored).not.toBeNull()

    const fetchMock = vi
      .spyOn(globalThis, 'fetch')
      .mockResolvedValueOnce(
        new Response(JSON.stringify({ id: 'u-1', displayName: 'Ana', email: 'ana@demo.com' }), {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        }),
      )

    const profile = await fetchMyProfile('http://localhost:5001', restored!.accessToken)

    expect(profile.displayName).toBe('Ana')
    expect(fetchMock).toHaveBeenCalledWith('http://localhost:5001/api/v1/users/me', expect.any(Object))

    fetchMock.mockRestore()
  })
})
