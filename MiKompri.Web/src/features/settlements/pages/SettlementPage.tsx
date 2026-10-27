import ui from '../../../shared/ui/AppSurface.module.css'
import { Button, Card, CardHeader, Spinner, Text } from '@fluentui/react-components'
import { useMsal } from '@azure/msal-react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { loginRequest } from '../../../auth/msal/msalConfig'
import { createAuthSessionService, executeAuthenticatedWrite } from '../../../auth/session/authSessionService'
import { env } from '../../../shared/config/env'
import { getGroupMembers } from '../../groups/api/groupsApi'
import { memberName } from '../../groups/memberName'
import { fetchMyProfile } from '../../profile/api/profileApi'
import { getSharedListById } from '../../shared-lists/api/sharedListsApi'
import { ExpenseForm } from '../components/ExpenseForm'
import { registerItemExpense } from '../api/expensesApi'
import { getSettlementProposal, getSettlementSummary } from '../api/settlementApi'

type SettlementPageProps = { sharedListId?: string }
const money = (amount: number) => new Intl.NumberFormat('es-ES', { style: 'currency', currency: 'EUR' }).format(amount)

export function SettlementPage({ sharedListId }: SettlementPageProps) {
  const { instance, accounts } = useMsal()
  const account = accounts[0]
  const authSession = createAuthSessionService(instance)
  const queryClient = useQueryClient()
  const detailKey = ['shared-list-detail', sharedListId, account?.homeAccountId]
  const detailQuery = useQuery({
    queryKey: detailKey,
    enabled: Boolean(account && sharedListId),
    retry: false,
    queryFn: async () => getSharedListById(env.shoppingListApiBaseUrl, await authSession.getAccessToken(loginRequest.scopes, account), sharedListId!),
  })
  const groupId = detailQuery.isSuccess ? detailQuery.data.groupId : undefined
  const membersQuery = useQuery({
    queryKey: ['group-members', groupId, account?.homeAccountId],
    enabled: Boolean(account && groupId),
    retry: false,
    queryFn: async () => getGroupMembers(env.usersApiBaseUrl, await authSession.getAccessToken(loginRequest.scopes, account), groupId!),
  })
  const profileQuery = useQuery({
    queryKey: ['profile', account?.homeAccountId],
    enabled: Boolean(account && sharedListId),
    retry: false,
    queryFn: async () => fetchMyProfile(env.usersApiBaseUrl, await authSession.getAccessToken(loginRequest.scopes, account)),
  })
  const members = membersQuery.isSuccess ? membersQuery.data : []
  const currentMember = members.find((member) => member.userId === profileQuery.data?.id)
  const canRead = Boolean(account && detailQuery.isSuccess && membersQuery.isSuccess && profileQuery.isSuccess && currentMember)
  const summaryKey = ['settlement-summary', sharedListId, account?.homeAccountId]
  const proposalKey = ['settlement-proposal', sharedListId, account?.homeAccountId]
  const summaryQuery = useQuery({
    queryKey: summaryKey,
    enabled: canRead,
    retry: false,
    queryFn: async () => getSettlementSummary(env.shoppingListApiBaseUrl, await authSession.getAccessToken(loginRequest.scopes, account), sharedListId!),
  })
  const proposalQuery = useQuery({
    queryKey: proposalKey,
    enabled: canRead,
    retry: false,
    queryFn: async () => getSettlementProposal(env.shoppingListApiBaseUrl, await authSession.getAccessToken(loginRequest.scopes, account), sharedListId!),
  })

  if (!account) return <Text>No hay sesión activa.</Text>
  if (!sharedListId) return <Text>Selecciona una lista compartida para ver su reparto de gastos.</Text>
  if (detailQuery.isError || membersQuery.isError || profileQuery.isError) {
    const failure = detailQuery.error ?? membersQuery.error ?? profileQuery.error
    const code = (failure as { status?: number })?.status
    return <div role="alert">
      {code === 403 ? 'Ya no tienes permiso para consultar esta lista o sus miembros.' : 'No fue posible cargar la lista y sus miembros.'}
      <Button onClick={() => {
        if (detailQuery.isError) void detailQuery.refetch()
        if (membersQuery.isError && groupId) void membersQuery.refetch()
        if (profileQuery.isError) void profileQuery.refetch()
      }}>Reintentar</Button>
    </div>
  }
  if (detailQuery.isPending || membersQuery.isPending || profileQuery.isPending) return <Spinner label="Cargando reparto y miembros del grupo" />
  if (!currentMember) return <p role="alert">No tienes una membresía disponible para operar en esta lista.</p>

  const role = currentMember.role.toLowerCase()
  const payerIds = role === 'owner' || role === 'admin' ? members.map((member) => member.userId)
    : role === 'member' ? [currentMember.userId] : []

  return <section aria-label="Reparto de gastos" className={ui.stack}>
    <h2>Reparto de gastos</h2>
    <p>Registra el importe pagado por un ítem y elige quiénes participan en el gasto.</p>
    <Card className={ui.panel}>
      <CardHeader header={<h3 style={{ margin: 0 }}>Registrar gasto compartido</h3>} />
      {detailQuery.data.items.length === 0 ? <p>Esta lista no tiene ítems. Añade un ítem a la lista antes de registrar un gasto.</p>
        : payerIds.length === 0 ? <p>No tienes permiso para registrar gastos con tu rol actual.</p>
        : <ExpenseForm key={`${account.homeAccountId}:${sharedListId}`} items={detailQuery.data.items} members={members} payerIds={payerIds}
          onSubmit={async ({ itemId, ...input }) => {
            if (!detailQuery.data.items.some((item) => item.id === itemId)) throw new Error('Selecciona un ítem de esta lista.')
            await executeAuthenticatedWrite(
              (token) => registerItemExpense(env.shoppingListApiBaseUrl, token, sharedListId, itemId, input),
              () => authSession.getAccessToken(loginRequest.scopes, account),
              async () => authSession.login(loginRequest.scopes),
            )
            await Promise.all([
              queryClient.invalidateQueries({ queryKey: summaryKey }),
              queryClient.invalidateQueries({ queryKey: proposalKey }),
              queryClient.invalidateQueries({ queryKey: detailKey }),
            ])
          }} />}
    </Card>
    <Card className={ui.panel}>
      <CardHeader header={<h3 style={{ margin: 0 }}>Balance por persona</h3>} />
      {summaryQuery.isPending ? <Spinner label="Cargando balance" /> : summaryQuery.isError ? <p role="alert">No fue posible cargar el balance.</p>
        : summaryQuery.data.balances.length ? summaryQuery.data.balances.map((balance) => <p className={ui.balanceRow} key={balance.userId}>
          <strong>{memberName(members, balance.userId)}</strong><span>Pagado {money(balance.totalPaid)} · Parte del gasto {money(balance.totalOwed)} · Saldo {money(balance.netBalance)}</span>
        </p>) : <p>Todavía no hay gastos para repartir.</p>}
    </Card>
    <Card className={ui.panel}>
      <CardHeader header={<h3 style={{ margin: 0 }}>Pagos propuestos para saldar cuentas</h3>} />
      {proposalQuery.isPending ? <Spinner label="Cargando pagos propuestos" /> : proposalQuery.isError ? <p role="alert">No fue posible cargar los pagos propuestos.</p>
        : proposalQuery.data.transfers.length ? proposalQuery.data.transfers.map((transfer, index) => <p className={ui.row} key={index} style={{ overflowWrap: 'anywhere' }}>
          {memberName(members, transfer.fromUserId)} debe pagar {money(transfer.amount)} a {memberName(members, transfer.toUserId)}.
        </p>) : <p>No hay pagos pendientes entre participantes.</p>}
    </Card>
  </section>
}
