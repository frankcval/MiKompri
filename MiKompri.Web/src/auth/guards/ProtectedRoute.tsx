import { InteractionStatus } from '@azure/msal-browser'
import { useIsAuthenticated, useMsal } from '@azure/msal-react'
import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { Spinner } from '@fluentui/react-components'
import { rememberReturnTo } from '../session/returnIntent'
import { selectDraftAccount } from '../session/draftStore'

export function ProtectedRoute() {
  const isAuthenticated = useIsAuthenticated()
  const { accounts, inProgress } = useMsal()
  const location = useLocation()

  if (inProgress !== InteractionStatus.None) {
    return <main><Spinner label="Recuperando tu sesión…" /></main>
  }

  if (!isAuthenticated && accounts.length === 0) {
    rememberReturnTo(location.pathname + location.search)
    return <Navigate to="/login" replace />
  }

  if (accounts[0]) selectDraftAccount(accounts[0].homeAccountId)

  return <Outlet />
}
