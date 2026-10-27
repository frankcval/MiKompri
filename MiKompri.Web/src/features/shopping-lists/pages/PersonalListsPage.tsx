import { useRef, useState } from 'react'
import { Button, Spinner } from '@fluentui/react-components'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { env } from '../../../shared/config/env'
import { PageHeader } from '../../../shared/ui/PageHeader'
import { addPersonalListItem, createPersonalList, getMyPersonalLists, markPersonalListItem } from '../api/shoppingListsApi'
import { CreateListDialog } from '../components/CreateListDialog'
import { AddProductsDialog } from '../components/AddProductsDialog'
import { PurchaseItems } from '../components/PurchaseItems'
import { useListSession } from '../hooks/useListSession'
import ui from '../../../shared/ui/AppSurface.module.css'

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
  const [pendingId, setPendingId] = useState('')
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')
  const marking = useRef(false)
  const key = ['personal-lists', account?.homeAccountId]
  const lists = useQuery({ queryKey: key, enabled: Boolean(account), retry: false,
    queryFn: async () => getMyPersonalLists(env.shoppingListApiBaseUrl, await token()) })
  const selected = lists.isSuccess ? lists.data.find(list => list.id === listId) : undefined
  if (!account) return <p>No hay sesión activa.</p>
  const refresh = () => client.invalidateQueries({ queryKey: key })
  return <section className={ui.page}>
    {!listId ? <>
      <div className={ui.detailHeader}><PageHeader title="Listas personales" description="Elige una lista o prepara tu próxima compra." />
        <CreateListDialog title="Crear lista personal" onCreate={async name => {
          const id = await write(token => createPersonalList(env.shoppingListApiBaseUrl, token, { name }))
          await refresh(); navigate(`/personal-lists/${id}`)
        }} />
      </div>
      {lists.isSuccess ? <section aria-label="Mis listas personales" className={ui.stack}>
        {lists.data.map(list => <Link className={ui.listLink} key={list.id} to={`/personal-lists/${list.id}`}>{list.name}</Link>)}
        {!lists.data.length ? <p>Sin listas personales todavía. Crea una lista para empezar.</p> : <p>Selecciona una lista para ver sus ítems y agregar productos.</p>}
      </section> : null}
    </> : <Link to="/personal-lists">Volver a mis listas</Link>}
    {listId && (lists.isPending || lists.isError) ? <h1>Lista personal</h1> : null}
    {lists.isPending ? <Spinner label="Cargando listas" /> : lists.isError ? <div role="alert">No fue posible cargar listas personales. <Button onClick={() => void lists.refetch()}>Reintentar</Button></div> : null}
    {listId && lists.isSuccess && !selected ? <><h1>Lista no disponible</h1><p role="alert">Esta lista no está disponible en tus listas personales.</p></> : null}
    {selected ? <section className={ui.panel} aria-label={`Detalle de ${selected.name}`}>
      <div className={ui.detailHeader}><div className={ui.detailTitle}><h1>{selected.name}</h1><p>Lista personal</p></div>
        <AddProductsDialog key={`${account.homeAccountId}:${selected.id}`} returnTo={`/personal-lists/${selected.id}`} onAdd={async item => {
          await write(token => addPersonalListItem(env.shoppingListApiBaseUrl, token, selected.id, item)); await refresh()
        }} />
      </div>
      {error ? <p role="alert">{error}</p> : null}{message ? <p role="status">{message}</p> : null}
      <PurchaseItems items={selected.items ?? []} pendingId={pendingId} onMark={async itemId => {
        if (marking.current || !selected.items?.some(item => item.id === itemId && !item.isPurchased)) return
        marking.current = true
        setPendingId(itemId); setError(''); setMessage('')
        try { await write(token => markPersonalListItem(env.shoppingListApiBaseUrl, token, selected.id, itemId)); await refresh(); setMessage('Producto marcado como comprado.') }
        catch (cause) { setError(cause instanceof Error ? cause.message : 'No fue posible marcar la compra. Inténtalo de nuevo.') }
        finally { marking.current = false; setPendingId('') }
      }} />
    </section> : null}
  </section>
}
