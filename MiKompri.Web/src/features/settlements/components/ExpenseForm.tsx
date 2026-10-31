import { accountSessionKey } from '../../../auth/session/accountIdentity'
import { useLocalToday } from '../../../shared/dates/useLocalToday'
import ui from '../../../shared/ui/AppSurface.module.css'
import { useQuery } from '@tanstack/react-query'
import { useListSession } from '../../shopping-lists/hooks/useListSession'
import { getMarkets } from '../../product-catalog/api/productCatalogApi'
import { env } from '../../../shared/config/env'
import { z } from 'zod'
import { useEffect, useRef, useState } from 'react'
import { useForm, useWatch } from 'react-hook-form'
import { readDraft, writeDraft } from '../../../auth/session/draftStore'
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
  operationId: z.string().uuid(),
  marketId: z.string().uuid().nullable(),
  purchasedOn: z.string().nullable(),
})
export type ExpenseFormValue = RegisterExpenseInput & { itemId: string }

type ExpenseFormProps = {
  items: SharedListDetail['items']
  members: GroupMember[]
  payerIds: string[]
  onSubmit: (value: ExpenseFormValue) => Promise<void>
  accountKey?: string
  scope?: string
  initialValue?: ExpenseFormValue
  defaultPayerId?: string
  submitLabel?: string
  personal?: boolean
  defaultItemId?: string
  onPendingChange?: (pending: boolean) => void
}

