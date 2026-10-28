import { beforeEach, expect, it, vi } from 'vitest'
import type { IPublicClientApplication } from '@azure/msal-browser'
import { createAuthSessionService, executeAuthenticatedWrite } from '../../../src/auth/session/authSessionService'
import { readDraft, writeDraft } from '../../../src/auth/session/draftStore'
import { consumeReturnTo, rememberReturnTo } from '../../../src/auth/session/returnIntent'

beforeEach(() => sessionStorage.clear())
it('requests interaction for silent token failure without submitting or replaying the write', async () => {
  const error = Object.assign(new Error('interaction_required'), { name: 'InteractionRequiredAuthError' })
  const write = vi.fn()
  const login = vi.fn().mockResolvedValue(undefined)
  await expect(executeAuthenticatedWrite(write, async () => { throw error }, login)).rejects.toBe(error)
  expect(login).toHaveBeenCalledTimes(1)
  expect(write).not.toHaveBeenCalled()
})
it('clears continuation and drafts before logout redirect', async () => {
  writeDraft('logout-test', 'list', { name: 'Privada' })
  rememberReturnTo('/personal-lists?create=1')
  const logoutRedirect = vi.fn(async () => {
    expect(consumeReturnTo()).toBe('/lists')
    expect(readDraft('logout-test', 'list', { name: '' })).toEqual({ name: '' })
  })
  await createAuthSessionService({ logoutRedirect } as unknown as IPublicClientApplication).logout()
  expect(logoutRedirect).toHaveBeenCalledTimes(1)
})
