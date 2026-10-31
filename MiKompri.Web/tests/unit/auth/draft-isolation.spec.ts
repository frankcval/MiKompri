import { act, cleanup, renderHook } from '@testing-library/react'
import { afterEach, expect, it } from 'vitest'
import { readDraft, useDraft } from '../../../src/auth/session/draftStore'
import { resetPrivateSession } from '../../../src/auth/session/sessionBoundary'
afterEach(cleanup)
it('does not copy an open form from A into B when its component remains mounted', () => {
  const hook = renderHook(({ account }) => useDraft(account, 'open-form', { name: '' }), { initialProps: { account: 'A' } })
  act(() => hook.result.current[1]({ name: 'Private A' }))
  hook.rerender({ account: 'B' })
  expect(hook.result.current[0]).toEqual({ name: '' })
  expect(readDraft('B', 'open-form', { name: '' })).toEqual({ name: '' })
})
it('resets an open draft on logout and ignores an old async setter', () => {
  const hook = renderHook(() => useDraft('A', 'pending-form', { name: '' }))
  const late = hook.result.current[1]
  act(() => { late({ name: 'Private A' }); resetPrivateSession() })
  act(() => late({ name: 'Late A' }))
  expect(hook.result.current[0]).toEqual({ name: '' })
})