export function ExpenseForm({ items, members, payerIds, onSubmit, accountKey = '', scope = 'expense', initialValue, defaultPayerId = '', submitLabel = 'Registrar gasto', personal = false, defaultItemId = '', onPendingChange }: ExpenseFormProps) {
  const today = useLocalToday()
  const { account, token } = useListSession()
  const markets = useQuery({ queryKey: ['catalog-markets', accountSessionKey(account), 'all'], enabled: Boolean(account), retry: false, queryFn: async () => getMarkets(env.productCatalogApiBaseUrl, await token(), true) })
  const defaults = { itemId: initialValue?.itemId ?? defaultItemId, amount: initialValue ? String(initialValue.realPaidPrice) : '', paidBy: initialValue?.paidBy ?? defaultPayerId, purchasedBy: initialValue?.purchasedBy ?? '', participants: personal ? [defaultPayerId] : initialValue?.participants ?? [], operationId: crypto.randomUUID(), marketId: initialValue?.marketId ?? '', purchasedOn: initialValue?.purchasedOn ?? '', uncertain: false }
  const stored = accountKey ? readDraft(accountKey, scope, defaults) : defaults
  const { control, setValue } = useForm({ defaultValues: { ...defaults, ...stored, operationId: stored.operationId || defaults.operationId } })
  const values = useWatch({ control })
  const itemId = values.itemId ?? ''; const amount = values.amount ?? ''; const paidBy = values.paidBy ?? ''; const purchasedBy = values.purchasedBy ?? ''; const participants = values.participants ?? []
  const setItemId = (v: string) => setValue('itemId', v)
  const setAmount = (v: string) => setValue('amount', v)
  const setPaidBy = (v: string) => setValue('paidBy', v)
  const setPurchasedBy = (v: string) => setValue('purchasedBy', v)
  const setParticipants = (v: string[] | ((current: string[]) => string[])) => setValue('participants', typeof v === 'function' ? v(participants) : v)
  useEffect(() => { if (accountKey) writeDraft(accountKey, scope, values) }, [accountKey, scope, values])
  const [pending, setPending] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const submitting = useRef(false)
  const uncertain = values.uncertain === true
  const locked = pending || uncertain
  const parsed = expenseSchema.safeParse({ itemId, realPaidPrice: Number(amount.replace(',', '.')), paidBy, purchasedBy: purchasedBy || null, participants, currency: 'EUR', operationId: values.operationId, marketId: values.marketId || null, purchasedOn: values.purchasedOn || null })
  const valid = parsed.success && items.some((item) => item.id === itemId) && payerIds.includes(paidBy)
    && participants.every((id) => members.some((member) => member.userId === id))
    && (!purchasedBy || payerIds.includes(purchasedBy))
    && (!values.purchasedOn || values.purchasedOn <= today)

  return <form className={ui.form} onSubmit={async (event) => {
    event.preventDefault()
    if (!valid || !parsed.success || submitting.current) return
    submitting.current = true
    setPending(true)
    onPendingChange?.(true)
    setError('')
    setSuccess('')
    try {
      await onSubmit({ ...parsed.data, expectedVersion: initialValue?.expectedVersion })
      setAmount('')
      setPaidBy(defaultPayerId)
      setPurchasedBy('')
      setParticipants(personal ? [defaultPayerId] : [])
      setValue('operationId', crypto.randomUUID())
      setValue('uncertain', false)
      setSuccess(initialValue ? 'Gasto actualizado.' : 'Gasto registrado.')
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : 'No fue posible registrar el gasto. Inténtalo de nuevo.')
      if ((cause as { status?: number }).status == null) setValue('uncertain', true)
    } finally {
      submitting.current = false
      setPending(false)
      onPendingChange?.(false)
    }
  }}>
    <Field label="Ítem de la lista" required>
      <Select value={itemId} required disabled={locked || Boolean(defaultItemId) || Boolean(initialValue)} onChange={(_, data) => { setItemId(data.value); setSuccess('') }}>
        <option value="">Selecciona un producto</option>
        {items.map((item) => <option key={item.id} value={item.id}>{item.productName} · Cantidad: {item.quantity}</option>)}
      </Select>
    </Field>
    <Field label="Importe pagado (EUR)" required>
      <Input type="number" min="0.01" step="0.01" required value={amount} disabled={locked} onChange={(_, data) => setAmount(data.value)} />
      <small>Importe total pagado por esta compra, sin multiplicar por la cantidad.</small>
    </Field>
    <Field label="Mercado de compra (opcional)"><Select value={values.marketId ?? ''} disabled={locked} onChange={(_, d) => setValue('marketId', d.value)}>
      <option value="">Sin indicar</option>{markets.data?.filter(m => m.isActive !== false || m.id === values.marketId).map(m => <option key={m.id} value={m.id}>{m.name}{m.isActive === false ? ' (inactivo)' : ''}</option>)}
      {values.marketId && !markets.data?.some(m => m.id === values.marketId) ? <option value={values.marketId}>Mercado no disponible</option> : null}
    </Select></Field>
    {markets.isError ? <p role="alert">No se pudieron cargar los mercados. Puedes guardar sin mercado o <Button onClick={() => void markets.refetch()}>Reintentar mercados</Button>.</p> : null}
    <Field label="Fecha de compra (opcional)"><Input type="date" value={values.purchasedOn ?? ''} max={today} disabled={locked} onChange={(_, d) => setValue('purchasedOn', d.value)} /></Field>
    {personal ? <p>Esta compra se registra a tu nombre. No se reparte entre otras personas.</p> : <>
    <Field label="Pagado por" required>
      <Select required value={paidBy} disabled={locked} onChange={(_, data) => setPaidBy(data.value)}>
        <option value="">Selecciona quién pagó</option>
        {members.filter((member) => payerIds.includes(member.userId)).map((member) => <option key={member.userId} value={member.userId}>{memberName(members, member.userId)}</option>)}
      </Select>
    </Field>
    <Field label="Comprado por (opcional)">
      <Select value={purchasedBy} disabled={locked} onChange={(_, data) => setPurchasedBy(data.value)}>
        <option value="">Sin indicar</option>
        {members.filter(member => payerIds.includes(member.userId)).map((member) => <option key={member.userId} value={member.userId}>{memberName(members, member.userId)}</option>)}
      </Select>
    </Field>
    <fieldset className={ui.fieldset}>
      <legend>Participantes del gasto</legend>
      <p>Revisa entre quiénes se reparte este importe antes de guardar.</p>
      {members.map((member) => <Checkbox key={member.userId} label={memberName(members, member.userId)}
        disabled={locked} checked={participants.includes(member.userId)} onChange={(_, data) => {
          setParticipants((current) => data.checked === true ? [...new Set([...current, member.userId])] : current.filter((id) => id !== member.userId))
        }} />)}
    </fieldset>
    </>}
    {participants.some(id => !members.some(m => m.userId === id)) ? <p role="alert">Hay participantes del borrador que ya no están disponibles. <Button disabled={pending} onClick={() => setParticipants(participants.filter(id => members.some(m => m.userId === id)))}>Revisar participantes disponibles</Button></p> : null}
    {error ? <p role="alert">{error}</p> : null}
    {success ? <p role="status">{success}</p> : null}
    {uncertain ? <p role="alert">No se confirmó el resultado. Conservamos los datos y la misma operación para consultar o guardar sin duplicarla.</p> : null}
    <Button appearance="primary" type="submit" disabled={!valid || pending}>{pending ? 'Guardando gasto…' : uncertain ? 'Consultar o guardar la misma operación' : submitLabel}</Button>
  </form>
}
