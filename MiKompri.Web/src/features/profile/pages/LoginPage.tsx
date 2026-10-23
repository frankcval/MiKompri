import { Button, Text } from '@fluentui/react-components'
import { InteractionStatus } from '@azure/msal-browser'
import { useIsAuthenticated, useMsal } from '@azure/msal-react'
import { useEffect, useMemo, useState } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { createAuthSessionService } from '../../../auth/session/authSessionService'
import { loginRequest } from '../../../auth/msal/msalConfig'
import { env } from '../../../shared/config/env'

export function LoginPage() {
  const { instance, accounts, inProgress } = useMsal()
  const isAuthenticated = useIsAuthenticated()
  const location = useLocation()
  const navigate = useNavigate()
  const [message, setMessage] = useState('Accede con tu cuenta de MiKompri para continuar.')

  const authSessionService = useMemo(() => createAuthSessionService(instance), [instance])
  const isPlaceholderClientId = env.entraClientId === '00000000-0000-0000-0000-000000000000'

  useEffect(() => {
    const hasMsalSession = isAuthenticated || accounts.length > 0
    const isReady = inProgress === InteractionStatus.None
    const isAuthPage = location.pathname === '/login' || location.pathname === '/auth/callback'

    if (isReady && hasMsalSession && isAuthPage) {
      navigate('/profile', { replace: true })
    }
  }, [accounts.length, inProgress, isAuthenticated, location.pathname, navigate])

  const account = accounts[0]

  const handleLogin = async () => {
    if (isPlaceholderClientId) {
      setMessage('Configura VITE_ENTRA_CLIENT_ID con el Application (client) ID real de Entra ID.')
      return
    }

    if (inProgress !== InteractionStatus.None) {
      return
    }

    try {
      await authSessionService.login(loginRequest.scopes)
    } catch {
      setMessage('No se pudo iniciar sesión. Revisa la configuración de Entra ID y vuelve a intentarlo.')
    }
  }

  const handleLogout = async () => {
    if (inProgress !== InteractionStatus.None) {
      return
    }

    await authSessionService.logout(account)
    navigate('/login', { replace: true })
  }

  return (
    <main style={{ padding: '1.5rem', display: 'grid', gap: '1rem' }}>
      <h1>Inicio de sesión</h1>
      <Text>{message}</Text>
      <div style={{ display: 'flex', gap: '0.75rem' }}>
        <Button appearance="primary" onClick={handleLogin} disabled={isPlaceholderClientId}>
          Iniciar sesión
        </Button>
        <Button appearance="secondary" onClick={handleLogout}>
          Cerrar sesión
        </Button>
      </div>
    </main>
  )
}
