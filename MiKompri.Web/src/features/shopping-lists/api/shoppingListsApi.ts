import { sessionFetch } from '../../../shared/api/request'
import { responseError } from '../../../shared/api/request'
import { request } from '../../../shared/api/request'

export type PersonalListSummary = {
  id: string
  name: string
  ownerId?: string
  groupId?: string | null
  createdAt?: string
  updatedAt?: string | null
  status?: string
  items?: { id: string; productName: string; quantity: number; productPrice?: number | null; isPurchased: boolean }[]
}

export type ListItemInput = {
  productId: string
  productName: string
  price: number | null
  quantity: number
}

export type CreatePersonalListInput = {
  name: string
  groupId?: string | null
}

async function parseError(response: Response): Promise<Error & { status?: number }> {
  return responseError(response)
}

export async function getMyPersonalLists(
  shoppingListApiBaseUrl: string,
  accessToken: string,
): Promise<PersonalListSummary[]> {
  const response = await sessionFetch(`${shoppingListApiBaseUrl}/api/v1/purchaselists/mine`, {
    method: 'GET',
    headers: {
      Authorization: `Bearer ${accessToken}`,
      Accept: 'application/json',
    },
  })

  if (!response.ok) {
    throw await parseError(response)
  }

  return (await response.json()) as PersonalListSummary[]
}

export async function createPersonalList(
  shoppingListApiBaseUrl: string,
  accessToken: string,
  input: CreatePersonalListInput,
): Promise<string> {
  const response = await sessionFetch(`${shoppingListApiBaseUrl}/api/v1/purchaselists`, {
    method: 'POST',
    headers: {
      Authorization: `Bearer ${accessToken}`,
      'Content-Type': 'application/json',
      Accept: 'application/json',
    },
    body: JSON.stringify(input),
  })

  if (!response.ok) {
    throw await parseError(response)
  }

  return (await response.json()) as string
}

export async function addPersonalListItem(
  shoppingListApiBaseUrl: string,
  accessToken: string,
  listId: string,
  item: ListItemInput,
): Promise<string> {
  const response = await sessionFetch(`${shoppingListApiBaseUrl}/api/v1/purchaselists/${listId}/items`, {
    method: 'POST',
    headers: {
      Authorization: `Bearer ${accessToken}`,
      'Content-Type': 'application/json',
      Accept: 'application/json',
    },
    body: JSON.stringify(item),
  })

  if (!response.ok) {
    throw await parseError(response)
  }

  return (await response.json()) as string
}

// Only called for a list and item resolved from the authenticated /mine response.
// The generic backend handler still requires ownership hardening; never reuse for shared lists.
export async function markPersonalListItem(baseUrl: string, token: string, listId: string, itemId: string): Promise<void> {
  const response = await sessionFetch(`${baseUrl}/api/v1/purchaselists/${listId}/items/${itemId}/mark-as-purchased`, {
    method: 'POST', headers: { Authorization: `Bearer ${token}`, Accept: 'application/json' },
  })
  if (!response.ok) throw await parseError(response)
}

export const setPurchaseState = (base: string, token: string, listId: string, itemId: string, isPurchased: boolean) => request<void>(`${base}/api/v1/purchaselists/${listId}/items/${itemId}/purchase-state`, token, 'PATCH', { isPurchased })
export const renamePersonalList = (base: string, token: string, id: string, name: string) => request<void>(`${base}/api/v1/purchaselists/${id}`, token, 'PUT', { name })
export const deletePersonalList = (base: string, token: string, id: string) => request<void>(`${base}/api/v1/purchaselists/${id}`, token, 'DELETE')
export const editPersonalItem = (base: string, token: string, id: string, itemId: string, quantity: number, price: number | null) => request<void>(`${base}/api/v1/purchaselists/${id}/items/${itemId}`, token, 'PUT', { quantity, price })
export const deletePersonalItem = (base: string, token: string, id: string, itemId: string) => request<void>(`${base}/api/v1/purchaselists/${id}/items/${itemId}`, token, 'DELETE')
