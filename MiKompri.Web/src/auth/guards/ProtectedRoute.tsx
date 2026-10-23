import { Navigate, Outlet } from 'react-router-dom'
import { useIsAuthenticated, useMsal } from '@azure/msal-react'
import { restoreSessionSnapshot } from '../session/authSessionService'

export function ProtectedRoute() {
  const isAuthenticated = useIsAuthenticated()
  const { accounts } = useMsal()
  const hasRestorableSession = Boolean(restoreSessionSnapshot())
  const hasMsalAccount = accounts.length > 0

  if (!isAuthenticated && !hasRestorableSession && !hasMsalAccount) {
    return <Navigate to="/login" replace />
  }

  return <Outlet />
}
