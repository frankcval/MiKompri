export type SharedListSummary = {
  id: string
  name: string
  groupId: string
  description?: string | null
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
  const error = new Error(`shared_lists_api_error_${response.status}`) as Error & { status?: number }
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
