export type ListPresentation = {
  id: string; name: string; createdAt?: string; updatedAt?: string | null; status?: string
  items?: { quantity: number; productPrice?: number | null; isPurchased: boolean }[]
}

export function listHistoryReason(list: ListPresentation): string | null {
  if (list.status === 'Closed') return 'Cerrada'
  if (list.status === 'Archived') return 'Archivada'
  return list.items?.length && list.items.every(item => item.isPurchased) ? 'Compra completada' : null
}

export function visibleLists<T extends ListPresentation>(lists: T[], history: boolean, sort: string): T[] {
  const timestamp = (date?: string | null) => date && Number.isFinite(Date.parse(date)) ? Date.parse(date) : 0
  return lists.filter(list => Boolean(listHistoryReason(list)) === history).sort((a, b) => {
    const order = sort === 'name' ? a.name.localeCompare(b.name, 'es')
      : sort === 'oldest' ? timestamp(a.createdAt) - timestamp(b.createdAt)
      : sort === 'updated' ? timestamp(b.updatedAt ?? b.createdAt) - timestamp(a.updatedAt ?? a.createdAt)
      : timestamp(b.createdAt) - timestamp(a.createdAt)
    return order || a.id.localeCompare(b.id)
  })
}

export function estimatedTotal(items: NonNullable<ListPresentation['items']>) {
  // Integer cents avoid binary floating point accumulation for display.
  const cents = items.reduce((sum, item) => sum + (item.productPrice == null ? 0 : Math.round(item.productPrice * 100) * item.quantity), 0)
  return { amount: cents / 100, unknown: items.filter(item => item.productPrice == null).length }
}
