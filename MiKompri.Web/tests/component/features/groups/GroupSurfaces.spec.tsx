import React from 'react'
import '../../../setup/jsdom-dialogs'
import { cleanup, render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { FluentProvider } from '@fluentui/react-components'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, beforeEach, expect, it, vi } from 'vitest'
import { GroupsPage } from '../../../../src/features/groups/pages/GroupsPage'
import { appTheme } from '../../../../src/shared/ui/theme'

vi.mock('@azure/msal-react', () => ({ useMsal: () => ({ accounts: [{ homeAccountId: 'groups-qa', tenantId: '00000000-0000-0000-0000-000000000000' }], instance: { acquireTokenSilent: async () => ({ accessToken: 'synthetic' }) } }) }))
let groups: { id: string; name: string; myRole?: string; memberCount?: number }[]
let failCreate: boolean
let fetchMock: ReturnType<typeof vi.fn>
beforeEach(() => {
  groups = []; failCreate = false
  fetchMock = vi.fn(async (_url: string, init?: RequestInit) => {
    if (init?.method === 'POST') {
      if (failCreate) return Response.json({}, { status: 409 })
      const group = { id: 'created', name: JSON.parse(String(init.body)).name, myRole: 'Owner', memberCount: 1 }
      groups.push(group); return Response.json(group, { status: 201 })
    }
    return Response.json(groups)
  })
  vi.stubGlobal('fetch', fetchMock)
})
afterEach(() => { cleanup(); vi.unstubAllGlobals() })
function mount() {
  render(<FluentProvider theme={appTheme}><QueryClientProvider client={new QueryClient({ defaultOptions: { queries: { retry: false } } })}><MemoryRouter><GroupsPage /></MemoryRouter></QueryClientProvider></FluentProvider>)
}
it('offers a real creation action in the empty state without advertising group lifecycle', async () => {
  mount()
  expect(await screen.findByRole('heading', { name: 'Aún no tienes grupos' })).toBeVisible()
  expect(screen.getByRole('button', { name: 'Crear tu primer grupo' })).toBeEnabled()
  expect(screen.queryByRole('button', { name: /invitar|eliminar|abandonar|transferir/i })).not.toBeInTheDocument()
})
it('renders the real role and member count and omits metadata absent from the response', async () => {
  groups = [{ id: 'home', name: 'Nuestra casa', myRole: 'Owner', memberCount: 3 }, { id: 'other', name: 'Otro espacio' }]
  mount()
  const cards = await screen.findByRole('list', { name: 'Tus grupos' })
  const home = within(cards).getByRole('link', { name: /Nuestra casa/ })
  expect(home).toHaveAttribute('href', '/groups/home')
  expect(home).toHaveTextContent('Propietario'); expect(home).toHaveTextContent('3 personas')
  expect(within(cards).getByRole('link', { name: /Otro espacio/ })).not.toHaveTextContent(/personas|Propietario|Miembro/)
  expect(home.querySelectorAll('a,button,input')).toHaveLength(0)
})
it('retains the draft on server error, creates once, refreshes groups and restores focus', async () => {
  const user = userEvent.setup(); mount()
  await screen.findByRole('heading', { name: 'Aún no tienes grupos' })
  const trigger = screen.getByRole('button', { name: 'Crear grupo', exact: true })
  await user.click(trigger)
  const dialog = screen.getByRole('dialog', { name: 'Crear grupo' })
  const input = within(dialog).getByRole('textbox', { name: 'Nombre del grupo' })
  await user.type(input, 'Casa QA')
  failCreate = true
  await user.click(within(dialog).getByRole('button', { name: 'Crear grupo', exact: true }))
  expect(await within(dialog).findByRole('alert')).toBeVisible(); expect(input).toHaveValue('Casa QA')
  failCreate = false
  await user.click(within(dialog).getByRole('button', { name: 'Crear grupo', exact: true }))
  await screen.findByRole('link', { name: /Casa QA/ })
  await waitFor(() => expect(trigger).toHaveFocus())
  expect(fetchMock.mock.calls.filter(([, init]) => init?.method === 'POST')).toHaveLength(2)
})
