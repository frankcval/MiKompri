import { Button, Card, CardHeader, Dialog, DialogActions, DialogBody, DialogContent, DialogSurface, DialogTitle, DialogTrigger, Field, Input, Select, Spinner, Text } from '@fluentui/react-components'
import { useMsal } from '@azure/msal-react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { useRef, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { loginRequest } from '../../../auth/msal/msalConfig'
import { createAuthSessionService, executeAuthenticatedWrite } from '../../../auth/session/authSessionService'
import { env } from '../../../shared/config/env'
import { getMyGroups, type GroupSummary } from '../../groups/api/groupsApi'
import { SettlementPage } from '../../settlements/pages/SettlementPage'
import { createSharedList, getSharedListsByGroup } from '../api/sharedListsApi'

export function SharedListsPage() {
  const { instance, accounts } = useMsal()
  const account = accounts[0]
  const authSession = createAuthSessionService(instance)
  const [searchParams, setSearchParams] = useSearchParams()
  const requestedGroupId = searchParams.get('groupId') ?? ''
  const groupsQuery = useQuery({
    queryKey: ['groups', account?.homeAccountId],
    enabled: Boolean(account),
    retry: false,
    queryFn: async () => {
      const token = await authSession.getAccessToken(loginRequest.scopes, account)
      return getMyGroups(env.usersApiBaseUrl, token)
    },
  })
  const selectedGroup = groupsQuery.isSuccess ? groupsQuery.data.find((group) => group.id === requestedGroupId) : undefined

  if (!account) return <Text>No hay sesión activa.</Text>

  return <section style={{ padding: '1.5rem', display: 'grid', gap: '1rem', minWidth: 0 }}>
    <h1>Listas compartidas</h1>
    {groupsQuery.isPending ? <Spinner label="Cargando tus grupos" /> : groupsQuery.isError ? (
      <div role="alert">No fue posible cargar tus grupos. <Button onClick={() => void groupsQuery.refetch()}>Reintentar</Button></div>
    ) : groupsQuery.data.length === 0 ? (
      <p>Todavía no perteneces a ningún grupo. <Link to="/groups">Crear grupo</Link></p>
    ) : <>
      <Card>
        <CardHeader header={<Text weight="semibold">Seleccionar grupo</Text>} />
        <Field label="Grupo">
          <Select value={selectedGroup?.id ?? ''} onChange={(_, data) => {
            const next = new URLSearchParams(searchParams)
            if (data.value) next.set('groupId', data.value)
            else next.delete('groupId')
            setSearchParams(next)
          }}>
            <option value="">Selecciona uno de tus grupos</option>
            {groupsQuery.data.map((group) => <option key={group.id} value={group.id}>{group.name}</option>)}
          </Select>
        </Field>
      </Card>
      {!selectedGroup ? <p role={requestedGroupId ? 'alert' : undefined}>
        {requestedGroupId ? 'El grupo indicado no está disponible entre tus grupos. Selecciona otro grupo.' : 'Selecciona un grupo para ver y crear sus listas compartidas.'}
      </p> : <GroupLists key={`${account.homeAccountId}:${selectedGroup.id}`} group={selectedGroup} />}
    </>}
  </section>
}

