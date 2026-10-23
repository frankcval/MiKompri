import { Button, Text } from '@fluentui/react-components'
import { useMsal } from '@azure/msal-react'
import { useEffect, useMemo, useRef, useState } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { createAuthSessionService, restoreSessionSnapshot } from '../../../auth/session/authSessionService'
import { loginRequest } from '../../../auth/msal/msalConfig'
import { env } from '../../../shared/config/env'

export function LoginPage() {
  const { instance, accounts } = useMsal()
  const location = useLocation()
  const navigate = useNavigate()
  const [message, setMessage] = useState('Accede con tu cuenta de MiKompri para continuar.')

  const authSessionService = useMemo(() => createAuthSessionService(instance), [instance])
  const isPlaceholderClientId = env.entraClientId === '00000000-0000-0000-0000-000000000000'
  const hasHandledRedirectRef = useRef(false)

  useEffect(() => {
    const snapshot = restoreSessionSnapshot()
    if ((snapshot || accounts.length > 0) && location.pathname === '/login') {
      navigate('/profile', { replace: true })
    }
  }, [accounts.length, location.pathname, navigate])

  useEffect(() => {
    const searchParams = new URLSearchParams(location.search)
    const hasAuthCode = searchParams.has('code') || searchParams.has('state') || searchParams.has('error')
    const hasAuthHash = location.hash.includes('code=') || location.hash.includes('state=') || location.hash.includes('error=')
    const shouldProcessRedirect = location.pathname === '/auth/callback' || hasAuthCode || hasAuthHash

    if (!shouldProcessRedirect || hasHandledRedirectRef.current) {
      return
    }

    hasHandledRedirectRef.current = true

    authSessionService
      .bootstrapFromRedirect()
      .then((snapshot) => {
        const hasAccount = accounts.length > 0 || instance.getAllAccounts().length > 0

        if (snapshot || hasAccount) {
          setMessage('Sesión restaurada correctamente.')
          navigate('/profile', { replace: true })
        }
      })
      .catch((error) => {
        console.error('[MiKompri.Web] Error al restaurar sesión MSAL', error)
        setMessage('No se pudo restaurar la sesión. Intenta iniciar sesión de nuevo.')
      })
  }, [accounts.length, authSessionService, instance, location.hash, location.pathname, location.search, navigate])

  const account = accounts[0]

  const handleLogin = async () => {
    if (isPlaceholderClientId) {
      setMessage('Configura VITE_ENTRA_CLIENT_ID con el Application (client) ID real de Entra ID.')
      return
    }

    try {
      await authSessionService.login(loginRequest.scopes)
    } catch {
      setMessage('No se pudo iniciar sesión. Revisa la configuración de Entra ID y vuelve a intentarlo.')
    }
  }

  const handleLogout = async () => {
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
