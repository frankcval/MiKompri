import { useEffect, useRef, useState } from 'react'
import { Controller, useForm, useWatch } from 'react-hook-form'
import { z } from 'zod'
import { Button, Field, Input, Select } from '@fluentui/react-components'
import type { CatalogProduct } from '../../product-catalog/api/productCatalogApi'
import type { ListItemInput } from '../api/shoppingListsApi'
import ui from '../../../shared/ui/AppSurface.module.css'
import { readDraft, writeDraft } from '../../../auth/session/draftStore'

const schema = z.object({ productId: z.string().uuid(), quantity: z.coerce.number().int().positive(), price: z.number().finite().nonnegative().nullable() })
export function ListItemsPanel({ products, onAddItem, accountKey = '', draftScope = 'add' }: { products: CatalogProduct[]; onAddItem: (item: ListItemInput) => Promise<void>; accountKey?: string; draftScope?: string }) {
  const defaults = { productId: '', quantity: '1', price: '' }
  const { control, handleSubmit, reset } = useForm({ defaultValues: accountKey ? readDraft(accountKey, draftScope, defaults) : defaults })
  const value = useWatch({ control })
  useEffect(() => { if (accountKey) writeDraft(accountKey, draftScope, value) }, [accountKey, draftScope, value])
  const product = products.find(p => p.id === value.productId && p.isActive !== false)
  const validQuantity = (value.quantity ?? '').trim() !== '' && Number.isSafeInteger(Number(value.quantity)) && Number(value.quantity) > 0
  const validPrice = !(value.price ?? '').trim() || (Number.isFinite(Number(value.price?.replace(',', '.'))) && Number(value.price?.replace(',', '.')) >= 0)
  const [pending, setPending] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const submitting = useRef(false)
  return <form className={ui.form} onSubmit={event => { void handleSubmit(async values => {
    const parsed = schema.safeParse({ ...values, price: values.price.trim() ? Number(values.price.replace(',', '.')) : null })
    if (submitting.current || !product || !parsed.success || !validQuantity || !validPrice) return
    submitting.current = true; setPending(true); setError(''); setSuccess('')
    try { await onAddItem({ ...parsed.data, productName: product.name }); reset(defaults); setSuccess(`${product.name} se agregó a esta lista.`) }
    catch (cause) { setError(cause instanceof Error ? cause.message : 'No fue posible agregar el ítem. Inténtalo de nuevo.') }
    finally { submitting.current = false; setPending(false) }
  })(event) }}>
    <Field label="Producto" required><Controller control={control} name="productId" render={({ field }) => <Select select={{ ref: field.ref }} name={field.name} value={field.value} onBlur={field.onBlur} onChange={(_, data) => field.onChange(data.value)} required disabled={pending || !products.length}>
      <option value="">Selecciona un producto</option>{products.filter(p => p.isActive !== false).map(p => <option key={p.id} value={p.id}>{p.name}</option>)}
    </Select>} /></Field>
    <Field label="Cantidad" required validationMessage={!validQuantity ? 'Introduce una cantidad entera mayor que cero.' : undefined}>
      <Controller control={control} name="quantity" render={({ field }) => <Input input={{ ref: field.ref }} name={field.name} value={field.value} onBlur={field.onBlur} onChange={(_, data) => field.onChange(data.value)} type="number" min={1} step={1} required disabled={pending} />} />
    </Field>
    <Field label="Precio estimado" hint="Opcional. Déjalo vacío si todavía no lo sabes." validationMessage={!validPrice ? 'Introduce un precio igual o mayor que cero.' : undefined}>
      <Controller control={control} name="price" render={({ field }) => <Input input={{ ref: field.ref }} name={field.name} value={field.value} onBlur={field.onBlur} onChange={(_, data) => field.onChange(data.value)} inputMode="decimal" disabled={pending} />} />
    </Field>
    {error ? <p role="alert">{error}</p> : null}{success ? <p role="status">{success}</p> : null}
    <Button type="submit" appearance="primary" disabled={pending || !product || !validQuantity || !validPrice}>{pending ? 'Agregando ítem…' : 'Agregar ítem'}</Button>
  </form>
}
