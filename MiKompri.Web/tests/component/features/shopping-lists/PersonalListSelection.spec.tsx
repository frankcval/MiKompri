import '../../../../src/features/shopping-lists/components/AddProductsDialogContent'
import '../../../setup/jsdom-dialogs'
import userEvent from '@testing-library/user-event'
import { FluentProvider } from '@fluentui/react-components'
import { appTheme } from '../../../../src/shared/ui/theme'
import React from 'react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { act, cleanup, fireEvent, render, screen, within, waitFor } from '@testing-library/react'
import { MemoryRouter, Route, Routes, useLocation, useNavigate } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { PersonalListsPage } from '../../../../src/features/shopping-lists/pages/PersonalListsPage'
import { ListsPage } from '../../../../src/features/shopping-lists/pages/ListsPage'

const account = vi.hoisted(() => ({ homeAccountId: 'account-1', tenantId: '00000000-0000-0000-0000-000000000000' }))
vi.mock('@azure/msal-react', () => ({
  useMsal: () => ({ accounts: [account], instance: { acquireTokenSilent: async () => ({ accessToken: 'test-token' }) } }),
}))

const firstId = '11111111-1111-4111-8111-111111111111'
const secondId = '22222222-2222-4222-8222-222222222222'
const productId = '33333333-3333-4333-8333-333333333333'
const product = { id: productId, name: 'Arroz', isActive: true }
let lists: { id: string; name: string; items: { id: string; productName: string; quantity: number; isPurchased: boolean }[] }[]
let products: typeof product[]
let failure: boolean
let fetchMock: ReturnType<typeof vi.fn>

function Navigation() {
  const navigate = useNavigate()
  return <><output data-testid="path">{useLocation().pathname}</output><button onClick={() => navigate(-1)}>Atrás</button><button onClick={() => navigate(1)}>Adelante</button></>
}

function mount(path = '/personal-lists') {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false }, mutations: { retry: false } } })
  render(<FluentProvider theme={appTheme}><QueryClientProvider client={client}><MemoryRouter initialEntries={[path]}>
    <Navigation /><Routes><Route path="/lists" element={<ListsPage />} /><Route path="/personal-lists/:listId?" element={<PersonalListsPage />} /></Routes>
  </MemoryRouter></QueryClientProvider></FluentProvider>)
}

let user: ReturnType<typeof userEvent.setup>
beforeEach(() => {
  user = userEvent.setup()
  lists = [{ id: firstId, name: 'Primera', items: [] }, { id: secondId, name: 'Segunda', items: [] }]
  products = [product]
  failure = false
  fetchMock = vi.fn(async (url: string, init?: RequestInit) => {
    if (url.endsWith('/groups')) return Response.json([])
    if (url.endsWith('/purchaselists/mine')) return Response.json(lists)
    if (url.endsWith('/users/me')) return Response.json({ id: 'owned-profile', displayName: 'QA', defaultListSort: 'newest', profileVersion: 0 })
    if (url.endsWith('/catalog-products')) return Response.json(products)
    if (url.endsWith('/items')) {
      if (failure) return Response.json({ errors: { Quantity: ['La cantidad no es válida.'] } }, { status: 400 })
      const item = JSON.parse(String(init?.body))
      lists.find((list) => url.includes(list.id))!.items.push({ ...item, id: 'item-1', isPurchased: false })
      return Response.json('item-1', { status: 201 })
    }
    if (url.endsWith('/purchaselists') && init?.method === 'POST') {
      lists.push({ id: 'new-list', name: JSON.parse(String(init.body)).name, items: [] })
      return Response.json('new-list', { status: 201 })
    }
    throw new Error(`Unexpected request: ${url}`)
  })
  vi.stubGlobal('fetch', fetchMock)
})
afterEach(() => { cleanup(); vi.unstubAllGlobals() })

async function openAdd() {
  await user.click(await screen.findByRole('button', { name: 'Añadir productos' }))
  await screen.findByRole('dialog')
}
async function chooseProduct() {
  if (!screen.queryByRole('group', { name: 'Seleccionar producto' })) await openAdd()
  await user.click(await screen.findByRole('button', { name: 'Arroz (unidad)', exact: true }))
}

