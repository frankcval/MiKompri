import { currentAccount, accountSessionKey } from '../session/accountIdentity'
import { InteractionStatus } from '@azure/msal-browser'
import { useMsal } from '@azure/msal-react'
import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { Spinner } from '@fluentui/react-components'
import { rememberReturnTo } from '../session/returnIntent'
import { selectDraftAccount } from '../session/draftStore'
import { useSyncExternalStore } from 'react'
import { onSessionReset, sessionStamp } from '../session/sessionBoundary'

export function ProtectedRoute() {
  const { instance, accounts, inProgress } = useMsal()
  const location = useLocation()
  const generation = useSyncExternalStore(onSessionReset, sessionStamp, sessionStamp)

  if (inProgress !== InteractionStatus.None) {
    return <main><Spinner label="Recuperando tu sesión…" /></main>
  }

  if (!currentAccount(instance, accounts)) {
    rememberReturnTo(location.pathname + location.search)
    return <Navigate to="/login" replace />
  }

  if (currentAccount(instance, accounts)) selectDraftAccount(accountSessionKey(currentAccount(instance, accounts)))

  return <Outlet key={`${accountSessionKey(currentAccount(instance, accounts))}:${generation}`} />
}
