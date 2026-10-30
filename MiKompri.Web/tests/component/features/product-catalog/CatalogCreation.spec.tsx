import '../../../setup/jsdom-dialogs'
import userEvent from '@testing-library/user-event'
import { FluentProvider } from '@fluentui/react-components'
import { appTheme } from '../../../../src/shared/ui/theme'
import React from 'react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { act, cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { ProductCatalogPage } from '../../../../src/features/product-catalog/pages/ProductCatalogPage'
import { PersonalListsPage } from '../../../../src/features/shopping-lists/pages/PersonalListsPage'

const account = vi.hoisted(() => ({ homeAccountId: 'catalog-user' }))
vi.mock('@azure/msal-react', () => ({ useMsal: () => ({ accounts: [account], instance: { acquireTokenSilent: async () => ({ accessToken: 'token' }) } }) }))
const listId = '11111111-1111-4111-8111-111111111111'
const productId = '22222222-2222-4222-8222-222222222222'
let products: { id: string; name: string; purchaseUnit: string }[]
let status: number
let errorBody: object
let fetchMock: ReturnType<typeof vi.fn>

let user: ReturnType<typeof userEvent.setup>
beforeEach(() => {
  user = userEvent.setup()
  products = []
  status = 201
  errorBody = {}
  fetchMock = vi.fn(async (url: string, init?: RequestInit) => {
    if (url.endsWith('/catalog-products')) {
      if (init?.method === 'POST') {
        if (status !== 201) return Response.json(errorBody, { status })
        const product = { id: productId, ...JSON.parse(String(init.body)) }
        products.push(product)
        return Response.json(product, { status: 201 })
      }
      return Response.json(products)
    }
    if (url.includes('/markets')) return Response.json([])
    if (url.endsWith('/purchaselists/mine')) return Response.json([{ id: listId, name: 'Compra semanal', items: [] }])
    throw new Error(`Unexpected request: ${url}`)
  })
  vi.stubGlobal('fetch', fetchMock)
})
afterEach(() => { cleanup(); vi.unstubAllGlobals() })

function mount(path = '/product-catalog') {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(<FluentProvider theme={appTheme}><QueryClientProvider client={client}><MemoryRouter initialEntries={[path]}><Routes>
    <Route path="/product-catalog" element={<ProductCatalogPage />} />
    <Route path="/personal-lists/:listId" element={<PersonalListsPage />} />
  </Routes></MemoryRouter></QueryClientProvider></FluentProvider>)
}
async function fill() {
  fireEvent.change(await screen.findByRole('textbox', { name: 'Nombre del producto' }), { target: { value: ' Arroz ' } })
  fireEvent.change(screen.getByRole('textbox', { name: 'Unidad de compra' }), { target: { value: ' kg ' } })
}

describe('crear productos desde el catálogo', () => {
  it('cierra el circuito lista → catálogo → producto nuevo → lista con producto disponible', async () => {
    mount(`/personal-lists/${listId}`)
    await user.click(await screen.findByRole('button', { name: 'Añadir productos' }))
    await user.click(await screen.findByRole('link', { name: 'Crear producto en el catálogo' }))
    await fill()
    await user.click(screen.getByRole('button', { name: 'Crear producto', exact: true }))
    await screen.findByText('Arroz se creó y ya está disponible para tus listas.')
    const writes = fetchMock.mock.calls.filter(([, init]) => init?.method === 'POST')
    expect(writes).toHaveLength(1)
    expect(JSON.parse(writes[0][1].body)).toEqual({ name: 'Arroz', purchaseUnit: 'kg' })
    expect(writes[0][1].headers.Authorization).toBe('Bearer token')
    expect(screen.getByText('Arroz (kg)')).toBeInTheDocument()
    await user.click(screen.getByRole('link', { name: 'Volver a la lista' }))
    await screen.findByRole('heading', { name: 'Compra semanal', exact: true })
    await user.click(await screen.findByRole('button', { name: 'Añadir productos' }))
    await user.click(await screen.findByRole('button', { name: 'Arroz (kg)' }))
    expect(screen.getByRole('button', { name: 'Agregar ítem', exact: true })).toBeEnabled()
  })

  it('no envía campos vacíos o solo espacios y aplica los límites del contrato', async () => {
    mount()
    const name = await screen.findByRole('textbox', { name: 'Nombre del producto' })
    const unit = screen.getByRole('textbox', { name: 'Unidad de compra' })
    expect(name).toHaveAttribute('maxlength', '120')
    expect(unit).toHaveAttribute('maxlength', '30')
    fireEvent.change(name, { target: { value: '   ' } })
    fireEvent.change(unit, { target: { value: 'kg' } })
    expect(screen.getByRole('button', { name: 'Crear producto', exact: true })).toBeDisabled()
    expect(fetchMock.mock.calls.filter(([, init]) => init?.method === 'POST')).toHaveLength(0)
  })

  it('conserva los campos ante conflicto de nombre/unidad', async () => {
    status = 409
    mount()
    await fill()
    await user.click(screen.getByRole('button', { name: 'Crear producto', exact: true }))
    expect(await screen.findByRole('alert')).toHaveTextContent(/Ya existe un producto/)
    expect(screen.getByRole('textbox', { name: 'Nombre del producto' })).toHaveValue(' Arroz ')
    expect(screen.getByRole('textbox', { name: 'Unidad de compra' })).toHaveValue(' kg ')
  })

  it.each([
    { errors: [{ field: 'Name', message: 'El nombre no es válido.' }] },
    { errors: { Name: ['El nombre no es válido.'] } },
  ])('muestra validaciones de API y permite reintentar', async (body) => {
    status = 400
    errorBody = body
    mount()
    await fill()
    await user.click(screen.getByRole('button', { name: 'Crear producto', exact: true }))
    expect(await screen.findByRole('alert')).toHaveTextContent('El nombre no es válido.')
    status = 201
    await user.click(screen.getByRole('button', { name: 'Crear producto', exact: true }))
    await screen.findByText('Arroz se creó y ya está disponible para tus listas.')
  })

  it('ignora destinos de regreso externos', async () => {
    mount('/product-catalog?returnTo=https://example.com')
    await screen.findByRole('textbox', { name: 'Nombre del producto' })
    expect(screen.queryByRole('link', { name: 'Volver a la lista' })).not.toBeInTheDocument()
  })

  it('conserva la pestaña del detalle compartido en el regreso al catálogo', async () => {
    const destination = `/shared-lists/${listId}?tab=expenses`
    mount(`/product-catalog?returnTo=${encodeURIComponent(destination)}`)
    expect(await screen.findByRole('link', { name: 'Volver a la lista' })).toHaveAttribute('href', destination)
  })

  it('impide envíos duplicados mientras se guarda', async () => {
    mount()
    await fill()
    let release!: () => void
    const original = fetchMock.getMockImplementation()!
    fetchMock.mockImplementation(async (url: string, init?: RequestInit) => {
      if (init?.method === 'POST') await new Promise<void>((resolve) => { release = resolve })
      return original(url, init)
    })
    const button = screen.getByRole('button', { name: 'Crear producto', exact: true })
    await user.click(button)
    await screen.findByRole('button', { name: 'Creando producto…' })
    fireEvent.submit(button.closest('form')!)
    await waitFor(() => expect(fetchMock.mock.calls.filter(([, init]) => init?.method === 'POST')).toHaveLength(1))
    await act(async () => { release() })
    await screen.findByText('Arroz se creó y ya está disponible para tus listas.')
  })
})
