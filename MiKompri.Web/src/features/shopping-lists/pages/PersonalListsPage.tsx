import { lazy, Suspense, useRef, useState } from 'react'
import { Button, Spinner, Tab, TabList } from '@fluentui/react-components'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, useNavigate, useParams, useSearchParams } from 'react-router-dom'
import { env } from '../../../shared/config/env'
import { PageHeader } from '../../../shared/ui/PageHeader'
import { describeError } from '../../../shared/api/request'
import { addPersonalListItem, createPersonalList, getMyPersonalLists, setPurchaseState, renamePersonalList, deletePersonalList, editPersonalItem, deletePersonalItem } from '../api/shoppingListsApi'
import { CreateListDialog } from '../components/CreateListDialog'
import { AddProductsDialog } from '../components/AddProductsDialog'
import { PurchaseItems } from '../components/PurchaseItems'
import { ConfirmAction, RenameListAction, EditItemAction, ProductOptions } from '../components/ManageListActions'
import { useListSession } from '../hooks/useListSession'
import ui from '../../../shared/ui/AppSurface.module.css'
import { visibleLists } from '../listPresentation'
import { ListOverview, ListOverviewControls } from '../components/ListOverview'
import { PurchaseCaptureDialog as PurchaseCapture } from '../../settlements/components/PurchaseCaptureDialog'
const PersonalExpenses = lazy(() => import('../../settlements/components/PurchaseCapture').then(module => ({ default: module.PersonalExpenses })))

export function PersonalListsPage() {
  const { listId } = useParams<{ listId: string }>()
  const { account } = useListSession()
  return <PersonalListScreen key={`${account?.homeAccountId}:${listId ?? 'index'}`} />
}

