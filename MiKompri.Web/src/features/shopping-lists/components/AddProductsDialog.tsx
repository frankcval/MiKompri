import { useState } from 'react'
import { Button, Dialog, DialogActions, DialogBody, DialogContent, DialogSurface, DialogTitle, DialogTrigger, Spinner } from '@fluentui/react-components'
import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { getCatalogProducts } from '../../product-catalog/api/productCatalogApi'
import { useListSession } from '../hooks/useListSession'
import { env } from '../../../shared/config/env'
import { ListItemsPanel } from './ListItemsPanel'
import type { ListItemInput } from '../api/shoppingListsApi'
import ui from '../../../shared/ui/AppSurface.module.css'

export function AddProductsDialog({ returnTo, onAdd }: { returnTo: string; onAdd: (item: ListItemInput) => Promise<void> }) {
  const [open, setOpen] = useState(false)
  const [pending, setPending] = useState(false)
  const { account, token } = useListSession()
  const products = useQuery({ queryKey: ['catalog-products', account?.homeAccountId], enabled: open && Boolean(account), retry: false,
    queryFn: async () => getCatalogProducts(env.productCatalogApiBaseUrl, await token()) })
  return <Dialog open={open} onOpenChange={(_, data) => { if (!pending) setOpen(data.open) }}>
    <DialogTrigger disableButtonEnhancement><Button appearance="primary">Añadir productos</Button></DialogTrigger>
    <DialogSurface className={`${ui.page} ${ui.panel}`}><DialogBody><DialogTitle>Añadir productos a la lista</DialogTitle>
      <DialogContent className={ui.stack}>
        {products.isPending ? <Spinner label="Cargando productos…" /> : products.isError ? <div role="alert">No fue posible cargar los productos del catálogo. <Button onClick={() => void products.refetch()}>Reintentar</Button></div>
          : products.data.some(p => p.isActive !== false) ? <ListItemsPanel products={products.data} onAddItem={async item => {
            setPending(true); try { await onAdd(item) } finally { setPending(false) }
          }} /> : <p>No hay productos disponibles en el catálogo. Crea un producto para añadirlo a tu lista.</p>}
        {!pending ? <Link to={`/product-catalog?returnTo=${encodeURIComponent(returnTo)}`}>Crear producto en el catálogo</Link> : null}
      </DialogContent><DialogActions><Button disabled={pending} onClick={() => setOpen(false)}>Terminar</Button></DialogActions>
    </DialogBody></DialogSurface>
  </Dialog>
}
