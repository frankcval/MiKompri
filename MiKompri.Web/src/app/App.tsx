import { InteractionStatus } from '@azure/msal-browser'
import { useMsal } from '@azure/msal-react'
import { Navigate } from 'react-router-dom'
import { Spinner } from '@fluentui/react-components'
import { HomePage } from '../features/home/HomePage'
import { currentAccount } from '../auth/session/accountIdentity'

function App() {
  const { instance, accounts, inProgress } = useMsal()
  if (inProgress !== InteractionStatus.None) return <Spinner label="Cargando sesión" />
  return currentAccount(instance, accounts) ? <Navigate to="/lists" replace /> : <HomePage />
}

export default App
