import { syncMyProfile } from '../api/profileApi'
import { currentAccount } from '../../../auth/session/accountIdentity'
import ui from '../../../shared/ui/AppSurface.module.css'
import { Button, Text } from '@fluentui/react-components'
import { InteractionStatus } from '@azure/msal-browser'
import { useIsAuthenticated, useMsal } from '@azure/msal-react'
import { useEffect, useMemo, useRef, useState, useSyncExternalStore } from 'react'
import { getLoginError, onLoginErrorChanged } from '../../../auth/session/accountEvents'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { PageHeader } from '../../../shared/ui/PageHeader'
import { createAuthSessionService } from '../../../auth/session/authSessionService'
import { loginRequest } from '../../../auth/msal/msalConfig'
import { googleAvailable, type AuthProvider } from '../../../auth/msal/providerSettings'
import { env } from '../../../shared/config/env'
import { consumeReturnTo } from '../../../auth/session/returnIntent'

export function LoginPage() {
  const { instance, accounts, inProgress } = useMsal()
  const isAuthenticated = useIsAuthenticated()
  const location = useLocation()
  const navigate = useNavigate()
  const redirected = useRef(false)
  const loginError = useSyncExternalStore(onLoginErrorChanged, getLoginError, getLoginError)
  const [message, setMessage] = useState('Accede con tu cuenta de MiKompri para continuar.')

  const authSessionService = useMemo(() => createAuthSessionService(instance), [instance])
  const isPlaceholderClientId = env.entraClientId === '00000000-0000-0000-0000-000000000000'

  useEffect(() => {
    const hasMsalSession = Boolean(currentAccount(instance, accounts))
    const isReady = inProgress === InteractionStatus.None
    const isAuthPage = location.pathname === '/login' || location.pathname === '/auth/callback'

    if (isReady && hasMsalSession && isAuthPage && !redirected.current) {
      redirected.current = true
      setMessage('Preparando tu cuenta…')
      void authSessionService.getAccessToken(loginRequest.scopes, currentAccount(instance, accounts))
        .then(token => syncMyProfile(env.usersApiBaseUrl, token))
        .then(() => navigate(consumeReturnTo(), { replace: true }))
        .catch(() => { redirected.current = false; setMessage('No se pudo preparar tu cuenta. Vuelve a iniciar sesión o inténtalo más tarde.') })
    }
  }, [accounts.length, inProgress, isAuthenticated, location.pathname, navigate, instance, accounts, authSessionService])

  const account = currentAccount(instance, accounts)

  const handleLogin = async (provider: AuthProvider = 'microsoft') => {
    if (provider === 'microsoft' && isPlaceholderClientId) {
      setMessage('Configura VITE_ENTRA_CLIENT_ID con el Application (client) ID real de Entra ID.')
      return
    }

    if (inProgress !== InteractionStatus.None) {
      return
    }

    try {
      await authSessionService.login(loginRequest.scopes, provider)
    } catch {
      setMessage('No se pudo iniciar sesión. Revisa el acceso del proveedor y vuelve a intentarlo.')
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
          <Text role="status">{loginError || message}</Text>
          <p>Si ya utilizas MiKompri con Microsoft, continúa con Microsoft para acceder a tu cuenta. La recuperación del acceso se realiza con tu proveedor original.</p>
          <div className={ui.actions}>
            {googleAvailable() ? <Button appearance="primary" onClick={() => void handleLogin('google')} disabled={inProgress !== InteractionStatus.None}>Continuar con Google</Button> : null}
            <Button appearance={googleAvailable() ? "secondary" : "primary"} onClick={() => void handleLogin('microsoft')} disabled={isPlaceholderClientId || inProgress !== InteractionStatus.None}>
              Continuar con Microsoft
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
