import { describe, expect, it } from 'vitest'
import {
  AUTH_SESSION_STORAGE_KEY,
  clearSessionSnapshot,
  createSessionSnapshot,
  isSessionRestorable,
  restoreSessionSnapshot,
} from '../../../src/auth/session/authSessionService'

describe('authSessionService helpers', () => {
  it('isSessionRestorable retorna true cuando la expiración es futura', () => {
    const now = 1_000
    expect(isSessionRestorable(now + 10_000, now)).toBe(true)
  })

  it('isSessionRestorable retorna false cuando la expiración es pasada', () => {
    const now = 1_000
    expect(isSessionRestorable(now - 1, now)).toBe(false)
  })

  it('restoreSessionSnapshot recupera sesión válida desde storage', () => {
    const storage = new Map<string, string>()
    const snapshot = createSessionSnapshot('acc-1', 'ana@demo.com', 'token-1', Date.now() + 60_000)

    storage.set(AUTH_SESSION_STORAGE_KEY, JSON.stringify(snapshot))

    const result = restoreSessionSnapshot(
      {
        getItem: (key: string) => storage.get(key) ?? null,
        setItem: (key: string, value: string) => storage.set(key, value),
        removeItem: (key: string) => storage.delete(key),
      },
      AUTH_SESSION_STORAGE_KEY,
      Date.now(),
    )

    expect(result).not.toBeNull()
    expect(result?.accountId).toBe('acc-1')
    expect(result?.accessToken).toBe('token-1')
  })

  it('clearSessionSnapshot elimina el valor persistido', () => {
    const storage = new Map<string, string>()
    storage.set(AUTH_SESSION_STORAGE_KEY, 'x')

    clearSessionSnapshot(
      {
        getItem: (key: string) => storage.get(key) ?? null,
        setItem: (key: string, value: string) => storage.set(key, value),
        removeItem: (key: string) => storage.delete(key),
      },
      AUTH_SESSION_STORAGE_KEY,
    )

    expect(storage.has(AUTH_SESSION_STORAGE_KEY)).toBe(false)
  })
})
