import { Button, ProgressBar } from '@fluentui/react-components'
import ui from '../../../shared/ui/AppSurface.module.css'

export type PurchaseItem = { id: string; productName: string; quantity: number; productPrice?: number | null; isPurchased: boolean }
const money = (price: number) => new Intl.NumberFormat('es-ES', { style: 'currency', currency: 'EUR' }).format(price)
export function PurchaseItems({ items, onMark, pendingId }: { items: PurchaseItem[]; onMark?: (id: string) => void; pendingId?: string }) {
  const purchased = items.filter(item => item.isPurchased).length
  return <section className={ui.stack} aria-label="Productos de la compra">
    <div className={ui.summary}><p role="status">{purchased} de {items.length} comprados · {items.length - purchased} pendientes</p>
      <ProgressBar aria-label="Progreso de tu lista" value={items.length ? purchased / items.length : 0} />
    </div>
    {!items.length ? <p>Esta lista todavía no tiene ítems. Añade productos para preparar tu compra.</p> : <ul className={ui.itemList}>
      {items.map(item => <li key={item.id}>
        <div className={ui.itemInfo}><strong>{item.productName}</strong><span>Cantidad: {item.quantity} · {item.isPurchased ? 'Comprado' : 'Pendiente'}</span>
          {item.productPrice != null ? <small>Precio estimado: {money(item.productPrice)}</small> : null}
        </div>
        {onMark ? <Button aria-label={item.isPurchased ? `Comprado: ${item.productName}` : `Marcar comprado: ${item.productName}`} aria-disabled={item.isPurchased || Boolean(pendingId)} onClick={() => { if (!item.isPurchased && !pendingId) onMark(item.id) }}>
          {pendingId === item.id ? 'Guardando…' : item.isPurchased ? '✓ Comprado' : 'Marcar comprado'}
        </Button> : null}
      </li>)}
    </ul>}
  </section>
}
