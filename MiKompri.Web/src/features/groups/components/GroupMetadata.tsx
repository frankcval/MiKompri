import type { GroupSummary } from '../api/groupsApi'
import s from './GroupSurfaces.module.css'

const roles: Record<string, string> = { owner: 'Propietario', admin: 'Administrador', member: 'Miembro' }
export function GroupMetadata({ group }: { group: GroupSummary }) {
  const role = group.myRole?.trim()
  const count = group.memberCount
  return <div className={s.metadata}>
    {role ? <span className={s.role}>{roles[role.toLowerCase()] ?? role}</span> : null}
    {typeof count === 'number' && Number.isSafeInteger(count) && count >= 0 ? <span>{count} {count === 1 ? 'persona' : 'personas'}</span> : null}
  </div>
}
