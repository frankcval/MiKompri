import { afterEach, describe, expect, it, vi } from 'vitest'
import { act, cleanup, renderHook } from '@testing-library/react'
import { localDate, localDateHeaders } from '../../../src/shared/dates/localDate'
import { useLocalToday } from '../../../src/shared/dates/useLocalToday'
import { request } from '../../../src/shared/api/request'
import { registerItemExpense } from '../../../src/features/settlements/api/expensesApi'

afterEach(() => { cleanup(); vi.useRealTimers(); vi.unstubAllGlobals() })

describe('civil dates', () => {
  it('formats local calendar components and sends minutes east of UTC', () => {
    const now = new Date(2026, 9, 30, 0, 30)
    expect(localDate(now)).toBe('2026-10-30')
    expect(localDateHeaders(now)['X-Client-Utc-Offset-Minutes']).toBe(String(-now.getTimezoneOffset()))
  })
  it('refreshes today at local midnight without waiting for UTC midnight', async () => {
    vi.useFakeTimers()
    vi.setSystemTime(new Date(2026, 9, 29, 23, 59, 59))
    const { result } = renderHook(useLocalToday)
    expect(result.current).toBe('2026-10-29')
    await act(async () => { await vi.advanceTimersByTimeAsync(2000) })
    expect(result.current).toBe('2026-10-30')
  })
  it('refreshes after a sleeping tab returns to focus', () => {
    vi.useFakeTimers()
    vi.setSystemTime(new Date(2026, 9, 29, 23, 30))
    const { result } = renderHook(useLocalToday)
    vi.setSystemTime(new Date(2026, 9, 30, 1, 30))
    act(() => window.dispatchEvent(new Event('focus')))
    expect(result.current).toBe('2026-10-30')
  })
  it('propagates date context for purchases, corrections and catalog prices without changing dates', async () => {
    const fetcher = vi.fn(async () => Response.json('record'))
    vi.stubGlobal('fetch', fetcher)
    await request('/purchases', 'token', 'POST', { purchasedOn: '2026-10-30' })
    await request('/expenses/id', 'token', 'PATCH', { purchasedOn: '2026-10-30' })
    await request('/product-prices', 'token', 'POST', { effectiveDate: '2026-10-30' })
    await registerItemExpense('https://shopping.example.test', 'token', 'list', 'item', { realPaidPrice: 1, paidBy: 'actor', participants: ['actor'], currency: 'EUR', purchasedOn: '2026-10-30' })
    for (const [, options] of fetcher.mock.calls as unknown as [string, RequestInit][]) {
      expect(options.headers).toMatchObject(localDateHeaders())
      expect(String(options.body)).toContain('2026-10-30')
    }
  })
})
