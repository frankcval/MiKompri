import '../../../setup/jsdom-dialogs'
import userEvent from '@testing-library/user-event'
import { FluentProvider } from '@fluentui/react-components'
import { appTheme } from '../../../../src/shared/ui/theme'
import React from 'react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { cleanup, render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes, useLocation, useNavigate } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { SharedListDetailPage } from '../../../../src/features/shared-lists/pages/SharedListDetailPage'

vi.mock('@azure/msal-react', () => ({ useMsal: () => ({ accounts: [{ homeAccountId: 'private-test' }], instance: { acquireTokenSilent: async () => ({ accessToken: 'test-token' }) } }) }))
vi.mock('../../../../src/features/settlements/pages/SettlementPage', () => ({ SettlementPage: ({ sharedListId }: { sharedListId: string }) => <h2>Gastos de {sharedListId}</h2> }))
const listId = '11111111-1111-4111-8111-111111111111'
const groupId = '22222222-2222-4222-8222-222222222222'
const productId = '33333333-3333-4333-8333-333333333333'
let status: number
let fetchMock: ReturnType<typeof vi.fn>
let items: { id: string; productName: string; quantity: number; isPurchased: boolean }[]
function Location() { const navigate = useNavigate(); return <><output data-testid="path">{useLocation().pathname}{useLocation().search}</output><button onClick={() => navigate(-1)}>Atrás</button><button onClick={() => navigate(1)}>Adelante</button></> }
function mount(tab = '') {
  render(<FluentProvider theme={appTheme}><QueryClientProvider client={new QueryClient({ defaultOptions: { queries: { retry: false } } })}><MemoryRouter initialEntries={[`/shared-lists/${listId}${tab}`]}><Location /><Routes><Route path="/shared-lists/:listId" element={<SharedListDetailPage />} /></Routes></MemoryRouter></QueryClientProvider></FluentProvider>)
}
let user: ReturnType<typeof userEvent.setup>
beforeEach(() => {
  user = userEvent.setup()
  status = 200; items = []
  fetchMock = vi.fn(async (url: string, init?: RequestInit) => {
    if (url.endsWith('/members')) return Response.json([])
    if (url.endsWith('/users/me')) return Response.json({ id: '44444444-4444-4444-8444-444444444444' })
    if (url.endsWith('/groups')) return Response.json([{ id: groupId, name: 'Familia' }])
    if (url.endsWith('/catalog-products')) return Response.json([{ id: productId, name: 'Arroz', isActive: true }])
    if (url.endsWith('/items')) { const value = JSON.parse(String(init?.body)); items.push({ id: 'new-item', productName: value.name, quantity: value.quantity, isPurchased: false }); return Response.json('new-item', { status: 201 }) }
    return status === 200 ? Response.json({ id: listId, name: 'Compra compartida', groupId, items }) : Response.json({}, { status })
  })
  vi.stubGlobal('fetch', fetchMock)
})
afterEach(() => { cleanup(); vi.unstubAllGlobals() })
describe('detalle compartido con contexto', () => {
  it('abre URL de gastos y conserva la misma lista al cambiar de pestaña e historial', async () => {
    mount('?tab=expenses')
    await screen.findByRole('heading', { name: 'Compra compartida' })
    expect(screen.getByRole('heading', { name: `Gastos de ${listId}` })).toBeInTheDocument()
    await user.click(screen.getByRole('tab', { name: 'Compra' }))
    expect(screen.getByTestId('path')).toHaveTextContent('tab=shopping')
    await user.click(screen.getByRole('button', { name: 'Atrás' }))
    await screen.findByRole('heading', { name: `Gastos de ${listId}` })
    expect(screen.getByRole('link', { name: 'Volver a listas compartidas' })).toHaveAttribute('href', `/shared-lists?groupId=${groupId}`)
  })
  it('añade con contrato compartido y ofrece estado de compra autorizado', async () => {
    mount()
    await user.click(await screen.findByRole('button', { name: 'Añadir productos' }))
    await user.click(await screen.findByRole('button', { name: 'Arroz (unidad)' }))
    await user.click(screen.getByRole('button', { name: 'Agregar ítem' }))
    await screen.findByText('Arroz se agregó a esta lista.')
    const writes = fetchMock.mock.calls.filter(([, init]) => init?.method === 'POST')
    expect(writes).toHaveLength(1)
    expect(writes[0][0]).toContain(`/shared-lists/${listId}/items`)
    expect(JSON.parse(writes[0][1].body)).toEqual({ productId, name: 'Arroz', quantity: 1, estimatedPrice: null })
    await user.click(screen.getByRole('button', { name: 'Terminar' }))
    await screen.findByText('Arroz')
    expect(screen.getByRole('checkbox', { name: 'Marcar comprado: Arroz' })).not.toBeChecked()
    expect(fetchMock.mock.calls.some(([url]) => url.endsWith('/mark-as-purchased'))).toBe(false)
  })
  it.each([401, 403, 404, 503])('no expone datos ni escrituras al recibir %s', async code => {
    status = code; mount()
    await screen.findByRole('alert')
    expect(screen.queryByRole('button', { name: 'Añadir productos' })).not.toBeInTheDocument()
    expect(screen.queryByRole('heading', { name: 'Compra compartida' })).not.toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Volver a listas compartidas' })).toBeInTheDocument()
  })
  it('presenta carga antes de revelar el detalle', () => {
    fetchMock.mockImplementation(() => new Promise(() => {})); mount()
    expect(screen.getByText('Cargando lista…')).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: 'Añadir productos' })).not.toBeInTheDocument()
  })
})
