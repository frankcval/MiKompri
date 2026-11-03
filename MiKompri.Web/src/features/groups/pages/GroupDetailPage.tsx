import { accountSessionKey } from '../../../auth/session/accountIdentity'
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
import s from '../components/GroupSurfaces.module.css'
import { AppIcon } from '../../../shared/ui/icons/AppIcon'
import { GroupMetadata } from '../components/GroupMetadata'

export function GroupDetailPage() {
  const { groupId } = useParams<{ groupId: string }>()
  const { account, token } = useListSession()
  const groups = useQuery({ queryKey: ['groups', accountSessionKey(account)], enabled: Boolean(account), retry: false, queryFn: async () => getMyGroups(env.usersApiBaseUrl, await token()) })
  const group = groups.data?.find(g => g.id === groupId)
  return <section className={ui.page}><Link className={s.back} to="/groups"><AppIcon name="back" size={20} />Volver a grupos</Link>
    <div className={s.detailHeader}><PageHeader icon="groups" title={group?.name ?? 'Grupo'} description="Las personas y las compras que compartís." />{group ? <GroupMetadata group={group} /> : null}</div>
    {groups.isPending ? <Spinner label="Cargando grupo…" /> : groups.isError ? <div role="alert">{describeError(groups.error)} <Button onClick={() => void groups.refetch()}>Reintentar</Button></div> : !group ? <p role="alert">Este grupo no está disponible entre tus grupos.</p> : <>
      <Link className={s.sharedEntry} aria-label={`Ver y crear listas de ${group.name}`} to={`/shared-lists?groupId=${encodeURIComponent(group.id)}`}>
        <h2><AppIcon name="lists" />Listas compartidas</h2><p>Organiza la compra con las personas de este grupo.</p><span className={s.entry}>Ver y crear listas de {group.name}</span>
      </Link>
      <div className={ui.panel}><GroupPeople groupId={group.id} /></div>
    </>}
  </section>
}
