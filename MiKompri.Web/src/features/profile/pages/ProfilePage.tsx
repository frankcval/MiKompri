import ui from '../../../shared/ui/AppSurface.module.css'
import { PageHeader } from '../../../shared/ui/PageHeader'
import { Button, Card, Spinner, Text } from '@fluentui/react-components'
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
    queryFn: async () => fetchMyProfile(env.usersApiBaseUrl, await authSessionService.getAccessToken(loginRequest.scopes, account)),
    retry: false,
    enabled: Boolean(account),
  })
  const handleLogout = async () => {
    await authSessionService.logout(account)
    navigate('/login', { replace: true })
  }
  const status = (profileQuery.error as { status?: number } | null)?.status
  return <section className={ui.page}>
    <PageHeader title="Perfil" description="Tu cuenta de MiKompri. Consulta tus datos y gestiona tu sesión." />
    {!account ? <Text>No hay sesión activa.</Text> : <>
      <Card className={ui.panel}>
        {profileQuery.isLoading ? <Spinner label="Cargando perfil" /> : profileQuery.isError ? <div role="alert">
          <p>{status === 401 ? 'Tu sesión no permite consultar el perfil. Vuelve a iniciar sesión.'
            : status === 403 ? 'No tienes permiso para consultar este perfil.'
            : 'No fue posible cargar tu perfil. Inténtalo de nuevo.'}</p>
          <Button onClick={() => void profileQuery.refetch()}>Reintentar</Button>
        </div> : profileQuery.data ? <>
          <h2>Tus datos</h2>
          <dl className={ui.definition}>
            <div><dt>Nombre</dt><dd>{profileQuery.data.displayName}</dd></div>
            <div><dt>Email</dt><dd>{profileQuery.data.email || 'Sin indicar'}</dd></div>
          </dl>
        </> : <Text>Perfil vacío.</Text>}
      </Card>
      <div className={ui.actions}><Button appearance="secondary" onClick={handleLogout}>Cerrar sesión</Button></div>
    </>}
  </section>
}
