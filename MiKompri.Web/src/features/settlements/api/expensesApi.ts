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
  const message = response.status === 403 ? 'No tienes permiso para registrar este pago o la membresía ha cambiado.'
    : response.status === 404 ? 'El ítem ya no está disponible. Vuelve a cargar la lista.'
    : response.status === 400 ? 'Revisa el importe, el pagador y los participantes del gasto.'
    : 'No fue posible registrar el gasto. Inténtalo de nuevo.'
  const error = new Error(message) as Error & { status?: number }
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
