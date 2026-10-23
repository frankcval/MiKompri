import { Button, Card, CardHeader, Field, Input, Spinner, Text } from '@fluentui/react-components'
import { useMsal } from '@azure/msal-react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { loginRequest } from '../../../auth/msal/msalConfig'
import { createAuthSessionService } from '../../../auth/session/authSessionService'
import { env } from '../../../shared/config/env'
import { createGroup, getMyGroups } from '../api/groupsApi'

export function GroupsPage() {
  const { instance, accounts } = useMsal()
  const account = accounts[0]
  const authSession = createAuthSessionService(instance)
  const queryClient = useQueryClient()
  const [name, setName] = useState('')

  const groupsQuery = useQuery({
    queryKey: ['groups', account?.homeAccountId],
    enabled: Boolean(account),
    queryFn: async () => {
      const token = await authSession.getAccessToken(loginRequest.scopes, account)
      return getMyGroups(env.usersApiBaseUrl, token)
    },
  })

  const createGroupMutation = useMutation({
    mutationFn: async () => {
      const token = await authSession.getAccessToken(loginRequest.scopes, account)
      return createGroup(env.usersApiBaseUrl, token, { name })
    },
    onSuccess: async () => {
      setName('')
      await queryClient.invalidateQueries({ queryKey: ['groups'] })
    },
  })

  if (!account) {
    return <Text>No hay sesión activa.</Text>
  }

  return (
    <main style={{ padding: '1.5rem', display: 'grid', gap: '1rem' }}>
      <h1>Grupos</h1>
      <Card>
        <CardHeader header={<Text weight="semibold">Crear grupo</Text>} />
        <Field label="Nombre del grupo">
          <Input value={name} onChange={(_, data) => setName(data.value)} />
        </Field>
        <Button
          appearance="primary"
          onClick={() => createGroupMutation.mutate()}
          disabled={!name.trim() || createGroupMutation.isPending}
        >
          Crear grupo
        </Button>
      </Card>

      {groupsQuery.isLoading ? (
        <Spinner label="Cargando grupos" />
      ) : groupsQuery.isError ? (
        <Text>No fue posible cargar grupos.</Text>
      ) : (
        <section style={{ display: 'grid', gap: '0.5rem' }}>
          {groupsQuery.data?.map((group) => (
            <Card key={group.id}>
              <Text>{group.name}</Text>
            </Card>
          ))}
          {groupsQuery.data?.length === 0 ? <Text>Sin grupos todavía.</Text> : null}
        </section>
      )}
    </main>
  )
}
