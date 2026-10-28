import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { consumeReturnTo, rememberReturnTo, safeReturnTo } from '../../../src/auth/session/returnIntent'
import { readDraft, writeDraft, clearDrafts } from '../../../src/auth/session/draftStore'

describe('safe continuation and isolated drafts', () => {
  beforeEach(() => { sessionStorage.clear(); clearDrafts() })
  afterEach(() => vi.useRealTimers())
  it('expires continuation and form drafts after 24 hours', () => {
    vi.useFakeTimers()
    rememberReturnTo('/personal-lists?create=1')
    writeDraft('alice', 'list', { name: 'Compra' })
    vi.advanceTimersByTime(86400001)
    expect(consumeReturnTo()).toBe('/lists')
    expect(readDraft('alice', 'list', { name: '' })).toEqual({ name: '' })
  })
  it('rejects external, encoded and unsupported destinations', () => {
    for (const path of ['https://evil.test', '//evil.test', '/\\evil.test', '/login', '/%2f%2fevil.test']) expect(safeReturnTo(path)).toBeNull()
  })
  it('restores an internal context once', () => {
    rememberReturnTo('/shared-lists/11111111-1111-1111-1111-111111111111?tab=expenses')
    expect(consumeReturnTo()).toContain('tab=expenses')
    expect(consumeReturnTo()).toBe('/lists')
  })
  it('does not disclose drafts after account switch', () => {
    writeDraft('alice', 'list', { name: 'Privada' })
    expect(readDraft('bob', 'list', { name: '' })).toEqual({ name: '' })
    expect(readDraft('alice', 'list', { name: '' })).toEqual({ name: '' })
  })
})
