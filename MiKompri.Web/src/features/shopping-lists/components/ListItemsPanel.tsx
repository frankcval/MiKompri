import { Button, Field, Input, Text } from '@fluentui/react-components'
import { useState } from 'react'

type ItemDraft = {
  productName: string
  quantity: number
  price: number
}

type ListItemsPanelProps = {
  onAddItem?: (item: ItemDraft) => Promise<void> | void
}

export function ListItemsPanel({ onAddItem }: ListItemsPanelProps) {
  const [productName, setProductName] = useState('')
  const [quantity, setQuantity] = useState(1)
  const [price, setPrice] = useState(0)

  const handleSubmit = async () => {
    if (!productName.trim()) {
      return
    }

    await onAddItem?.({ productName, quantity, price })
    setProductName('')
    setQuantity(1)
    setPrice(0)
  }

  return (
    <section style={{ display: 'grid', gap: '0.75rem' }}>
      <Text weight="semibold">Ítems</Text>
      <Field label="Producto">
        <Input value={productName} onChange={(_, data) => setProductName(data.value)} />
      </Field>
      <Field label="Cantidad">
        <Input
          type="number"
          value={String(quantity)}
          onChange={(_, data) => setQuantity(Number(data.value) || 1)}
        />
      </Field>
      <Field label="Precio estimado">
        <Input type="number" value={String(price)} onChange={(_, data) => setPrice(Number(data.value) || 0)} />
      </Field>
      <Button appearance="primary" onClick={handleSubmit}>
        Agregar ítem
      </Button>
    </section>
  )
}
