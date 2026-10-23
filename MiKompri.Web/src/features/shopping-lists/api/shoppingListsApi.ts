export type PersonalListSummary = {
  id: string
  name: string
  ownerId?: string
  groupId?: string | null
}

export type ListItemInput = {
  productId?: string | null
  productName: string
  price: number
  quantity: number
}

export type CreatePersonalListInput = {
  name: string
  groupId?: string | null
}

async function parseError(response: Response): Promise<Error & { status?: number }> {
  const error = new Error(`shopping_lists_api_error_${response.status}`) as Error & { status?: number }
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
