import { Navigate, type RouteObject } from 'react-router-dom'
import App from '../App'
import { ProtectedRoute } from '../../auth/guards/ProtectedRoute'

function LoginPage() {
  return <main style={{ padding: '1.5rem' }}>Inicia sesión para continuar.</main>
}

export const appRoutes: RouteObject[] = [
  {
    path: '/login',
    element: <LoginPage />,
  },
  {
    element: <ProtectedRoute />,
    children: [
      {
        path: '/',
        element: <App />,
      },
    ],
  },
  {
    path: '*',
    element: <Navigate to="/" replace />,
  },
]
