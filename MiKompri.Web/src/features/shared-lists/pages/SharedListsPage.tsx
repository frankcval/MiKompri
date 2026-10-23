import { Button, Card, CardHeader, Field, Input, Spinner, Text } from '@fluentui/react-components'
import { useMsal } from '@azure/msal-react'
import { useMutation, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { loginRequest } from '../../../auth/msal/msalConfig'
import { createAuthSessionService } from '../../../auth/session/authSessionService'
import { env } from '../../../shared/config/env'
import { SettlementPage } from '../../settlements/pages/SettlementPage'
import { createSharedList, getSharedListsByGroup } from '../api/sharedListsApi'

export function SharedListsPage() {
  const { instance, accounts } = useMsal()
  const account = accounts[0]
  const authSession = createAuthSessionService(instance)
  const [groupId, setGroupId] = useState('')
  const [listName, setListName] = useState('')
  const [selectedSharedListId, setSelectedSharedListId] = useState<string | null>(null)

  const sharedListsQuery = useQuery({
    queryKey: ['shared-lists', groupId, account?.homeAccountId],
    enabled: Boolean(account && groupId.trim()),
    queryFn: async () => {
      const token = await authSession.getAccessToken(loginRequest.scopes, account)
      return getSharedListsByGroup(env.shoppingListApiBaseUrl, token, groupId)
    },
    retry: false,
  })

  const createSharedListMutation = useMutation({
    mutationFn: async () => {
      const token = await authSession.getAccessToken(loginRequest.scopes, account)
      return createSharedList(env.shoppingListApiBaseUrl, token, { name: listName, groupId })
    },
    onSuccess: () => setListName(''),
  })

  if (!account) {
    return <Text>No hay sesión activa.</Text>
  }

  const status = (sharedListsQuery.error as { status?: number } | undefined)?.status

  return (
    <main style={{ padding: '1.5rem', display: 'grid', gap: '1rem' }}>
      <h1>Listas compartidas</h1>

      <Card>
        <CardHeader header={<Text weight="semibold">Seleccionar grupo</Text>} />
        <Field label="GroupId">
          <Input value={groupId} onChange={(_, data) => setGroupId(data.value)} />
        </Field>
      </Card>

      <Card>
        <CardHeader header={<Text weight="semibold">Crear lista compartida</Text>} />
        <Field label="Nombre de lista">
          <Input value={listName} onChange={(_, data) => setListName(data.value)} />
        </Field>
        <Button
          appearance="primary"
          disabled={!groupId.trim() || !listName.trim()}
          onClick={() => createSharedListMutation.mutate()}
        >
          Crear lista compartida
        </Button>
      </Card>

      {sharedListsQuery.isLoading ? (
        <Spinner label="Cargando listas compartidas" />
      ) : sharedListsQuery.isError ? (
        <Text>{status === 403 ? 'No tienes permisos para este grupo.' : 'No fue posible cargar listas compartidas.'}</Text>
      ) : (
        <section style={{ display: 'grid', gap: '0.5rem' }}>
          {sharedListsQuery.data?.map((list) => (
            <Card key={list.id}>
              <div style={{ display: 'flex', justifyContent: 'space-between', gap: '0.75rem', alignItems: 'center' }}>
                <Text>{list.name}</Text>
                <Button
                  appearance="secondary"
                  onClick={() => setSelectedSharedListId(list.id)}
                >
                  Ver settlement
                </Button>
              </div>
            </Card>
          ))}
          {sharedListsQuery.data?.length === 0 && groupId ? <Text>Sin listas compartidas para este grupo.</Text> : null}
        </section>
      )}

      {selectedSharedListId ? (
        <Card>
          <CardHeader header={<Text weight="semibold">Settlement de la lista seleccionada</Text>} />
          <SettlementPage sharedListId={selectedSharedListId} />
        </Card>
      ) : null}
    </main>
  )
}
