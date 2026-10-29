import { describe, expect, it } from 'vitest'
import { estimatedTotal, listHistoryReason, visibleLists } from '../../../../src/features/shopping-lists/listPresentation'

describe('list presentation', () => {
  it('keeps zero known and multiplies unit prices without inventing unknown prices', () => {
    expect(estimatedTotal([{ quantity: 3, productPrice: 2, isPurchased: false }, { quantity: 2, productPrice: null, isPurchased: false }, { quantity: 1, productPrice: 0, isPurchased: true }])).toEqual({ amount: 6, unknown: 1 })
  })
  it('keeps empty lists active and reactivates a completed purchase without changing closed lists', () => {
    const list = { id: 'a', name: 'A', status: 'Active', items: [{ quantity: 1, isPurchased: true }] }
    expect(listHistoryReason({ ...list, items: [] })).toBeNull()
    expect(listHistoryReason(list)).toBe('Compra completada')
    expect(listHistoryReason({ ...list, items: [{ quantity: 1, isPurchased: false }] })).toBeNull()
    expect(listHistoryReason({ ...list, status: 'Closed' })).toBe('Cerrada')
  })
  it('sorts across personal/shared sources by creation, with stable ties', () => {
    const lists = [{ id: 'b', name: 'Z', createdAt: '2026-10-20', items: [] }, { id: 'c', name: 'B', createdAt: '2026-10-22', items: [] }, { id: 'a', name: 'A', createdAt: '2026-10-20', items: [] }]
    expect(visibleLists(lists, false, 'newest').map(x => x.id)).toEqual(['c', 'a', 'b'])
    expect(visibleLists(lists, false, 'name').map(x => x.id)).toEqual(['a', 'c', 'b'])
  })
})
