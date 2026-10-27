export type SharedListSummary = {
  id: string
  name: string
  groupId: string
  description?: string | null
}

export type SharedListDetail = SharedListSummary & {
  items: { id: string; productName: string; quantity: number; isPurchased: boolean }[]
}

export async function getSharedListById(baseUrl: string, token: string, listId: string): Promise<SharedListDetail> {
  const response = await fetch(`${baseUrl}/api/v1/shared-lists/${listId}`, {
    headers: { Authorization: `Bearer ${token}`, Accept: 'application/json' },
  })
  if (!response.ok) throw await parseError(response)
  return await response.json() as SharedListDetail
}

export type CreateSharedListInput = {
  name: string
  groupId: string
  description?: string
}

export type AddSharedListItemInput = {
  productId?: string | null
  name: string
  estimatedPrice: number
  quantity: number
}

async function parseError(response: Response): Promise<Error & { status?: number }> {
  const messages: Record<number, string> = {
    400: 'Revisa el nombre, la cantidad y el precio de los productos.',
    401: 'Tu sesión ha caducado. Inicia sesión de nuevo.',
    403: 'No tienes permiso para operar en esta lista compartida.',
    404: 'Esta lista ya no está disponible. Vuelve a tus listas.',
    409: 'Los datos han cambiado o el producto ya está en la lista. Revisa la compra e inténtalo de nuevo.',
    503: 'El servicio no está disponible temporalmente. Inténtalo de nuevo.',
  }
  const error = new Error(messages[response.status] ?? 'No fue posible guardar los cambios. Inténtalo de nuevo.') as Error & { status?: number }
  error.status = response.status
  return error
}

export async function getSharedListsByGroup(
  shoppingListApiBaseUrl: string,
  accessToken: string,
  groupId: string,
): Promise<SharedListSummary[]> {
  const url = new URL(`${shoppingListApiBaseUrl}/api/v1/shared-lists`)
  url.searchParams.set('groupId', groupId)

  const response = await fetch(url.toString(), {
    method: 'GET',
    headers: {
      Authorization: `Bearer ${accessToken}`,
      Accept: 'application/json',
    },
  })

  if (!response.ok) {
    throw await parseError(response)
  }

  return (await response.json()) as SharedListSummary[]
}

export async function createSharedList(
  shoppingListApiBaseUrl: string,
  accessToken: string,
  input: CreateSharedListInput,
): Promise<string> {
  const response = await fetch(`${shoppingListApiBaseUrl}/api/v1/shared-lists`, {
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

export async function addSharedListItem(
  shoppingListApiBaseUrl: string,
  accessToken: string,
  sharedListId: string,
  input: AddSharedListItemInput,
): Promise<string> {
  const response = await fetch(`${shoppingListApiBaseUrl}/api/v1/shared-lists/${sharedListId}/items`, {
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
