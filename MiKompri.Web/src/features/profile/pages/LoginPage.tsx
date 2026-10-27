import ui from '../../../shared/ui/AppSurface.module.css'
import { Button, Text } from '@fluentui/react-components'
import { InteractionStatus } from '@azure/msal-browser'
import { useIsAuthenticated, useMsal } from '@azure/msal-react'
import { useEffect, useMemo, useState } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { PageHeader } from '../../../shared/ui/PageHeader'
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
      navigate('/personal-lists', { replace: true })
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
    <main className={ui.login}>
      <div className={ui.loginInner}>
        <Link className={ui.brand} to="/" aria-label="MiKompri, inicio"><span aria-hidden="true">✓</span>MiKompri.</Link>
        <section className={`${ui.page} ${ui.panel}`}>
          <PageHeader title="Inicio de sesión" description="Tu próxima compra empieza aquí. Entra para guardar tus listas y organizarte con los tuyos." />
          <Text role="status">{message}</Text>
          <div className={ui.actions}>
            <Button appearance="primary" onClick={handleLogin} disabled={isPlaceholderClientId || inProgress !== InteractionStatus.None}>
              Iniciar sesión
            </Button>
            {account ? <Button appearance="secondary" onClick={handleLogout}>
              Cerrar sesión
            </Button> : null}
          </div>
          <Link to="/">Volver a la portada</Link>
        </section>
      </div>
    </main>
  )
}
