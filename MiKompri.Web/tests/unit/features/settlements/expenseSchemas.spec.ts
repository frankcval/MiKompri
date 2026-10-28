import { describe, expect, it } from 'vitest'

type ExpenseDraft = {
  amount: number
  paidBy: string
  participantIds: string[]
}

function validateExpenseDraft(input: ExpenseDraft) {
  const errors: string[] = []

  if (!Number.isFinite(input.amount) || input.amount <= 0) {
    errors.push('amount')
  }

  if (!input.paidBy || input.paidBy.trim().length === 0) {
    errors.push('paidBy')
  }

  if (!input.participantIds || input.participantIds.length === 0) {
    errors.push('participantIds')
  }

  return {
    success: errors.length === 0,
    errors,
  }
}

describe('expenseSchemas', () => {
  it('rechaza monto inválido', () => {
    const result = validateExpenseDraft({ amount: 0, paidBy: 'u-1', participantIds: ['u-1'] })
    expect(result.success).toBe(false)
    expect(result.errors).toContain('amount')
  })

  it('acepta gasto válido', () => {
    const result = validateExpenseDraft({ amount: 99.5, paidBy: 'u-1', participantIds: ['u-1', 'u-2'] })
    expect(result.success).toBe(true)
  })
})
