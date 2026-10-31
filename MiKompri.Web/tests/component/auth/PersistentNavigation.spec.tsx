import React from 'react'
import { act, cleanup, fireEvent, render, screen, within } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { MemoryRouter, useLocation, useNavigate, useRoutes } from 'react-router-dom'
import { InteractionStatus } from '@azure/msal-browser'
import { useIsAuthenticated, useMsal } from '@azure/msal-react'
import { appRoutes } from '../../../src/app/router/routes'

vi.mock('@azure/msal-react', () => ({ useIsAuthenticated: vi.fn(), useMsal: vi.fn() }))
vi.mock('../../../src/features/profile/pages/ProfilePage', () => ({ ProfilePage: () => <h1>Perfil de prueba</h1> }))
vi.mock('../../../src/features/profile/pages/LoginPage', () => ({ LoginPage: () => <h1>Acceso de prueba</h1> }))
vi.mock('../../../src/features/groups/pages/GroupsPage', () => ({ GroupsPage: () => <h1>Grupos de prueba</h1> }))
vi.mock('../../../src/features/groups/pages/GroupDetailPage', () => ({ GroupDetailPage: () => <h1>Detalle de grupo de prueba</h1> }))
vi.mock('../../../src/features/shopping-lists/pages/ListsPage', () => ({ ListsPage: () => <h1>Mis listas de prueba</h1> }))
vi.mock('../../../src/features/shopping-lists/pages/PersonalListsPage', () => ({ PersonalListsPage: () => <h1>Personales de prueba</h1> }))
vi.mock('../../../src/features/shared-lists/pages/SharedListsPage', () => ({ SharedListsPage: () => <h1>Compartidas de prueba</h1> }))
vi.mock('../../../src/features/shared-lists/pages/SharedListDetailPage', () => ({ SharedListDetailPage: () => <h1>Detalle compartido de prueba</h1> }))
vi.mock('../../../src/features/product-catalog/pages/ProductCatalogPage', () => ({ ProductCatalogPage: () => <h1>Catálogo de prueba</h1> }))

const directPaths = ['/profile', '/groups', '/groups/group-qa', '/personal-lists', '/shared-lists', '/product-catalog', '/lists']
function TestRoutes() {
  const navigate = useNavigate(); const location = useLocation(); const content = useRoutes(appRoutes)
  return <><output data-testid="path">{location.pathname}</output><button onClick={() => navigate(-1)}>Atrás</button><button onClick={() => navigate(1)}>Adelante</button>{content}</>
}
beforeEach(() => {
  vi.mocked(useIsAuthenticated).mockReturnValue(true)
  vi.mocked(useMsal).mockReturnValue({ accounts: [{ homeAccountId: 'nav-user', tenantId: '00000000-0000-0000-0000-000000000000' }], inProgress: InteractionStatus.None } as never)
})
afterEach(cleanup)

describe('navegación autenticada persistente', () => {
  it.each([['/personal-lists/example', 'Mis listas'], ['/shared-lists/example', 'Mis listas'], ['/groups/group-qa', 'Grupos']])('identifica el destino padre de %s con icono decorativo y etiqueta', (path, label) => {
    render(<MemoryRouter initialEntries={[path]}><TestRoutes /></MemoryRouter>)
    const nav = screen.getByRole('navigation', { name: 'Navegación principal' })
    const lists = within(nav).getByRole('link', { name: label })
    expect(lists).toHaveAttribute('aria-current', 'page')
    expect(lists.querySelector('svg')).toHaveAttribute('aria-hidden', 'true')
    expect(lists.querySelector('svg')).toHaveAttribute('focusable', 'false')
  })
  it('conserva la sección y el detalle de grupo con Atrás y Adelante', async () => {
    render(<MemoryRouter initialEntries={['/groups', '/groups/group-qa']}><TestRoutes /></MemoryRouter>)
    const nav = screen.getByRole('navigation', { name: 'Navegación principal' })
    await act(async () => { fireEvent.click(screen.getByRole('button', { name: 'Atrás' })) })
    expect(screen.getByTestId('path')).toHaveTextContent('/groups')
    await act(async () => { fireEvent.click(screen.getByRole('button', { name: 'Adelante' })) })
    expect(screen.getByTestId('path')).toHaveTextContent('/groups/group-qa')
    expect(within(nav).getByRole('link', { name: 'Grupos' })).toHaveAttribute('aria-current', 'page')
  })
  it('mantiene navegación y contexto con Atrás/Adelante', async () => {
    render(<MemoryRouter initialEntries={['/']}><TestRoutes /></MemoryRouter>)
    const menu = screen.getByRole('navigation', { name: 'Navegación principal' })
    for (const [path, label] of [['/groups', 'Grupos'], ['/product-catalog', 'Productos'], ['/lists', 'Mis listas']]) {
      fireEvent.click(within(menu).getByRole('link', { name: label }))
      expect(screen.getByTestId('path')).toHaveTextContent(path)
      expect(within(menu).getByRole('link', { name: label })).toHaveAttribute('aria-current', 'page')
      expect(screen.getAllByRole('main')).toHaveLength(1)
    }
    await act(async () => { fireEvent.click(screen.getByRole('button', { name: 'Atrás' })) })
    expect(within(menu).getByRole('link', { name: 'Productos' })).toHaveAttribute('aria-current', 'page')
    await act(async () => { fireEvent.click(screen.getByRole('button', { name: 'Adelante' })) })
    expect(within(menu).getByRole('link', { name: 'Mis listas' })).toHaveAttribute('aria-current', 'page')
    expect(screen.getByRole('navigation', { name: 'Navegación principal' })).toBe(menu)
  })
  it.each(directPaths)('conserva enlace directo y navegación en %s', path => {
    render(<MemoryRouter initialEntries={[path]}><TestRoutes /></MemoryRouter>)
    expect(screen.getByRole('navigation', { name: 'Navegación principal' })).toBeInTheDocument()
    expect(screen.getByTestId('path')).toHaveTextContent(path)
    expect(screen.getByRole('link', { name: 'Tu cuenta y sesión' })).toHaveAttribute('href', '/profile')
  })
  it.each(directPaths)('protege el acceso directo a %s', path => {
    vi.mocked(useIsAuthenticated).mockReturnValue(false)
    vi.mocked(useMsal).mockReturnValue({ accounts: [], inProgress: InteractionStatus.None } as never)
    render(<MemoryRouter initialEntries={[path]}><TestRoutes /></MemoryRouter>)
    expect(screen.getByRole('heading', { name: 'Acceso de prueba' })).toBeInTheDocument()
    expect(screen.queryByRole('navigation')).not.toBeInTheDocument()
  })
  it('muestra carga durante bootstrap', () => {
    vi.mocked(useMsal).mockReturnValue({ accounts: [{ homeAccountId: 'nav-user', tenantId: '00000000-0000-0000-0000-000000000000' }], inProgress: InteractionStatus.Startup } as never)
    render(<MemoryRouter initialEntries={['/groups']}><TestRoutes /></MemoryRouter>)
    expect(screen.queryByRole('navigation')).not.toBeInTheDocument()
    expect(screen.getByText('Recuperando tu sesión…')).toBeInTheDocument()
  })
})
