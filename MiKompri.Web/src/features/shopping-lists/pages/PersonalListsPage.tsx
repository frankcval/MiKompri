import { Button, Card, CardHeader, Field, Input, Spinner, Text } from '@fluentui/react-components'
import { useMsal } from '@azure/msal-react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { loginRequest } from '../../../auth/msal/msalConfig'
import { createAuthSessionService, executeAuthenticatedWrite } from '../../../auth/session/authSessionService'
import { env } from '../../../shared/config/env'
import { ListItemsPanel } from '../components/ListItemsPanel'
import { addPersonalListItem, createPersonalList, getMyPersonalLists } from '../api/shoppingListsApi'

export function PersonalListsPage() {
  const { instance, accounts } = useMsal()
  const account = accounts[0]
  const authSession = createAuthSessionService(instance)
  const queryClient = useQueryClient()
  const [name, setName] = useState('')

  const listsQuery = useQuery({
    queryKey: ['personal-lists', account?.homeAccountId],
    enabled: Boolean(account),
    queryFn: async () => {
      const token = await authSession.getAccessToken(loginRequest.scopes, account)
      return getMyPersonalLists(env.shoppingListApiBaseUrl, token)
    },
  })

  const createListMutation = useMutation({
    mutationFn: async () => {
      return executeAuthenticatedWrite(
        (token) => createPersonalList(env.shoppingListApiBaseUrl, token, { name }),
        () => authSession.getAccessToken(loginRequest.scopes, account),
        async () => authSession.login(loginRequest.scopes),
      )
    },
    onSuccess: async () => {
      setName('')
      await queryClient.invalidateQueries({ queryKey: ['personal-lists'] })
    },
  })

  const selectedListId = listsQuery.data?.[0]?.id

  if (!account) {
    return <Text>No hay sesión activa.</Text>
  }

  return (
    <main style={{ padding: '1.5rem', display: 'grid', gap: '1rem' }}>
      <h1>Listas personales</h1>

      <Card>
        <CardHeader header={<Text weight="semibold">Crear lista personal</Text>} />
        <Field label="Nombre de lista">
          <Input value={name} onChange={(_, data) => setName(data.value)} />
        </Field>
        <Button appearance="primary" disabled={!name.trim()} onClick={() => createListMutation.mutate()}>
          Crear lista
        </Button>
      </Card>

      {listsQuery.isLoading ? (
        <Spinner label="Cargando listas" />
      ) : listsQuery.isError ? (
        <Text>No fue posible cargar listas personales.</Text>
      ) : (
        <section style={{ display: 'grid', gap: '0.5rem' }}>
          {listsQuery.data?.map((list) => (
            <Card key={list.id}>
              <Text>{list.name}</Text>
            </Card>
          ))}
          {listsQuery.data?.length === 0 ? <Text>Sin listas personales todavía.</Text> : null}
        </section>
      )}

      <Card>
        <CardHeader header={<Text weight="semibold">Agregar ítem (primera lista)</Text>} />
        <ListItemsPanel
          onAddItem={async (item) => {
            if (!selectedListId) {
              return
            }

            await executeAuthenticatedWrite(
              (token) =>
                addPersonalListItem(env.shoppingListApiBaseUrl, token, selectedListId, {
                  productName: item.productName,
                  quantity: item.quantity,
                  price: item.price,
                  productId: null,
                }),
              () => authSession.getAccessToken(loginRequest.scopes, account),
              async () => authSession.login(loginRequest.scopes),
            )

            await queryClient.invalidateQueries({ queryKey: ['personal-lists'] })
          }}
        />
      </Card>
    </main>
  )
}