describe('recorrido privado personal', () => {
  it('prioriza el índice sin formulario ni consultas de productos', async () => {
    mount()
    await screen.findByRole('link', { name: /^Segunda/ })
    expect(screen.queryByRole('textbox')).not.toBeInTheDocument()
    expect(screen.queryByRole('button', { name: 'Agregar ítem' })).not.toBeInTheDocument()
    expect(fetchMock.mock.calls.every(([url]) => !url.endsWith('/catalog-products'))).toBe(true)
  })
  it('envía los IDs del producto y la segunda lista sin tocar la primera', async () => {
    mount()
    await user.click(await screen.findByRole('link', { name: /^Segunda/ }))
    await screen.findByRole('heading', { name: 'Segunda', exact: true })
    await chooseProduct()
    fireEvent.change(screen.getByRole('spinbutton', { name: /cantidad/i }), { target: { value: '2' } })
    await user.click(screen.getByRole('button', { name: 'Agregar ítem' }))
    await screen.findByText('Arroz se agregó a esta lista.')
    const writes = fetchMock.mock.calls.filter(([url]) => url.endsWith('/items'))
    expect(writes).toHaveLength(1)
    expect(writes[0][0]).toContain(`/purchaselists/${secondId}/items`)
    expect(JSON.parse(writes[0][1].body)).toEqual({ productId, productName: 'Arroz', quantity: 2, price: null })
    expect(lists[0].items).toEqual([])
    await user.click(screen.getByRole('button', { name: 'Terminar' }))
    await screen.findByRole('listitem')
    expect(screen.getByRole('listitem')).toHaveTextContent(/Arroz.*Cantidad: 2/)
    await user.click(screen.getByRole('button', { name: 'Atrás' }))
    await screen.findByRole('link', { name: /^Segunda/ })
    await user.click(screen.getByRole('button', { name: 'Adelante' }))
    await screen.findByRole('heading', { name: 'Segunda', exact: true })
  })
  it('abre el detalle por URL sin crear una selección implícita', async () => {
    mount(`/personal-lists/${secondId}`)
    await screen.findByRole('heading', { name: 'Segunda', exact: true })
    expect(screen.getByTestId('path')).toHaveTextContent(`/personal-lists/${secondId}`)
    expect(screen.queryByRole('textbox')).not.toBeInTheDocument()
  })
  it('no permite escribir en un ID que no pertenece a la cuenta', async () => {
    mount('/personal-lists/unknown')
    await screen.findByRole('alert')
    expect(screen.queryByRole('button', { name: 'Añadir productos' })).not.toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Volver a mis listas' })).toBeInTheDocument()
  })
  it('explica catálogo vacío y ofrece alta con retorno a la lista', async () => {
    products = []
    mount(`/personal-lists/${secondId}`)
    await openAdd()
    await screen.findByText(/No hay productos disponibles en el catálogo/)
    expect(screen.queryByRole('button', { name: 'Agregar ítem' })).not.toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Crear producto en el catálogo' })).toHaveAttribute('href', `/product-catalog?returnTo=${encodeURIComponent(`/personal-lists/${secondId}`)}`)
  })
  it('conserva producto y cantidades ante 400 y permite reintentar', async () => {
    failure = true
    mount(`/personal-lists/${secondId}`)
    await chooseProduct()
    fireEvent.change(screen.getByRole('spinbutton', { name: /cantidad/i }), { target: { value: '3' } })
    await user.click(screen.getByRole('button', { name: 'Agregar ítem' }))
    expect(await screen.findByRole('alert')).toHaveTextContent('La cantidad no es válida.')
    expect(screen.getByRole('button', { name: 'Arroz (unidad)' })).toHaveAttribute('aria-pressed', 'true')
    expect(screen.getByRole('spinbutton', { name: /cantidad/i })).toHaveValue(3)
    failure = false
    await user.click(screen.getByRole('button', { name: 'Agregar ítem' }))
    await screen.findByText('Arroz se agregó a esta lista.')
  })
  it.each([['0', '0'], ['1.5', '0'], ['-1', '0'], ['1', '-1'], ['', '0']])('bloquea cantidad/precio inválidos %s/%s', async (quantity, price) => {
    mount(`/personal-lists/${secondId}`)
    await chooseProduct()
    fireEvent.change(screen.getByRole('spinbutton', { name: /cantidad/i }), { target: { value: quantity } })
    fireEvent.change(screen.getByRole('textbox', { name: /precio/i }), { target: { value: price } })
    expect(screen.getByRole('button', { name: 'Agregar ítem' })).toBeDisabled()
    expect(fetchMock.mock.calls.filter(([url]) => url.endsWith('/items'))).toHaveLength(0)
  })
  it('crea en diálogo y abre exclusivamente la lista devuelta', async () => {
    mount()
    await user.click(await screen.findByRole('button', { name: 'Crear lista' }))
    fireEvent.change(await screen.findByRole('textbox', { name: /nombre de lista/i }), { target: { value: 'Nueva compra' } })
    await user.click(within(screen.getByRole('dialog')).getByRole('button', { name: 'Crear lista', exact: true }))
    await screen.findByRole('heading', { name: 'Nueva compra', exact: true })
    expect(screen.getByTestId('path')).toHaveTextContent('/personal-lists/new-list')
  })
  it('impide doble envío y reinicia borrador al cambiar de lista', async () => {
    mount(`/personal-lists/${secondId}`)
    await chooseProduct()
    let release!: () => void
    const original = fetchMock.getMockImplementation()!
    fetchMock.mockImplementation(async (url: string, init?: RequestInit) => {
      if (url.endsWith('/items')) await new Promise<void>(resolve => { release = resolve })
      return original(url, init)
    })
    const button = screen.getByRole('button', { name: 'Agregar ítem' })
    await user.click(button)
    await waitFor(() => expect(screen.getByRole('button', { name: /Agregando ítem/ })).toBeDisabled())
    fireEvent.submit(button.closest('form')!)
    await waitFor(() => expect(fetchMock.mock.calls.filter(([url]) => url.endsWith('/items'))).toHaveLength(1))
    await act(async () => { release() })
    await screen.findByText('Arroz se agregó a esta lista.')
    await user.click(screen.getByRole('button', { name: 'Terminar' }))
    await user.click(await screen.findByRole('link', { name: 'Volver a mis listas' }))
    await user.click(await screen.findByRole('link', { name: /Primera/ }))
    await openAdd()
    expect(await screen.findByRole('button', { name: 'Arroz (unidad)' })).toHaveAttribute('aria-pressed', 'false')
  })
  it('marca y desmarca solo el ítem de la lista abierta', async () => {
    lists[1].items.push({ id: 'item-1', productName: 'Arroz', quantity: 1, isPurchased: false })
    const original = fetchMock.getMockImplementation()!
    fetchMock.mockImplementation(async (url: string, init?: RequestInit) => {
      if (url.endsWith('/purchase-state')) { lists[1].items[0].isPurchased = JSON.parse(String(init?.body)).isPurchased; return new Response(null, { status: 204 }) }
      return original(url, init)
    })
    mount(`/personal-lists/${secondId}`)
    await user.click(await screen.findByRole('checkbox', { name: 'Marcar comprado: Arroz' }))
    await user.click(await screen.findByRole('button', { name: 'Ahora no' }))
    await screen.findByText('Producto marcado como comprado.')
    expect(screen.getByRole('checkbox', { name: 'Desmarcar comprado: Arroz' })).toBeChecked()
    expect(fetchMock.mock.calls.filter(([url]) => url.endsWith('/purchase-state'))[0][0]).toContain(`/purchaselists/${secondId}/items/item-1/`)
    await user.click(screen.getByRole('checkbox', { name: 'Desmarcar comprado: Arroz' }))
    await waitFor(() => expect(screen.getByRole('checkbox', { name: 'Marcar comprado: Arroz' })).not.toBeChecked())
    expect(lists[0].items).toEqual([])
  })
  it.each([401, 403, 503])('conserva la compra sin fingir éxito ante %s', async status => {
    lists[1].items.push({ id: 'item-1', productName: 'Arroz', quantity: 1, isPurchased: false })
    const original = fetchMock.getMockImplementation()!
    fetchMock.mockImplementation(async (url: string, init?: RequestInit) => url.endsWith('/purchase-state') ? Response.json({}, { status }) : original(url, init))
    mount(`/personal-lists/${secondId}`)
    await user.click(await screen.findByRole('checkbox', { name: 'Marcar comprado: Arroz' }))
    await screen.findByRole('alert')
    expect(screen.getByRole('checkbox', { name: 'Marcar comprado: Arroz' })).not.toBeChecked()
    expect(lists[1].items[0].isPurchased).toBe(false)
  })
})
