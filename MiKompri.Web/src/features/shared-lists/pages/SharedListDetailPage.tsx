import { Spinner, Button, TabList, Tab } from '@fluentui/react-components'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, useParams, useSearchParams } from 'react-router-dom'
import { getSharedListById, addSharedListItem } from '../api/sharedListsApi'
import { useListSession } from '../../shopping-lists/hooks/useListSession'
import { AddProductsDialog } from '../../shopping-lists/components/AddProductsDialog'
import { PurchaseItems } from '../../shopping-lists/components/PurchaseItems'
import { SettlementPage } from '../../settlements/pages/SettlementPage'
import { env } from '../../../shared/config/env'
import { getMyGroups } from '../../groups/api/groupsApi'
import ui from '../../../shared/ui/AppSurface.module.css'

export function SharedListDetailPage() {
  const { listId } = useParams<{ listId: string }>()
  const { account, token, write } = useListSession()
  const client = useQueryClient()
  const [params, setParams] = useSearchParams()
  const tab = params.get('tab') === 'expenses' ? 'expenses' : 'shopping'
  const key = ['shared-list-detail', listId, account?.homeAccountId]
  const detail = useQuery({ queryKey: key, enabled: Boolean(account && listId), retry: false,
    queryFn: async () => getSharedListById(env.shoppingListApiBaseUrl, await token(), listId!) })
  const groups = useQuery({ queryKey: ['groups', account?.homeAccountId], enabled: Boolean(account && detail.isSuccess), retry: false,
    queryFn: async () => getMyGroups(env.usersApiBaseUrl, await token()) })
  const group = groups.data?.find(group => group.id === detail.data?.groupId)
  if (!account) return <p>No hay sesión activa.</p>
  const back = detail.data?.groupId ? `/shared-lists?groupId=${encodeURIComponent(detail.data.groupId)}` : '/shared-lists'
  return <section className={ui.page}>
    <Link to={back}>Volver a listas compartidas</Link>
    {detail.isPending ? <><h1>Lista compartida</h1><Spinner label="Cargando lista…" /></> : detail.isError ? <><h1>Lista no disponible</h1><p role="alert">{detail.error instanceof Error ? detail.error.message : 'No fue posible cargar esta lista.'}</p><Button onClick={() => void detail.refetch()}>Reintentar</Button></> : <section className={ui.panel}>
      <div className={ui.detailHeader}><div className={ui.detailTitle}><h1>{detail.data.name}</h1><p>Lista compartida{group ? ` · ${group.name}` : ''}</p></div>
        <AddProductsDialog key={`${account.homeAccountId}:${listId}`} returnTo={`/shared-lists/${listId}?tab=${tab}`} onAdd={async item => {
          await write(token => addSharedListItem(env.shoppingListApiBaseUrl, token, listId!, { productId: item.productId, name: item.productName, quantity: item.quantity, estimatedPrice: item.price }))
          await client.invalidateQueries({ queryKey: key })
        }} />
      </div>
      <TabList selectedValue={tab} aria-label="Vistas de la lista" onTabSelect={(_, data) => {
        const next = new URLSearchParams(params); next.set('tab', String(data.value)); setParams(next)
      }}>
        <Tab id="shopping-tab" value="shopping" aria-controls="shopping-panel">Compra</Tab><Tab id="expenses-tab" value="expenses" aria-controls="expenses-panel">Gastos</Tab>
      </TabList>
      {tab === 'shopping' ? <div id="shopping-panel" role="tabpanel" aria-labelledby="shopping-tab" className={ui.stack}>
        <PurchaseItems items={detail.data.items} />
        <p>El marcado de compra compartida aún no está disponible. Puedes añadir productos y consultar o registrar los gastos de esta lista.</p>
      </div> : <div id="expenses-panel" role="tabpanel" aria-labelledby="expenses-tab"><SettlementPage key={listId} sharedListId={listId} /></div>}
    </section>}
  </section>
}
