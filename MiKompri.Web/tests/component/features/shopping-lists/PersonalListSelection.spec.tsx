import React from 'react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { act, cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter, Route, Routes, useLocation, useNavigate } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { PersonalListsPage } from '../../../../src/features/shopping-lists/pages/PersonalListsPage'

const account = vi.hoisted(() => ({ homeAccountId: 'account-1' }))
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
  render(<QueryClientProvider client={client}><MemoryRouter initialEntries={[path]}>
    <Navigation /><Routes><Route path="/personal-lists/:listId?" element={<PersonalListsPage />} /></Routes>
  </MemoryRouter></QueryClientProvider>)
}

beforeEach(() => {
  lists = [{ id: firstId, name: 'Primera', items: [] }, { id: secondId, name: 'Segunda', items: [] }]
  products = [product]
  failure = false
  fetchMock = vi.fn(async (url: string, init?: RequestInit) => {
    if (url.endsWith('/purchaselists/mine')) return Response.json(lists)
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

async function chooseProduct() {
  fireEvent.change(await screen.findByRole('combobox', { name: /producto/i }), { target: { value: productId } })
}

describe('selección explícita de listas personales', () => {
  it('no muestra el formulario ni consulta productos sin seleccionar lista', async () => {
    mount()
    await screen.findByRole('link', { name: 'Segunda' })
    expect(screen.getByText(/Selecciona una lista para ver/)).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: 'Agregar ítem' })).not.toBeInTheDocument()
    expect(fetchMock.mock.calls.every(([url]) => !url.endsWith('/catalog-products'))).toBe(true)
  })

  it('envía el ID del producto y el de la segunda lista, muestra el ítem y conserva selección en historial', async () => {
    mount()
    fireEvent.click(await screen.findByRole('link', { name: 'Primera' }))
    await screen.findByRole('heading', { name: 'Agregar ítem a Primera' })
    fireEvent.click(screen.getByRole('link', { name: 'Segunda' }))
    await screen.findByRole('heading', { name: 'Agregar ítem a Segunda' })
    await chooseProduct()
    fireEvent.change(screen.getByRole('spinbutton', { name: /cantidad/i }), { target: { value: '2' } })
    fireEvent.click(screen.getByRole('button', { name: 'Agregar ítem' }))
    await screen.findByText('Arroz se agregó a esta lista.')
    const writes = fetchMock.mock.calls.filter(([url]) => url.endsWith('/items'))
    expect(writes).toHaveLength(1)
    expect(writes[0][0]).toContain(`/purchaselists/${secondId}/items`)
    expect(JSON.parse(writes[0][1].body)).toEqual({ productId, productName: 'Arroz', quantity: 2, price: 0 })
    expect(lists[0].items).toEqual([])
    expect(screen.getByText(/Arroz · Cantidad: 2/)).toBeInTheDocument()
    fireEvent.click(screen.getByRole('button', { name: 'Atrás' }))
    await screen.findByRole('heading', { name: 'Agregar ítem a Primera' })
    fireEvent.click(screen.getByRole('button', { name: 'Adelante' }))
    await screen.findByRole('heading', { name: 'Agregar ítem a Segunda' })
  })

  it('abre el detalle por URL y no elige la primera lista', async () => {
    mount(`/personal-lists/${secondId}`)
    await screen.findByRole('heading', { name: 'Agregar ítem a Segunda' })
    expect(screen.getByRole('link', { name: /Segunda/ })).toHaveAttribute('aria-current', 'page')
  })

  it('no permite escribir en un ID que no pertenece a las listas cargadas', async () => {
    mount('/personal-lists/unknown')
    await screen.findByRole('alert')
    expect(screen.queryByRole('button', { name: 'Agregar ítem' })).not.toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Volver a mis listas' })).toBeInTheDocument()
  })

  it('explica el catálogo vacío sin ofrecer envíos nulos', async () => {
    products = []
    mount(`/personal-lists/${secondId}`)
    await screen.findByText(/No hay productos disponibles en el catálogo/)
    expect(screen.queryByRole('button', { name: 'Agregar ítem' })).not.toBeInTheDocument()
  })

  it('conserva producto y cantidades ante 400 y permite reintentar', async () => {
    failure = true
    mount(`/personal-lists/${secondId}`)
    await chooseProduct()
    fireEvent.change(screen.getByRole('spinbutton', { name: /cantidad/i }), { target: { value: '3' } })
    fireEvent.click(screen.getByRole('button', { name: 'Agregar ítem' }))
    expect(await screen.findByRole('alert')).toHaveTextContent('La cantidad no es válida.')
    expect(screen.getByRole('combobox', { name: /producto/i })).toHaveValue(productId)
    expect(screen.getByRole('spinbutton', { name: /cantidad/i })).toHaveValue(3)
    failure = false
    fireEvent.click(screen.getByRole('button', { name: 'Agregar ítem' }))
    await screen.findByText('Arroz se agregó a esta lista.')
  })

  it.each([['0', '0'], ['1.5', '0'], ['-1', '0'], ['1', '-1'], ['', '0']])('bloquea cantidad/precio inválidos %s/%s', async (quantity, price) => {
    mount(`/personal-lists/${secondId}`)
    await chooseProduct()
    fireEvent.change(screen.getByRole('spinbutton', { name: /cantidad/i }), { target: { value: quantity } })
    fireEvent.change(screen.getByRole('spinbutton', { name: /precio/i }), { target: { value: price } })
    expect(screen.getByRole('button', { name: 'Agregar ítem' })).toBeDisabled()
    expect(fetchMock.mock.calls.filter(([url]) => url.endsWith('/items'))).toHaveLength(0)
  })

  it('abre la lista recién creada', async () => {
    mount()
    await screen.findByRole('link', { name: 'Primera' })
    fireEvent.change(screen.getByRole('textbox', { name: /nombre de lista/i }), { target: { value: 'Nueva compra' } })
    fireEvent.click(screen.getByRole('button', { name: 'Crear lista' }))
    await screen.findByRole('heading', { name: 'Agregar ítem a Nueva compra' })
    expect(screen.getByTestId('path')).toHaveTextContent('/personal-lists/new-list')
  })

  it('impide doble envío y borra el borrador al cambiar de lista', async () => {
    mount(`/personal-lists/${secondId}`)
    await chooseProduct()
    let release!: () => void
    const original = fetchMock.getMockImplementation()!
    fetchMock.mockImplementation(async (url: string, init?: RequestInit) => {
      if (url.endsWith('/items')) await new Promise<void>((resolve) => { release = resolve })
      return original(url, init)
    })
    const button = screen.getByRole('button', { name: 'Agregar ítem' })
    fireEvent.click(button)
    await waitFor(() => expect(screen.getByRole('button', { name: /Agregando ítem/ })).toBeDisabled())
    fireEvent.submit(button.closest('form')!)
    await waitFor(() => expect(fetchMock.mock.calls.filter(([url]) => url.endsWith('/items'))).toHaveLength(1))
    await act(async () => { release() })
    await screen.findByText('Arroz se agregó a esta lista.')
    await chooseProduct()
    fireEvent.click(screen.getByRole('link', { name: 'Primera' }))
    await screen.findByRole('heading', { name: 'Agregar ítem a Primera' })
    expect(screen.getByRole('combobox', { name: /producto/i })).toHaveValue('')
  })
})
