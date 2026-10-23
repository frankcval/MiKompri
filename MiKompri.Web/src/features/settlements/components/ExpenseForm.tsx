import { z } from 'zod'
import { useState } from 'react'
import { Button, Field, Input, Textarea } from '@fluentui/react-components'

const expenseSchema = z.object({
  amount: z.number().positive(),
  paidByUserId: z.string().min(1),
  participantUserIds: z.array(z.string().min(1)).min(1),
  note: z.string().optional(),
})

export type ExpenseFormValue = z.infer<typeof expenseSchema>

type ExpenseFormProps = {
  onSubmit: (value: ExpenseFormValue) => Promise<void> | void
  isSubmitting?: boolean
}

export function ExpenseForm({ onSubmit, isSubmitting = false }: ExpenseFormProps) {
  const [amount, setAmount] = useState('')
  const [paidByUserId, setPaidByUserId] = useState('')
  const [participantUserIds, setParticipantUserIds] = useState('')
  const [note, setNote] = useState('')
  const [error, setError] = useState<string | null>(null)

  const handleSubmit = async () => {
    const parsed = expenseSchema.safeParse({
      amount: Number(amount),
      paidByUserId: paidByUserId.trim(),
      participantUserIds: participantUserIds
        .split(',')
        .map((x) => x.trim())
        .filter(Boolean),
      note: note.trim().length > 0 ? note.trim() : undefined,
    })

    if (!parsed.success) {
      setError('Completa los datos del gasto correctamente.')
      return
    }

    setError(null)
    await onSubmit(parsed.data)
    setAmount('')
    setPaidByUserId('')
    setParticipantUserIds('')
    setNote('')
  }

  return (
    <section style={{ display: 'grid', gap: '0.75rem' }}>
      <Field label="Monto">
        <Input type="number" value={amount} onChange={(_, d) => setAmount(d.value)} />
      </Field>
      <Field label="Pagado por (UserId)">
        <Input value={paidByUserId} onChange={(_, d) => setPaidByUserId(d.value)} />
      </Field>
      <Field label="Participantes (UserId separados por coma)">
        <Input value={participantUserIds} onChange={(_, d) => setParticipantUserIds(d.value)} />
      </Field>
      <Field label="Nota (opcional)">
        <Textarea value={note} onChange={(_, d) => setNote(d.value)} />
      </Field>
      {error ? <span>{error}</span> : null}
      <Button appearance="primary" onClick={handleSubmit} disabled={isSubmitting}>
        Registrar gasto
      </Button>
    </section>
  )
}
