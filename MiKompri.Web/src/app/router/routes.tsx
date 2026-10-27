import { Navigate, type RouteObject } from 'react-router-dom'
import App from '../App'
import { MainLayout } from '../layout/MainLayout'
import { ProtectedRoute } from '../../auth/guards/ProtectedRoute'
import { LoginPage } from '../../features/profile/pages/LoginPage'
import { ProfilePage } from '../../features/profile/pages/ProfilePage'
import { GroupsPage } from '../../features/groups/pages/GroupsPage'
import { PersonalListsPage } from '../../features/shopping-lists/pages/PersonalListsPage'
import { SharedListsPage } from '../../features/shared-lists/pages/SharedListsPage'
import { SharedListDetailPage } from '../../features/shared-lists/pages/SharedListDetailPage'
import { ProductCatalogPage } from '../../features/product-catalog/pages/ProductCatalogPage'

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
      {
        path: '/profile',
        element: <ProfilePage />,
      },
      {
        path: '/groups',
        element: <GroupsPage />,
      },
      {
        path: '/personal-lists',
        element: <PersonalListsPage />,
      },
      {
        path: '/personal-lists/:listId',
        element: <PersonalListsPage />,
      },
      {
        path: '/shared-lists',
        element: <SharedListsPage />,
      },
      { path: '/shared-lists/:listId', element: <SharedListDetailPage /> },
      {
        path: '/product-catalog',
        element: <ProductCatalogPage />,
      },
      ],
    }],
  },
  {
    path: '*',
    element: <Navigate to="/" replace />,
  },
]
