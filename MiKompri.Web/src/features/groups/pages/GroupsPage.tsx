import { currentAccount, accountSessionKey } from '../../../auth/session/accountIdentity'
import { PageHeader } from '../../../shared/ui/PageHeader'
import ui from '../../../shared/ui/AppSurface.module.css'
import s from '../components/GroupSurfaces.module.css'
import { Button, Dialog, DialogActions, DialogBody, DialogContent, DialogSurface, DialogTitle, Field, Input, Spinner, Text } from '@fluentui/react-components'
import { useMsal } from '@azure/msal-react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useRef, useState, type MouseEvent } from 'react'
import { Link } from 'react-router-dom'
import { loginRequest } from '../../../auth/msal/msalConfig'
import { createAuthSessionService } from '../../../auth/session/authSessionService'
import { env } from '../../../shared/config/env'
import { createGroup, getMyGroups } from '../api/groupsApi'
import { AppIcon } from '../../../shared/ui/icons/AppIcon'
import { GroupMetadata } from '../components/GroupMetadata'

export function GroupsPage() {
  const { instance, accounts } = useMsal()
  const account = currentAccount(instance, accounts)
  const authSession = createAuthSessionService(instance)
  const queryClient = useQueryClient()
  const [name, setName] = useState('')
  const [open, setOpen] = useState(false)
  const trigger = useRef<HTMLButtonElement | null>(null)
  const headerTrigger = useRef<HTMLButtonElement>(null)
  const close = () => { setOpen(false); requestAnimationFrame(() => (trigger.current?.isConnected ? trigger.current : headerTrigger.current)?.focus()) }
  const groupsQuery = useQuery({
    queryKey: ['groups', accountSessionKey(account)], enabled: Boolean(account),
    queryFn: async () => getMyGroups(env.usersApiBaseUrl, await authSession.getAccessToken(loginRequest.scopes, account)),
  })
  const createGroupMutation = useMutation({
    mutationFn: async () => createGroup(env.usersApiBaseUrl, await authSession.getAccessToken(loginRequest.scopes, account), { name }),
    onSuccess: async () => { setName(''); await queryClient.invalidateQueries({ queryKey: ['groups'] }); close() },
  })
  const showCreate = (event: MouseEvent<HTMLButtonElement>) => { trigger.current = event.currentTarget; event.currentTarget.focus(); createGroupMutation.reset(); setOpen(true) }
  if (!account) return <Text>No hay sesión activa.</Text>
  return <section className={ui.page}>
    <div className={ui.detailHeader}>
      <PageHeader icon="groups" title="Grupos" description="Para tu casa, tu piso y los planes que compartís. Organiza aquí tus grupos." />
      <Button ref={headerTrigger} appearance="primary" icon={<AppIcon name="add" />} disabled={createGroupMutation.isPending} onClick={showCreate}>Crear grupo</Button>
    </div>
    {groupsQuery.isLoading ? <Spinner label="Cargando grupos" /> : groupsQuery.isError ? <p role="alert">No fue posible cargar grupos.</p>
      : groupsQuery.data?.length ? <ul className={s.grid} aria-label="Tus grupos">{groupsQuery.data.map(group => <li key={group.id}>
        <Link className={s.card} to={`/groups/${group.id}`}>
          <div className={s.identity}><span className={s.symbol}><AppIcon name="groups" /></span><div><h2>{group.name}</h2><span className={s.context}>Espacio compartido</span></div></div>
          <GroupMetadata group={group} /><span className={s.entry}>Ver grupo</span>
        </Link>
      </li>)}</ul> : <section className={s.empty}>
        <span className={s.symbol}><AppIcon name="groups" /></span><h2>Aún no tienes grupos</h2>
        <p>Crea un grupo para organizar compras con otras personas.</p>
        <Button icon={<AppIcon name="add" />} disabled={createGroupMutation.isPending} onClick={showCreate}>Crear tu primer grupo</Button>
      </section>}
    <Dialog open={open} onOpenChange={(_, d) => { if (!createGroupMutation.isPending) { if (d.open) setOpen(true); else close() } }}>
      <DialogSurface className={`${ui.panel} ${ui.page}`}><form className={ui.form} onSubmit={event => { event.preventDefault(); if (name.trim() && !createGroupMutation.isPending) createGroupMutation.mutate() }}>
        <DialogBody><DialogTitle>Crear grupo</DialogTitle><DialogContent className={ui.stack}>
          <Field label="Nombre del grupo" required><Input required value={name} disabled={createGroupMutation.isPending} onChange={(_, d) => setName(d.value)} /></Field>
          {createGroupMutation.isError ? <p role="alert">No fue posible crear el grupo. Revisa el nombre e inténtalo de nuevo.</p> : null}
        </DialogContent><DialogActions>
          <Button disabled={createGroupMutation.isPending} onClick={close}>Cancelar</Button>
          <Button type="submit" appearance="primary" disabled={!name.trim() || createGroupMutation.isPending}>{createGroupMutation.isPending ? 'Creando grupo…' : 'Crear grupo'}</Button>
        </DialogActions></DialogBody></form></DialogSurface>
    </Dialog>
  </section>
}
