import { accountSessionKey } from '../../../auth/session/accountIdentity'
import { useRef, useState } from 'react'
import { Spinner, Button, TabList, Tab } from '@fluentui/react-components'
import { lazy, Suspense } from 'react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, useNavigate, useParams, useSearchParams } from 'react-router-dom'
import { getSharedListById, addSharedListItem } from '../api/sharedListsApi'
import { updateSharedList, removeSharedList, closeSharedList, updateSharedItem, removeSharedItem } from '../api/sharedListActions'
import { useListSession } from '../../shopping-lists/hooks/useListSession'
import { setPurchaseState } from '../../shopping-lists/api/shoppingListsApi'
import { AddProductsDialog } from '../../shopping-lists/components/AddProductsDialog'
import { PurchaseItems } from '../../shopping-lists/components/PurchaseItems'
import { ConfirmAction, RenameListAction, EditItemAction, ProductOptions } from '../../shopping-lists/components/ManageListActions'
import { GroupPeople } from '../../groups/components/GroupPeople'
import { getMyGroups, getGroupMembers } from '../../groups/api/groupsApi'
import { memberName } from '../../groups/memberName'
import { fetchMyProfile } from '../../profile/api/profileApi'
import { env } from '../../../shared/config/env'
import { describeError } from '../../../shared/api/request'
import ui from '../../../shared/ui/AppSurface.module.css'
import { PurchaseCaptureDialog as PurchaseCapture } from '../../settlements/components/PurchaseCaptureDialog'

const SettlementPage = lazy(() => import('../../settlements/pages/SettlementPage').then(module => ({ default: module.SettlementPage })))

