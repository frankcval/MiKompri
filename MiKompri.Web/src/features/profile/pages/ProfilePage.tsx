import { Button, Card, CardHeader, Spinner, Text } from '@fluentui/react-components'
import { useMsal } from '@azure/msal-react'
import { useQuery } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { loginRequest } from '../../../auth/msal/msalConfig'
import { createAuthSessionService } from '../../../auth/session/authSessionService'
import { env } from '../../../shared/config/env'
import { fetchMyProfile } from '../api/profileApi'

export function ProfilePage() {
  const { instance, accounts } = useMsal()
  const navigate = useNavigate()
  const authSessionService = createAuthSessionService(instance)

  const account = accounts[0]

  const profileQuery = useQuery({
    queryKey: ['profile', account?.homeAccountId],
    queryFn: async () => {
      const token = await authSessionService.getAccessToken(loginRequest.scopes, account)
      return fetchMyProfile(env.usersApiBaseUrl, token)
    },
    retry: false,
    enabled: Boolean(account),
  })

  const handleLogout = async () => {
    await authSessionService.logout(account)
    navigate('/login', { replace: true })
  }

  if (!account) {
    return (
      <main style={{ padding: '1.5rem' }}>
        <Text>No hay sesión activa.</Text>
      </main>
    )
  }

  if (profileQuery.isLoading) {
    return (
      <main style={{ padding: '1.5rem', display: 'grid', gap: '0.75rem' }}>
        <Spinner label="Cargando perfil" />
      </main>
    )
  }

  if (profileQuery.isError) {
    const status = (profileQuery.error as { status?: number })?.status
    return (
      <main style={{ padding: '1.5rem', display: 'grid', gap: '0.75rem' }}>
        <Text>
          {status === 401
            ? 'Sesión iniciada. El perfil no está disponible aún en este entorno local.'
            : 'Sesión iniciada. El perfil se habilitará cuando esté configurado el acceso al API.'}
        </Text>
        <Button appearance="secondary" onClick={handleLogout}>
          Cerrar sesión
        </Button>
      </main>
    )
  }

  if (!profileQuery.data) {
    return (
      <main style={{ padding: '1.5rem' }}>
        <Text>Perfil vacío.</Text>
      </main>
    )
  }

  return (
    <main style={{ padding: '1.5rem', display: 'grid', gap: '1rem' }}>
      <Card>
        <CardHeader header={<Text weight="semibold">Perfil</Text>} />
        <Text>Nombre: {profileQuery.data.displayName}</Text>
        <Text>Email: {profileQuery.data.email}</Text>
      </Card>
      <Button appearance="secondary" onClick={handleLogout}>
        Cerrar sesión
      </Button>
    </main>
  )
}
