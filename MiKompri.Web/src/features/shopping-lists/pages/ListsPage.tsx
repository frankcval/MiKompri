import { AppIcon } from '../../../shared/ui/icons/AppIcon'
import { useProfileListSort } from '../../profile/hooks/useProfileListSort'
import { accountSessionKey } from '../../../auth/session/accountIdentity'
import { Button, Spinner } from '@fluentui/react-components'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { useListSession } from '../hooks/useListSession'
import { createPersonalList, getMyPersonalLists } from '../api/shoppingListsApi'
import { getMyGroups } from '../../groups/api/groupsApi'
import { getSharedListsByGroup } from '../../shared-lists/api/sharedListsApi'
import { CreateListDialog } from '../components/CreateListDialog'
import { env } from '../../../shared/config/env'
import { describeError } from '../../../shared/api/request'
import { PageHeader } from '../../../shared/ui/PageHeader'
import ui from '../../../shared/ui/AppSurface.module.css'
import { visibleLists } from '../listPresentation'
import { ListOverview, ListOverviewControls } from '../components/ListOverview'
import type { SharedListSummary } from '../../shared-lists/api/sharedListsApi'

export function ListsPage() {
  const { account, token, write } = useListSession()
  const defaultSort = useProfileListSort(account, token)
  const [params] = useSearchParams()
  const filter = params.get('filter') ?? 'all'
  const navigate = useNavigate()
  const client = useQueryClient()
  const personal = useQuery({ queryKey: ['personal-lists', accountSessionKey(account)], enabled: Boolean(account), retry: false, queryFn: async () => getMyPersonalLists(env.shoppingListApiBaseUrl, await token()) })
  const groups = useQuery({ queryKey: ['groups', accountSessionKey(account)], enabled: Boolean(account), retry: false, queryFn: async () => getMyGroups(env.usersApiBaseUrl, await token()) })
  const shared = useQuery({ queryKey: ['all-lists', accountSessionKey(account), groups.data?.map(g => g.id)], enabled: Boolean(account && groups.isSuccess), retry: false, queryFn: async () => {
    const entries: (SharedListSummary & { groupName: string })[] = []
    const failures: string[] = []
    const work = [...groups.data!]
    await Promise.all(Array.from({ length: Math.min(4, work.length) }, async () => {
      for (let group = work.shift(); group; group = work.shift()) {
        try { const lists = await getSharedListsByGroup(env.shoppingListApiBaseUrl, await token(), group.id); entries.push(...lists.map(list => ({ ...list, groupName: group!.name }))) }
        catch { failures.push(group.name) }
      }
    }))
    return { entries, failures }
  } })
  const showPersonal = filter !== 'shared'
  const showShared = filter !== 'personal'
  const entries = visibleLists([
    ...(showPersonal ? (personal.data ?? []).map(list => ({ ...list, context: 'Personal', to: `/personal-lists/${list.id}` })) : []),
    ...(showShared ? (shared.data?.entries ?? []).map(list => ({ ...list, context: list.groupName, to: `/shared-lists/${list.id}?groupId=${encodeURIComponent(list.groupId)}` })) : []),
  ], params.get('view') === 'history', params.get('sort') ?? defaultSort)
  const count = entries.length
  const complete = (!showPersonal || personal.isSuccess) && (!showShared || (groups.isSuccess && shared.isSuccess && !shared.data.failures.length))
  return <section className={ui.page}><div className={`${ui.detailHeader} ${ui.board}`}><PageHeader title="Mis listas" description="Prepara tu próxima compra o retoma la que ya está en marcha." />
    <CreateListDialog title="Crear lista personal" initialOpen={params.get('create') === '1'} onCreate={async name => { const id = await write(t => createPersonalList(env.shoppingListApiBaseUrl, t, { name })); await client.invalidateQueries({ queryKey: ['personal-lists'] }); navigate(`/personal-lists/${id}`) }} />
  </div>
    <nav className={ui.filters} aria-label="Filtrar listas">{[['all', 'Todas'], ['personal', 'Personales'], ['shared', 'Compartidas']].map(([value, label]) => { const next = new URLSearchParams(params); next.set('filter', value); return <Link key={value} aria-current={filter === value ? 'page' : undefined} to={`/lists?${next}`}>{label}</Link> })}</nav>
    <ListOverviewControls defaultSort={defaultSort} />
    <div className={`${ui.actions} ${ui.listShortcuts}`}><Link to="/shared-lists">Crear una lista compartida</Link><Link to="/groups">Ver mis grupos</Link></div>
    {showPersonal && personal.isError ? <p role="alert">{describeError(personal.error)} <Button onClick={() => void personal.refetch()}>Reintentar personales</Button></p> : null}
    {showShared && groups.isError ? <p role="alert">No se pudieron cargar tus grupos. <Button onClick={() => void groups.refetch()}>Reintentar grupos</Button></p> : null}
    {showShared && (shared.isError || Boolean(shared.data?.failures.length)) ? <p role="alert">Algunas listas compartidas no se han podido cargar. Las listas que ves pueden estar incompletas. <Button onClick={() => void shared.refetch()}>Reintentar compartidas</Button></p> : null}
    {(showPersonal && personal.isPending) || (showShared && (groups.isPending || (groups.isSuccess && shared.isPending))) ? <Spinner label="Cargando tus listas…" /> : null}
    <div className={ui.listGrid}>
      {entries.map(list => <Link className={ui.listCard} key={list.id} to={list.to}><span className={ui.listContext}><AppIcon name={list.to.startsWith('/personal-lists') ? 'personal-context' : 'groups'} size={20} />{list.context}</span><strong>{list.name}</strong><ListOverview list={list} /></Link>)}
    </div>
    {complete && !count ? <div className={ui.feedback}><h2>{params.get('view') === 'history' ? 'Sin listas en el historial' : 'Sin listas activas'}</h2><p>{params.get('view') === 'history' ? 'Aquí encontrarás tus compras completadas y listas cerradas.' : 'Crea una lista o consulta tus compras anteriores en Historial.'}</p></div> : null}
  </section>
}
