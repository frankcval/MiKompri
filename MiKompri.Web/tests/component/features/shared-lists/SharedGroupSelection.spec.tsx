import '../../../setup/jsdom-dialogs'
import userEvent from '@testing-library/user-event'
import { FluentProvider } from '@fluentui/react-components'
import { appTheme } from '../../../../src/shared/ui/theme'
import React from 'react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { cleanup, fireEvent, render, screen, within, waitFor } from '@testing-library/react'
import { MemoryRouter, useLocation, useNavigate } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { SharedListsPage } from '../../../../src/features/shared-lists/pages/SharedListsPage'

const account = vi.hoisted(() => ({ homeAccountId: 'group-user', tenantId: '00000000-0000-0000-0000-000000000000' }))
vi.mock('@azure/msal-react', () => ({ useMsal: () => ({ accounts: [account], instance: { acquireTokenSilent: async () => ({ accessToken: 'token' }) } }) }))
vi.mock('../../../../src/features/settlements/pages/SettlementPage', () => ({ SettlementPage: ({ sharedListId }: { sharedListId: string }) => <div role="region" aria-label={`Settlement ${sharedListId}`} /> }))
const first = '11111111-1111-4111-8111-111111111111'
const second = '22222222-2222-4222-8222-222222222222'
let groups: { id: string; name: string }[]
let lists: Record<string, { id: string; name: string; groupId: string }[]>
let groupsError: boolean
let forbidden: boolean
let createError: boolean
let fetchMock: ReturnType<typeof vi.fn>

let user: ReturnType<typeof userEvent.setup>
beforeEach(() => {
  user = userEvent.setup()
  groups = [{ id: first, name: 'Familia' }, { id: second, name: 'Amigos' }]
  lists = { [first]: [{ id: 'list-family', name: 'Compra casa', groupId: first }], [second]: [] }
  groupsError = false
  forbidden = false
  createError = false
  fetchMock = vi.fn(async (url: string, init?: RequestInit) => {
    if (url.endsWith('/groups')) return Response.json(groupsError ? {} : groups, { status: groupsError ? 503 : 200 })
    if (init?.method === 'POST') {
      if (createError) return Response.json({}, { status: 403 })
      const body = JSON.parse(String(init.body))
      lists[body.groupId].push({ id: 'created', ...body })
      return Response.json('created', { status: 201 })
    }
    const groupId = new URL(url).searchParams.get('groupId')!
    return Response.json(forbidden ? {} : lists[groupId], { status: forbidden ? 403 : 200 })
  })
  vi.stubGlobal('fetch', fetchMock)
})
afterEach(() => { cleanup(); vi.unstubAllGlobals() })

function Navigation() {
  const navigate = useNavigate()
  return <><output data-testid="query">{useLocation().search}</output><button onClick={() => navigate(-1)}>Atrás</button><button onClick={() => navigate(1)}>Adelante</button></>
}
function mount(search = '') {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(<FluentProvider theme={appTheme}><QueryClientProvider client={client}><MemoryRouter initialEntries={['/shared-lists' + search]}><Navigation /><SharedListsPage /></MemoryRouter></QueryClientProvider></FluentProvider>)
}
async function select(id: string) {
  fireEvent.change(await screen.findByRole('combobox', { name: 'Grupo' }), { target: { value: id } })
}

