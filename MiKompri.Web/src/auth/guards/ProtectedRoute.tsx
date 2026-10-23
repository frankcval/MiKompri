import { InteractionStatus } from '@azure/msal-browser'
import { useIsAuthenticated, useMsal } from '@azure/msal-react'
import { Navigate, Outlet } from 'react-router-dom'

export function ProtectedRoute() {
  const isAuthenticated = useIsAuthenticated()
  const { accounts, inProgress } = useMsal()

  if (inProgress !== InteractionStatus.None) {
    return null
  }

  if (!isAuthenticated && accounts.length === 0) {
    return <Navigate to="/login" replace />
  }

  return <Outlet />
}
