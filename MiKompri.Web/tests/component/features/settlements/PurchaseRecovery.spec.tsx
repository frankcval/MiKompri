import React from 'react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { ExpenseForm } from '../../../../src/features/settlements/components/ExpenseForm'

vi.mock('../../../../src/features/shopping-lists/hooks/useListSession', () => ({ useListSession: () => ({ account: { homeAccountId: 'recovery', tenantId: '00000000-0000-0000-0000-000000000000' }, token: async () => 'test' }) }))
const actor = '11111111-1111-4111-8111-111111111111'
const item = '22222222-2222-4222-8222-222222222222'
afterEach(() => { cleanup(); vi.unstubAllGlobals() })

describe('purchase recovery', () => {
  it('keeps the same operation and payload after a lost response instead of making a second purchase', async () => {
    vi.stubGlobal('fetch', vi.fn(async () => Response.json([])))
    const submit = vi.fn().mockRejectedValueOnce(new TypeError('Respuesta perdida')).mockResolvedValueOnce(undefined)
    render(<QueryClientProvider client={new QueryClient()}><ExpenseForm personal defaultItemId={item} defaultPayerId={actor} items={[{ id: item, productName: 'Pan', quantity: 3, isPurchased: true }]} members={[{ userId: actor, displayName: 'Ana', role: 'Owner' }]} payerIds={[actor]} onSubmit={submit} /></QueryClientProvider>)
    fireEvent.change(screen.getByRole('spinbutton', { name: 'Importe pagado (EUR)' }), { target: { value: '6.50' } })
    fireEvent.click(screen.getByRole('button', { name: 'Registrar gasto' }))
    await screen.findByText(/No se confirmó el resultado/)
    expect(screen.getByRole('spinbutton', { name: 'Importe pagado (EUR)' })).toBeDisabled()
    fireEvent.click(screen.getByRole('button', { name: 'Consultar o guardar la misma operación' }))
    await waitFor(() => expect(submit).toHaveBeenCalledTimes(2))
    expect(submit.mock.calls[0][0]).toEqual(submit.mock.calls[1][0])
    expect(submit.mock.calls[0][0]).toMatchObject({ realPaidPrice: 6.5, operationId: expect.stringMatching(/^[0-9a-f-]{36}$/) })
    expect(screen.queryByRole('checkbox')).not.toBeInTheDocument()
    expect(screen.queryByRole('combobox', { name: 'Pagado por' })).not.toBeInTheDocument()
  })
})
