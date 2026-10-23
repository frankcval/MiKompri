export type RegisterExpenseInput = {
  amount: number
  paidByUserId: string
  participantUserIds: string[]
  note?: string
}

export type RegisteredExpense = {
  expenseId: string
}

async function parseExpenseError(response: Response): Promise<Error & { status?: number }> {
  const error = new Error(`expenses_api_error_${response.status}`) as Error & { status?: number }
  error.status = response.status
  return error
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
        amount: input.amount,
        paidByUserId: input.paidByUserId,
        participantUserIds: input.participantUserIds,
        note: input.note,
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