export function SharedListDetailPage() {
  const { listId = '' } = useParams<{ listId: string }>()
  const { account, token, write } = useListSession()
  const client = useQueryClient()
  const navigate = useNavigate()
  const [params, setParams] = useSearchParams()
  const tab = params.get('tab') === 'expenses' ? 'expenses' : params.get('tab') === 'people' ? 'people' : 'shopping'
  const [error, setError] = useState('')
  const [message, setMessage] = useState('')
  const [pendingIds, setPendingIds] = useState<string[]>([])
  const [capture, setCapture] = useState('')
  const captureTrigger = useRef<HTMLElement | null>(null)
  const closeCapture = () => { setCapture(''); requestAnimationFrame(() => { if (captureTrigger.current?.isConnected) captureTrigger.current.focus() }) }
  const pending = useRef(new Set<string>())
  const key = ['shared-list-detail', listId, accountSessionKey(account)]
  const detail = useQuery({ queryKey: key, enabled: Boolean(account && listId), retry: false, queryFn: async () => getSharedListById(env.shoppingListApiBaseUrl, await token(), listId) })
  const groups = useQuery({ queryKey: ['groups', accountSessionKey(account)], enabled: Boolean(account), retry: false, queryFn: async () => getMyGroups(env.usersApiBaseUrl, await token()) })
  const groupId = detail.data?.groupId ?? ''
  const members = useQuery({ queryKey: ['group-members', groupId, accountSessionKey(account)], enabled: Boolean(account && groupId), retry: false, queryFn: async () => getGroupMembers(env.usersApiBaseUrl, await token(), groupId) })
  const profile = useQuery({ queryKey: ['profile', accountSessionKey(account)], enabled: Boolean(account), retry: false, queryFn: async () => fetchMyProfile(env.usersApiBaseUrl, await token()) })
  const group = groups.data?.find(g => g.id === groupId)
  const role = (group?.myRole || members.data?.find(m => m.userId === profile.data?.id)?.role || '').toLowerCase()
  const manager = role === 'owner' || role === 'admin'
  const active = !detail.data?.status || detail.data.status.toLowerCase() === 'active'
  const back = groupId ? `/shared-lists?groupId=${encodeURIComponent(groupId)}` : '/shared-lists'
  const refresh = async () => {
    await client.invalidateQueries({ queryKey: key })
    await client.invalidateQueries({ queryKey: ['shared-lists'] })
    await client.invalidateQueries({ queryKey: ['all-lists'] })
  }
  if (!account) return <p>No hay sesión activa.</p>
  return <section className={ui.page}><Link to={back}>Volver a listas compartidas</Link>
    {detail.isPending ? <><h1>Lista compartida</h1><Spinner label="Cargando lista…" /></> : detail.isError ? <><h1>Lista no disponible</h1><p role="alert">{describeError(detail.error)}</p><Button onClick={() => void detail.refetch()}>Reintentar</Button></> : <section className={ui.shoppingDetail}>
      <div className={`${ui.detailHeader} ${ui.board}`}><div className={ui.detailTitle}><h1>{detail.data.name}</h1><p>Lista compartida{group ? ` · ${group.name}` : ''}{!active ? ' · Cerrada' : ''}</p></div>
        {active ? <AddProductsDialog disabled={Boolean(capture) || pendingIds.length > 0} key={`${accountSessionKey(account)}:${listId}`} returnTo={`/shared-lists/${listId}?tab=${tab}`} onAdd={async item => {
          await write(t => addSharedListItem(env.shoppingListApiBaseUrl, t, listId, { productId: item.productId, name: item.productName, quantity: item.quantity, estimatedPrice: item.price })); await refresh()
        }} /> : null}
      </div>
      {manager && active ? <div className={ui.actions}><RenameListAction name={detail.data.name} accountKey={accountSessionKey(account)} scope={listId} onRename={async name => { await write(t => updateSharedList(env.shoppingListApiBaseUrl, t, listId, name)); await refresh() }} />
        <ConfirmAction icon="close" label="Cerrar lista" description="La lista quedará disponible para consulta. Sus gastos se conservarán." onConfirm={async () => { await write(t => closeSharedList(env.shoppingListApiBaseUrl, t, listId)); await refresh() }} />
        <ConfirmAction label="Eliminar lista" description={`Se eliminará «${detail.data.name}» si no tiene gastos asociados. Si tiene gastos, ciérrala para conservar las cuentas.`} onConfirm={async () => { await write(t => removeSharedList(env.shoppingListApiBaseUrl, t, listId)); await refresh(); navigate(back) }} />
      </div> : null}
      <TabList selectedValue={tab} aria-label="Vistas de la lista" onTabSelect={(_, data) => { const next = new URLSearchParams(params); next.set('tab', String(data.value)); setParams(next) }}>
        <Tab id="shopping-tab" value="shopping" aria-controls="shopping-panel">Compra</Tab><Tab id="expenses-tab" value="expenses" aria-controls="expenses-panel">Gastos</Tab><Tab id="people-tab" value="people" aria-controls="people-panel">Personas</Tab>
      </TabList>
      {tab === 'shopping' ? <div id="shopping-panel" role="tabpanel" aria-labelledby="shopping-tab" className={ui.stack}>
        {error ? <p role="alert">{error} <Button onClick={() => void detail.refetch()}>Actualizar lista</Button></p> : null}{message ? <p role="status">{message}</p> : null}
        <PurchaseItems items={detail.data.items} pendingIds={pendingIds} authorName={id => memberName(members.data ?? [], id)} onToggle={active ? async (id, value) => {
          if (pending.current.has(id)) return
          if (value) captureTrigger.current = document.activeElement as HTMLElement | null
          pending.current.add(id); setPendingIds([...pending.current]); setError(''); setMessage('')
          try { await write(t => setPurchaseState(env.shoppingListApiBaseUrl, t, listId, id, value)); if (value) setCapture(id); await refresh(); setMessage(value ? 'Producto marcado como comprado.' : 'Producto pendiente de nuevo. Los gastos se conservan.') }
          catch (cause) { setError(describeError(cause)) }
          finally { pending.current.delete(id); setPendingIds([...pending.current]) }
        } : undefined} actions={active ? item => <ProductOptions name={item.productName}>
          <Button onClick={event => { captureTrigger.current = event.currentTarget; setCapture(item.id) }}>Datos de compra de {item.productName}</Button>
          {manager || item.addedBy === profile.data?.id ? <>
          <EditItemAction item={item} accountKey={accountSessionKey(account)} onEdit={async (quantity, price) => { await write(t => updateSharedItem(env.shoppingListApiBaseUrl, t, listId, item.id, quantity, price)); await refresh() }} />
          <ConfirmAction label={`Eliminar ${item.productName}`} text="Eliminar" description="Se eliminará este producto si no tiene gastos asociados." onConfirm={async () => { await write(t => removeSharedItem(env.shoppingListApiBaseUrl, t, listId, item.id)); await refresh(); document.getElementById('main-content')?.focus() }} />
          </> : null}
        </ProductOptions> : undefined} />
      </div> : tab === 'people' ? <div id="people-panel" role="tabpanel" aria-labelledby="people-tab"><GroupPeople groupId={groupId} /></div> : <div id="expenses-panel" role="tabpanel" aria-labelledby="expenses-tab"><Suspense fallback={<Spinner label="Cargando reparto…" />}><SettlementPage key={listId} sharedListId={listId} /></Suspense></div>}
      {detail.data.items.find(item => item.id === capture) ? <PurchaseCapture key={`${accountSessionKey(account)}:${listId}:${capture}`} listId={listId} groupId={groupId} item={detail.data.items.find(item => item.id === capture)!} restoreFocusRef={captureTrigger} onClose={closeCapture} /> : null}
    </section>}
  </section>
}
