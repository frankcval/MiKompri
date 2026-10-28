import { Navigate, type RouteObject } from 'react-router-dom'
import { Suspense, type ReactNode } from 'react'
import { Spinner } from '@fluentui/react-components'
import App from '../App'
import { MainLayout } from '../layout/MainLayout'
import { ProtectedRoute } from '../../auth/guards/ProtectedRoute'
import { LoginPage } from '../../features/profile/pages/LoginPage'
import { ProfilePage, GroupsPage, PersonalListsPage, SharedListsPage, SharedListDetailPage, ProductCatalogPage, ListsPage, GroupDetailPage } from './DeferredPages'
const deferred = (element: ReactNode) => <Suspense fallback={<Spinner label="Cargando tu compra…" />}>{element}</Suspense>

export const appRoutes: RouteObject[] = [
  { path: '/', element: <App /> },
  {
    path: '/login',
    element: <LoginPage />,
  },
  {
    path: '/auth/callback',
    element: <LoginPage />,
  },
  {
    element: <ProtectedRoute />,
    children: [{
      element: <MainLayout />,
      children: [
      { path: '/lists', element: deferred(<ListsPage />) },
      { path: '/groups/:groupId', element: deferred(<GroupDetailPage />) },
      {
        path: '/profile',
        element: deferred(<ProfilePage />),
      },
      {
        path: '/groups',
        element: deferred(<GroupsPage />),
      },
      {
        path: '/personal-lists',
        element: deferred(<PersonalListsPage />),
      },
      {
        path: '/personal-lists/:listId',
        element: deferred(<PersonalListsPage />),
      },
      {
        path: '/shared-lists',
        element: deferred(<SharedListsPage />),
      },
      { path: '/shared-lists/:listId', element: deferred(<SharedListDetailPage />) },
      {
        path: '/product-catalog',
        element: deferred(<ProductCatalogPage />),
      },
      ],
    }],
  },
  {
    path: '*',
    element: <Navigate to="/" replace />,
  },
]
