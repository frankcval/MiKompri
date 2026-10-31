import { accountSessionKey } from '../../../auth/session/accountIdentity'
import { Button, Spinner, Select, Field } from '@fluentui/react-components'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { getMyGroups } from '../../groups/api/groupsApi'
import { createSharedList, getSharedListsByGroup } from '../api/sharedListsApi'
import { useListSession } from '../../shopping-lists/hooks/useListSession'
import { CreateListDialog } from '../../shopping-lists/components/CreateListDialog'
import { env } from '../../../shared/config/env'
import { PageHeader } from '../../../shared/ui/PageHeader'
import ui from '../../../shared/ui/AppSurface.module.css'
import { visibleLists } from '../../shopping-lists/listPresentation'
import { ListOverview, ListOverviewControls } from '../../shopping-lists/components/ListOverview'

export function SharedListsPage() {
  const { account, token } = useListSession()
  const [params, setParams] = useSearchParams()
  const requested = params.get('groupId') ?? ''
  const groups = useQuery({ queryKey: ['groups', accountSessionKey(account)], enabled: Boolean(account), retry: false,
    queryFn: async () => getMyGroups(env.usersApiBaseUrl, await token()) })
  const group = groups.isSuccess ? groups.data.find(g => g.id === requested) : undefined
  if (!account) return <p>No hay sesión activa.</p>
  return <section className={ui.page}>
    <div className={ui.board}><PageHeader title="Listas compartidas" description="Elige tu grupo y prepara la compra en compañía." /></div>
    {groups.isPending ? <Spinner label="Cargando tus grupos" /> : groups.isError ? <div role="alert">No fue posible cargar tus grupos. <Button onClick={() => void groups.refetch()}>Reintentar</Button></div>
      : !groups.data.length ? <p>Todavía no perteneces a ningún grupo. <Link to="/groups">Crear grupo</Link></p> : <>
        <Field label="Grupo"><Select value={group?.id ?? ''} onChange={(_, data) => { const next = new URLSearchParams(params); if (data.value) next.set('groupId', data.value); else next.delete('groupId'); setParams(next) }}>
          <option value="">Selecciona uno de tus grupos</option>{groups.data.map(g => <option key={g.id} value={g.id}>{g.name}</option>)}
        </Select></Field>
        {group ? <GroupLists key={`${accountSessionKey(account)}:${group.id}`} group={group} /> : <p role={requested ? 'alert' : undefined}>{requested ? 'El grupo indicado no está disponible entre tus grupos. Selecciona otro grupo.' : 'Selecciona un grupo para ver y crear sus listas compartidas.'}</p>}
      </>}
  </section>
}
function GroupLists({ group }: { group: { id: string; name: string } }) {
  const [params] = useSearchParams()
  const { account, token, write } = useListSession()
  const client = useQueryClient()
  const navigate = useNavigate()
  const key = ['shared-lists', group.id, accountSessionKey(account)]
  const lists = useQuery({ queryKey: key, enabled: Boolean(account), retry: false,
    queryFn: async () => getSharedListsByGroup(env.shoppingListApiBaseUrl, await token(), group.id) })
  const visible = visibleLists(lists.data ?? [], params.get('view') === 'history', params.get('sort') ?? 'newest')
  return <section className={ui.stack} aria-label={`Listas de ${group.name}`}>
    <div className={ui.detailHeader}><h2>Listas de {group.name}</h2>
      <CreateListDialog label="Crear lista compartida" title={`Crear lista en ${group.name}`} draftScope={group.id} disabled={!lists.isSuccess} onCreate={async name => {
        try {
          const id = await write(token => createSharedList(env.shoppingListApiBaseUrl, token, { name, groupId: group.id }))
          await client.invalidateQueries({ queryKey: key }); navigate(`/shared-lists/${id}?groupId=${encodeURIComponent(group.id)}`)
        } catch (cause) { if ((cause as { status?: number }).status === 403) throw new Error('No tienes permiso para crear listas en este grupo.'); throw new Error('No fue posible crear la lista. Revisa el nombre e inténtalo de nuevo.') }
      }} />
    </div>
    <ListOverviewControls />
    {lists.isPending ? <Spinner label="Cargando listas compartidas" /> : lists.isError ? <div role="alert">{(lists.error as { status?: number }).status === 403 ? 'No tienes permisos para consultar este grupo.' : 'No fue posible cargar las listas compartidas.'}<Button onClick={() => void lists.refetch()}>Reintentar</Button></div> : <>
      {!lists.data.length ? <p>Este grupo todavía no tiene listas. Crea la primera lista compartida.</p> : null}
      {lists.data.length > 0 && !visible.length ? <p>No hay listas compartidas en esta vista. Cambia entre Activas e Historial.</p> : null}
      {visible.map(list => <div key={list.id} className={ui.panel}><Link className={ui.listLink} to={`/shared-lists/${list.id}?groupId=${encodeURIComponent(group.id)}`}>{list.name}</Link>
        <p><ListOverview list={list} /></p>
        <Link to={`/shared-lists/${list.id}?groupId=${encodeURIComponent(group.id)}&tab=expenses`}>Ver reparto de gastos de {list.name}</Link>
      </div>)}
    </>}
  </section>
}
