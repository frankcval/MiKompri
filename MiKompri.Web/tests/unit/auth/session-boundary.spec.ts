import { beforeEach, expect, it, vi } from 'vitest'
import { resetPrivateSession, sessionStamp, assertCurrentSession, onSessionReset } from '../../../src/auth/session/sessionBoundary'
import { request } from '../../../src/shared/api/request'

beforeEach(() => { sessionStorage.clear(); vi.unstubAllGlobals() })
it('invalidates captured session generations and clears registered private caches', () => {
  const stamp = sessionStamp()
  const clear = vi.fn()
  const remove = onSessionReset(clear)
  resetPrivateSession()
  expect(clear).toHaveBeenCalledTimes(1)
  expect(() => assertCurrentSession(stamp)).toThrow(/sesión/)
  remove()
})
it('discards a late HTTP response from account A after logout or switching', async () => {
  let finish!: (response: Response) => void
  vi.stubGlobal('fetch', vi.fn(() => new Promise<Response>(resolve => { finish = resolve })))
  const pending = request('/private', 'test-only')
  resetPrivateSession()
  finish(new Response(JSON.stringify({ name: 'private-A' }), { status: 200 }))
  await expect(pending).rejects.toThrow(/sesión/)
})