function PersonalListScreen() {
  const { account, token, write } = useListSession()
  const client = useQueryClient()
  const navigate = useNavigate()
  const { listId } = useParams<{ listId: string }>()
  const [params, setParams] = useSearchParams()
  const [capture, setCapture] = useState('')
  const captureTrigger = useRef<HTMLElement | null>(null)
  const closeCapture = () => { setCapture(''); requestAnimationFrame(() => { if (captureTrigger.current?.isConnected) captureTrigger.current.focus() }) }
  const [pendingIds, setPendingIds] = useState<string[]>([])
  const pending = useRef(new Set<string>())
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')
  const key = ['personal-lists', account?.homeAccountId]
  const lists = useQuery({ queryKey: key, enabled: Boolean(account), retry: false,
    queryFn: async () => getMyPersonalLists(env.shoppingListApiBaseUrl, await token()) })
  const selected = lists.data?.find(list => list.id === listId)
  const visible = visibleLists(lists.data ?? [], params.get('view') === 'history', params.get('sort') ?? 'newest')
  if (!account) return <p>No hay sesión activa.</p>
  const refresh = async () => { await client.invalidateQueries({ queryKey: key }); await client.invalidateQueries({ queryKey: ['all-lists'] }) }
  return <section className={ui.page}>
    {!listId ? <>
      <div className={ui.detailHeader}><PageHeader title="Listas personales" description="Elige una lista o prepara tu próxima compra." />
        <CreateListDialog title="Crear lista personal" initialOpen={params.get('create') === '1'} onCreate={async name => {
          const id = await write(token => createPersonalList(env.shoppingListApiBaseUrl, token, { name }))
          await refresh(); navigate(`/personal-lists/${id}`)
        }} />
      </div>
      {lists.isSuccess ? <section aria-label="Mis listas personales" className={ui.stack}>
        <ListOverviewControls />
        {visible.map(list => <Link className={ui.listCard} key={list.id} to={`/personal-lists/${list.id}`}><strong>{list.name}</strong><ListOverview list={list} /></Link>)}
        {lists.data.length > 0 && !visible.length ? <p>No hay listas personales en esta vista. Cambia entre Activas e Historial.</p> : null}
        {!lists.data.length ? <p>Sin listas personales todavía. Crea una lista para empezar.</p> : <p>Selecciona una lista para ver sus productos y preparar la compra.</p>}
      </section> : null}
    </> : <Link to="/lists">Volver a mis listas</Link>}
    {listId && !selected ? <h1>{lists.isSuccess ? 'Lista no disponible' : 'Lista personal'}</h1> : null}
    {lists.isPending ? <Spinner label="Cargando listas…" /> : lists.isError ? <div role="alert">{describeError(lists.error)} <Button onClick={() => void lists.refetch()}>Reintentar</Button></div> : null}
    {listId && lists.isSuccess && !selected ? <p role="alert">Esta lista no está disponible en tus listas personales.</p> : null}
    {selected ? <section className={ui.shoppingDetail} aria-label={`Detalle de ${selected.name}`}>
      <div className={ui.detailHeader}><div className={ui.detailTitle}><h1>{selected.name}</h1><p>Lista personal</p></div>
        <AddProductsDialog disabled={Boolean(capture) || pendingIds.length > 0} key={`${account.homeAccountId}:${selected.id}`} returnTo={`/personal-lists/${selected.id}`} onAdd={async item => {
          await write(token => addPersonalListItem(env.shoppingListApiBaseUrl, token, selected.id, item)); await refresh()
        }} />
      </div>
      <div className={ui.actions}><RenameListAction name={selected.name} accountKey={account.homeAccountId} scope={selected.id} onRename={async name => { await write(t => renamePersonalList(env.shoppingListApiBaseUrl, t, selected.id, name)); await refresh() }} />
        <ConfirmAction label="Eliminar lista" description={`Se eliminarán «${selected.name}» y sus productos. Esta acción no se puede deshacer. Las listas con gastos registrados se conservan para proteger su historial.`} onConfirm={async () => { await write(t => deletePersonalList(env.shoppingListApiBaseUrl, t, selected.id)); await refresh(); navigate('/lists') }} />
      </div>
      {error ? <p role="alert">{error} <Button onClick={() => void lists.refetch()}>Actualizar lista</Button></p> : null}{message ? <p role="status">{message}</p> : null}
      <TabList selectedValue={params.get('tab') === 'expenses' ? 'expenses' : 'shopping'} aria-label="Vistas de la lista" onTabSelect={(_, d) => { const next = new URLSearchParams(params); next.set('tab', String(d.value)); setParams(next) }}><Tab value="shopping">Compra</Tab><Tab value="expenses">Gastos</Tab></TabList>
      {params.get('tab') === 'expenses' ? <Suspense fallback={<Spinner label="Cargando gastos…" />}><PersonalExpenses listId={selected.id} items={selected.items ?? []} /></Suspense> : <PurchaseItems items={selected.items ?? []} pendingIds={pendingIds} onToggle={async (itemId, value) => {
        if (pending.current.has(itemId)) return
          if (value) captureTrigger.current = document.activeElement as HTMLElement | null
        pending.current.add(itemId); setPendingIds([...pending.current]); setError(''); setMessage('')
          try { await write(t => setPurchaseState(env.shoppingListApiBaseUrl, t, selected.id, itemId, value)); if (value) setCapture(itemId); await refresh(); setMessage(value ? 'Producto marcado como comprado.' : 'Producto pendiente de nuevo. Los gastos se conservan.') }
        catch (cause) { setError(describeError(cause)) }
        finally { pending.current.delete(itemId); setPendingIds([...pending.current]) }
      }} actions={item => <ProductOptions name={item.productName}>
        <Button onClick={event => { captureTrigger.current = event.currentTarget; setCapture(item.id) }}>Datos de compra de {item.productName}</Button>
        <EditItemAction item={item} accountKey={account.homeAccountId} onEdit={async (quantity, price) => { await write(t => editPersonalItem(env.shoppingListApiBaseUrl, t, selected.id, item.id, quantity, price)); await refresh() }} />
        <ConfirmAction label={`Eliminar ${item.productName}`} text="Eliminar" description="Se eliminará este producto de la lista." onConfirm={async () => { await write(t => deletePersonalItem(env.shoppingListApiBaseUrl, t, selected.id, item.id)); await refresh(); document.getElementById('main-content')?.focus() }} />
      </ProductOptions>} />}
      {selected.items?.find(item => item.id === capture) ? <PurchaseCapture key={`${account.homeAccountId}:${capture}`} listId={selected.id} item={selected.items.find(item => item.id === capture)!} restoreFocusRef={captureTrigger} onClose={closeCapture} /> : null}
    </section> : null}
  </section>
}
