import { useState } from 'react'
import { Button, Spinner } from '@fluentui/react-components'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { request, describeError } from '../../../shared/api/request'
import { env } from '../../../shared/config/env'
import { useListSession } from '../../shopping-lists/hooks/useListSession'
import { ConfirmAction } from '../../shopping-lists/components/ManageListActions'
import { ExpenseForm } from './ExpenseForm'
import type { GroupMember } from '../../groups/api/groupsApi'
import { memberName } from '../../groups/memberName'
import type { SharedListDetail } from '../../shared-lists/api/sharedListsApi'
import type { RegisterExpenseInput } from '../api/expensesApi'
import ui from '../../../shared/ui/AppSurface.module.css'
import { getMarkets } from '../../product-catalog/api/productCatalogApi'

type Expense = RegisterExpenseInput & { expenseId: string; itemId: string; productName: string; createdAt: string; version?: string }
const money = (value: number) => new Intl.NumberFormat('es-ES', { style: 'currency', currency: 'EUR' }).format(value)

export function ExpenseHistoryPanel({ listId, items, members, canManage, personal = false }: { listId: string; items: SharedListDetail['items']; members: GroupMember[]; canManage: boolean; personal?: boolean }) {
  const { account, token, write } = useListSession()
  const client = useQueryClient()
  const [editing, setEditing] = useState('')
  const key = ['expenses', listId, account?.homeAccountId]
  const base = `${env.shoppingListApiBaseUrl}/api/v1/${personal ? 'purchaselists' : 'shared-lists'}/${listId}`
  const segment = personal ? 'purchases' : 'expenses'
  const expenses = useQuery({ queryKey: key, enabled: Boolean(account), retry: false, queryFn: async () => request<Expense[]>(`${base}/${segment}`, await token()) })
  const markets = useQuery({ queryKey: ['catalog-markets', account?.homeAccountId, 'all'], enabled: Boolean(account), retry: false, queryFn: async () => getMarkets(env.productCatalogApiBaseUrl, await token(), true) })
  const refresh = async () => { for (const prefix of ['expenses', 'settlement-summary', 'settlement-proposal', 'shared-list-detail']) await client.invalidateQueries({ queryKey: [prefix, listId] }) }
  return <section className={ui.stack} aria-label="Historial de gastos"><h3>Gastos registrados</h3>
    {expenses.isPending ? <Spinner label="Cargando gastos…" /> : expenses.isError ? <div role="alert">{describeError(expenses.error)} <Button onClick={() => void expenses.refetch()}>Reintentar gastos</Button></div> : !expenses.data.length ? <p>No hay gastos registrados todavía.</p> : expenses.data.map(expense => <section className={ui.panel} key={expense.expenseId} aria-label={`Gasto de ${expense.productName}`}>
      <p><strong>{expense.productName}</strong> · {money(expense.realPaidPrice)} · Pagado por {memberName(members, expense.paidBy)}</p>
      {!personal ? <p>Participan: {expense.participants.map(id => memberName(members, id)).join(', ')}</p> : null}
      <p>Fecha de compra: {expense.purchasedOn ?? 'Sin indicar'} · Mercado: {expense.marketId ? markets.data?.find(m => m.id === expense.marketId)?.name ?? 'No disponible' : 'Sin indicar'}</p>
      {canManage ? <div className={ui.actions}><Button onClick={() => setEditing(editing === expense.expenseId ? '' : expense.expenseId)}>Corregir gasto de {expense.productName}</Button>
        <ConfirmAction label={`Eliminar gasto de ${expense.productName}`} description={personal ? 'Se eliminará este registro de compra.' : 'Se eliminará este gasto y se actualizará el reparto del grupo.'} onConfirm={async () => { await write(t => request<void>(`${base}/items/${expense.itemId}/${segment}/${expense.expenseId}`, t, 'DELETE')); await refresh() }} /></div> : null}
      {editing === expense.expenseId ? <ExpenseForm key={`${expense.expenseId}:${expense.version}`} items={items} members={members} payerIds={members.map(m => m.userId)} personal={personal} defaultPayerId={personal ? expense.paidBy : ''} accountKey={account?.homeAccountId} scope={`edit-expense:${expense.expenseId}:${expense.version}`} initialValue={{ ...expense, expectedVersion: expense.version, itemId: expense.itemId }} submitLabel="Guardar corrección" onSubmit={async value => {
        const { itemId, ...input } = value
        if (itemId !== expense.itemId) throw new Error('Para cambiar el producto, elimina el gasto y registra uno nuevo.')
        await write(t => request<void>(`${base}/items/${expense.itemId}/${segment}/${expense.expenseId}`, t, 'PATCH', input)); await refresh(); setEditing('')
      }} /> : null}
    </section>)}
  </section>
}
