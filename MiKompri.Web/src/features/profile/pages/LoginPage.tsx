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
import { googleAvailable } from '../../../auth/msal/providerSettings'
import { env } from '../../../shared/config/env'
import { errorMessages } from '../../../shared/api/request'
import { consumeReturnTo } from '../../../auth/session/returnIntent'

export function LoginPage() {
  const { instance, accounts, inProgress } = useMsal()
  const isAuthenticated = useIsAuthenticated()
  const location = useLocation()
  const navigate = useNavigate()
  const redirected = useRef(false)
  const loginError = useSyncExternalStore(onLoginErrorChanged, getLoginError, getLoginError)
  const [message, setMessage] = useState('Accede con tu cuenta de MiKompri para continuar.')
  const [bootstrapFailed, setBootstrapFailed] = useState(false)
  const [preparing, setPreparing] = useState(false)
  const [retryAttempt, setRetryAttempt] = useState(0)

  const authSessionService = useMemo(() => createAuthSessionService(instance), [instance])
  const hasGoogle = googleAvailable()

  useEffect(() => {
    const hasMsalSession = Boolean(currentAccount(instance, accounts))
    const isReady = inProgress === InteractionStatus.None
    const isAuthPage = location.pathname === '/login' || location.pathname === '/auth/callback'

    if (isReady && hasMsalSession && isAuthPage && !redirected.current) {
      redirected.current = true
      setPreparing(true)
      setBootstrapFailed(false)
      setMessage('Preparando tu cuenta…')
      void authSessionService.getAccessToken(loginRequest.scopes, currentAccount(instance, accounts))
        .then(token => syncMyProfile(env.usersApiBaseUrl, token))
        .then(() => navigate(consumeReturnTo(), { replace: true }))
        .catch(error => {
          // Do not automatically retry on re-render: a failed API is not a failed login.
          const status = typeof error === 'object' && error !== null && 'status' in error ? Number(error.status) : undefined
          setMessage((status && errorMessages[status]) || 'No se pudo preparar tu cuenta. Comprueba la conexión con el servicio y reintenta el acceso.')
          setBootstrapFailed(true)
          setPreparing(false)
        })
    }
  }, [accounts.length, inProgress, isAuthenticated, location.pathname, navigate, instance, accounts, authSessionService, retryAttempt])

  const account = currentAccount(instance, accounts)

  const handleLogin = async () => {
    if (!hasGoogle || preparing || inProgress !== InteractionStatus.None) {
      return
    }

    try {
      await authSessionService.login(loginRequest.scopes, 'google')
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
          <Text role="status">{loginError || (!hasGoogle && !account ? 'El acceso con Google no está disponible en este entorno.' : message)}</Text>
          {hasGoogle ? <p>En la siguiente pantalla, pulsa «Iniciar sesión con Google» para continuar con Google.</p> : null}
          <div className={ui.actions}>
            {hasGoogle ? <Button appearance="primary" onClick={() => void handleLogin()} disabled={preparing || inProgress !== InteractionStatus.None}>Continuar con Google</Button> : null}
            {account && bootstrapFailed ? <Button appearance="secondary" disabled={inProgress !== InteractionStatus.None} onClick={() => {
              redirected.current = false
              setRetryAttempt(attempt => attempt + 1)
            }}>Reintentar acceso</Button> : null}
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
