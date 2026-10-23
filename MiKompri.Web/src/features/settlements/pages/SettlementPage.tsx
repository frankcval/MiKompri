import { Card, CardHeader, Spinner, Text } from '@fluentui/react-components'
import { useMsal } from '@azure/msal-react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { loginRequest } from '../../../auth/msal/msalConfig'
import { createAuthSessionService, executeAuthenticatedWrite } from '../../../auth/session/authSessionService'
import { env } from '../../../shared/config/env'
import { ExpenseForm, type ExpenseFormValue } from '../components/ExpenseForm'
import { registerItemExpense } from '../api/expensesApi'
import { getSettlementProposal, getSettlementSummary } from '../api/settlementApi'

const PLACEHOLDER_SHARED_LIST_ID = '00000000-0000-0000-0000-000000000001'
const PLACEHOLDER_ITEM_ID = '00000000-0000-0000-0000-000000000002'

type SettlementPageProps = {
  sharedListId?: string
  itemId?: string
}

export function SettlementPage({ sharedListId = PLACEHOLDER_SHARED_LIST_ID, itemId = PLACEHOLDER_ITEM_ID }: SettlementPageProps) {
  const { instance, accounts } = useMsal()
  const account = accounts[0]
  const authSession = createAuthSessionService(instance)
  const queryClient = useQueryClient()

  const summaryQuery = useQuery({
    queryKey: ['settlement-summary', sharedListId, account?.homeAccountId],
    enabled: Boolean(account && sharedListId),
    retry: false,
    queryFn: async () => {
      const token = await authSession.getAccessToken(loginRequest.scopes, account)
      return getSettlementSummary(env.shoppingListApiBaseUrl, token, sharedListId)
    },
  })

  const proposalQuery = useQuery({
    queryKey: ['settlement-proposal', sharedListId, account?.homeAccountId],
    enabled: Boolean(account && sharedListId),
    retry: false,
    queryFn: async () => {
      const token = await authSession.getAccessToken(loginRequest.scopes, account)
      return getSettlementProposal(env.shoppingListApiBaseUrl, token, sharedListId)
    },
  })

  const registerExpenseMutation = useMutation({
    mutationFn: async (payload: ExpenseFormValue) => {
      return executeAuthenticatedWrite(
        (token) =>
          registerItemExpense(
            env.shoppingListApiBaseUrl,
            token,
            sharedListId,
            itemId,
            payload,
          ),
        () => authSession.getAccessToken(loginRequest.scopes, account),
        async () => authSession.login(loginRequest.scopes),
      )
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['settlement-summary'] })
      await queryClient.invalidateQueries({ queryKey: ['settlement-proposal'] })
    },
  })

  if (!account) {
    return <Text>No hay sesión activa.</Text>
  }

  if (summaryQuery.isLoading || proposalQuery.isLoading) {
    return <Spinner label="Cargando settlement" />
  }

  if (summaryQuery.isError || proposalQuery.isError) {
    const status = (summaryQuery.error as { status?: number } | undefined)?.status
      ?? (proposalQuery.error as { status?: number } | undefined)?.status

    return (
      <Text>
        {status === 503
          ? 'Servicio temporalmente no disponible para settlement.'
          : 'No fue posible cargar balances de settlement.'}
      </Text>
    )
  }

  return (
    <main style={{ padding: '1.5rem', display: 'grid', gap: '1rem' }}>
      <h1>Settlement</h1>

      <Card>
        <CardHeader header={<Text weight="semibold">Registrar gasto compartido</Text>} />
        <ExpenseForm
          onSubmit={async (value) => {
            await registerExpenseMutation.mutateAsync(value)
          }}
          isSubmitting={registerExpenseMutation.isPending}
        />
      </Card>

      <Card>
        <CardHeader header={<Text weight="semibold">Balances</Text>} />
        {summaryQuery.data?.balances?.length ? (
          summaryQuery.data.balances.map((balance) => (
            <Text key={`${balance.userId}-${balance.amount}`}>
              {balance.userId}: {balance.amount}
            </Text>
          ))
        ) : (
          <Text>Sin balances para mostrar.</Text>
        )}
      </Card>

      <Card>
        <CardHeader header={<Text weight="semibold">Transferencias propuestas</Text>} />
        {proposalQuery.data?.transfers?.length ? (
          proposalQuery.data.transfers.map((transfer) => (
            <Text key={`${transfer.fromUserId}-${transfer.toUserId}-${transfer.amount}`}>
              {transfer.fromUserId} → {transfer.toUserId}: {transfer.amount}
            </Text>
          ))
        ) : (
          <Text>Sin transferencias propuestas.</Text>
        )}
      </Card>
    </main>
  )
}
