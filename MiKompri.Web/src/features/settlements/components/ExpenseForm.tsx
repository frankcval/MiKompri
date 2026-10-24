import { z } from 'zod'
import { useRef, useState } from 'react'
import { Button, Checkbox, Field, Input, Select } from '@fluentui/react-components'
import type { GroupMember } from '../../groups/api/groupsApi'
import { memberName } from '../../groups/memberName'
import type { SharedListDetail } from '../../shared-lists/api/sharedListsApi'
import type { RegisterExpenseInput } from '../api/expensesApi'

const expenseSchema = z.object({
  itemId: z.string().uuid(),
  realPaidPrice: z.number().positive().finite(),
  paidBy: z.string().uuid(),
  purchasedBy: z.string().uuid().nullable(),
  participants: z.array(z.string().uuid()).min(1),
  currency: z.literal('EUR'),
})
export type ExpenseFormValue = RegisterExpenseInput & { itemId: string }

type ExpenseFormProps = {
  items: SharedListDetail['items']
  members: GroupMember[]
  payerIds: string[]
  onSubmit: (value: ExpenseFormValue) => Promise<void>
}

export function ExpenseForm({ items, members, payerIds, onSubmit }: ExpenseFormProps) {
  const [itemId, setItemId] = useState('')
  const [amount, setAmount] = useState('')
  const [paidBy, setPaidBy] = useState('')
  const [purchasedBy, setPurchasedBy] = useState('')
  const [participants, setParticipants] = useState<string[]>([])
  const [pending, setPending] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const submitting = useRef(false)
  const parsed = expenseSchema.safeParse({ itemId, realPaidPrice: Number(amount), paidBy, purchasedBy: purchasedBy || null, participants, currency: 'EUR' })
  const valid = parsed.success && items.some((item) => item.id === itemId) && payerIds.includes(paidBy)
    && participants.every((id) => members.some((member) => member.userId === id))
    && (!purchasedBy || members.some((member) => member.userId === purchasedBy))

  return <form style={{ display: 'grid', gap: '0.75rem', minWidth: 0 }} onSubmit={async (event) => {
    event.preventDefault()
    if (!valid || !parsed.success || submitting.current) return
    submitting.current = true
    setPending(true)
    setError('')
    setSuccess('')
    try {
      await onSubmit(parsed.data)
      setAmount('')
      setPaidBy('')
      setPurchasedBy('')
      setParticipants([])
      setSuccess('Gasto registrado. El reparto se ha actualizado.')
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : 'No fue posible registrar el gasto. Inténtalo de nuevo.')
    } finally {
      submitting.current = false
      setPending(false)
    }
  }}>
    <Field label="Ítem de la lista" required>
      <Select value={itemId} required disabled={pending} onChange={(_, data) => { setItemId(data.value); setSuccess('') }}>
        <option value="">Selecciona un ítem</option>
        {items.map((item) => <option key={item.id} value={item.id}>{item.productName} · Cantidad: {item.quantity}</option>)}
      </Select>
    </Field>
    <Field label="Importe pagado (EUR)" required>
      <Input type="number" min="0.01" step="0.01" required value={amount} disabled={pending} onChange={(_, data) => setAmount(data.value)} />
    </Field>
    <Field label="Pagado por" required>
      <Select required value={paidBy} disabled={pending} onChange={(_, data) => setPaidBy(data.value)}>
        <option value="">Selecciona quién pagó</option>
        {members.filter((member) => payerIds.includes(member.userId)).map((member) => <option key={member.userId} value={member.userId}>{memberName(members, member.userId)}</option>)}
      </Select>
    </Field>
    <Field label="Comprado por (opcional)">
      <Select value={purchasedBy} disabled={pending} onChange={(_, data) => setPurchasedBy(data.value)}>
        <option value="">Sin indicar</option>
        {members.map((member) => <option key={member.userId} value={member.userId}>{memberName(members, member.userId)}</option>)}
      </Select>
    </Field>
    <fieldset style={{ minWidth: 0, margin: 0, padding: '0.75rem' }}>
      <legend>Participantes del gasto</legend>
      <p>Selecciona entre quiénes se reparte este importe.</p>
      {members.map((member) => <Checkbox key={member.userId} label={memberName(members, member.userId)}
        disabled={pending} checked={participants.includes(member.userId)} onChange={(_, data) => {
          setParticipants((current) => data.checked === true ? [...new Set([...current, member.userId])] : current.filter((id) => id !== member.userId))
        }} />)}
    </fieldset>
    {error ? <p role="alert">{error}</p> : null}
    {success ? <p role="status">{success}</p> : null}
    <Button appearance="primary" type="submit" disabled={!valid || pending}>{pending ? 'Registrando gasto…' : 'Registrar gasto'}</Button>
  </form>
}
