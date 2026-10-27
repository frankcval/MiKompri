import { InteractionStatus } from '@azure/msal-browser'
import { useIsAuthenticated, useMsal } from '@azure/msal-react'
import { Navigate } from 'react-router-dom'
import { Spinner } from '@fluentui/react-components'
import { HomePage } from '../features/home/HomePage'

function App() {
  const authenticated = useIsAuthenticated()
  const { accounts, inProgress } = useMsal()
  if (inProgress !== InteractionStatus.None) return <Spinner label="Cargando sesión" />
  return authenticated || accounts.length > 0 ? <Navigate to="/personal-lists" replace /> : <HomePage />
}

export default App