function GroupLists({ group }: { group: GroupSummary }) {
  const { instance, accounts } = useMsal()
  const account = accounts[0]
  const authSession = createAuthSessionService(instance)
  const queryClient = useQueryClient()
  const [listName, setListName] = useState('')
  const [selectedListId, setSelectedListId] = useState<string | null>(null)
  const [open, setOpen] = useState(false)
  const [pending, setPending] = useState(false)
  const [error, setError] = useState('')
  const submitting = useRef(false)
  const queryKey = ['shared-lists', group.id, account?.homeAccountId]
  const listsQuery = useQuery({
    queryKey,
    enabled: Boolean(account),
    retry: false,
    queryFn: async () => {
      const token = await authSession.getAccessToken(loginRequest.scopes, account)
      return getSharedListsByGroup(env.shoppingListApiBaseUrl, token, group.id)
    },
  })
  const status = (listsQuery.error as { status?: number } | null)?.status
  const selectedList = listsQuery.isSuccess ? listsQuery.data.find((list) => list.id === selectedListId) : undefined

  return <section aria-label={`Listas de ${group.name}`} style={{ display: 'grid', gap: '1rem', minWidth: 0 }}>
    <h2 style={{ overflowWrap: 'anywhere' }}>Listas de {group.name}</h2>
    <Dialog open={open} onOpenChange={(_, data) => { if (!pending) setOpen(data.open) }}>
      <DialogTrigger disableButtonEnhancement>
        <Button appearance="primary" disabled={!listsQuery.isSuccess} onClick={() => setError('')}>Crear lista compartida</Button>
      </DialogTrigger>
      <DialogSurface>
        <form onSubmit={async (event) => {
          event.preventDefault()
          if (submitting.current || !listName.trim() || listName.trim().length > 120) return
          submitting.current = true
          setPending(true)
          setError('')
          try {
            await executeAuthenticatedWrite(
              (token) => createSharedList(env.shoppingListApiBaseUrl, token, { name: listName.trim(), groupId: group.id }),
              () => authSession.getAccessToken(loginRequest.scopes, account),
              async () => authSession.login(loginRequest.scopes),
            )
            setListName('')
            setOpen(false)
            await queryClient.invalidateQueries({ queryKey })
          } catch (cause) {
            const code = (cause as { status?: number })?.status
            setError(code === 403 ? 'No tienes permiso para crear listas en este grupo.' : 'No fue posible crear la lista. Revisa el nombre e inténtalo de nuevo.')
          } finally {
            submitting.current = false
            setPending(false)
          }
        }}>
          <DialogBody>
            <DialogTitle>Crear lista en {group.name}</DialogTitle>
            <DialogContent>
              <Field label="Nombre de lista" required>
                <Input required maxLength={120} disabled={pending} value={listName} onChange={(_, data) => setListName(data.value)} />
              </Field>
              {error ? <p role="alert">{error}</p> : null}
            </DialogContent>
            <DialogActions>
              <Button type="button" disabled={pending} onClick={() => setOpen(false)}>Cancelar</Button>
              <Button appearance="primary" type="submit" disabled={pending || !listName.trim()}>{pending ? 'Creando lista…' : 'Crear lista'}</Button>
            </DialogActions>
          </DialogBody>
        </form>
      </DialogSurface>
    </Dialog>
    {listsQuery.isPending ? <Spinner label="Cargando listas compartidas" /> : listsQuery.isError ? (
      <div role="alert">
        {status === 403 ? 'No tienes permisos para consultar este grupo.' : 'No fue posible cargar las listas compartidas.'}
        <Button onClick={() => void listsQuery.refetch()}>Reintentar</Button>
      </div>
    ) : <>
      {listsQuery.data.length === 0 ? <p>Este grupo todavía no tiene listas. Crea la primera lista compartida.</p> : null}
      {listsQuery.data.map((list) => <Card key={list.id}>
        <div style={{ display: 'flex', flexWrap: 'wrap', justifyContent: 'space-between', gap: '0.75rem', alignItems: 'center' }}>
          <Text style={{ overflowWrap: 'anywhere', minWidth: 0 }}>{list.name}</Text>
          <Button appearance="secondary" onClick={() => setSelectedListId(list.id)}>Ver reparto de gastos de {list.name}</Button>
        </div>
      </Card>)}
    </>}
    {selectedList ? <Card>
      <CardHeader header={<Text weight="semibold">Reparto de gastos de {selectedList.name}</Text>} />
      <SettlementPage key={selectedList.id} sharedListId={selectedList.id} />
    </Card> : null}
  </section>
}
