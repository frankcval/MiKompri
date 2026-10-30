import { useRef, useState } from 'react'
import { Button, Field, Input } from '@fluentui/react-components'
import { useQueryClient } from '@tanstack/react-query'
import { createCatalogProduct, getCatalogProducts } from '../../product-catalog/api/productCatalogApi'
import { useListSession } from '../hooks/useListSession'
import { useDraft } from '../../../auth/session/draftStore'
import { describeError } from '../../../shared/api/request'
import { env } from '../../../shared/config/env'
import type { ListItemInput } from '../api/shoppingListsApi'
import ui from '../../../shared/ui/AppSurface.module.css'

type Draft = { name: string; unit: string; quantity: string; price: string; createdId: string; createdName: string; uncertain: boolean }
export function InlineProductCreation({ scope, onAdd, onPendingChange, initialName = '' }: { scope: string; onAdd: (item: ListItemInput) => Promise<void>; onPendingChange?: (pending: boolean) => void; initialName?: string }) {
  const { account, token, write } = useListSession()
  const client = useQueryClient()
  const [draft, setDraft] = useDraft<Draft>(account?.homeAccountId ?? '', `new-product:${scope}`, { name: initialName, unit: 'unidad', quantity: '1', price: '', createdId: '', createdName: '', uncertain: false })
  const [pending, setPending] = useState(false)
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')
  const submitting = useRef(false)
  const field = (name: keyof Draft, value: string) => setDraft(current => ({ ...current, [name]: value }))
  return <form className={ui.form} onSubmit={async e => {
    e.preventDefault(); if (submitting.current) return
    const quantity = Number(draft.quantity)
    const price = draft.price.trim() ? Number(draft.price.replace(',', '.')) : null
    if (!draft.name.trim() || !draft.unit.trim() || !Number.isSafeInteger(quantity) || quantity < 1 || (price != null && (!Number.isFinite(price) || price < 0))) { setError('Revisa nombre, unidad, cantidad y precio.'); return }
    submitting.current = true; setPending(true); onPendingChange?.(true); setError(''); setMessage('')
    let id = draft.createdId
    let name = draft.createdName || draft.name.trim()
    try {
      // Reconcile creation before retrying after a lost response or a duplicate.
      if (!id) {
        const products = await getCatalogProducts(env.productCatalogApiBaseUrl, await token())
        const existing = products.find(p => p.isActive !== false && p.name.trim().toLocaleLowerCase() === draft.name.trim().toLocaleLowerCase() && (p.purchaseUnit ?? '').trim().toLocaleLowerCase() === draft.unit.trim().toLocaleLowerCase())
        if (existing) { id = existing.id; name = existing.name }
        else {
          const product = await write(t => createCatalogProduct(env.productCatalogApiBaseUrl, t, { name: draft.name.trim(), purchaseUnit: draft.unit.trim() }))
          id = product.id; name = product.name
        }
        setDraft(current => ({ ...current, createdId: id, createdName: name }))
        await client.invalidateQueries({ queryKey: ['catalog-products'] })
      }
      if (draft.uncertain) { setError('Comprueba primero si el producto ya está en la lista. Para repetir la incorporación, confirma la revisión.'); return }
      try { await onAdd({ productId: id, productName: name, quantity, price }) }
      catch (cause) {
        const status = (cause as { status?: number }).status
        setDraft(current => ({ ...current, createdId: id, createdName: name, uncertain: status == null }))
        throw cause
      }
      setDraft({ name: '', unit: 'unidad', quantity: '1', price: '', createdId: '', createdName: '', uncertain: false })
      setMessage(`${name} se ha añadido a la lista.`)
    } catch (cause) { setError(`${id ? 'El producto ya está disponible en el catálogo. ' : ''}${describeError(cause)}`) }
    finally { submitting.current = false; setPending(false); onPendingChange?.(false) }
  }}>
    <h3>Crear y añadir a esta lista</h3>
    <Field label="Nombre del producto" required><Input value={draft.name} required maxLength={120} disabled={pending || Boolean(draft.createdId)} onChange={(_, d) => field('name', d.value)} /></Field>
    <Field label="Unidad de compra" required hint="Por ejemplo: unidad, kg o litro."><Input value={draft.unit} required maxLength={30} disabled={pending || Boolean(draft.createdId)} onChange={(_, d) => field('unit', d.value)} /></Field>
    <Field label="Cantidad" required><Input value={draft.quantity} type="number" min={1} step={1} required disabled={pending} onChange={(_, d) => field('quantity', d.value)} /></Field>
    <Field label="Precio estimado (opcional)"><Input value={draft.price} inputMode="decimal" disabled={pending} onChange={(_, d) => field('price', d.value)} /></Field>
    {error ? <p role="alert">{error}</p> : null}{message ? <p role="status">{message}</p> : null}
    {draft.uncertain ? <Button disabled={pending} onClick={() => setDraft(current => ({ ...current, uncertain: false }))}>He revisado la lista; permitir otro intento</Button> : null}
    <Button type="submit" appearance="primary" disabled={pending || draft.uncertain}>{pending ? 'Guardando…' : draft.createdId ? 'Reintentar incorporación' : 'Crear y añadir'}</Button>
  </form>
}
