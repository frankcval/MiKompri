import { expect, it, vi } from 'vitest'
import { EventType, type AccountInfo, type EventMessage, type IPublicClientApplication } from '@azure/msal-browser'
import { applyAccountEvent, getLoginError } from '../../../src/auth/session/accountEvents'
it('reports redirect cancellation and provider failure without exposing the provider payload', () => {
  const instance = {} as IPublicClientApplication
  applyAccountEvent(instance, { eventType: EventType.LOGIN_FAILURE, error: { errorCode: 'user_cancelled', message: 'private provider details' } } as unknown as EventMessage)
  expect(getLoginError()).toMatch(/cancelado/)
  expect(getLoginError()).not.toContain('private')
  applyAccountEvent(instance, { eventType: EventType.LOGIN_START } as EventMessage)
  expect(getLoginError()).toBe('')
  applyAccountEvent(instance, { eventType: EventType.LOGIN_FAILURE, error: new Error('private provider details') } as EventMessage)
  expect(getLoginError()).toMatch(/proveedor/)
  expect(getLoginError()).not.toContain('private')
  applyAccountEvent(instance, { eventType: EventType.LOGIN_START } as EventMessage)
})
it('does not replace B with a late silent token result from A', () => {
  const setActiveAccount = vi.fn()
  const instance = { getActiveAccount: () => ({ homeAccountId: 'B', tenantId: '00000000-0000-0000-0000-000000000000' }), setActiveAccount } as unknown as IPublicClientApplication
  applyAccountEvent(instance, { eventType: EventType.ACQUIRE_TOKEN_SUCCESS, payload: { account: { homeAccountId: 'A', tenantId: '00000000-0000-0000-0000-000000000000' } } } as EventMessage)
  expect(setActiveAccount).not.toHaveBeenCalled()
  const b = { homeAccountId: 'B', tenantId: '00000000-0000-0000-0000-000000000000' } as AccountInfo
  applyAccountEvent(instance, { eventType: EventType.LOGIN_SUCCESS, payload: { account: b } } as EventMessage)
  expect(setActiveAccount).toHaveBeenCalledWith(b)
})
