import { useLocalToday } from '../../../shared/dates/useLocalToday'
import { useState } from 'react'
import { Button, Field, Input, Select } from '@fluentui/react-components'
import { useQueryClient } from '@tanstack/react-query'
import { useListSession } from '../../shopping-lists/hooks/useListSession'
import { ConfirmAction } from '../../shopping-lists/components/ManageListActions'
import { describeError } from '../../../shared/api/request'
import { env } from '../../../shared/config/env'
import { deactivateMarket, registerProductPrice, saveMarket, updateCatalogProduct, type CatalogProduct, type Market } from '../api/productCatalogApi'
import ui from '../../../shared/ui/AppSurface.module.css'

export function CatalogEditor({ product, market, onDone }: { product?: CatalogProduct; market?: Market; onDone?: () => void }) {
  const { write } = useListSession()
  const client = useQueryClient()
  const [name, setName] = useState(product?.name ?? market?.name ?? '')
  const [extra, setExtra] = useState(product?.purchaseUnit ?? market?.locationHint ?? '')
  const [pending, setPending] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  return <form className={ui.form} aria-label={product ? `Editar ${product.name}` : market ? `Editar mercado ${market.name}` : 'Crear mercado'} onSubmit={async event => {
    event.preventDefault(); if (pending || !name.trim() || (product && !extra.trim())) return
    setPending(true); setError(''); setSuccess('')
    try {
      await write(t => product ? updateCatalogProduct(env.productCatalogApiBaseUrl, t, product.id, { name: name.trim(), purchaseUnit: extra.trim() }) : saveMarket(env.productCatalogApiBaseUrl, t, { name: name.trim(), locationHint: extra.trim() || null }, market?.id))
      await client.invalidateQueries({ queryKey: [product ? 'catalog-products' : 'catalog-markets'] })
      await client.invalidateQueries({ queryKey: ['catalog-price-history'] })
      setSuccess(product ? 'Producto actualizado.' : 'Mercado guardado.'); onDone?.()
    } catch (cause) { setError(describeError(cause)) } finally { setPending(false) }
  }}>
    <Field label={product ? 'Nombre del producto' : 'Nombre del mercado'} required><Input value={name} maxLength={120} required disabled={pending} onChange={(_, d) => setName(d.value)} /></Field>
    <Field label={product ? 'Unidad de compra' : 'Ubicación (opcional)'} required={Boolean(product)}><Input value={extra} maxLength={product ? 30 : 180} required={Boolean(product)} disabled={pending} onChange={(_, d) => setExtra(d.value)} /></Field>
    {product ? <p>Los nombres de listas y sus precios históricos se conservan. Cambiar la unidad no convierte los precios anteriores.</p> : null}
    {error ? <p role="alert">{error}</p> : null}{success ? <p role="status">{success}</p> : null}
    <Button type="submit" disabled={pending} appearance="primary">{pending ? 'Guardando…' : product ? 'Guardar producto' : 'Guardar mercado'}</Button>
  </form>
}

export function MarketManagement({ markets }: { markets: Market[] }) {
  const { write } = useListSession(); const client = useQueryClient()
  const [editing, setEditing] = useState('')
  return <section className={ui.stack}><h3>Gestionar mercados</h3>
    <CatalogEditor />
    {markets.map(market => <section className={ui.panel} key={market.id}>
      <p><strong>{market.name}</strong>{market.isActive === false ? ' · Inactivo' : ''} {market.locationHint}</p>
      {market.isActive !== false ? <div className={ui.actions}><Button onClick={() => setEditing(editing === market.id ? '' : market.id)}>Editar mercado {market.name}</Button>
        <ConfirmAction label={`Desactivar ${market.name}`} description="Se conservará su historial. No estará disponible para nuevos precios o compras." onConfirm={async () => { await write(t => deactivateMarket(env.productCatalogApiBaseUrl, t, market.id)); await client.invalidateQueries({ queryKey: ['catalog-markets'] }) }} />
      </div> : null}
      {editing === market.id ? <CatalogEditor key={market.id} market={market} onDone={() => setEditing('')} /> : null}
    </section>)}
  </section>
}

export function RegisterPriceForm({ product, markets }: { product: CatalogProduct; markets: Market[] }) {
  const { write } = useListSession(); const client = useQueryClient()
  const today = useLocalToday()
  const [marketId, setMarketId] = useState(''); const [amount, setAmount] = useState(''); const [date, setDate] = useState(today)
  const [pending, setPending] = useState(false); const [error, setError] = useState(''); const [success, setSuccess] = useState('')
  return <form className={ui.form} aria-label="Registrar precio de catálogo" onSubmit={async event => {
    event.preventDefault(); if (pending) return
    const price = Number(amount.replace(',', '.'))
    if (!marketId || !Number.isFinite(price) || price <= 0 || !date || date > today) { setError('Revisa mercado, fecha e importe positivo.'); return }
    setPending(true); setError(''); setSuccess('')
    try { await write(t => registerProductPrice(env.productCatalogApiBaseUrl, t, { catalogProductId: product.id, marketId, effectiveDate: date, priceAmount: price })); setAmount(''); setSuccess('Precio registrado en el historial.'); await client.invalidateQueries({ queryKey: ['catalog-price-history'] }) }
    catch (cause) { setError(describeError(cause)) } finally { setPending(false) }
  }}><h3>Registrar precio de {product.name}</h3><p>Precio por {product.purchaseUnit ?? 'unidad'}. Este dato no registra un gasto.</p>
    <Field label="Mercado del precio" required><Select value={marketId} required disabled={pending} onChange={(_, d) => setMarketId(d.value)}><option value="">Selecciona mercado</option>{markets.filter(m => m.isActive !== false).map(m => <option key={m.id} value={m.id}>{m.name}</option>)}</Select></Field>
    <Field label="Fecha del precio" required><Input type="date" value={date} required max={today} disabled={pending} onChange={(_, d) => setDate(d.value)} /></Field>
    <Field label="Precio unitario (EUR)" required><Input type="number" min="0.01" step="0.01" value={amount} required disabled={pending} onChange={(_, d) => setAmount(d.value)} /></Field>
    {error ? <p role="alert">{error}</p> : null}{success ? <p role="status">{success}</p> : null}<Button type="submit" disabled={pending}>Registrar precio</Button>
  </form>
}
