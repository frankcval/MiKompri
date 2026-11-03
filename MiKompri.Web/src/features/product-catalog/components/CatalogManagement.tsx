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
import { AppIcon } from '../../../shared/ui/icons/AppIcon'
import { IconAction } from '../../../shared/ui/icons/IconAction'
import { CatalogDialog } from './CatalogDialog'
import s from './CatalogSurface.module.css'

export function CatalogEditor({ product, market, onDone, onPendingChange }: { product?: CatalogProduct; market?: Market; onDone?: () => void; onPendingChange?: (pending: boolean) => void }) {
  const { write } = useListSession()
  const client = useQueryClient()
  const [name, setName] = useState(product?.name ?? market?.name ?? '')
  const [extra, setExtra] = useState(product?.purchaseUnit ?? market?.locationHint ?? '')
  const [pending, setPending] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  return <form className={ui.form} aria-label={product ? `Editar ${product.name}` : market ? `Editar mercado ${market.name}` : 'Crear mercado'} onSubmit={async event => {
    event.preventDefault(); if (pending || !name.trim() || (product && !extra.trim())) return
    setPending(true); onPendingChange?.(true); setError(''); setSuccess('')
    try {
      await write(t => product ? updateCatalogProduct(env.productCatalogApiBaseUrl, t, product.id, { name: name.trim(), purchaseUnit: extra.trim() }) : saveMarket(env.productCatalogApiBaseUrl, t, { name: name.trim(), locationHint: extra.trim() || null }, market?.id))
      await client.invalidateQueries({ queryKey: [product ? 'catalog-products' : 'catalog-markets'] })
      await client.invalidateQueries({ queryKey: ['catalog-price-history'] })
      setSuccess(product ? 'Producto actualizado.' : 'Mercado guardado.'); onDone?.()
    } catch (cause) { setError(describeError(cause)) } finally { setPending(false); onPendingChange?.(false) }
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
  return <section className={ui.stack}>
    <div className={s.toolbar}><div><h2>Tus mercados</h2><p>Lugares de compra y sus precios registrados.</p></div>
      <CatalogDialog title="Añadir mercado" trigger={<Button icon={<AppIcon name="add" />}>Añadir mercado</Button>}>{(close, onPendingChange) => <CatalogEditor onDone={close} onPendingChange={onPendingChange} />}</CatalogDialog>
    </div>
    {!markets.length ? <p>No hay mercados todavía. Añade uno para registrar precios.</p> : null}
    <ul className={s.products} aria-label="Tus mercados">{markets.map(market => <li className={s.product} key={market.id}>
      <div className={s.identity}><strong>{market.name}{market.isActive === false ? ' · Inactivo' : ''}</strong>{market.locationHint ? <span>{market.locationHint}</span> : null}</div>
      {market.isActive !== false ? <div className={s.actions}>
        <CatalogDialog title={`Editar mercado ${market.name}`} trigger={<IconAction name="edit" label={`Editar mercado ${market.name}`} />}>{(close, onPendingChange) => <CatalogEditor market={market} onDone={close} onPendingChange={onPendingChange} />}</CatalogDialog>
        <ConfirmAction icon="close" label={`Desactivar ${market.name}`} description="Se conservará su historial. No estará disponible para nuevos precios o compras." onConfirm={async () => { await write(t => deactivateMarket(env.productCatalogApiBaseUrl, t, market.id)); await client.invalidateQueries({ queryKey: ['catalog-markets'] }) }} />
      </div> : null}
    </li>)}</ul>
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
