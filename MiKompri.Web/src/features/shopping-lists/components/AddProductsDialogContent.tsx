import { useEffect, useState } from 'react'
import { Button, Spinner } from '@fluentui/react-components'
import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { getCatalogProducts } from '../../product-catalog/api/productCatalogApi'
import { useListSession } from '../hooks/useListSession'
import { env } from '../../../shared/config/env'
import { ListItemsPanel } from './ListItemsPanel'
import type { ListItemInput } from '../api/shoppingListsApi'
import ui from '../../../shared/ui/AppSurface.module.css'
import { ProductSearch } from './ProductSearch'
import { InlineProductCreation } from './InlineProductCreation'

export function AddProductsDialogContent({ returnTo, onAdd, pending, setPending, onReady }: {
  returnTo: string; onAdd: (item: ListItemInput) => Promise<void>; pending: boolean; setPending: (pending: boolean) => void; onReady: (ready: boolean) => void
}) {
  useEffect(() => onReady(true), [onReady])
  const [search, setSearch] = useState('')
  const [creating, setCreating] = useState(false)
  const { account, token } = useListSession()
  const products = useQuery({ queryKey: ['catalog-products', account?.homeAccountId], enabled: Boolean(account), retry: false,
    queryFn: async () => getCatalogProducts(env.productCatalogApiBaseUrl, await token()) })
  return <>
        <div className={ui.actions}><Button disabled={pending} appearance={!creating ? 'primary' : 'secondary'} onClick={() => setCreating(false)}>Elegir producto</Button><Button disabled={pending} appearance={creating ? 'primary' : 'secondary'} onClick={() => setCreating(true)}>Crear producto aquí</Button></div>
        {creating ? <InlineProductCreation scope={returnTo} initialName={search.trim()} onPendingChange={setPending} onAdd={onAdd} /> : <>
        <ProductSearch value={search} onChange={setSearch} disabled={pending} />
        {products.isPending ? <Spinner label="Cargando productos…" /> : products.isError ? <div role="alert">No fue posible cargar los productos del catálogo. <Button onClick={() => void products.refetch()}>Reintentar</Button></div>
          : products.data.some(p => p.isActive !== false) ? <ListItemsPanel accountKey={account?.homeAccountId} draftScope={`add:${returnTo}`} products={products.data.filter(p => p.name.toLocaleLowerCase().includes(search.toLocaleLowerCase()))} onAddItem={async item => {
            setPending(true); try { await onAdd(item) } finally { setPending(false) }
          }} /> : <p>No hay productos disponibles en el catálogo. Crea un producto aquí para añadirlo a tu lista.</p>}
        </>}
        {!pending ? <Link to={`/product-catalog?returnTo=${encodeURIComponent(returnTo)}`}>Crear producto en el catálogo</Link> : null}
  </>
}
