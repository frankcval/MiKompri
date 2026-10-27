export type PersonalListSummary = {
  id: string
  name: string
  ownerId?: string
  groupId?: string | null
  items?: { id: string; productName: string; quantity: number; productPrice?: number | null; isPurchased: boolean }[]
}

export type ListItemInput = {
  productId: string
  productName: string
  price: number
  quantity: number
}

export type CreatePersonalListInput = {
  name: string
  groupId?: string | null
}

async function parseError(response: Response): Promise<Error & { status?: number }> {
  let message = 'No fue posible guardar los cambios. Inténtalo de nuevo.'
  if (response.status === 401) message = 'Tu sesión no permite esta operación. Inicia sesión de nuevo.'
  if (response.status === 403) message = 'No tienes permiso para modificar esta lista.'
  if (response.status === 404) message = 'La lista ya no está disponible. Vuelve a tus listas.'
  try {
    const body = await response.json() as { errors?: Record<string, string[]>; error?: string }
    if (response.status === 400) {
      message = body.errors ? Object.values(body.errors).flat().join(' ') : body.error || 'Revisa los datos del ítem.'
    }
  } catch { /* Keep a useful message when the response has no JSON body. */ }
  const error = new Error(message) as Error & { status?: number }
  error.status = response.status
  return error
}

export async function getMyPersonalLists(
  shoppingListApiBaseUrl: string,
  accessToken: string,
): Promise<PersonalListSummary[]> {
  const response = await fetch(`${shoppingListApiBaseUrl}/api/v1/purchaselists/mine`, {
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
  const response = await fetch(`${shoppingListApiBaseUrl}/api/v1/purchaselists`, {
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
  const response = await fetch(`${shoppingListApiBaseUrl}/api/v1/purchaselists/${listId}/items`, {
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
  const response = await fetch(`${baseUrl}/api/v1/purchaselists/${listId}/items/${itemId}/mark-as-purchased`, {
    method: 'POST', headers: { Authorization: `Bearer ${token}`, Accept: 'application/json' },
  })
  if (!response.ok) throw await parseError(response)
}
