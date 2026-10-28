import { Checkbox, ProgressBar } from '@fluentui/react-components'
import ui from './AppSurface.module.css'

export type ShoppingRow = { id: string; productName: string; quantity: number; productPrice?: number | null; isPurchased: boolean; addedBy?: string; quantityLabel?: string }
const money = (value: number) => new Intl.NumberFormat('es-ES', { style: 'currency', currency: 'EUR' }).format(value)

export function ShoppingListView({ items, onToggle, pendingIds = [], actions, authorName }: {
  items: ShoppingRow[]; onToggle?: (id: string, purchased: boolean) => void; pendingIds?: string[];
  actions?: (item: ShoppingRow) => React.ReactNode; authorName?: (id: string) => string
}) {
  const purchased = items.filter(i => i.isPurchased).length
  return <section className={ui.stack} aria-label="Productos de la compra">
    <div className={ui.summary}><p role="status">{purchased} de {items.length} comprados · {items.length - purchased} pendientes</p>
      <ProgressBar aria-label="Progreso de tu lista" value={items.length ? purchased / items.length : 0} />
    </div>
    {!items.length ? <p>Esta lista todavía no tiene productos. Añade el primero para preparar tu compra.</p> : <ul className={ui.itemList}>
      {items.map(item => <li key={item.id} data-purchased={item.isPurchased}>
        {onToggle ? <Checkbox checked={item.isPurchased} aria-disabled={pendingIds.includes(item.id)}
          aria-label={item.isPurchased ? `Desmarcar comprado: ${item.productName}` : `Marcar comprado: ${item.productName}`}
          onChange={(_, data) => { if (!pendingIds.includes(item.id)) onToggle(item.id, data.checked === true) }} /> : null}
        <div className={ui.itemInfo}><strong className={item.isPurchased ? ui.purchased : undefined}>{item.productName}</strong>
          <span>{item.quantityLabel ?? `Cantidad: ${item.quantity}`} · {item.isPurchased ? 'Comprado' : 'Pendiente'}</span>
          <small>{item.productPrice == null ? 'Sin estimación de precio' : `Precio estimado: ${money(item.productPrice)}`}</small>
          {item.addedBy && authorName ? <small>Añadido por {authorName(item.addedBy)}</small> : null}
          {pendingIds.includes(item.id) ? <small role="status">Guardando…</small> : null}
        </div>
        {actions?.(item)}
      </li>)}
    </ul>}
  </section>
}
