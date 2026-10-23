import { Button, Text } from '@fluentui/react-components'
import { useMsal } from '@azure/msal-react'
import { useEffect, useMemo, useState } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { createAuthSessionService, restoreSessionSnapshot } from '../../../auth/session/authSessionService'
import { loginRequest } from '../../../auth/msal/msalConfig'

export function LoginPage() {
  const { instance, accounts } = useMsal()
  const location = useLocation()
  const navigate = useNavigate()
  const [message, setMessage] = useState('Accede con tu cuenta de MiKompri para continuar.')

  const authSessionService = useMemo(() => createAuthSessionService(instance), [instance])

  useEffect(() => {
    const snapshot = restoreSessionSnapshot()
    if (snapshot && location.pathname === '/login') {
      navigate('/profile', { replace: true })
    }
  }, [location.pathname, navigate])

  useEffect(() => {
    if (location.pathname !== '/auth/callback') {
      return
    }

    authSessionService
      .bootstrapFromRedirect()
      .then(() => {
        setMessage('Sesión restaurada correctamente.')
        navigate('/profile', { replace: true })
      })
      .catch(() => setMessage('No se pudo restaurar la sesión. Intenta iniciar sesión de nuevo.'))
  }, [authSessionService, location.pathname, navigate])

  const account = accounts[0]

  const handleLogin = async () => {
    await authSessionService.login(loginRequest.scopes)
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
        <Button appearance="primary" onClick={handleLogin}>
          Iniciar sesión
        </Button>
        <Button appearance="secondary" onClick={handleLogout}>
          Cerrar sesión
        </Button>
      </div>
    </main>
  )
}
