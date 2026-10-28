import { lazy } from 'react'
export const ProfilePage = lazy(() => import('../../features/profile/pages/ProfilePage').then(m => ({ default: m.ProfilePage })))
export const GroupsPage = lazy(() => import('../../features/groups/pages/GroupsPage').then(m => ({ default: m.GroupsPage })))
export const PersonalListsPage = lazy(() => import('../../features/shopping-lists/pages/PersonalListsPage').then(m => ({ default: m.PersonalListsPage })))
export const SharedListsPage = lazy(() => import('../../features/shared-lists/pages/SharedListsPage').then(m => ({ default: m.SharedListsPage })))
export const SharedListDetailPage = lazy(() => import('../../features/shared-lists/pages/SharedListDetailPage').then(m => ({ default: m.SharedListDetailPage })))
export const ProductCatalogPage = lazy(() => import('../../features/product-catalog/pages/ProductCatalogPage').then(m => ({ default: m.ProductCatalogPage })))
export const ListsPage = lazy(() => import('../../features/shopping-lists/pages/ListsPage').then(m => ({ default: m.ListsPage })))
export const GroupDetailPage = lazy(() => import('../../features/groups/pages/GroupDetailPage').then(m => ({ default: m.GroupDetailPage })))
