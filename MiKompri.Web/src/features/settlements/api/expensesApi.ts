import { responseError } from '../../../shared/api/request'
export type RegisterExpenseInput = {
  realPaidPrice: number
  paidBy: string
  purchasedBy?: string | null
  participants: string[]
  currency: 'EUR'
}

export type RegisteredExpense = {
  expenseId: string
}

async function parseExpenseError(response: Response): Promise<Error & { status?: number }> {
  return responseError(response)
}

export async function registerItemExpense(
  shoppingListApiBaseUrl: string,
  accessToken: string,
  sharedListId: string,
  itemId: string,
  input: RegisterExpenseInput,
): Promise<RegisteredExpense> {
  const response = await fetch(
    `${shoppingListApiBaseUrl}/api/v1/shared-lists/${sharedListId}/items/${itemId}/expenses`,
    {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${accessToken}`,
        'Content-Type': 'application/json',
        Accept: 'application/json',
      },
      body: JSON.stringify({
        realPaidPrice: input.realPaidPrice,
        paidBy: input.paidBy,
        purchasedBy: input.purchasedBy ?? null,
        participants: input.participants,
        currency: input.currency,
      }),
    },
  )

  if (!response.ok) {
    throw await parseExpenseError(response)
  }

  const payload = (await response.json()) as string | { id?: string; expenseId?: string }
  if (typeof payload === 'string') {
    return { expenseId: payload }
  }

  return { expenseId: payload.expenseId ?? payload.id ?? '' }
}
