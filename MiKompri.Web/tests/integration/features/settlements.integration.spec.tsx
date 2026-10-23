import { describe, expect, it, vi } from 'vitest'
import { getSettlementSummary } from '../../../src/features/settlements/api/settlementApi'

describe('settlements integration', () => {
  it('consulta settlement summary y maneja 503', async () => {
    const fetchMock = vi
      .spyOn(globalThis, 'fetch')
      .mockResolvedValueOnce(
        new Response(JSON.stringify({ sharedListId: 'sl-1', balances: [], transfers: [] }), {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        }),
      )
      .mockResolvedValueOnce(new Response('{}', { status: 503 }))

    const summary = await getSettlementSummary('http://localhost:5002', 'token-1', 'sl-1')
    expect(summary.sharedListId).toBe('sl-1')

    await expect(getSettlementSummary('http://localhost:5002', 'token-1', 'sl-1')).rejects.toBeDefined()

    fetchMock.mockRestore()
  })
})
