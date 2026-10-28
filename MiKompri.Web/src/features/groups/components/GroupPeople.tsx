import { Button, Spinner } from '@fluentui/react-components'
import { useQuery } from '@tanstack/react-query'
import { getGroupMembers } from '../api/groupsApi'
import { memberName } from '../memberName'
import { useListSession } from '../../shopping-lists/hooks/useListSession'
import { env } from '../../../shared/config/env'
import { describeError } from '../../../shared/api/request'
import ui from '../../../shared/ui/AppSurface.module.css'
const roleNames: Record<string, string> = { owner: 'Propietario', admin: 'Administrador', member: 'Miembro' }

export function GroupPeople({ groupId }: { groupId: string }) {
  const { account, token } = useListSession()
  const people = useQuery({ queryKey: ['group-members', groupId, account?.homeAccountId], enabled: Boolean(account), retry: false, queryFn: async () => getGroupMembers(env.usersApiBaseUrl, await token(), groupId) })
  return <section className={ui.stack} aria-label="Personas del grupo"><h2>Personas del grupo</h2>
    {people.isPending ? <Spinner label="Cargando personas…" /> : people.isError ? <div role="alert">{describeError(people.error)} <Button onClick={() => void people.refetch()}>Reintentar</Button></div>
      : !people.data.length ? <p>No hay personas disponibles en este grupo.</p> : <ul className={ui.peopleList}>{people.data.map(person => <li key={person.userId}><strong>{memberName(people.data, person.userId)}</strong><span>{roleNames[person.role.toLowerCase()] ?? 'Miembro'}</span></li>)}</ul>}
  </section>
}
