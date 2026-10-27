import { PageHeader } from '../../../shared/ui/PageHeader'
import ui from '../../../shared/ui/AppSurface.module.css'
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
    <section className={ui.page}>
      <PageHeader title="Grupos" description="Para tu casa, tu piso y los planes que compartís. Organiza aquí tus grupos." />
      <Card className={ui.panel}>
        <CardHeader header={<h2>Crear grupo</h2>} />
        <div className={ui.form}>
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
        {createGroupMutation.isError ? <p role="alert">No fue posible crear el grupo. Revisa el nombre e inténtalo de nuevo.</p> : null}
        </div>
      </Card>

      {groupsQuery.isLoading ? (
        <Spinner label="Cargando grupos" />
      ) : groupsQuery.isError ? (
        <p role="alert">No fue posible cargar grupos.</p>
      ) : (
        <section className={ui.panel} aria-label="Tus grupos">
          <h2>Tus grupos</h2>
          {groupsQuery.data?.map((group) => (
            <div className={ui.row} key={group.id}>
              <Text>{group.name}</Text>
            </div>
          ))}
          {groupsQuery.data?.length === 0 ? <Text>Sin grupos todavía.</Text> : null}
        </section>
      )}
    </section>
  )
}
