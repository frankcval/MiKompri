import { sessionFetch } from '../../../shared/api/request'
import { responseError } from '../../../shared/api/request'
export type ParticipantBalance = {
  userId: string
  totalPaid: number
  totalOwed: number
  netBalance: number
  type: string
}

export type SettlementTransfer = {
  fromUserId: string
  toUserId: string
  amount: number
}

export type SettlementSummary = {
  sharedListId: string
  balances: ParticipantBalance[]
  transfers: SettlementTransfer[]
}

export type SettlementProposal = {
  sharedListId: string
  transfers: SettlementTransfer[]
}

async function parseSettlementError(response: Response): Promise<Error & { status?: number }> {
  return responseError(response)
}

export async function getSettlementSummary(
  shoppingListApiBaseUrl: string,
  accessToken: string,
  sharedListId: string,
): Promise<SettlementSummary> {
  const response = await sessionFetch(
    `${shoppingListApiBaseUrl}/api/v1/shared-lists/${sharedListId}/settlement/summary`,
    {
      method: 'GET',
      headers: {
        Authorization: `Bearer ${accessToken}`,
        Accept: 'application/json',
      },
    },
  )

  if (!response.ok) {
    throw await parseSettlementError(response)
  }

  return (await response.json()) as SettlementSummary
}

export async function getSettlementProposal(
  shoppingListApiBaseUrl: string,
  accessToken: string,
  sharedListId: string,
): Promise<SettlementProposal> {
  const response = await sessionFetch(
    `${shoppingListApiBaseUrl}/api/v1/shared-lists/${sharedListId}/settlement/proposal`,
    {
      method: 'GET',
      headers: {
        Authorization: `Bearer ${accessToken}`,
        Accept: 'application/json',
      },
    },
  )

  if (!response.ok) {
    throw await parseSettlementError(response)
  }

  return (await response.json()) as SettlementProposal
}
