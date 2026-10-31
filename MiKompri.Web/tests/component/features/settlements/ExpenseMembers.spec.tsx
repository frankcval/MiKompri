import React from 'react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { act, cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { SettlementPage } from '../../../../src/features/settlements/pages/SettlementPage'

const account = vi.hoisted(() => ({ homeAccountId: 'expense-user', tenantId: '00000000-0000-0000-0000-000000000000' }))
vi.mock('@azure/msal-react', () => ({ useMsal: () => ({ accounts: [account], instance: { acquireTokenSilent: async () => ({ accessToken: 'token' }) } }) }))
const listId = '11111111-1111-4111-8111-111111111111'
const groupId = '22222222-2222-4222-8222-222222222222'
const itemId = '33333333-3333-4333-8333-333333333333'
const anaId = '44444444-4444-4444-8444-444444444444'
const luisId = '55555555-5555-4555-8555-555555555555'
let members: { userId: string; displayName: string; role: string }[]
let items: { id: string; productName: string; quantity: number; isPurchased: boolean }[]
let forbidden: boolean
let expenseFailure: boolean
let detailFailure: boolean
let recorded: boolean
let fetchMock: ReturnType<typeof vi.fn>

beforeEach(() => {
  members = [{ userId: anaId, displayName: 'Ana', role: 'Owner' }, { userId: luisId, displayName: 'Luis', role: 'Member' }]
  items = [{ id: itemId, productName: 'Arroz', quantity: 2, isPurchased: false }]
  forbidden = false
  expenseFailure = false
  detailFailure = false
  recorded = false
  fetchMock = vi.fn(async (url: string, _init?: RequestInit) => {
    if (url.includes('/markets')) return Response.json([])
    if (url.endsWith('/users/me')) return Response.json({ id: anaId, displayName: 'Ana' })
    if (url.endsWith('/members')) return Response.json(forbidden ? {} : members, { status: forbidden ? 403 : 200 })
    if (url.endsWith('/summary')) return Response.json({ sharedListId: listId, balances: recorded ? [
      { userId: anaId, totalPaid: 12, totalOwed: 6, netBalance: 6, type: 'Creditor' },
      { userId: luisId, totalPaid: 0, totalOwed: 6, netBalance: -6, type: 'Debtor' },
    ] : [] })
    if (url.endsWith('/proposal')) return Response.json({ sharedListId: listId, transfers: recorded ? [{ fromUserId: luisId, toUserId: anaId, amount: 6 }] : [] })
    if (url.endsWith('/expenses')) {
      if (!_init?.method || _init.method === 'GET') return Response.json([])
      if (expenseFailure) return Response.json({}, { status: 403 })
      recorded = true
      return Response.json('expense-id', { status: 201 })
    }
    if (url.endsWith('/' + listId)) return Response.json(detailFailure ? {} : { id: listId, groupId, name: 'Compra', items }, { status: detailFailure ? 503 : 200 })
    throw new Error(`Unexpected request ${url}`)
  })
  vi.stubGlobal('fetch', fetchMock)
})
afterEach(() => { cleanup(); vi.unstubAllGlobals() })
function mount(id: string | undefined = listId) {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(<QueryClientProvider client={client}><SettlementPage sharedListId={id} /></QueryClientProvider>)
}
async function fill() {
  fireEvent.change(await screen.findByRole('combobox', { name: 'Ítem de la lista' }), { target: { value: itemId } })
  fireEvent.change(screen.getByRole('spinbutton', { name: 'Importe pagado (EUR)' }), { target: { value: '12' } })
  fireEvent.change(screen.getByRole('combobox', { name: 'Pagado por' }), { target: { value: anaId } })
  fireEvent.click(screen.getByRole('checkbox', { name: 'Ana' }))
  fireEvent.click(screen.getByRole('checkbox', { name: 'Luis' }))
}

