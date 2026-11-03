import { accountSessionKey } from '../../../auth/session/accountIdentity'
import { useState } from 'react'
import { Button, Spinner } from '@fluentui/react-components'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { useListSession } from '../../shopping-lists/hooks/useListSession'
import { fetchMyProfile } from '../../profile/api/profileApi'
import { getGroupMembers } from '../../groups/api/groupsApi'
import { env } from '../../../shared/config/env'
import { request, describeError } from '../../../shared/api/request'
import { registerItemExpense, type RegisterExpenseInput } from '../api/expensesApi'
import { ExpenseForm } from './ExpenseForm'
import { ExpenseHistoryPanel } from './ExpenseHistoryPanel'
import type { SharedListItem } from '../../shared-lists/api/sharedListsApi'
import { Link } from 'react-router-dom'
import { discardDraft } from '../../../auth/session/draftStore'
import { PurchaseCaptureDialog as PurchaseCapture } from './PurchaseCaptureDialog'
import ui from '../../../shared/ui/AppSurface.module.css'

export type PurchaseRecord = RegisterExpenseInput & { expenseId: string; itemId: string; productName: string; createdAt: string; version: string }

export function PurchaseCaptureContent({ listId, item, groupId, onClose, onPendingChange }: { listId: string; item: SharedListItem; groupId?: string; onClose: () => void; onPendingChange: (pending: boolean) => void }) {
  const { account, token, write } = useListSession(); const client = useQueryClient()
  const [another, setAnother] = useState(false)
  const personal = !groupId
  const base = `${env.shoppingListApiBaseUrl}/api/v1/${personal ? 'purchaselists' : 'shared-lists'}/${listId}`
  const profile = useQuery({ queryKey: ['profile', accountSessionKey(account)], queryFn: async () => fetchMyProfile(env.usersApiBaseUrl, await token()), retry: false })
  const members = useQuery({ queryKey: ['group-members', groupId, accountSessionKey(account)], enabled: Boolean(groupId), queryFn: async () => getGroupMembers(env.usersApiBaseUrl, await token(), groupId!), retry: false })
  const records = useQuery({ queryKey: ['expenses', listId, accountSessionKey(account)], retry: false, queryFn: async () => request<PurchaseRecord[]>(`${base}/${personal ? 'purchases' : 'expenses'}`, await token()) })
  const people = personal && profile.data ? [{ userId: profile.data.id, displayName: profile.data.displayName, role: 'Owner', joinedAt: '' }] : members.data ?? []
  const member = people.find(m => m.userId === profile.data?.id)
  const manager = ['owner', 'admin'].includes(member?.role.toLowerCase() ?? '')
  const payers = manager ? people.map(p => p.userId) : member ? [member.userId] : []
  const existing = records.data?.filter(record => record.itemId === item.id) ?? []
  const failure = records.error ?? profile.error ?? (groupId ? members.error : null)
  return <>
      <p>Registrar estos datos es opcional. El estado de compra se guarda por separado.</p>
      {failure ? <p role="alert">{describeError(failure)} <Button onClick={() => { void records.refetch(); void profile.refetch(); if (groupId) void members.refetch() }}>Reintentar datos</Button></p>
        : records.isPending || profile.isPending || (groupId && members.isPending) ? <Spinner label="Cargando datos de compra…" />
        : !member ? <p role="alert">No tienes una membresía disponible para registrar esta compra.</p>
        : existing.length && !another ? <><p>Este producto ya tiene {existing.length} {existing.length === 1 ? 'registro' : 'registros'}. Consúltalos en Gastos para evitar duplicados.</p>
          <Link to={personal ? `/personal-lists/${listId}?tab=expenses` : `/shared-lists/${listId}?tab=expenses`} onClick={onClose}>Ver gastos registrados</Link>
          <Button onClick={() => setAnother(true)}>Registrar otra compra distinta</Button></>
        : <ExpenseForm key={`${accountSessionKey(account)}:${listId}:${item.id}`} items={[item]} defaultItemId={item.id} members={people} payerIds={payers} personal={personal} defaultPayerId={profile.data!.id} accountKey={accountSessionKey(account)} scope={`capture:${listId}:${item.id}`} submitLabel="Guardar datos de compra" onPendingChange={onPendingChange} onSubmit={async ({ itemId, ...input }) => {
          await write(async t => { if (personal) await request<string>(`${base}/items/${itemId}/purchases`, t, 'POST', input); else await registerItemExpense(env.shoppingListApiBaseUrl, t, listId, itemId, input) })
          if (account) discardDraft(accountSessionKey(account), `capture:${listId}:${item.id}`)
          for (const prefix of ['expenses', 'settlement-summary', 'settlement-proposal']) await client.invalidateQueries({ queryKey: [prefix, listId] })
          onClose()
        }} />}
  </>
}

export function PersonalExpenses({ listId, items }: { listId: string; items: SharedListItem[] }) {
  const { account, token } = useListSession()
  const [selected, setSelected] = useState('')
  const profile = useQuery({ queryKey: ['profile', accountSessionKey(account)], queryFn: async () => fetchMyProfile(env.usersApiBaseUrl, await token()), enabled: Boolean(account) })
  const member = profile.data ? [{ userId: profile.data.id, displayName: profile.data.displayName, role: 'Owner', joinedAt: '' }] : []
  return <section className={ui.stack} aria-label="Gastos personales"><h2>Gastos</h2><p>Consulta y registra compras de esta lista sin repartirlas entre otras personas.</p>
    {items.map(item => <Button key={item.id} onClick={() => setSelected(item.id)}>Registrar compra de {item.productName}</Button>)}
    <ExpenseHistoryPanel listId={listId} items={items} members={member} canManage={Boolean(profile.data)} personal />
    {items.find(item => item.id === selected) ? <PurchaseCapture listId={listId} item={items.find(item => item.id === selected)!} onClose={() => setSelected('')} /> : null}
  </section>
}
