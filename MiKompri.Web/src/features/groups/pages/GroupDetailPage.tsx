import { useQuery } from '@tanstack/react-query'
import { Button, Spinner } from '@fluentui/react-components'
import { Link, useParams } from 'react-router-dom'
import { getMyGroups } from '../api/groupsApi'
import { GroupPeople } from '../components/GroupPeople'
import { useListSession } from '../../shopping-lists/hooks/useListSession'
import { env } from '../../../shared/config/env'
import { PageHeader } from '../../../shared/ui/PageHeader'
import { describeError } from '../../../shared/api/request'
import ui from '../../../shared/ui/AppSurface.module.css'

export function GroupDetailPage() {
  const { groupId } = useParams<{ groupId: string }>()
  const { account, token } = useListSession()
  const groups = useQuery({ queryKey: ['groups', account?.homeAccountId], enabled: Boolean(account), retry: false, queryFn: async () => getMyGroups(env.usersApiBaseUrl, await token()) })
  const group = groups.data?.find(g => g.id === groupId)
  return <section className={ui.page}><Link to="/groups">Volver a grupos</Link><PageHeader title={group?.name ?? 'Grupo'} description="Las personas y las compras que compartís." />
    {groups.isPending ? <Spinner label="Cargando grupo…" /> : groups.isError ? <div role="alert">{describeError(groups.error)} <Button onClick={() => void groups.refetch()}>Reintentar</Button></div> : !group ? <p role="alert">Este grupo no está disponible entre tus grupos.</p> : <>
      <Link className={ui.listLink} to={`/shared-lists?groupId=${encodeURIComponent(group.id)}`}>Ver y crear listas de {group.name}</Link>
      <GroupPeople groupId={group.id} />
    </>}
  </section>
}
