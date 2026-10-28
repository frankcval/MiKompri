import { ShoppingListView, type ShoppingRow } from '../../../shared/ui/ShoppingListView'
export type PurchaseItem = ShoppingRow
export function PurchaseItems({ items, onMark, onToggle, pendingId, pendingIds, actions, authorName }: {
  items: PurchaseItem[]; onMark?: (id: string) => void; onToggle?: (id: string, value: boolean) => void;
  pendingId?: string; pendingIds?: string[]; actions?: (item: PurchaseItem) => React.ReactNode; authorName?: (id: string) => string
}) {
  return <ShoppingListView items={items} onToggle={onToggle ?? (onMark ? (id, purchased) => { if (purchased) onMark(id) } : undefined)} pendingIds={pendingIds ?? (pendingId ? [pendingId] : [])} actions={actions} authorName={authorName} />
}
