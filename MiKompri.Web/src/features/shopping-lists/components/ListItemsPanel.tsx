import { Button, Field, Input, Select } from '@fluentui/react-components'
import { useRef, useState } from 'react'
import type { CatalogProduct } from '../../product-catalog/api/productCatalogApi'
import type { ListItemInput } from '../api/shoppingListsApi'

type ListItemsPanelProps = {
  products: CatalogProduct[]
  onAddItem: (item: ListItemInput) => Promise<void>
}

export function ListItemsPanel({ products, onAddItem }: ListItemsPanelProps) {
  const [productId, setProductId] = useState('')
  const [quantity, setQuantity] = useState('1')
  const [price, setPrice] = useState('0')
  const [pending, setPending] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const submitting = useRef(false)
  const product = products.find((candidate) => candidate.id === productId && candidate.isActive !== false)
  const validQuantity = quantity.trim() !== '' && Number.isSafeInteger(Number(quantity)) && Number(quantity) > 0
  const validPrice = price.trim() !== '' && Number.isFinite(Number(price)) && Number(price) >= 0

  return (
    <form
      style={{ display: 'grid', gap: '0.75rem', minWidth: 0 }}
      onSubmit={async (event) => {
        event.preventDefault()
        if (submitting.current || !product || !validQuantity || !validPrice) return
        submitting.current = true
        setPending(true)
        setError('')
        setSuccess('')
        try {
          await onAddItem({ productId: product.id, productName: product.name, quantity: Number(quantity), price: Number(price) })
          setProductId('')
          setQuantity('1')
          setPrice('0')
          setSuccess(`${product.name} se agregó a esta lista.`)
        } catch (cause) {
          setError(cause instanceof Error ? cause.message : 'No fue posible agregar el ítem. Inténtalo de nuevo.')
        } finally {
          submitting.current = false
          setPending(false)
        }
      }}
    >
      <Field label="Producto" required>
        <Select value={productId} disabled={pending || products.length === 0} onChange={(_, data) => setProductId(data.value)} required>
          <option value="">Selecciona un producto</option>
          {products.filter((candidate) => candidate.isActive !== false).map((candidate) => (
            <option key={candidate.id} value={candidate.id}>{candidate.name}</option>
          ))}
        </Select>
      </Field>
      <Field label="Cantidad" required validationMessage={!validQuantity ? 'Introduce una cantidad entera mayor que cero.' : undefined}>
        <Input type="number" min={1} step={1} required disabled={pending} value={quantity} onChange={(_, data) => setQuantity(data.value)} />
      </Field>
      <Field label="Precio estimado" required validationMessage={!validPrice ? 'Introduce un precio igual o mayor que cero.' : undefined}>
        <Input type="number" min={0} step="0.01" required disabled={pending} value={price} onChange={(_, data) => setPrice(data.value)} />
      </Field>
      {error ? <p role="alert">{error}</p> : null}
      {success ? <p role="status">{success}</p> : null}
      <Button appearance="primary" type="submit" disabled={pending || !product || !validQuantity || !validPrice}>
        {pending ? 'Agregando ítem…' : 'Agregar ítem'}
      </Button>
    </form>
  )
}