describe('reparto de gastos con personas e ítems reales', () => {
  it('usa nombres y envía el contrato real sobre el ítem elegido; refresca saldos y transferencias', async () => {
    mount()
    await fill()
    expect(screen.queryByRole('textbox')).not.toBeInTheDocument()
    fireEvent.click(screen.getByRole('button', { name: 'Registrar gasto', exact: true }))
    await screen.findByText('Gasto registrado.')
    await screen.findByText(/Luis debe pagar.*a Ana/)
    const writes = fetchMock.mock.calls.filter(([, init]) => init?.method === 'POST')
    expect(writes).toHaveLength(1)
    expect(writes[0][0]).toContain(`/shared-lists/${listId}/items/${itemId}/expenses`)
    expect(JSON.parse(writes[0][1].body)).toEqual({ realPaidPrice: 12, paidBy: anaId, purchasedBy: null, participants: [anaId, luisId], currency: 'EUR', operationId: expect.stringMatching(/^[0-9a-f-]{36}$/), marketId: null, purchasedOn: null })
    expect(screen.getByText(/Luis debe pagar.*a Ana/)).toBeInTheDocument()
    expect(document.body.textContent).not.toContain(anaId)
    expect(document.body.textContent).not.toContain(luisId)
    expect(screen.getByText(/Pagado 12/)).toBeInTheDocument()
  })

  it('limita el pagador de Member al propio usuario', async () => {
    members[0].role = 'Member'
    mount()
    const payer = await screen.findByRole('combobox', { name: 'Pagado por' })
    expect(payer.querySelectorAll('option')).toHaveLength(2)
    expect(payer).not.toHaveTextContent('Luis')
    expect(screen.getByRole('combobox', { name: 'Comprado por (opcional)' })).not.toHaveTextContent('Luis')
    expect(screen.getByRole('checkbox', { name: 'Luis' })).toBeInTheDocument()
  })

  it('no ofrece gastos ni IDs de ejemplo si la lista no tiene ítems', async () => {
    items = []
    mount()
    await screen.findByText(/Esta lista no tiene ítems/)
    expect(screen.queryByRole('button', { name: 'Registrar gasto' })).not.toBeInTheDocument()
    expect(fetchMock.mock.calls.some(([url]) => url.includes('00000000-0000'))).toBe(false)
  })

  it('no hace peticiones sin lista elegida', () => {
    mount('')
    expect(screen.getByText(/Selecciona una lista compartida/)).toBeInTheDocument()
    expect(fetchMock).not.toHaveBeenCalled()
  })

  it('bloquea gastos cuando no se puede verificar la membresía', async () => {
    forbidden = true
    mount()
    expect(await screen.findByRole('alert')).toHaveTextContent(/no tienes permiso/i)
    expect(screen.queryByRole('button', { name: 'Registrar gasto' })).not.toBeInTheDocument()
    expect(fetchMock.mock.calls.some(([url]) => url.endsWith('/summary'))).toBe(false)
  })

  it('bloquea el envío hasta elegir ítem, pagador, participantes e importe válido', async () => {
    mount()
    await screen.findByRole('spinbutton', { name: 'Importe pagado (EUR)' })
    expect(screen.getByRole('button', { name: 'Registrar gasto' })).toBeDisabled()
    await fill()
    fireEvent.change(screen.getByRole('spinbutton', { name: 'Importe pagado (EUR)' }), { target: { value: '-1' } })
    expect(screen.getByRole('button', { name: 'Registrar gasto' })).toBeDisabled()
    expect(fetchMock.mock.calls.filter(([, init]) => init?.method === 'POST')).toHaveLength(0)
  })

  it('conserva los valores cuando la API rechaza el gasto y permite reintentar', async () => {
    expenseFailure = true
    mount()
    await fill()
    fireEvent.click(screen.getByRole('button', { name: 'Registrar gasto' }))
    expect(await screen.findByRole('alert')).toHaveTextContent(/No tienes permiso/)
    expect(screen.getByRole('combobox', { name: 'Pagado por' })).toHaveValue(anaId)
    expect(screen.getByRole('checkbox', { name: 'Luis' })).toBeChecked()
    expect(screen.getByRole('spinbutton', { name: 'Importe pagado (EUR)' })).toHaveValue(12)
    expenseFailure = false
    fireEvent.click(screen.getByRole('button', { name: 'Registrar gasto' }))
    await screen.findByText('Gasto registrado.')
  })

  it('reintenta detalle sin emitir peticiones con groupId indefinido', async () => {
    detailFailure = true
    mount()
    await screen.findByRole('alert')
    detailFailure = false
    fireEvent.click(screen.getByRole('button', { name: 'Reintentar' }))
    await screen.findByRole('combobox', { name: 'Pagado por' })
    expect(fetchMock.mock.calls.some(([url]) => url.includes('undefined'))).toBe(false)
  })

  it('evita duplicar el pago mientras está pendiente', async () => {
    mount()
    await fill()
    let release!: () => void
    const original = fetchMock.getMockImplementation()!
    fetchMock.mockImplementation(async (url: string, init?: RequestInit) => {
      if (url.endsWith('/expenses') && init?.method === 'POST') await new Promise<void>((resolve) => { release = resolve })
      return original(url, init)
    })
    const button = screen.getByRole('button', { name: 'Registrar gasto' })
    fireEvent.click(button)
    await screen.findByRole('button', { name: 'Guardando gasto…' })
    fireEvent.submit(button.closest('form')!)
    await waitFor(() => expect(fetchMock.mock.calls.filter(([, init]) => init?.method === 'POST')).toHaveLength(1))
    await act(async () => { release() })
    await screen.findByText('Gasto registrado.')
  })
})