describe('grupos propios por nombre en listas compartidas', () => {
  it('no muestra GroupId ni consulta listas antes de elegir grupo', async () => {
    mount()
    await screen.findByRole('option', { name: 'Familia' })
    expect(screen.queryByRole('textbox', { name: 'GroupId' })).not.toBeInTheDocument()
    expect(screen.queryByRole('button', { name: 'Crear lista compartida' })).not.toBeInTheDocument()
    expect(fetchMock.mock.calls.every(([url]) => url.endsWith('/groups'))).toBe(true)
  })

  it('consulta y crea usando el grupo elegido por nombre y refresca solo sus listas', async () => {
    mount()
    await select(second)
    await screen.findByText('Este grupo todavía no tiene listas. Crea la primera lista compartida.')
    expect(screen.getByTestId('query')).toHaveTextContent(`groupId=${second}`)
    await user.click(screen.getByRole('button', { name: 'Crear lista compartida' }))
    expect(await screen.findByRole('dialog')).toHaveAccessibleName('Crear lista en Amigos')
    fireEvent.change(screen.getByRole('textbox', { name: 'Nombre de lista' }), { target: { value: ' Viaje ' } })
    await user.click(within(screen.getByRole('dialog')).getByRole('button', { name: 'Crear lista', exact: true }))
    await screen.findByText('Viaje')
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
    const writes = fetchMock.mock.calls.filter(([, init]) => init?.method === 'POST')
    expect(writes).toHaveLength(1)
    expect(JSON.parse(writes[0][1].body)).toEqual({ name: 'Viaje', groupId: second })
    expect(fetchMock.mock.calls.filter(([url]) => url.includes('groupId=')).every(([url]) => url.includes(second))).toBe(true)
  })

  it('restaura el grupo desde la URL sin consultar otro grupo', async () => {
    mount(`?groupId=${second}`)
    await screen.findByRole('heading', { name: 'Listas de Amigos' })
    expect(screen.getByRole('combobox', { name: 'Grupo' })).toHaveValue(second)
    expect(fetchMock.mock.calls.filter(([url]) => url.includes('groupId=')).every(([url]) => url.includes(second))).toBe(true)
  })

  it('rechaza grupo ajeno de la URL sin enviar su ID a ShoppingList', async () => {
    mount('?groupId=unknown')
    expect(await screen.findByRole('alert')).toHaveTextContent(/no está disponible entre tus grupos/)
    expect(fetchMock.mock.calls.every(([url]) => url.endsWith('/groups'))).toBe(true)
  })

  it('ofrece crear grupo si no hay membresías', async () => {
    groups = []
    mount()
    expect(await screen.findByRole('link', { name: 'Crear grupo' })).toHaveAttribute('href', '/groups')
    expect(screen.queryByRole('combobox')).not.toBeInTheDocument()
  })

  it('muestra error de grupos y permite reintentar', async () => {
    groupsError = true
    mount()
    await screen.findByRole('alert')
    groupsError = false
    await user.click(screen.getByRole('button', { name: 'Reintentar' }))
    await screen.findByRole('option', { name: 'Familia' })
  })

  it('limpia settlement y borradores al cambiar de grupo; Atrás y Adelante restauran el grupo', async () => {
    mount(`?groupId=${first}`)
    expect(await screen.findByRole('link', { name: 'Ver reparto de gastos de Compra casa' })).toHaveAttribute('href', `/shared-lists/list-family?groupId=${first}&tab=expenses`)
    await user.click(screen.getByRole('button', { name: 'Crear lista compartida' }))
    fireEvent.change(await screen.findByRole('textbox', { name: 'Nombre de lista' }), { target: { value: 'Borrador' } })
    await user.click(screen.getByRole('button', { name: 'Cancelar' }))
    await select(second)
    await screen.findByRole('heading', { name: 'Listas de Amigos' })
    expect(screen.queryByRole('region', { name: 'Settlement list-family' })).not.toBeInTheDocument()
    await user.click(screen.getByRole('button', { name: 'Atrás' }))
    await screen.findByRole('heading', { name: 'Listas de Familia' })
    await user.click(screen.getByRole('button', { name: 'Crear lista compartida' }))
    expect(await screen.findByRole('textbox', { name: 'Nombre de lista' })).toHaveValue('')
    await user.click(screen.getByRole('button', { name: 'Cancelar' }))
    await user.click(screen.getByRole('button', { name: 'Adelante' }))
    await screen.findByRole('heading', { name: 'Listas de Amigos' })
  })

  it('no habilita crear ni muestra datos cuando ShoppingList rechaza la membresía', async () => {
    forbidden = true
    mount(`?groupId=${first}`)
    expect(await screen.findByRole('alert')).toHaveTextContent(/No tienes permisos/)
    expect(screen.getByRole('button', { name: 'Crear lista compartida' })).toBeDisabled()
    expect(screen.queryByText('Compra casa')).not.toBeInTheDocument()
  })

  it('conserva el nombre y muestra permiso denegado al fallar la creación', async () => {
    createError = true
    mount(`?groupId=${first}`)
    await screen.findByText('Compra casa')
    await user.click(screen.getByRole('button', { name: 'Crear lista compartida' }))
    fireEvent.change(await screen.findByRole('textbox', { name: 'Nombre de lista' }), { target: { value: 'Mi lista' } })
    await user.click(within(screen.getByRole('dialog')).getByRole('button', { name: 'Crear lista', exact: true }))
    expect(await screen.findByRole('alert')).toHaveTextContent(/No tienes permiso para crear listas/)
    expect(screen.getByRole('textbox', { name: 'Nombre de lista' })).toHaveValue('Mi lista')
    await waitFor(() => expect(screen.getByRole('button', { name: 'Crear lista', exact: true })).toBeEnabled())
  })
})
